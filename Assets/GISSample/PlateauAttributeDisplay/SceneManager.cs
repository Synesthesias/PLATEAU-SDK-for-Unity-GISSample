using System.Collections.Generic;
using GISSample.PlateauAttributeDisplay.Gml;
using GISSample.PlateauAttributeDisplay.UI;
using PLATEAU.CityInfo;
using PLATEAU.Samples;
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
        private GisUiController gisUiController;

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
        private GISCameraMove gisCameraMove;



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
            var floodingAreaNames = gmlDict.FindAllAreaNames();
            
            gisUiController = GetComponentInChildren<GisUiController>();
            gisUiController.Init(this, floodingAreaNames);
            gisCameraMove = new GISCameraMove(gisUiController);
            inputActions.GISSample.SetCallbacks(gisCameraMove);
            
            filterByLodAndHeight = new FilterByLodAndHeight(gisUiController.MenuUi, gmlDict);
            weatherController = new WeatherController(gisUiController.MenuUi);
        }

        public SampleAttribute GetAttribute(string gmlFileName, string cityObjectID)
        {
            return gmlDict.GetAttribute(gmlFileName, cityObjectID);
        }

        public void ColorCity(ColorCodeType type, string areaName)
        {
            gmlDict.ColorCity(type, areaName, gisUiController.heightColorTable, gisUiController.floodingRankColorTable);
        }

        public SemanticCityObject GetCityObject(string gmlName, string cityObjName)
        {
            return gmlDict.GetCityObject(gmlName, cityObjName);
        }

        public IEnumerator<FeatureGameObj> FeatureGameObjs()
        {
            return gmlDict.FeatureGameObjs();
        }

    }
}
