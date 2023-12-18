using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#endif


/// <summary>
/// 遠くにあるMeshRendererを削除します。
/// </summary>
public class FarLineDestroyer : MonoBehaviour
{
    private const float Threshold = 4000f;
    void Start()
    {
        DestroyFarLines();        
    }
    

    #if UNITY_EDITOR
    [MenuItem("PLATEAU GIS Sample/Destroy Far Lines")]
    #endif
    public static void DestroyFarLines()
    {
        var lines = FindObjectsOfType<LineRenderer>();
        foreach (var line in lines)
        {
            if (line == null) continue;
            var bounds = line.bounds;
            var min = bounds.min;
            var max = bounds.max;
            float t = Threshold;
            bool isFar =
                Mathf.Abs(min.x) > t || Mathf.Abs(min.y) > t || Mathf.Abs(min.z) > t ||
                Mathf.Abs(max.x) > t || Mathf.Abs(max.y) > t || Mathf.Abs(max.z) > t;
            if (isFar)
            {
                DestroyImmediate(line.gameObject);
            }
        }
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        
#endif
    }
}
