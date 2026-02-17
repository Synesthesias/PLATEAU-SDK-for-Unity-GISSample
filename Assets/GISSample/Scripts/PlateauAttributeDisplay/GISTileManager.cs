using GISSample.PlateauAttributeDisplay.Gml;
using PLATEAU.DynamicTile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay
{
    public class GISTileManager : MonoBehaviour
    {
        public static readonly int YIELD_STEP = 20; // Coroutine実行時に一度に処理するGameObject数

        [SerializeField] public bool UseCoroutineForTiles = true; //タイル読込後の色変更等の処理にコルーチン使用（タイル毎）
        [SerializeField] public bool RunCoroutineOnEveryLoad = true; //タイル読込完了時に毎回コルーチン実行 / 全タイル読込完了時のみにコルーチン実行
        [SerializeField] public bool UseCoroutineForOperations = false; //タイル読込後のフィルター・色変更等処理に各GameObject毎にコルーチン使用 (YIELD_STEP単位)
        [HideInInspector][SerializeField] public bool UseCoroutineForInteraction = false; //ボタンクリック時のフィルター・色変更等処理にコルーチン使用  （全体処理なので遅延が大きすぎるためOFF推奨）  
        [SerializeField] public bool ZoomLevel11Only = true; // Zoom Level 11 以外は無視
        [SerializeField] public bool ShowDebugLogs = false;

        /// <summary>
        /// 各Zoomレベルごとのカメラからのロード距離定義をオーバーライドします。
        /// {zoomLevel, (最小距離, 最大距離)}
        /// </summary>
        public Dictionary<int, (float, float)> loadDistances = new Dictionary<int, (float, float)>
        {
            //{ 11, (-10000f, 500f) },
            //{ 10, (500f, 1500f) },
            //{ 9, (1500f, 10000f) },
            { 11, (-10000f, 500f) },
            { 10, (500f, 1500f) },
            { 9, (1500f, 100000f) },
        };

        [SerializeField]
        private PLATEAUTileManager tileManager;

        [SerializeField]
        private SceneManager sceneManager;

        private IEnumerable<SampleGml> GmlsList => tileGmlCache.Values;

        private Dictionary<string, SampleGml> tileGmlCache = new();

        private Dictionary<string, Func<IEnumerator>> coroutineQueue = new Dictionary<string, Func<IEnumerator>>();

        private Coroutine baseCoroutine;
        private Coroutine currentCoroutine;
        private string currentCoroutineTileAddress;

        public bool IsCoroutineRunning => isCoroutineRunning;
        private bool isCoroutineRunning = false;

        public int NumCoroutines => coroutineQueue.Count;

        public bool IsTileLoading => tileManager?.IsCoroutineRunning == true || tileManager?.HasCurrentTask == true;

        public bool IsTileInitialized { get; private set; } = false; // 初回タイルロード完了

        void Start()
        {
            if (tileManager == null)
            {
                tileManager = FindFirstObjectByType<PLATEAUTileManager>();
                if (tileManager == null)
                {
                    Debug.LogError("PLATEAUTileManager component not found in the scene.");
                    return;
                }
            }

            if (sceneManager == null)
            {
                sceneManager = FindFirstObjectByType<SceneManager>();
            }

#if UNITY_EDITOR
            PLATEAUSceneViewCameraTracker.Release(); //Editor/Runtime切替時のエラー軽減
#endif

            PLATEAURuntimeCameraTracker.StopCameraTracking(); //　自前のUpdateでカメラ移動を監視

            tileManager.loadDistances = loadDistances;
            tileManager.onTileInstantiatedAction += onTileInstanciated;
            tileManager.onTileUnloadBegin += onTileUnloaded;
            tileManager.onTileInstantiationComplete += onAllTileLoaded;

            StartCoroutine(Initialize());　//初回ロード
        }

        private void OnDestroy()
        {
            tileManager.onTileInstantiatedAction -= onTileInstanciated;
            tileManager.onTileUnloadBegin -= onTileUnloaded;
            tileManager.onTileInstantiationComplete -= onAllTileLoaded;
        }


        /// <summary>
        /// 初期化処理
        /// タイル読込が完了しない場合があるので、各処理の終了を待って処理を開始
        /// </summary>
        /// <returns></returns>
        IEnumerator Initialize()
        {

#if UNITY_EDITOR
            PLATEAUSceneViewCameraTracker.Release(); //Editor/Runtime切替時のエラー軽減
#endif
            PLATEAURuntimeCameraTracker.StopCameraTracking(); //　自前のUpdateでカメラ移動を監視 (自動アップデートしない）

            yield return new WaitUntil(() => sceneManager?.IsInitialized == true);
            yield return new WaitUntil(() => tileManager?.State == PLATEAUTileManager.ManagerState.Operating);
            yield return new WaitUntil(() => !IsTileLoading);

            UpdateCameraPosition(sceneManager.CameraPosition); //初回ロード開始

            Debug.Log($"GISTileManager Tile Initialized.");
        }

        /// <summary>
        /// タイル読込が全て完了した際の処理
        /// </summary>
        private void onAllTileLoaded()
        {
            StartCoroutineProcess();

            if (!IsTileInitialized)
            {
                // 更新されないことがあるので再読み込み
                UpdateCameraPosition(sceneManager.CameraPosition);

                if(!IsTileLoading)
                    IsTileInitialized = true;
            }
            else
                IsTileInitialized = true;
        }

        /// <summary>
        /// カメラ位置に応じてタイル読込実行
        /// </summary>
        /// <param name="position"></param>
        public void UpdateCameraPosition(Vector3 position)
        {
            if (tileManager?.CheckIfCameraPositionHasChanged(position) == true)
                tileManager?.UpdateAssetsByCameraPosition(position);
        }

        /// <summary>
        /// タイル読込時の処理
        /// </summary>
        /// <param name="tile"></param>
        private void onTileInstanciated(PLATEAUDynamicTile tile)
        {
            Log($"<color=yellow>Tile instantiated: {tile.Address}</color>");

            if (ZoomLevel11Only && tile.ZoomLevel < 11) // ZoomLevel 11のみ
                return;

            // Check cache first
            if (tileGmlCache.TryGetValue(tile.Address, out var cachedGml))
            {
                ProcessGml(cachedGml);
                return;
            }

            InitializeTile(tile);
        }

        /// <summary>
        /// タイルがアンロードされた際の処理
        /// </summary>
        /// <param name="tile"></param>
        private void onTileUnloaded(PLATEAUDynamicTile tile)
        {
            Log($"<color=red>Tile unload begin: {tile.Address}</color>");

            if (ZoomLevel11Only && tile.ZoomLevel < 11) // ZoomLevel 11のみ
                return;

            RemoveCoroutineByAddress(tile.Address);
        }

        private void InitializeTile(PLATEAUDynamicTile tile)
        {
            var gml = new SampleGml();
            gml.InitializeTile(tile);
            if(tileGmlCache.TryAdd(tile.Address, gml))
            {
                ProcessGml(gml); // コルーチンスタート
            }
        }

        /// <summary>
        /// sceneManager側で指定samplegmlのFilter, マテリアル変更等の処理を行う
        /// </summary>
        /// <param name="gml"></param>
        private void ProcessGml(SampleGml gml)
        {
            if (!IsTileInitialized) //初期化時は無視
                return;

            if (UseCoroutineForTiles)
            {
                if(coroutineQueue.TryAdd(gml.Tile.Address, () => sceneManager.SampleGmlAddedHandlerCoroutine(gml))) // 特に前回のものを削除しなくてもデータが一緒なので問題ないはず
                {
                    if(RunCoroutineOnEveryLoad)
                        StartCoroutineProcess();
                }   
            }
            else
            {
                sceneManager.SampleGmlAddedHandler(gml);
            }
        }

        /// <summary>
        /// キューにつまれたコルーチン実行処理スタート
        /// </summary>
        public void StartCoroutineProcess()
        {
            if (!IsTileInitialized) // 初回ロード完了前は変更がないので除外
                return;

            if (!isCoroutineRunning)
            {
                baseCoroutine = StartCoroutine(ProcessCoroutineQueue());
            }
            else
            {
                //Log($"<color=red>StartCoroutineProcess Failed (alredy running ) {coroutineQueue.Count}</color>");
            }
        }

        /// <summary>
        /// キューにつまれたコルーチン実行処理
        /// </summary>
        /// <returns></returns>
        IEnumerator ProcessCoroutineQueue()
        {
            Log($"<color=green>ProcessCoroutineQueue start {coroutineQueue.Count}</color>");
            isCoroutineRunning = true;
            while (coroutineQueue.Count > 0)
            {
                var coroutineKV = coroutineQueue.First();
                coroutineQueue.Remove(coroutineKV.Key);
                currentCoroutineTileAddress = coroutineKV.Key;
                currentCoroutine = StartCoroutine(coroutineKV.Value());
                yield return currentCoroutine;

                Log($"<color=green>ProcessCoroutineQueue running {coroutineQueue.Count}</color>");
            }
            currentCoroutineTileAddress = null;
            isCoroutineRunning = false;

            Log($"<color=green>ProcessCoroutineQueue End</color>");
        }

        /// <summary>
        /// タイルのコルーチンを削除
        /// </summary>
        /// <param name="addr"></param>
        void RemoveCoroutineByAddress(string addr)
        {
            //実行中なら停止
            if (currentCoroutineTileAddress == addr)
            {
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }
                currentCoroutineTileAddress = null;
            }

            coroutineQueue.Remove(addr);
        }

        /// <summary>
        /// コルーチンの実行を停止
        /// キューはそのまま
        /// </summary>
        public void StopCoroutineProcess()
        {
            if (baseCoroutine != null)
            {
                StopCoroutine(baseCoroutine);
                Log($"<color=yellow>StopCoroutineProcess {coroutineQueue.Count}</color>");
            }
            isCoroutineRunning = false;
        }

        /// <summary>
        /// コルーチンの実行を停止
        /// キューを全て削除
        /// </summary>
        public void ClearCoroutineProcess()
        {
            StopCoroutineProcess();
            coroutineQueue.Clear();
        }

        /// <summary>
        /// 実行中のコルーチンを廃棄して読込タイル全てについて処理を行う
        /// </summary>
        public void ProcessAllLoadedTiles()
        {
            if (UseCoroutineForTiles)
            {
                ClearCoroutineProcess();

                int coroutineCount = 0;
                foreach (var gml in GmlsList)
                {
                    if (gml.Tile.LoadedObject != null)
                    {
                        if(coroutineQueue.TryAdd(gml.Tile.Address, () => sceneManager.SampleGmlAddedHandlerCoroutine(gml)))
                            coroutineCount++;
                    }
                }
                if(coroutineCount > 0)
                    StartCoroutineProcess();
            }
            else
            {
                foreach (var gml in GmlsList)
                    sceneManager.SampleGmlAddedHandler(gml);
            }
        }

        public void ProccessInteraction(Action interactAction)
        {
            if (UseCoroutineForInteraction)
                ProcessAllLoadedTiles(); //読込タイル全て処理
            else
            {
                //ClearCoroutineProcess();
                interactAction.Invoke();
            }
        }

        public FloodingTitleSet FindAllFloodingTitlesOfBuildings()
        {
            return FindAllFloodingTitlesWhere(gml => !gml.IsFlooding);
        }

        public FloodingTitleSet FindAllFloodingTitlesOfFlds()
        {
            return FindAllFloodingTitlesWhere(gml => gml.IsFlooding);
        }

        private FloodingTitleSet FindAllFloodingTitlesWhere(Func<SampleGml, bool> gmlCondition)
        {
            var floodingTitles = new FloodingTitleSet();
            foreach (var gml in GmlsList)
            {
                if (!gmlCondition(gml)) continue;
                floodingTitles.UnionWith(gml.FloodingTitles);
            }
            return floodingTitles;
        }

        private SampleGml GetGml(string gmlName)
        {
            if (tileGmlCache.TryGetValue(gmlName, out var gml))
            {
                return gml;
            }

            return null;
        }

        /// <summary>
        /// クリック時のGameObjecct取得用
        /// </summary>
        public SemanticCityObject GetCityObject(string gmlName, string cityObjName)
        {
            var gml = GetGml(gmlName);
            return gml?.GetCityObject(cityObjName);
        }

        /// <summary>
        /// クリック時の属性表示用
        /// </summary>
        public SampleAttribute GetAttribute(string gmlFileName, string cityObjectId)
        {
            var gml = GetGml(gmlFileName);
            if (gml != null)
            {
                return gml.GetAttribute(cityObjectId);
            }

            Debug.LogWarning("gml not found.");
            return null;
        }

        public IEnumerable<SemanticCityObject> SemanticCityObjects()
        {
            foreach (var gml in GmlsList)
            {
                if (gml.Tile?.LoadedObject == null)
                    continue;

                foreach (var obj in gml.SemanticCityObjs())
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// CityGmlの色変更用
        /// </summary>
        public IEnumerable<SampleGml> Gmls()
        {
            return GmlsList.Where(x => x.Tile.LoadedObject != null);
        }

        private void Log(object message)
        {
            if(ShowDebugLogs) 
                Debug.Log(message);
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GISTileManager))]
    public class GISTileManagerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var manager = (GISTileManager)target;
            GUILayout.Label("Coroutine Running : " + manager.IsCoroutineRunning.ToString());
            GUILayout.Label("Coroutine Count: " + manager.NumCoroutines);
        }
    }
#endif
}

