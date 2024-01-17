using System.Collections.Generic;
using PLATEAU.CityInfo;
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
    /// GMLファイル1つに対応するゲームオブジェクトをサンプル上で扱うクラス
    /// </summary>
    public class SampleGml
    {
        public readonly Dictionary<string, SampleCityObject> CityObjects;
        public readonly HashSet<string> FloodingAreaNames;

        public SampleGml(GameObject gmlGameObj)
        {
            CityObjects = new Dictionary<string, SampleCityObject>();
            FloodingAreaNames = new HashSet<string>();

            foreach (Transform lodTransform in gmlGameObj.transform)
            {
                foreach (Transform cityObjectTransform in lodTransform)
                {
                    var id = cityObjectTransform.name;
                    if (!CityObjects.ContainsKey(id))
                    {
                        try
                        {
                            var cityObjComponent = cityObjectTransform.GetComponent<PLATEAUCityObjectGroup>();
                            if (cityObjComponent != null)
                            {
                                CityObjects[id] = new SampleCityObject(id, cityObjComponent);

                                foreach (var info in CityObjects[id].Attribute.GetFloodingAreaInfos())
                                {
                                    FloodingAreaNames.Add(info.AreaName);
                                }
                            }
                            
                        }
                        catch (KeyNotFoundException)
                        {
                            continue;
                        }
                    }

                    var level = -1;
                    if (lodTransform.name == "LOD0")
                    {
                        level = 0;
                    }
                    else if (lodTransform.name == "LOD1")
                    {
                        level = 1;
                    }
                    else if (lodTransform.name == "LOD2")
                    {
                        level = 2;
                    }
                    else if (lodTransform.name == "LOD3")
                    {
                        level = 3;
                    }

                    if (level != -1)
                    {
                        var go = cityObjectTransform.gameObject;
                        var material = cityObjectTransform.GetComponent<Renderer>()?.material;
                        CityObjects[id].LodObjects[level] = go;
                    }
                }
            }
        }

        /// <summary>
        /// フィルタリング
        /// </summary>
        /// <param name="parameter"></param>
        public void Filter(FilterParameter parameter)
        {
            foreach (var keyValue in CityObjects)
            {
                keyValue.Value.Filter(parameter);
            }
        }

        /// <summary>
        /// 色分け
        /// </summary>
        /// <param name="type">色分けタイプ</param>
        /// <param name="colorTable">色テーブル</param>
        /// <param name="areaName">浸水エリア名</param>
        public void ColorCode(ColorCodeType type, Color[] colorTable, string areaName = null)
        {
            foreach (var keyValue in CityObjects)
            {
                keyValue.Value.ColorCode(type, colorTable, areaName);
            }
        }
    }

}
