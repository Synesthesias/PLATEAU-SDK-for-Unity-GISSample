using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

public class LineAdjuster : MonoBehaviour
{
    private void Start()
    {
        Exec();
    }
    
    #if UNITY_EDITOR
    [MenuItem("PLATEAU GIS Sample/Adjust Lines")]
    #endif
    public static void Exec()
    {
        var lines = FindObjectsOfType<LineRenderer>();
        foreach (var line in lines)
        {
            line.loop = false;
        }
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        #endif
    }
}
