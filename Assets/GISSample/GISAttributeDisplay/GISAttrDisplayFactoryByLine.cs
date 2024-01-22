using PlateauToolkit.Maps;
using UnityEngine;

namespace GISSample.GISAttributeDisplay
{
    /// <summary>
    /// <see cref="GISAttrDisplayFactoryBase"/>について、GISの形式がLineである場合の実装です。
    /// </summary>
    public class GISAttrDisplayFactoryByLine : GISAttrDisplayFactoryBase
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
}
