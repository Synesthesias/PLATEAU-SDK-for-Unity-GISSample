using System.Collections;
using System.Collections.Generic;
using PlateauToolkit.Maps;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#endif


/// <summary>
/// 遠くにあるMeshRendererを削除します。
/// </summary>
public class FarGISDestroyer : MonoBehaviour
{
    private const float Threshold = 4000f;
    void Start()
    {
        DestroyFarGISs();        
    }
    

    #if UNITY_EDITOR
    [MenuItem("PLATEAU GIS Sample/Destroy Far GISs")]
    #endif
    public static void DestroyFarGISs()
    {
        // 遠くのPointを消します。
        var dbfs = FindObjectsOfType<DbfComponent>();
        foreach (var dbf in dbfs)
        {
            if (IsFar(dbf.transform.position))
            {
                DestroyImmediate(dbf.gameObject);
            }
        }
        
        
        // 遠くのLineRendererを消します。
        var lines = FindObjectsOfType<LineRenderer>();
        foreach (var line in lines)
        {
            if (line == null) continue;
            var bounds = line.bounds;
            var min = bounds.min;
            var max = bounds.max;
            float t = Threshold;
            bool isFar = IsFar(min) || IsFar(max);
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

    private static bool IsFar(Vector3 v)
    {
        float t = Threshold;
        return Mathf.Abs(v.x) > t || Mathf.Abs(v.y) > t || Mathf.Abs(v.z) > t;
    }
}
