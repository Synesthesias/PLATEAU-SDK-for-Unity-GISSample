using System.Collections;
using System.Collections.Generic;
using GISSample;
using GISSample.GISAttributeDisplay;
using PlateauToolkit.Maps;
using UnityEngine;

public class GISAttrDisplayFactoryByPoint : GISAttrDisplayFactroyBase
{
    protected override Vector3 CalcPosition(DbfComponent dbf, out bool isSucceed)
    {
        isSucceed = true;
        return dbf.transform.position + Vector3.up * HeightOffset;
    }
}
