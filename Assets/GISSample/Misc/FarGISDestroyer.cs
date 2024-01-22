using PlateauToolkit.Maps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace GISSample.Misc
{
    /// <summary>
    /// 遠くにあるMeshRendererを削除します。
    /// 国土数値情報を読み込んだあと、欲しい範囲から遠すぎる場所にあるGISを削除する目的で利用します。
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
}
