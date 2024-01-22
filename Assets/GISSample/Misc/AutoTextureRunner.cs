#if UNITY_EDITOR
using System;
using System.Reflection;
using PlateauToolkit.Editor;
using PlateauToolkit.Rendering;
using PlateauToolkit.Rendering.Editor;
using UnityEditor;
using UnityEngine;

namespace GISSample.Misc
{
    /// <summary>
    /// インポートしたPLATEAU都市オブジェクトを対象に、この処理を一度だけ実行すると、
    /// GIS Sampleで利用可能な形式を保ったまま、Rendering ToolkitsのAuto Texturing（夜に窓が光る見た目にする）を適用できます。
    ///
    /// 普通にRendering ToolkitsのGUIからAuto Texturingするのとのこのクラスの違いは、
    /// 前者はゲームオブジェクトの階層構造をかなり変えてしまうのに対し、後者はあまり変えません。
    /// </summary>
    public class AutoTextureRunner : MonoBehaviour
    {
        [SerializeField, Tooltip("処理の対象となるPLATEAU都市オブジェクトを指定してください。")]
        private GameObject target;
    
        [SerializeField, Tooltip("洪水モデルに適用するマテリアルを指定してください。")]
        private Material floodingMaterial;

        public void Run()
        {
            // Rendering ToolkitsのAuto Texturingの機能を用意します。
            var renderers = target.transform.GetComponentsInChildren<MeshRenderer>(true);
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
            
                // インポートで非表示になっているものはGIS Sampleでは使わないので削除します。
                if (!r.gameObject.activeInHierarchy)
                {
                    DestroyImmediate(r.gameObject);
                    continue;
                }
            
                // 洪水モデルにマテリアルを適用します。
                if (objName.Contains("fld"))
                {
                    r.sharedMaterial = floodingMaterial;
                }
            
                // Auto Texturingで光らせたいのは建物だけなので、建物以外は飛ばします。
                if (!objName.Contains("bldg")) continue;
            
                var go = r.gameObject;
                string lod = go.transform.parent.name;
            
                // Auto Texturingを適用します。
                // 普通の処理と異なり、ゲームオブジェクトの階層構造はなるべく維持されるようにします。
                var meshFilter = go.GetComponent<MeshFilter>();
                if (lod == "LOD1")
                {
                    ExecAutoTexturing(autoTexturingType, autoTexturing, "ProcessLOD1", go, r, meshFilter);
                }else if (lod == "LOD2")
                {
                    ExecAutoTexturing(autoTexturingType, autoTexturing, "ProcessLod2", go, r, meshFilter);
                }
            
            }
        }


        private static object CreateInstanceOfType(Type autoTexturingType)
        {
            object instance = Activator.CreateInstance(autoTexturingType, nonPublic: true);
            return instance;
        }

        private static void ExecAutoTexturing(Type autoTexturingType, object instance, string methodName, GameObject go, Renderer r, MeshFilter mf)
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

    [CustomEditor(typeof(AutoTextureRunner))]
    public class AutoTextureRunnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("インポートしたPLATEAU都市オブジェクトを対象に、この処理を一度だけ実行すると、");
            EditorGUILayout.LabelField("GIS Sampleで利用可能な形式を保ったまま、");
            EditorGUILayout.LabelField("Rendering ToolkitsのAuto Texturing（夜に窓が光る見た目にする）を適用できます。");
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        
            if (GUILayout.Button("Run"))
            {
                ((AutoTextureRunner)target).Run();
            }
        }
    }

}
#endif