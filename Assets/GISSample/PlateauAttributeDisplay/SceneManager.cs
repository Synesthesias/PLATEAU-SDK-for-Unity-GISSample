using System.Collections.Generic;
using System.Linq;
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
        public GisUiController gisUiController;

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

        public readonly GmlDictionary gmls = new GmlDictionary();

        public readonly List<string> floodingAreaNames = new List<string>(); 
        

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
            gisCameraMove = new GISCameraMove(this);
            instancedCityModels = FindObjectsOfType<PLATEAUInstancedCityModel>();
            if (instancedCityModels == null || instancedCityModels.Length == 0)
            {
                return;
            }

            
            gmls.Init(instancedCityModels, this);
            

            inputActions.GISSample.SetCallbacks(gisCameraMove);

            
            gisUiController = GetComponentInChildren<GisUiController>();
            gisUiController.Init(this);
            filterByLodAndHeight = new FilterByLodAndHeight(gisUiController.MenuUi, gmls);
            weatherController = new WeatherController(gisUiController.MenuUi);
        }
        
        

    }
}
