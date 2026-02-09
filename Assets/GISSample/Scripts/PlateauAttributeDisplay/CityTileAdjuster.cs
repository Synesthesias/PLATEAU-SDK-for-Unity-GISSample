#if UNITY_EDITOR

using AWSIM.TrafficSimulation;
using CesiumForUnity;
using PLATEAU.CityInfo;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.DynamicTile;
using PLATEAU.Editor.Window.Common.Tile;
using PLATEAU.GranularityConvert;
using PLATEAU.Native;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using PlateauToolkit.Editor;
using PlateauToolkit.Maps;
using PlateauToolkit.Rendering;
using PlateauToolkit.Rendering.Editor;
using PlateauToolkit.Sandbox.Editor;
using PlateauToolkit.Sandbox.RoadNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
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
    public class CityTileAdjuster : MonoBehaviour
    {
        #if UNITY_EDITOR
        [SerializeField] private PLATEAUInstancedCityModel fldTarget;
        [SerializeField] internal PLATEAUTileManager tileManager;

        private bool isAutoTextureExecuting = false;

        /// <summary>
        /// PLATEAUTileManagerの位置合わせを行います。
        /// </summary>
        /// <returns></returns>
        public IEnumerator AdjustTile()
        {
            if (tileManager == null)
                Debug.LogError("target is null.");

            // 都市モデルをCesiumGeoReferenceの子にします
            var cesiumGeoReference = FindFirstObjectByType<CesiumGeoreference>();
            if (cesiumGeoReference != null)
            {
                tileManager.transform.parent = cesiumGeoReference.transform;
                tileManager.gameObject.AddComponent<CesiumGlobeAnchor>();
            }

            // Cesiumとの位置合わせをします。
            // 内容はPlateauToolkitMapsWindowとほぼ同じです。
            var geoRef = tileManager.CityModel.GeoReference;
            GeoCoordinate geoCoord = geoRef.Unproject(new PlateauVector3d(0, 0, 0));
            var cityModelPosition = new double2 { x = geoCoord.Latitude, y = geoCoord.Longitude };
            string geoidRequestUri = "https://vldb.gsi.go.jp/sokuchi/surveycalc/geoid/calcgh/cgi/geoidcalc.pl?outputType=json" + "&latitude=" + cityModelPosition.x +
                                     "&longitude=" + cityModelPosition.y;

            yield return StartCoroutine(RequestGeoidHeightToUri(geoidRequestUri,
                 (height) => MoveCityModel(cityModelPosition, height, tileManager.CityModel)));

        }

        /// <summary>
        /// PLATEAUInstancedCityModelの位置合わせを行います。
        /// </summary>
        /// <returns></returns>
        public IEnumerator AdjustCityGml()
        {
            if (fldTarget != null)
            {
                var cesiumGeoReference = FindFirstObjectByType<CesiumGeoreference>();
                if (cesiumGeoReference != null)
                {
                    fldTarget.transform.parent = cesiumGeoReference.transform;
                    fldTarget.gameObject.AddComponent<CesiumGlobeAnchor>();
                }

                // Cesiumとの位置合わせをします。
                // 内容はPlateauToolkitMapsWindowとほぼ同じです。
                var geoRef = fldTarget.GeoReference;
                GeoCoordinate geoCoord = geoRef.Unproject(new PlateauVector3d(0, 0, 0));
                var cityModelPosition = new double2 { x = geoCoord.Latitude, y = geoCoord.Longitude };
                string geoidRequestUri = "https://vldb.gsi.go.jp/sokuchi/surveycalc/geoid/calcgh/cgi/geoidcalc.pl?outputType=json" + "&latitude=" + cityModelPosition.x +
                                         "&longitude=" + cityModelPosition.y;

                yield return StartCoroutine(RequestGeoidHeightToUri(geoidRequestUri,
                     (height) => MoveCityModel(cityModelPosition, height, fldTarget)));

                // 洪水情報はstaticをoffにします（高さを変えるため）
                foreach (Transform gmlTrans in fldTarget.transform)
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
        }

        /// <summary>
        /// PLATEAUTileManagerにToolkitのAuto Texturingを適用します。
        /// </summary>
        /// <param name="zoomLevels"></param>
        /// <param name="convertToPrimary"></param>
        public void RunAutoTextureTile(ObservableCollection<TileSelectionItem> selection , bool convertToPrimary)
        {
            if (isAutoTextureExecuting)
            {
                Debug.LogWarning("すでに実行中です。");
                return;
            }

            if (tileManager == null)
            {
                Debug.LogError("target is null.");
                return;
            }

            AutoTextureAsync(tileManager, selection, convertToPrimary).ContinueWithErrorCatch();
        }

        // AutoTextureHandler用パラメータ
        class AutoTextureParams
        {
            public ObservableCollection<TileSelectionItem> SelectedItems;
            public bool ConvertToPrimary;
        }

        private async Task AutoTextureAsync(PLATEAUTileManager tileManager, ObservableCollection<TileSelectionItem> selection, bool convertToPrimary)
        {
            isAutoTextureExecuting = true;
            PLATEAUSceneViewCameraTracker.Release();
            using (var cts = new CancellationTokenSource())
            {
                var selectedBuildingTileAddresses = selection.Select(t => t.TileAddress).ToList();  //アドレスリスト
                //var selectedBuildingTileAddresses = tileManager.DynamicTiles.Where(t => t.Package == PLATEAU.Dataset.PredefinedCityModelPackage.Building && zoomLevels.Contains(t.ZoomLevel)).Select(t => t.Address).ToList();  // 建物地物に絞り込み
                var loadedTiles = await tileManager.ForceLoadTiles(selectedBuildingTileAddresses, cts.Token);

                //List<TileSelectionItem> selectedBuildingTilesList = selectedBuildingTileAddresses.Select(addr => new TileSelectionItem(addr)).ToList();
                //var selectedBuildingTiles = new ObservableCollection<TileSelectionItem>(selectedBuildingTilesList);
                var selectedBuildingTiles = selection;

                Debug.Log($"AutoTextureAsync: selectedBuildingTiles.Count={selectedBuildingTiles.Count}");

                var tileRebuilder = new TileRebuilder();
                try
                {
                    CancellationToken ct = cts.Token;
                    AutoTextureParams autTexParam = new AutoTextureParams
                    {
                        SelectedItems = selectedBuildingTiles,
                        ConvertToPrimary = convertToPrimary
                    };

                    await TileConvertCommon.EditAndSaveSelectedTilesAsync<AutoTextureParams>(selectedBuildingTiles, tileManager, tileRebuilder, ApplyAutoTextureHandler, autTexParam, ct);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    tileRebuilder?.CancelRebuild();
                }
                finally
                {
                    PLATEAUSceneViewCameraTracker.Initialize();
                    isAutoTextureExecuting = false;
                }
            }
        }

        async Task ApplyAutoTextureHandler(TileConvertCommon.EditAndSaveTilesParams param, AutoTextureParams autoTexParam)
        {
            var textureRunner = new TileTextureRunner();

            List<Transform> tileTransforms = param.TileTransforms;

            Debug.Log($"ApplyAutoTextureHandler: tileTransforms.Count={tileTransforms.Count}");

            await Task.Yield();

            int count = param.TileTransforms.Count;

            if (param.TileTransforms.Count > 0)
            {
                if (autoTexParam.ConvertToPrimary)
                {
                    IEnumerable<Transform> convertibleTiles = tileTransforms.Where(t => Enumerable.Range(0, t.childCount)
                        .Select(i => t.GetChild(i).name).All(n => n.StartsWith("LOD"))); // タイル直下の子が全てLODであるタイルのみ抽出 (改造構造が変わっていると変換できない）
                    if (convertibleTiles.Any())
                    {
                        // 主要地物に変換します。
                        PLATEAU.CityImport.Import.Convert.GranularityConvertResult result = await new CityGranularityConverter().ConvertAsync(
                            new GranularityConvertOptionUnity(
                                new GranularityConvertOption(ConvertGranularity.PerPrimaryFeatureObject, 1), new UniqueParentTransformList(convertibleTiles), true));
                        tileTransforms = TileConvertCommon.GetEditableTransforms(autoTexParam.SelectedItems, param.EditingTile); // 変換後の Tile を再取得
                    }
                }

                foreach (Transform transform in tileTransforms)
                {
                    var tcs = new TaskCompletionSource<bool>();
                    void OnProcessingFinishedHandler()
                    {
                        // 処理完了時のコールバック
                        textureRunner.OnProcessingFinished -= OnProcessingFinishedHandler;
                        tcs.TrySetResult(true);
                    }

                    Debug.Log($"Auto texturing started for tile: {transform.name} {count}");

                    textureRunner.OnProcessingFinished += OnProcessingFinishedHandler;
                    textureRunner.RunDelayed(transform.gameObject);

                    await tcs.Task; // 完了待ち

                    Debug.Log($"Auto texturing completed for tile: {transform.name} {count--}");
                }
            }

            Debug.Log($"ApplyAutoTextureHandler finished.");
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
            //targetTrans.parent.GetComponent<CesiumGeoreference>().SetOriginLongitudeLatitudeHeight(longLatHeight[0], longLatHeight[1], longLatHeight[2]);
            //targetTrans.GetComponent<CesiumGlobeAnchor>().longitudeLatitudeHeight = longLatHeight;
            targetTrans.rotation = Quaternion.identity;

            if (resultOfGeoidHeightQuery == 0f)
            {
                Debug.LogError("高さ合わせが失敗しました\nインターネットに接続されていると確認した上で再度お試しください。高さはゼロに設定されました。");
            }
        }
        #endif
    }

    /// <summary>
    /// インポートしたPLATEAU都市オブジェクトを対象に、この処理を一度だけ実行すると、
    /// GIS Sampleで利用可能な形式を保ったまま、Rendering ToolkitsのAuto Texturing（夜に窓が光る見た目にする）を適用できます。
    ///
    /// 普通にRendering ToolkitsのGUIからAuto Texturingするのとのこのクラスの違いは、
    /// 前者はゲームオブジェクトの階層構造をかなり変えてしまうのに対し、後者はあまり変えません。
    /// </summary>
    public class TileTextureRunner
    {
        [SerializeField, Tooltip("処理の対象となるPLATEAU都市オブジェクトを指定してください。")]
        private GameObject target;

        private const string FloodingMaterialName = "FloodingMaterial";

        public event Action OnProcessingFinished;

        public void RunDelayed(GameObject targetObj)
        {
            EditorApplication.delayCall += () => RunDelayedInner(targetObj);
        }

        public void RunDelayedInner(GameObject targetObj)
        {
            // Rendering ToolkitsのAuto Texturingの機能を用意します。
            var renderers = targetObj.transform.GetComponentsInChildren<MeshRenderer>(true);
            var assembly = typeof(EnvironmentControllerEditor).Assembly;
            var autoTexturingType = assembly.GetType("PlateauToolkit.Rendering.Editor.AutoTexturing");
            var autoTexturing = CreateInstanceOfType(autoTexturingType);
            var materialTableInfo = autoTexturingType.GetField("s_BuildingMaterialAssignmentTable", BindingFlags.NonPublic | BindingFlags.Static);
            if (materialTableInfo == null)
            {
                Debug.LogError("field not found");
                return;
            }
            materialTableInfo.SetValue(null, AssetDatabase.LoadAssetAtPath<PlateauRenderingMaterialAssignment>(PlateauToolkitRenderingPaths.k_BuildingTextureAssetUrp));

            // 対象の各Rendererに対して処理を適用します。
            foreach (var r in renderers)
            {
                var objName = r.gameObject.name;

                // AutoTexturingで生成されるゲームオブジェクトを除外することで処理のダブりを防ぎます。
                if (objName == "FloorEmission") continue;
                if (objName == "ObstacleLight") continue;

                // 洪水モデルにマテリアルを適用します。
                if (objName.Contains("fld"))
                {
                    r.sharedMaterial = Resources.Load<Material>(FloodingMaterialName);
                }

                // Auto Texturingで光らせたいのは建物だけなので、建物以外は飛ばします。
                //if (!objName.Contains("bldg")) continue;

                var go = r.gameObject;
                string lod = go.transform.parent.name;

                // Auto Texturingを適用します。
                // 普通の処理と異なり、ゲームオブジェクトの階層構造はなるべく維持されるようにします。
                var meshFilter = go.GetComponent<MeshFilter>();

                if (lod == "LOD1")
                {
                    ExecAutoTexturing(autoTexturingType, autoTexturing, "ProcessLOD1", go, r, meshFilter);
                }
                else 
                {
                    ExecAutoTexturing(autoTexturingType, autoTexturing, "ProcessLod2", go, r, meshFilter);
                }

                Undo.ClearAll();
            }

            OnProcessingFinished?.Invoke();
        }

        private object CreateInstanceOfType(Type autoTexturingType)
        {
            object instance = Activator.CreateInstance(autoTexturingType, nonPublic: true);
            return instance;
        }

        private void ExecAutoTexturing(Type autoTexturingType, object instance, string methodName, GameObject go, Renderer r, MeshFilter mf)
        {
            MethodInfo processMethod =
                autoTexturingType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (processMethod == null)
            {
                throw new Exception("method not found");
            }
            processMethod.Invoke(instance, new object[] { go, r, mf });

        }
    }

    [CustomEditor(typeof(CityTileAdjuster))]
    public class CityTileAdjusterEditor : Editor
    {
        //private bool zl9 = true;
        //private bool zl10 = true;
        //private bool zl11 = true;
        private bool convPrimary = true;

        private TileListElementData tileData;
        private TileListElement tileListElement;

        public override void OnInspectorGUI()
        {
            var cityAdjuster = (CityTileAdjuster)target;

            if (tileData == null)
            {
                var windows = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
                tileData = new TileListElementData(windows.FirstOrDefault() as EditorWindow);
                tileData.TileManager = cityAdjuster.tileManager;
                tileListElement = new TileListElement(tileData);
            }
            tileListElement.DrawContent();

            //GUILayout.Label("Zoom Levels");
            //using (new GUILayout.HorizontalScope())
            //{
            //    zl9 = GUILayout.Toggle(zl9, "9");
            //    zl10 = GUILayout.Toggle(zl10, "10");
            //    zl11 = GUILayout.Toggle(zl11, "11");
            //    GUILayout.FlexibleSpace();
            //}
            convPrimary = GUILayout.Toggle(convPrimary, "Convert To Primary Objects");

            if (GUILayout.Button("Convert Tiles"))
            {
                //List<int> zoomLevels = new List<int>();
                //if (zl9) zoomLevels.Add(9);
                //if (zl10) zoomLevels.Add(10);
                //if (zl11) zoomLevels.Add(11);
                //cityAdjuster.RunAutoTextureTile(zoomLevels, convPrimary);

                cityAdjuster.RunAutoTextureTile(tileData.ObservableSelectedTiles, convPrimary);
            }

            if (GUILayout.Button("Adjust Tile"))
            {
                cityAdjuster.StartCoroutine(cityAdjuster.AdjustTile());
            }

            if (GUILayout.Button("Adjust CityGml"))
            {
                cityAdjuster.StartCoroutine(cityAdjuster.AdjustCityGml());
            }

            if (GUILayout.Button("Create Layers"))
            {
                if (!Layers.LayerExists(PlateauSandboxTrafficManagerConstants.LAYER_MASK_VEHICLE))
                    Layers.CreateLayer(PlateauSandboxTrafficManagerConstants.LAYER_MASK_VEHICLE);
                if (!Layers.LayerExists(PlateauSandboxTrafficManagerConstants.LAYER_MASK_GROUND))
                    Layers.CreateLayer(PlateauSandboxTrafficManagerConstants.LAYER_MASK_GROUND);

                // ReproducedRoadのレイヤーを変更
                PLATEAUReproducedRoad[] reproducedRoads = GameObject.FindObjectsByType<PLATEAUReproducedRoad>(FindObjectsSortMode.None);
                if (reproducedRoads != null)
                {
                    for (int i = 0; i < reproducedRoads.Length; i++)
                    {
                        ChangeLayersIncludeChildren(reproducedRoads[i].transform, LayerMask.NameToLayer(PlateauSandboxTrafficManagerConstants.LAYER_MASK_GROUND));
                    }
                }

                // TrafficManagerのレイヤーを変更
                var trafficManager = FindFirstObjectByType<TrafficManager>();
                var type = trafficManager.GetType();
                var gfield = type.GetField("groundLayerMask",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (gfield == null)
                    Debug.LogError("groundLayerMask フィールドが見つかりません");
                else
                {
                    LayerMask groundLayerMask = 1 << LayerMask.NameToLayer(PlateauSandboxTrafficManagerConstants.LAYER_MASK_GROUND);
                    gfield.SetValue(trafficManager, groundLayerMask);
                }

                var vfield = type.GetField("vehicleLayerMask",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (vfield == null)
                    Debug.LogError("vehicleLayerMask フィールドが見つかりません");
                else
                {
                    LayerMask vehicleLayerMask = 1 << LayerMask.NameToLayer(PlateauSandboxTrafficManagerConstants.LAYER_MASK_VEHICLE);
                    vfield.SetValue(trafficManager, vehicleLayerMask);
                }

            }
            base.OnInspectorGUI();
        }

        void ChangeLayersIncludeChildren(Transform trans, LayerMask layer)
        {
            trans.gameObject.layer = layer;
            int len = trans.childCount;
            for (int i = 0; i < len; i++)
            {
                ChangeLayersIncludeChildren(trans.GetChild(i), layer);
            }
        }
    }
}

#endif