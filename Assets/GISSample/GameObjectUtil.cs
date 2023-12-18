using System.Collections.Generic;
using PlateauToolkit.Maps;
using UnityEngine;

namespace GISSample
{
    public static class GameObjectUtil
    {
        public static List<DbfComponent> FindDbfsInChild(Transform parent)
        {
            var ret = new List<DbfComponent>();
            FindDbfsInChildRecursive(parent, ret);
            return ret;
        }
        private static void FindDbfsInChildRecursive(Transform parent, List<DbfComponent> outDbfs)
        {

            var dbf = parent.GetComponent<DbfComponent>();
            if (dbf != null)
            {
                outDbfs.Add(dbf);
            }

            // 子を再帰的に検索
            foreach (Transform child in parent)
            {
                FindDbfsInChildRecursive(child, outDbfs);
            }
        }
    }
}