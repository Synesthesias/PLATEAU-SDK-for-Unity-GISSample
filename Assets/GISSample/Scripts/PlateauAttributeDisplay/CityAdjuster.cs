using System.Collections;
using System.Collections.Generic;
using CesiumForUnity;
using GISSample.Misc;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using PlateauToolkit.Maps;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// PLATEAUでインポートした都市モデルをGIS Sampleで利用可能なように調整します。
    /// 利用方法：
    /// 任意のゲームオブジェクトにこのコンポーネントをアタッチし、インスペクタでtargetを指定してExecボタンを押します。
    /// 1つの都市モデルに対して1度だけ実行してください。複数回の実行は想定していません。
    /// </summary>
    public class CityAdjuster : MonoBehaviour
    {
        #if UNITY_EDITOR
        [SerializeField] private PLATEAUInstancedCityModel target;

        public IEnumerator Exec()
        {
            if (target == null)
            {
                Debug.LogError("target is null.");
            }

            // 都市モデルをCesiumGeoReferenceの子にします
            var cesiumGeoReference = FindObjectOfType<CesiumGeoreference>();
            target.transform.parent = cesiumGeoReference.transform;
            target.gameObject.AddComponent<CesiumGlobeAnchor>();
        
            // Cesiumとの位置合わせをします。
            // 内容はPlateauToolkitMapsWindowとほぼ同じです。
            var geoRef = target.GeoReference;
            GeoCoordinate geoCoord = geoRef.Unproject(new PlateauVector3d(0, 0, 0));
            var cityModelPosition = new double2 { x = geoCoord.Latitude, y = geoCoord.Longitude };
            string geoidRequestUri = "http://vldb.gsi.go.jp/sokuchi/surveycalc/geoid/calcgh/cgi/geoidcalc.pl?outputType=json" + "&latitude=" + cityModelPosition.x +
                                     "&longitude=" + cityModelPosition.y;
            yield return StartCoroutine(RequestGeoidHeightToUri(geoidRequestUri, 
                (height) => MoveCityModel(cityModelPosition, height, target)));
        
        
            DeleteDuplicateGmls(target);
        
            // AutoTextureRunnerを実行します。
            AutoTextureRunner.Run(target.gameObject);
            
            
            // 洪水情報はstaticをoffにします（高さを変えるため）
            foreach (Transform gmlTrans in target.transform)
            {
                if (gmlTrans.name.Contains("_fld_"))
                {
                    foreach (var r in gmlTrans.GetComponentsInChildren<Renderer>())
                    {
                        r.gameObject.isStatic = false;
                    }
                }
            }
        
        }

        /// <summary>
        /// 同名のGML相当のゲームオブジェクトがある場合、1つを残して削除します。
        /// </summary>
        private void DeleteDuplicateGmls(PLATEAUInstancedCityModel cityModel)
        {
            var gmlNameSet = new HashSet<string>();
            var cityTrans = cityModel.transform;
            var gmlsToDestroy = new List<Transform>(); 
            for (int i = 0; i < cityTrans.childCount; i++)
            {
                var gmlTrans = cityTrans.GetChild(i);
                var gmlName = gmlTrans.name;
                if (gmlNameSet.Contains(gmlName))
                {
                    gmlsToDestroy.Add(gmlTrans);
                }
                else
                {
                    gmlNameSet.Add(gmlName);
                }
            
            }

            foreach (var gml in gmlsToDestroy)
            {
                DestroyImmediate(gml.gameObject);
            }
        }
    
        IEnumerator RequestGeoidHeightToUri(string uri, PlateauMapsHeightClient.GeoidDataCallback callback)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(uri);
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                RootObject rootObject = JsonUtility.FromJson<RootObject>(webRequest.downloadHandler.text);
                callback.Invoke(rootObject.OutputData.geoidHeight);
            }
            else
            {
                Debug.Log(": Error: " + webRequest.error);
                callback.Invoke(0f);
            }
        }

    
        // ここの内容はPlateauToolkitMapsWindowとほぼ同じです
        public void MoveCityModel(double2 cityModelPosition, float resultOfGeoidHeightQuery, PLATEAUInstancedCityModel targetCityModel)
        {
            double3 longLatHeight = new double3 { x = cityModelPosition.y, y = cityModelPosition.x, z = resultOfGeoidHeightQuery };
            var targetTrans = targetCityModel.transform;
            targetTrans.parent.GetComponent<CesiumGeoreference>().SetOriginLongitudeLatitudeHeight(longLatHeight[0], longLatHeight[1], longLatHeight[2]);
            targetTrans.GetComponent<CesiumGlobeAnchor>().longitudeLatitudeHeight = longLatHeight;
            targetTrans.rotation = Quaternion.identity;

            if (resultOfGeoidHeightQuery == 0f)
            {
                Debug.LogError("高さ合わせが失敗しました\nインターネットに接続されていると確認した上で再度お試しください。高さはゼロに設定されました。");
            }
        }
        #endif
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(CityAdjuster))]
    public class CityAdjusterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var cityAdjuster = (CityAdjuster)target;
            if (GUILayout.Button("Exec"))
            {
                cityAdjuster.StartCoroutine(cityAdjuster.Exec());
            }
            base.OnInspectorGUI();
        }
    }
#endif
}