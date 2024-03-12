using System.Collections.Generic;
using GISSample.PlateauAttributeDisplay.Gml;
using GISSample.PlateauAttributeDisplay.UI;
using GISSample.PlateauAttributeDisplay.UI.UIWindow;
using PLATEAU.CityInfo;
using UnityEngine;

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

        private readonly GmlDictionary gmlDict = new ();

        private FilterByLodAndHeight filterByLodAndHeight;
        private WeatherController weatherController;
        public ColorChangerByAttribute ColorChangerByAttribute { get; private set; }
        private GISCameraMove gisCameraMove;
        public FloatingTextList FloatingTextList { get; private set; }
        public CameraPositionMemory CameraPositionMemory { get; private set; }



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
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void OnDestroy()
        {
            inputActions.Dispose();
        }


        private void Update()
        {
            gisCameraMove.Update();
            GisUiController.Update();
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
            
            CameraPositionMemory = new CameraPositionMemory(Camera.main);
            ColorChangerByAttribute = new ColorChangerByAttribute(this);
            FloatingTextList = new FloatingTextList();
            
            GisUiController = GetComponentInChildren<GisUiController>();
            // どのような洪水情報があるか検索します
            var floodingAreaNamesBldg = gmlDict.FindAllFloodingTitlesOfBuildings();
            var floodingAreaNamesFld = gmlDict.FindAllFloodingTitlesOfFlds();
            GisUiController.Init(this, ColorChangerByAttribute, floodingAreaNamesBldg, floodingAreaNamesFld);
            ColorChangerByAttribute.ChangeToDefault();
            
            gisCameraMove = new GISCameraMove(GisUiController);
            inputActions.GISSample.SetCallbacks(gisCameraMove);
            
            filterByLodAndHeight = new FilterByLodAndHeight(GisUiController.MenuUi, gmlDict);
            weatherController = new WeatherController(GisUiController.MenuUi);
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

    }
}
