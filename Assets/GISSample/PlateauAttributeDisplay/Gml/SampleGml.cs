using System.Collections.Generic;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay
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
        public readonly HashSet<string> FloodingAreaNames;

        public SampleGml(GameObject gmlGameObj)
        {
            FloodingAreaNames = new HashSet<string>();
            cityObjDict = new CityObjDict(gmlGameObj);
            FloodingAreaNames = cityObjDict.FindAllFloodingAreaNames();
        }

        /// <summary>
        /// フィルタリング
        /// </summary>
        /// <param name="parameter"></param>
        public void Filter(FilterParameter parameter)
        {
            cityObjDict.Filter(parameter);
        }

        /// <summary>
        /// 色分け
        /// </summary>
        /// <param name="type">色分けタイプ</param>
        /// <param name="colorTable">色テーブル</param>
        /// <param name="areaName">浸水エリア名</param>
        public void ColorGml(ColorCodeType type, Color[] colorTable, string areaName = null)
        {
            cityObjDict.ColorGml(type, colorTable, areaName);
        }

        public SampleCityObject GetCityObject(string cityObjId)
        {
            return cityObjDict.Get(cityObjId);
        }

        public SampleAttribute GetAttribute(string cityObjID)
        {
            var cityObj = GetCityObject(cityObjID);
            return cityObj.Attribute;
        }
    }

}
