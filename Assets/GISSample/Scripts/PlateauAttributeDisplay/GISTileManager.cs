using GISSample.PlateauAttributeDisplay.Gml;
using PLATEAU.DynamicTile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GISSample.PlateauAttributeDisplay
{
    public class GISTileManager : MonoBehaviour
    {
        public static readonly bool USE_COROUTINE_FOR_TILES = true; //タイル読込後の処理にコルーチン使用
        public static readonly bool RUN_COROUTINE_EVERY_LOAD = false; //タイル読込完了時に毎回コルーチン実行 / 全タイル読込完了時のみにコルーチン実行
        public static readonly bool USE_COROUTINE_FOR_OPERATIONS = false; //タイル読込後のフィルター・色変更等処理にコルーチン使用
        public static readonly bool USE_COROUTINE_FOR_INTERACTION = false; //ボタンクリック時のフィルター・色変更等処理にコルーチン使用     
        public static readonly bool ZOOMLEVEL11_ONLY = true; // Zoom Level 11 以外は無視
        public static readonly bool SHOW_DEBUG_LOGS = false;

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

        public bool IsTileLoading => tileManager?.IsCoroutineRunning == true || tileManager.HasCurrentTask == true;

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

            if(!sceneManager.IsInitialized)
                tileManager.DisableUpdateByCameraPosition(true); // sceneManager initialize完了まで待機
            tileManager.onTileInstantiatedAction += onTileInstanciated;
            tileManager.onTileUnloadBegin += onTileUnloaded;
            tileManager.onTileInstantiationComplete += onAllTileLoaded;
            sceneManager.OnInitialize += onSceneInitialized;
        }

        private void OnDestroy()
        {
            tileManager.onTileInstantiatedAction -= onTileInstanciated;
            tileManager.onTileUnloadBegin -= onTileUnloaded;
            sceneManager.OnInitialize -= onSceneInitialized;
        }

        private void onSceneInitialized()
        {
            tileManager?.UpdateAssetsByCameraPosition(Camera.main.transform.position);
            tileManager?.DisableUpdateByCameraPosition(false);
        }

        /// <summary>
        /// タイル読込が全て完了した際の処理
        /// </summary>
        private void onAllTileLoaded()
        {
            StartCoroutineProcess();
        }

        /// <summary>
        /// タイル読込時の処理
        /// </summary>
        /// <param name="tile"></param>
        private void onTileInstanciated(PLATEAUDynamicTile tile)
        {
            if (ZOOMLEVEL11_ONLY && tile.ZoomLevel < 11) // ZoomLevel 11のみ
                return;

            Log($"<color=yellow>Tile instantiated: {tile.Address}</color>");

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
            if (ZOOMLEVEL11_ONLY && tile.ZoomLevel < 11) // ZoomLevel 11のみ
                return;

            Log($"<color=red>Tile unload begin: {tile.Address}</color>");

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
            if (USE_COROUTINE_FOR_TILES)
            {
                if(coroutineQueue.TryAdd(gml.Tile.Address, () => sceneManager.SampleGmlAddedHandlerCoroutine(gml)))
                {
                    if(RUN_COROUTINE_EVERY_LOAD)
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
            if (IsTileLoading) // タイルロード完了を待ってスタート
            {
                Log($"<color=red>StartCoroutineProcess Failed (Tile Loading ) {coroutineQueue.Count}</color>");
                return;
            }

            if (!isCoroutineRunning)
            {
                baseCoroutine = StartCoroutine(ProcessCoroutineQueue());
            }
            else
            {
                Log($"<color=red>StartCoroutineProcess Failed (alredy running ) {coroutineQueue.Count}</color>");
            }
        }

        /// <summary>
        /// コルーチンの実行を停止
        /// キューはそのまま
        /// </summary>
        public void StopCoroutineProcess()
        {
            if(baseCoroutine != null){
                StopCoroutine(baseCoroutine);
                Log($"<color=yellow>StopCoroutineProcess {coroutineQueue.Count}</color>");
            }
            isCoroutineRunning = false;
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
        /// 実行中のコルーチンを廃棄して読込タイル全てについて処理を行う
        /// </summary>
        public void ProcessAllLoadedTiles()
        {
            if (USE_COROUTINE_FOR_TILES)
            {
                StopCoroutineProcess();
                coroutineQueue.Clear();

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

        public SemanticCityObject GetCityObject(string gmlName, string cityObjName)
        {
            var gml = GetGml(gmlName);
            return gml?.GetCityObject(cityObjName);
        }

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

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            return GmlsList.SelectMany(gml => gml.FeatureGameObjs());
        }

        public IEnumerable<SemanticCityObject> SemanticCityObjects()
        {
            foreach (var gml in GmlsList)
            {
                foreach (var obj in gml.SemanticCityObjs())
                {
                    yield return obj;
                }
            }
        }

        public IEnumerable<SampleGml> Gmls()
        {
            return GmlsList.Where(x => x.Tile.LoadedObject != null);
        }

        private void Log(object message)
        {
            if(SHOW_DEBUG_LOGS) 
                Debug.Log(message);
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GISTileManager))]
    public class GISTileManagerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            var manager = (GISTileManager)target;

            GUILayout.Label("Coroutine Running : " + manager.IsCoroutineRunning.ToString());
        }
    }
#endif
}

