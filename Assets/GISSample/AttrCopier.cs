using System.Collections.Generic;
using PLATEAU.CityInfo;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GISSample
{
    public class AttrCopier : MonoBehaviour
    {
        [SerializeField] private Transform srcParent;
        [SerializeField] private Transform dstParent;

        private void Start()
        {
            CopyAttrs();
        }

        public void CopyAttrs()
        {
            var cogs = GameObjectUtil.FindComponentsInChild<PLATEAUCityObjectGroup>(srcParent);
            foreach (var cog in cogs)
            {
                var srcName = cog.gameObject.name;
                var dst = GameObjectUtil.RecursiveFindChild(dstParent, srcName);
                if (dst == null)
                {
                    Debug.LogWarning($"{srcName} is not found.");
                    continue;
                }

                if (!cog.gameObject.activeInHierarchy) continue;
                
                if (dst.GetComponent<PLATEAUCityObjectGroup>() != null)
                {
                    Debug.LogWarning("duplicate cog.");
                    continue;
                }

                if (dst.GetComponent<MeshCollider>() == null)
                {
                    dst.AddComponent<MeshCollider>();
                }

                var newCog = dst.AddComponent<PLATEAUCityObjectGroup>();
                newCog.Init(cog.CityObjects, cog.InfoForToolkits, cog.Granularity);

            }
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(AttrCopier))]
    public class CopyAttrsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var component = (AttrCopier)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Exec"))
            {
                component.CopyAttrs();
            }
        }
    }
    #endif
}