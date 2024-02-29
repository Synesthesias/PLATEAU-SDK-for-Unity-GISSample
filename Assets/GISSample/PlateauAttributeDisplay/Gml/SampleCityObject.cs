using System.Linq;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using UnityEngine;
using UnityEngine.Assertions;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// CityObjectのラッパー
    /// </summary>
    public class SampleCityObject
    {
        private readonly LodCityObjs lodCityObjs;
        public SampleAttribute Attribute { get; }
        

        public SampleCityObject(PLATEAUCityObjectGroup cityObjComponent)
        {
            Attribute = new SampleAttribute(cityObjComponent.PrimaryCityObjects.First().AttributesMap);
            lodCityObjs = new LodCityObjs();
        }

        /// <summary>
        /// フィルタリング
        /// パラメータに応じてオブジェクトを表示、非表示化します。
        /// </summary>
        /// <param name="parameter"></param>
        public void Filter(FilterParameter parameter)
        {
            if (Attribute.MeasuredHeight.HasValue)
            {
                var measuredHeight = Attribute.MeasuredHeight.Value;
                bool heightFilter = measuredHeight >= parameter.MinHeight && measuredHeight <= parameter.MaxHeight;
                lodCityObjs.FilterByHeight(heightFilter);
                
            }
            lodCityObjs.FilterByLod(parameter);
            lodCityObjs.ApplyFilter();
        }

        /// <summary>
        /// 色分け
        /// </summary>
        /// <param name="type"></param>
        /// <param name="colorTable"></param>
        /// <param name="areaName">浸水エリア名</param>
        public void ColorCityObj(ColorCodeType type, Color[] colorTable, string areaName)
        {
            switch (type)
            {
                case ColorCodeType.None:
                default:
                    ChangeToDefaultState();
                    break;
                case ColorCodeType.Height:
                    ColorByHeight(colorTable);
                    break;
                case ColorCodeType.FloodingRank:
                    ColorByFloodingRank(colorTable, areaName);
                    break;
            }
        }

        public void AddCityObjectForLod(Transform lodTrans, Transform cityObjectTrans, bool isFlooding)
        {
            lodCityObjs.Add(lodTrans, cityObjectTrans, isFlooding);
        }


        private void ColorByHeight(Color[] colorTable)
        {
            Assert.AreEqual(6, colorTable.Length, "高さの色分けは6色");

            if (!Attribute.MeasuredHeight.HasValue)
            {
                ChangeToDefaultState();
                return;
            }

            var height = Attribute.MeasuredHeight.Value;
            if (height <= 12)
            {
                SetMaterialColor(colorTable[0]);
            }
            else if (height > 12 && height <= 31)
            {
                SetMaterialColor(colorTable[1]);
            }
            else if (height > 31 && height <= 60)
            {
                SetMaterialColor(colorTable[2]);
            }
            else if (height > 60 && height <= 120)
            {
                SetMaterialColor(colorTable[3]);
            }
            else if (height > 120 && height <= 180)
            {
                SetMaterialColor(colorTable[4]);
            }
            else
            {
                SetMaterialColor(colorTable[5]);
            }
        }

        private void ColorByFloodingRank(Color[] colorTable, string areaName)
        {
            Assert.AreEqual(5, colorTable.Length, "ランクの色分けは5色");

            var infos = Attribute.GetFloodingAreaInfos();
            var index = infos.FindIndex(info => info.AreaName == areaName);
            if (index < 0)
            {
                ChangeToDefaultState();
                return;
            }
            lodCityObjs.FilterByFlooding(true);
            lodCityObjs.ApplyFilter();
            var info = infos[index];
            switch (info.Rank)
            {
                case 1:
                    SetMaterialColor(colorTable[0]);
                    break;
                case 2:
                    SetMaterialColor(colorTable[1]);
                    break;
                case 3:
                    SetMaterialColor(colorTable[2]);
                    break;
                case 4:
                    SetMaterialColor(colorTable[3]);
                    break;
                case 5:
                    SetMaterialColor(colorTable[4]);
                    break;
                default:
                    ChangeToDefaultState();
                    break;
            }
        }

        public void SetMaterialColor(Color color)
        {
            lodCityObjs.SetMaterialColor(color);
        }

        private void ChangeToDefaultState()
        {
            lodCityObjs.FilterByFlooding(false);
            lodCityObjs.ApplyFilter();
            lodCityObjs.RestoreDefaultMaterials();
        }
    }
}