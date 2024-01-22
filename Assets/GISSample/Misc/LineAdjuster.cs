using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace GISSample.Misc
{
    /// <summary>
    /// 国土数値情報でLineを読み込んだあと、Lineがループしている場合にループを解除する目的で利用します。
    /// </summary>
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
}
