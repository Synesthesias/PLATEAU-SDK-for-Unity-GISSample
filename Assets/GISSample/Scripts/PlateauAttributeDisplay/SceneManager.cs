using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using GISSample.PlateauAttributeDisplay.Gml;
using GISSample.PlateauAttributeDisplay.UI;
using GISSample.PlateauAttributeDisplay.UI.UIWindow;
using PLATEAU.CityInfo;
using PlateauToolkit.Sandbox;
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
        private PLATEAUInstancedCityModel[] instancedCityModels;

        private readonly GmlDictionary gmlDict = new();

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
        private ActionButtonsUi actionButtonsUi;
        private PlateauSandboxCameraManager plateauSandboxCameraManager;
        private WalkControllUI walkControlUI;


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
        }

        private void OnDisable()
        {
            inputActions.Disable();
            walkerMoveByUserInput?.OnDisable();
        }

        private void OnDestroy()
        {
            inputActions.Dispose();
        }


        private void Update()
        {
            gisCameraMove.Update();
            GisUiController.Update();
            walkerMoveByUserInput.Update(Time.deltaTime);

            if (plateauSandboxCameraManager != null && actionButtonsUi != null)
            {
                if (plateauSandboxCameraManager.CurrentCameraMode == PlateauSandboxCameraMode.None)
                {
                    actionButtonsUi.SetWalkerToggleEnabled(true);
                    actionButtonsUi.SetVehicleToggleEnabled(false);
                }
                else
                {
                    actionButtonsUi.SetWalkerToggleEnabled(false);
                    actionButtonsUi.SetVehicleToggleEnabled(true);
                    walkControlUI.CloseWindowBody();
                }
            }

            if (walkControlUI != null)
            {
                walkControlUI.WalkControllerHeightText = walkerMoveByUserInput.CameraOffsetY.ToString("F2");
            }
        }


        /// <summary>
        /// 初期化処理
        /// 属性情報から必要なデータをまとめます。
        /// </summary>
        /// <returns></returns>
        private void Initialize()
        {

            instancedCityModels = FindObjectsOfType<PLATEAUInstancedCityModel>();
            if (instancedCityModels == null || instancedCityModels.Length == 0)
            {
                return;
            }

            gmlDict.Init(instancedCityModels);

            cameraPositionMemory = new CameraPositionMemory(Camera.main);
            ColorChangerByAttribute = new ColorChangerByAttribute(this);
            FloatingTextList = new FloatingTextList();
            TextureSwitcher = new TextureSwitcher(gmlDict);

            GisUiController = GetComponentInChildren<GisUiController>();
            // どのような洪水情報があるか検索します
            var floodingAreaNamesBldg = gmlDict.FindAllFloodingTitlesOfBuildings();
            var floodingAreaNamesFld = gmlDict.FindAllFloodingTitlesOfFlds();
            GisUiController.Init(this, ColorChangerByAttribute, floodingAreaNamesBldg, floodingAreaNamesFld, cameraPositionMemory);
            ColorChangerByAttribute.ChangeToDefault();

            gisCameraMove = new GISCameraMove(GisUiController);
            inputActions.GISSample.SetCallbacks(gisCameraMove);

            filterByLodAndHeight = new FilterByLodAndHeight(GisUiController.MenuUi, gmlDict);
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
                actionButtonsUi.OnVehicleButtonClicked += () =>
                {
                    if (plateauSandboxCameraManager != null)
                    {
                        plateauSandboxCameraManager.SwitchCamera(PlateauSandboxCameraMode.None);
                    }
                };
            }

            SetupWalkControlUI();
            SetupWalkerCamera();
        }

        public SampleAttribute GetAttribute(string gmlFileName, string cityObjectID)
        {
            return gmlDict.GetAttribute(gmlFileName, cityObjectID);
        }

        public SemanticCityObject GetCityObject(string gmlName, string cityObjName)
        {
            return gmlDict.GetCityObject(gmlName, cityObjName);
        }

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            return gmlDict.FeatureGameObjs();
        }

        public IEnumerable<SampleGml> Gmls()
        {
            return gmlDict.Gmls();
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

                        walkControlUI.CloseWindowBody();
                    }
                };
            }
        }
    }
}
