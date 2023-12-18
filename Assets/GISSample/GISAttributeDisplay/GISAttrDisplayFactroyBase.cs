using PlateauToolkit.Maps;
using UnityEngine;

namespace GISSample.GISAttributeDisplay
{
    public abstract class GISAttrDisplayFactroyBase : MonoBehaviour
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
            var dbfs = GameObjectUtil.FindDbfsInChild(target.transform);
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