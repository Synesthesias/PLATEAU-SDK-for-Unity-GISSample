#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using PlateauToolkit.Rendering.Editor;
using UnityEditor;
using UnityEngine;

public class AutoTextureRunner : MonoBehaviour
{
    [SerializeField] private GameObject target;

    public void Run()
    {
        var renderers = target.transform.GetComponentsInChildren<MeshRenderer>(true);
        var assembly = typeof(EnvironmentControllerEditor).Assembly;
        var autoTexturingType = assembly.GetType("PlateauToolkit.Rendering.Editor.AutoTexturing");
        var autoTexturing = CreateInstanceOfType(autoTexturingType);
        foreach (var r in renderers)
        {
            if (r.gameObject.name == "FloorEmission") continue;
            if (r.gameObject.name == "ObstacleLight") continue;
            if (!r.gameObject.activeInHierarchy)
            {
                DestroyImmediate(r.gameObject);
                continue;
            }
            if (!r.gameObject.name.Contains("bldg")) return;
            var go = r.gameObject;
            string lod = go.transform.parent.name;
            
            
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
        processMethod.Invoke(instance, new object[] { go, r, mf });

    }
}

[CustomEditor(typeof(AutoTextureRunner))]
public class AutoTextureRunnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Run"))
        {
            ((AutoTextureRunner)target).Run();
        }
    }
}
#endif