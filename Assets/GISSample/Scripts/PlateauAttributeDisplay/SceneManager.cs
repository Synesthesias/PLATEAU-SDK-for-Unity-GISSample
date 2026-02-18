using AWSIM.TrafficSimulation;
using Cinemachine;
using GISSample.PlateauAttributeDisplay.Gml;
using GISSample.PlateauAttributeDisplay.UI;
using GISSample.PlateauAttributeDisplay.UI.UIWindow;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PlateauToolkit.Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// シーンマネージャ
    /// GIS Sampleの主要機能を提供します。
    /// カメラ、入力、UIの制御を行います。
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        public GisUiController GisUiController { get; private set; }

        /// <summary>
        /// InputActions
        /// Assets/GISSample/GISSampleInputActionsから生成されたクラスです。
        /// </summary>
        private GISSampleInputActions inputActions;

        /// <summary>
        /// シーン中のPLATEAUInstancedCityModel
        /// 複数の都市データのインポートに対応するため、配列にしています。
        /// </summary>
        [SerializeField]
        private PLATEAUInstancedCityModel[] instancedCityModels;

        private readonly GmlDictionary gmlDict = new();

        [SerializeField]
        private GISTileManager gislTileManager;
        public GISTileManager GisTileManager => gislTileManager;

        [SerializeField]
        private TrafficManager trafficManager;

        private FilterByLodAndHeight filterByLodAndHeight;
        private WeatherController weatherController;
        public ColorChangerByAttribute ColorChangerByAttribute { get; private set; }
        private GISCameraMove gisCameraMove;
        public FloatingTextList FloatingTextList { get; private set; }
        private CameraPositionMemory cameraPositionMemory;
        public TextureSwitcher TextureSwitcher { get; private set; }
        private WalkerMoveByUserInput walkerMoveByUserInput;
        private Vector3 lastMainCameraPosition;
        private Quaternion lastMainCameraRotation;
        private float lastMainCameraNearClipPlane;
        private float lastMainCameraFarClipPlane;
        private ActionButtonsUi actionButtonsUi;
        private PlateauSandboxCameraManager plateauSandboxCameraManager;
        private WalkControllUI walkControlUI;

        public bool IsInitialized { get; private set; } = false;

        public bool IsMouseDragging => gisCameraMove?.IsMouseDragging ?? false;
        public bool IsKeyPressed => gisCameraMove?.IsKeyPressed ?? false;

        public bool IsTileLoading => gislTileManager?.IsTileLoading ?? false; // タイル読込処理中
        public bool IsTileCoroutineRunning => gislTileManager?.IsCoroutineRunning ?? false; // タイル読込後の色変更等のコルーチン

        public bool IsTileInitialized => gislTileManager?.IsTileInitialized ?? false; // 初回タイルロード完了

        /// <summary>
        /// カメラ位置（一応SceneManagerで管理）
        /// </summary>
        public Vector3 CameraPosition {
            get {
                var mainCam = Camera.main;
                return mainCam?.transform?.position ?? Vector3.zero;
            }
        }

        public Action OnInitialize = null;

        private void Awake()
        {
            inputActions = new GISSampleInputActions();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            inputActions.Enable();
            walkerMoveByUserInput?.OnEnable();

            if(gisCameraMove != null){
                gisCameraMove.OnMouseDrag += OnInteractionHandler;
                gisCameraMove.OnKeyPress += OnInteractionHandler;
            }
        }

        private void OnDisable()
        {
            inputActions.Disable();
            walkerMoveByUserInput?.OnDisable();
            if (gisCameraMove != null)
            {
                gisCameraMove.OnMouseDrag -= OnInteractionHandler;
                gisCameraMove.OnKeyPress -= OnInteractionHandler;
            }
        }

        private void OnDestroy()
        {
            inputActions.Dispose();
        }


        private void Update()
        {
            if (trafficManager != null)
            {
                // マウス・キーでの操作時はTrafficManagerを非活性化
                if (IsMouseDragging || IsKeyPressed || IsTileLoading || !IsInitialized || !IsTileInitialized)
                {
                    trafficManager.gameObject.SetActive(false);
                }
                else
                {
                    trafficManager.gameObject.SetActive(true);
                }
            }

            if (!IsInitialized)
                return;

            gisCameraMove.Update();

            //if(!IsTileLoading)
            {
                GisUiController.Update();
                walkerMoveByUserInput.Update(Time.deltaTime);

                if (plateauSandboxCameraManager != null && actionButtonsUi != null)
                {
                    if (plateauSandboxCameraManager.CurrentCameraMode == PlateauSandboxCameraMode.None)
                    {
                        if (actionButtonsUi.IsWalkerActive)
                        {
                            // 歩行時
                            actionButtonsUi.SetVehicleToggleEnabled(false);
                            return;
                        }

                        // 俯瞰視点
                        if (actionButtonsUi.IsVehicleActive)
                        {
                            actionButtonsUi.SetVehicleToggleOff();
                        }
                        actionButtonsUi.SetWalkerToggleEnabled(true);
                        actionButtonsUi.SetVehicleToggleEnabled(true);
                    }
                    else
                    {
                        // 車両視点時
                        if (!actionButtonsUi.IsVehicleActive)
                        {
                            // 車両を直接指定時。車両ボタンをアクティブにする
                            actionButtonsUi.SetVehicleToggleOn();
                        }

                        actionButtonsUi.SetWalkerToggleEnabled(false);
                        walkControlUI.CloseWindowBody();
                    }
                }

                if (walkControlUI != null)
                {
                    walkControlUI.WalkControllerHeightText = walkerMoveByUserInput.CameraOffsetY.ToString("F2");
                }
            }

            //FloatingTextList.SetActive(!IsTileLoading);
            ShowLoadingUI(IsTileLoading || !IsTileInitialized);//タイルロード中は一部UI非表示
        }

        /// <summary>
        /// 初期化処理
        /// 属性情報から必要なデータをまとめます。
        /// </summary>
        /// <returns></returns>
        private void Initialize()
        {
            if (gislTileManager == null)
                gislTileManager = FindFirstObjectByType<GISTileManager>();

            if (trafficManager == null)
                trafficManager = FindFirstObjectByType<TrafficManager>();

            if (instancedCityModels == null || instancedCityModels.Length <= 0)
            {
                var instancedCityModelsList = FindObjectsByType<PLATEAUInstancedCityModel>(FindObjectsSortMode.None).ToList();
                instancedCityModelsList.RemoveAll(x => x.GetComponentInParent<PLATEAUTileManager>() != null);
                instancedCityModels = instancedCityModelsList.ToArray();

                //Debug.LogWarning($"SceneManager Initialize: instancedCityModels Count={instancedCityModelsList.Count}");
            }

            gmlDict.Init(instancedCityModels);

            cameraPositionMemory = new CameraPositionMemory(Camera.main);
            ColorChangerByAttribute = new ColorChangerByAttribute(this);
            FloatingTextList = new FloatingTextList();
            TextureSwitcher = new TextureSwitcher(gmlDict, gislTileManager);

            GisUiController = GetComponentInChildren<GisUiController>();
            // どのような洪水情報があるか検索します
            var floodingAreaNamesBldg = gmlDict.FindAllFloodingTitlesOfBuildings();
            var floodingAreaNamesFld = gmlDict.FindAllFloodingTitlesOfFlds();

            var floodingAreaNamesBldgTiles = gislTileManager?.FindAllFloodingTitlesOfBuildings();
            if (floodingAreaNamesBldgTiles?.Count > 0)
                floodingAreaNamesBldg?.UnionWith(floodingAreaNamesBldgTiles);

            GisUiController.Init(this, ColorChangerByAttribute, floodingAreaNamesBldg, floodingAreaNamesFld, cameraPositionMemory);
            ColorChangerByAttribute.ChangeToDefault();

            gisCameraMove = new GISCameraMove(GisUiController);
            inputActions.GISSample.SetCallbacks(gisCameraMove);

            gisCameraMove.OnMouseDrag += OnInteractionHandler;
            gisCameraMove.OnKeyPress += OnInteractionHandler;

            filterByLodAndHeight = new FilterByLodAndHeight(GisUiController.MenuUi, gmlDict, gislTileManager);
            weatherController = new WeatherController(GisUiController.MenuUi);


            GameObject mainCamera = Camera.main.gameObject;
            // MainCameraにCinemachineBrainがアタッチされていない場合は追加
            if (mainCamera.GetComponent<CinemachineBrain>() == null)
            {
                var brain = mainCamera.AddComponent<CinemachineBrain>();
                brain.enabled = false;
            }

            actionButtonsUi = FindObjectOfType<ActionButtonsUi>();
            plateauSandboxCameraManager = FindObjectOfType<PlateauSandboxCameraManager>();
            if (actionButtonsUi != null)
            {
                actionButtonsUi.OnVehicleToggle += (isActive) =>
                {
                    if (isActive)
                    {
                        // ランダムで車を選んで１人称視点に
                        var vehicle = GetRandomVehicle();
                        if (vehicle != null)
                        {
                            var trafficColliders = vehicle.GetComponentsInChildren<Collider>(true);
                            if (trafficColliders?.Length > 0)
                            {
                                plateauSandboxCameraManager.SetCameraTarget(trafficColliders[0]);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("車両が見つかりませんでした");
                        }
                    }
                    else
                    {
                        plateauSandboxCameraManager.SwitchCamera(PlateauSandboxCameraMode.None);
                    }
                };
            }

            SetupWalkControlUI();
            SetupWalkerCamera();

            IsInitialized = true;
            OnInitialize?.Invoke();
        }

        /// <summary>
        /// マウス。キーボード操作開始、終了時のイベントハンドラ
        /// </summary>
        /// <param name="started"></param>
        private void OnInteractionHandler(bool started)
        {
            gislTileManager?.OnInteractionHandler(started);
        }

        /// <summary>
        /// ロード中のメニュー有効：無効
        /// </summary>
        /// <param name="enabled"></param>
        public void ShowLoadingUI(bool isLoading)
        {
            GisUiController.ShowLoading(isLoading);
            actionButtonsUi.SetWalkerToggleEnabled(!isLoading);
            actionButtonsUi.SetVehicleToggleEnabled(!isLoading);
        }

        // TileManagerからSampleGmlが追加されたときに呼ばれるハンドラ
        public void SampleGmlAddedHandler(SampleGml gml)
        {
            CoroutineUtil.RunToEnd(SampleGmlAddedHandlerCoroutine(gml));
        }

        // TileManagerからSampleGmlが追加されたときに呼ばれるハンドラのコルーチン実行
        public IEnumerator SampleGmlAddedHandlerCoroutine(SampleGml gml)
        {

            if (gml.Tile.LoadedObject == null)
                yield break;

            if (!gml.IsCached) //初回読込時のみ処理
            {
                var floodingTitles = new FloodingTitleSet();
                if (!gml.IsFlooding)
                    floodingTitles.UnionWith(gml.FloodingTitles);
                GisUiController.MenuUi.ColorByAttrUi.AppendFloodingTitlesBuilding(floodingTitles);
            }

            if (GisTileManager.UseCoroutineForOperations)
                yield return TextureSwitcher.SetCurrentTextureCoroutine(gml);
            else
                TextureSwitcher.SetCurrentTexture(gml);

            yield return null;

            if (ColorChangerByAttribute.BuildingColorType != BuildingColorType.None)
            {
                if (GisTileManager.UseCoroutineForOperations)
                    yield return ColorChangerByAttribute.RedrawBuildingsCoroutine(new List<SampleGml>() { gml });
                else
                    ColorChangerByAttribute.RedrawBuildings(new List<SampleGml>() { gml });
            }

            yield return null;

            if (!filterByLodAndHeight.IsDefaultFilterParameter)
            {
                if (GisTileManager.UseCoroutineForOperations)
                    yield return filterByLodAndHeight.FilterCoroutine(gml);
                else
                    filterByLodAndHeight.Filter(gml);
            }

            yield return null;
        }

        /// <summary>
        /// クリック時の属性表示用
        /// scene/tile両方取得
        /// </summary>
        /// <param name="gmlName"></param>
        /// <param name="cityObjName"></param>
        /// <returns></returns>
        public SampleAttribute GetAttribute(string gmlName, string cityObjName)
        {
            var result = gislTileManager?.GetAttribute(gmlName, cityObjName);
            if (result != null)
                return result;

            return gmlDict.GetAttribute(gmlName, cityObjName);
        }

        /// <summary>
        /// クリック時のGameObjecct取得用
        /// scene/tile両方取得
        /// </summary>
        /// <param name="gmlName"></param>
        /// <param name="cityObjName"></param>
        /// <returns></returns>
        public SemanticCityObject GetCityObject(string gmlName, string cityObjName)
        {
            var result = gislTileManager?.GetCityObject(gmlName, cityObjName);
            if (result != null)
                return result;

            if(gmlName == null)
                return null;
            return gmlDict.GetCityObject(gmlName, cityObjName);
        }

        /// <summary>
        /// CityGmlの色変更用
        /// Tileの処理は除外
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SampleGml> Gmls()
        {
            var gmls = gmlDict.Gmls();
            return gmls;
        }

        private void SetupWalkControlUI()
        {
            walkControlUI = FindObjectOfType<WalkControllUI>();
            if (walkControlUI == null)
            {
                return;
            }

            walkControlUI.OnWalkControlPressing += (controlName) =>
            {
                if (controlName == "W")
                {
                    walkerMoveByUserInput.DeltaWASD = new Vector2(0, 1);
                }
                else if (controlName == "A")
                {
                    walkerMoveByUserInput.DeltaWASD = new Vector2(-1, 0);
                }
                else if (controlName == "S")
                {
                    walkerMoveByUserInput.DeltaWASD = new Vector2(0, -1);
                }
                else if (controlName == "D")
                {
                    walkerMoveByUserInput.DeltaWASD = new Vector2(1, 0);
                }
                else if (controlName == "Q")
                {
                    walkerMoveByUserInput.DeltaUpDown = -1;
                }
                else if (controlName == "E")
                {
                    walkerMoveByUserInput.DeltaUpDown = 1;
                }
            };

            walkControlUI.OnWalkControlUp += () =>
            {
                walkerMoveByUserInput.DeltaWASD = Vector2.zero;
                walkerMoveByUserInput.DeltaUpDown = 0;
            };

            walkControlUI.OnWalkSpeedChanged += (speed) =>
            {
                walkerMoveByUserInput.WalkerMoveSpeedMultiplier = speed;
            };

            walkControlUI.OnWalkModeQuitClicked += () =>
            {
                var brain = Camera.main.GetComponent<CinemachineBrain>();
                WalkerMoveByUserInput.IsActive = false;
                if (brain != null)
                {
                    brain.enabled = false;
                }

                Camera.main.transform.SetPositionAndRotation(lastMainCameraPosition, lastMainCameraRotation);
                Camera.main.nearClipPlane = lastMainCameraNearClipPlane;
                Camera.main.farClipPlane = lastMainCameraFarClipPlane;
                walkControlUI.CloseWindowBody();
                actionButtonsUi.SetWalkerToggleOff();
            };
        }

        private void SetupWalkerCamera()
        {
            //歩行者視点用のオブジェクトの生成と設定
            GameObject walker = new("Walker");
            CharacterController characterController = walker.AddComponent<CharacterController>();
            characterController.slopeLimit = 90;
            characterController.stepOffset = 0.3f;
            characterController.skinWidth = 0.05f;

            // Respawnタグのオブジェクトを探して、その位置に歩行者を配置
            GameObject respawn = GameObject.FindGameObjectWithTag("Respawn");
            if (respawn != null)
            {
                walker.transform.position = respawn.transform.position;
            }

            //歩行者視点用のカメラの生成と設定
            GameObject walkerCam = new("WalkerCamera");
            walkerCam.transform.rotation = Quaternion.Euler(30, 0, 0);
            CinemachineVirtualCamera walkerCamVC = walkerCam.AddComponent<CinemachineVirtualCamera>();
            walkerCamVC.m_Lens.FieldOfView = 60;
            walkerCamVC.m_Lens.NearClipPlane = 0.3f;
            walkerCamVC.m_Lens.FarClipPlane = 1000;
            walkerCamVC.Priority = 9;
            walkerCamVC.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Never;
            var trans = walkerCamVC.AddCinemachineComponent<CinemachineTransposer>();
            trans.m_FollowOffset = Vector3.zero;
            walkerCamVC.AddCinemachineComponent<CinemachinePOV>();
            CustomCinemachineInputProvider walkerCamInput = walkerCam.AddComponent<CustomCinemachineInputProvider>();
            walkerCamInput.XYAxis = InputActionReference.Create(new DefaultInputActions().Player.Look);
            walkerCam.SetActive(false);
            walkerCam.SetActive(true);
            walkerCamVC.Follow = walker.transform;

            walkerMoveByUserInput = new WalkerMoveByUserInput(walkerCamVC, walker);

            walkerMoveByUserInput.OnEnable();
            walkerMoveByUserInput.Start();

            if (actionButtonsUi != null)
            {
                actionButtonsUi.OnWalkerToggle += (isOn) =>
                {
                    var brain = Camera.main.GetComponent<CinemachineBrain>();
                    if (isOn)
                    {
                        lastMainCameraPosition = Camera.main.transform.position;
                        lastMainCameraRotation = Camera.main.transform.rotation;
                        lastMainCameraFarClipPlane = Camera.main.farClipPlane;
                        lastMainCameraNearClipPlane = Camera.main.nearClipPlane;

                        // Respawnタグのオブジェクトを探して、その位置に歩行者を配置
                        GameObject respawn = GameObject.FindGameObjectWithTag("Respawn");
                        if (respawn != null)
                        {
                            var cc = walker.GetComponent<CharacterController>();
                            cc.enabled = false;
                            walker.transform.position = respawn.transform.position;
                            cc.enabled = true;

                            Camera.main.transform.rotation = respawn.transform.rotation;
                            var pov = walkerCamVC.GetCinemachineComponent<CinemachinePOV>();
                            var euler = respawn.transform.rotation.eulerAngles;
                            float normalizedYaw = euler.y > 180 ? euler.y - 360 : euler.y;
                            float normalizedPitch = euler.x > 180 ? euler.x - 360 : euler.x;
                            pov.m_HorizontalAxis.Value = normalizedYaw;
                            pov.m_VerticalAxis.Value = normalizedPitch;
                        }

                        WalkerMoveByUserInput.IsActive = true;
                        if (brain != null)
                        {
                            brain.enabled = true;
                        }

                        walkControlUI.OpenWindowBody();
                    }
                    else
                    {
                        WalkerMoveByUserInput.IsActive = false;
                        if (brain != null)
                        {
                            brain.enabled = false;
                        }

                        Camera.main.transform.SetPositionAndRotation(lastMainCameraPosition, lastMainCameraRotation);
                        Camera.main.nearClipPlane = lastMainCameraNearClipPlane;
                        Camera.main.farClipPlane = lastMainCameraFarClipPlane;
                        walkControlUI.CloseWindowBody();
                    }
                };
            }
        }
        
        /// <summary>
        /// ランダムで取得した車両のオブジェクト
        /// </summary>
        /// <returns></returns>
        private GameObject GetRandomVehicle()
        {
            var traffics = GameObject.FindObjectsOfType<PlateauSandboxTrafficMovement>();
            if (traffics == null || traffics.Length == 0)
            {
                return null;
            }
            
            // 1台ランダムで取得
            var traffic = traffics[UnityEngine.Random.Range(0, traffics.Length)];
            if (traffic == null)
            {
                return null;
            }

            return traffic.gameObject;
        }
    }

    //[Serializable]
    //public class ControllerParameters
    //{
    //    public float WalkSpeed = 5f;
    //    public float RunSpeed = 10f;
    //    public float SprintSpeed = 15f;
    //    public float Acceleration = 10f;
    //    public float Deceleration = 10f;
    //    public float MouseSensitivityX = 1f;
    //    public float MouseSensitivityY = 1f;
    //    public float MinPitch = -89f;
    //    public float MaxPitch = 89f;
    //    public float CameraOffsetY = 1.6f;
    //}
}
