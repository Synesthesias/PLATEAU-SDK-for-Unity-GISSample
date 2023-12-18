using System.Collections;
using System.Collections.Generic;
using PlateauToolkit.Maps;
using UnityEngine;

public class GISAttrDisplayFactory : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private int propertyIndex;
    [SerializeField] private GISAttrDisplay display;
    private const float HeightOffset = 300;
    private void Start()
    {
        Exec();
    }

    public void Exec()
    {
        var dbfs = FindDbfsInChild(target.transform);
        foreach (var dbf in dbfs)
        {
            if (dbf.Properties.Count <= propertyIndex)
            {
                Debug.LogError("Invalid propertyIndex.");
                return;
            }

            var line = dbf.GetComponent<LineRenderer>();
            if (line == null || line.positionCount <= 0) return;

            var instanced = Instantiate(display, dbf.transform);
            instanced.transform.position = line.GetPosition(line.positionCount / 2) + Vector3.up * HeightOffset;

            instanced.SetContent(dbf.Properties[propertyIndex].Trim());
        }
    }

    private List<DbfComponent> FindDbfsInChild(Transform parent)
    {
        var ret = new List<DbfComponent>();
        FindDbfsInChildRecursive(parent, ret);
        return ret;
    }
    private void FindDbfsInChildRecursive(Transform parent, List<DbfComponent> outDbfs)
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
