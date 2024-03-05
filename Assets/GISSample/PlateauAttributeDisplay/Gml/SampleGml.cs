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
    public enum ColorCodeType
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
        public HashSet<string> FloodingAreaNames { get; }

        public SampleGml(GameObject gmlGameObjArg)
        {
            FloodingAreaNames = new HashSet<string>();
            cityObjDict = new CityObjDict(gmlGameObjArg, IsFlooding(gmlGameObjArg));
            FloodingAreaNames = cityObjDict.FindAllFloodingAreaNames();
        }

        
        private static bool IsFlooding(GameObject gmlGameObj) => gmlGameObj.name.Contains("fld");
        

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
