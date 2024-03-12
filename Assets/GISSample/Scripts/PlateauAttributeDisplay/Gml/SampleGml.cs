using System.Collections.Generic;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// フィルターパラメータ
    /// </summary>
    public struct FilterParameter
    {
        /// <summary>
        /// 最小高さ
        /// </summary>
        public float MinHeight;

        /// <summary>
        /// 最大高さ
        /// </summary>
        public float MaxHeight;

        /// <summary>
        /// 最小LOD
        /// </summary>
        public int MinLod;

        /// <summary>
        /// 最大LOD
        /// </summary>
        public int MaxLod;
    }


    /// <summary>
    /// 色分けタイプ
    /// </summary>
    public enum BuildingColorType
    {
        /// <summary>
        /// なし
        /// </summary>
        None,

        /// <summary>
        /// 高さ
        /// </summary>
        Height,

        /// <summary>
        /// 浸水ランク
        /// </summary>
        FloodingRank,
    }
    

    /// <summary>
    /// GMLファイル1つに対応するゲームオブジェクトをサンプル上で扱いやすくしたラッパーです。
    /// </summary>
    public class SampleGml
    {
        private readonly CityObjDict cityObjDict;
        public FloodingTitleSet FloodingTitles { get; }
        public bool IsFlooding { get; private set; }

        public SampleGml(GameObject gmlGameObjArg)
        {
            FloodingTitles = new FloodingTitleSet();
            IsFlooding = gmlGameObjArg.name.Contains("fld");
            cityObjDict = new CityObjDict(gmlGameObjArg, this);
            FloodingTitles = cityObjDict.FindAllFloodingTitles();
        }

        
        

        public SemanticCityObject GetCityObject(string cityObjId)
        {
            return cityObjDict.Get(cityObjId);
        }

        public SampleAttribute GetAttribute(string cityObjID)
        {
            var cityObj = GetCityObject(cityObjID);
            return cityObj.Attribute;
        }

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            return cityObjDict.FeatureGameObjs();
        }

        public IEnumerable<SemanticCityObject> SemanticCityObjs()
        {
            return cityObjDict.SemanticCityObjs();
        }
    }

}
