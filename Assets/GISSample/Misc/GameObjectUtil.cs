using System.Collections.Generic;
using UnityEngine;

namespace GISSample.Misc
{
    public static class GameObjectUtil
    {
        public static List<T> FindComponentsInChild<T>(Transform parent) 
        {
            var ret = new List<T>();
            FindComponentsInChildRecursive<T>(parent, ret);
            return ret;
        }
        private static void FindComponentsInChildRecursive<T>(Transform parent, List<T> outDbfs)
        {

            var dbf = parent.GetComponent<T>();
            if (dbf != null)
            {
                outDbfs.Add(dbf);
            }

            // 子を再帰的に検索
            foreach (Transform child in parent)
            {
                FindComponentsInChildRecursive<T>(child, outDbfs);
            }
        }
        
        public static Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if(child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform found = RecursiveFindChild(child, childName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }
    }
}