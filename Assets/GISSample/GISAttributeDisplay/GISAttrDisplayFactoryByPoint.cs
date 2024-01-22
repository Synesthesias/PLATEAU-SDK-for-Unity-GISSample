using PlateauToolkit.Maps;
using UnityEngine;

namespace GISSample.GISAttributeDisplay
{
    /// <summary>
    /// <see cref="GISAttrDisplayFactoryBase"/>の実装について、GISの形式がPointである場合の実装です。
    /// </summary>
    public class GISAttrDisplayFactoryByPoint : GISAttrDisplayFactoryBase
    {
        protected override Vector3 CalcPosition(DbfComponent dbf, out bool isSucceed)
        {
            isSucceed = true;
            return dbf.transform.position + Vector3.up * HeightOffset;
        }
    }
}
