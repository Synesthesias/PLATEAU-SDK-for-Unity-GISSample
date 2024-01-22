using GISSample.Misc;
using PlateauToolkit.Maps;
using UnityEngine;

namespace GISSample.GISAttributeDisplay
{
    /// <summary>
    /// 国土数値情報を格納したゲームオブジェクトを参照し、
    /// それを元に指定した色で属性ディスプレイ（宙に浮かぶ文字ウィンドウとその下に突き刺さる円柱）を生成します。
    /// </summary>
    public abstract class GISAttrDisplayFactoryBase : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private int propertyIndex;
        [SerializeField] private GISAttrDisplay display;
        [SerializeField] private Color backgroundColor;
        [SerializeField] private Color textColor;
        [SerializeField] private Color pillarColor;
        protected const float HeightOffset = 300;
        
        private void Start()
        {
            Exec();
        }

        public void Exec()
        {
            var dbfs = GameObjectUtil.FindComponentsInChild<DbfComponent>(target.transform);
            foreach (var dbf in dbfs)
            {
                if (dbf.Properties.Count <= propertyIndex)
                {
                    Debug.LogError("Invalid propertyIndex.");
                    return;
                }

                

                var instanced = Instantiate(display, dbf.transform);
                instanced.transform.position = CalcPosition(dbf, out bool isSucceed);
                if (!isSucceed) return;

                instanced.SetContent(dbf.Properties[propertyIndex].Trim());
                instanced.SetColor(backgroundColor, textColor, pillarColor);
            }
        }

        protected abstract Vector3 CalcPosition(DbfComponent dbf, out bool isSucceed);

    }
}