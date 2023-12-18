using System.Collections;
using System.Collections.Generic;
using GISSample;
using GISSample.GISAttributeDisplay;
using PlateauToolkit.Maps;
using UnityEngine;

public class GISAttrDisplayFactoryByLine : GISAttrDisplayFactroyBase
{

    protected override Vector3 CalcPosition(DbfComponent dbf, out bool isSucceed)
    {
        var line = dbf.GetComponent<LineRenderer>();
        if (line == null || line.positionCount <= 0)
        {
            isSucceed = false;
            return Vector3.zero;
        }

        isSucceed = true;
        return line.GetPosition(line.positionCount / 2) + Vector3.up * HeightOffset;
    }

    

    
}
