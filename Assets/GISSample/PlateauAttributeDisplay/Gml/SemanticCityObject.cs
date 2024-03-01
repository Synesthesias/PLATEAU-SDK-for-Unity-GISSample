using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;
using UnityEngine.Assertions;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// ある地物を、意味的に意味のある単位でまとめたものです。
    /// 具体的には、LODと<see cref="FeatureGameObj"/>の対応関係をまとめた<see cref="LodCityObjs"/>と、属性情報を保持します。
    /// 例えば建物ならば、建物1つに関する情報(LOD0～3, 属性情報)をこのクラスが保持します。
    /// </summary>
    public class SemanticCityObject
    {
        private readonly LodCityObjs lodCityObjs;
        public SampleAttribute Attribute { get; }

        private const float FloodingHeightMultiplier = 1f;

        public SemanticCityObject(PLATEAUCityObjectGroup cityObjComponent)
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
            switch (height)
            {
                case <= 12:
                    SetMaterialColor(colorTable[0]);
                    break;
                case > 12 and <= 31:
                    SetMaterialColor(colorTable[1]);
                    break;
                case > 31 and <= 60:
                    SetMaterialColor(colorTable[2]);
                    break;
                case > 60 and <= 120:
                    SetMaterialColor(colorTable[3]);
                    break;
                case > 120 and <= 180:
                    SetMaterialColor(colorTable[4]);
                    break;
                default:
                    SetMaterialColor(colorTable[5]);
                    break;
            }
        }

        private void ColorByFloodingRank(Color[] colorTable, string areaName)
        {
            Assert.AreEqual(5, colorTable.Length, "ランクの色分けは5色");

            var info = Attribute.GetFloodingAreaInfoByName(areaName);
            if(info == null)
            {
                ChangeToDefaultState();
                return;
            }

            lodCityObjs.FilterByFlooding(true);
            lodCityObjs.ApplyFilter();
            var rank = info.Rank;
            int colorTableIndex = rank.Rank - 1;
            if (colorTableIndex < colorTable.Length)
            {
                SetMaterialColor(colorTable[colorTableIndex]);
                foreach (var feature in FeatureGameObjs())
                {
                    var trans = feature.GameObj.transform;
                    trans.position += Vector3.up * rank.Height * FloodingHeightMultiplier;
                }
            }
            else
            {
                ChangeToDefaultState();
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

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            return lodCityObjs.FeatureGameObjs();
        }
    }
}