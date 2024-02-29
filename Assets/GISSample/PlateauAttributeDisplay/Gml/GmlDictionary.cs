using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// GML辞書
    /// 対象GameObjectやGMLの属性情報等の必要な情報をまとめたものです。
    /// </summary>
    public class GmlDictionary
    {
        /// <summary>
        /// キーはGML名です
        /// </summary>
        private readonly Dictionary<string, SampleGml> gmls = new ();


        public void Init(PLATEAUInstancedCityModel[] instancedCityModels)
        {
            foreach (var instancedCityModel in instancedCityModels)
            {

                for (int i = 0; i < instancedCityModel.transform.childCount; ++i)
                {
                    // 子オブジェクトの名前はGMLファイル名です。
                    // ロードするときは、一引数に、対応するGameObjectを渡します。
                    var go = instancedCityModel.transform.GetChild(i).gameObject;

                    // サンプルではdemを除外します。
                    if (go.name.Contains("dem")) continue;
                    

                    // ロードしたデータをアプリ用に扱いやすくしたクラスに変換します。
                    var gml = new SampleGml(go);
                    if (!gmls.TryAdd(go.name, gml))
                    {
                        Debug.LogWarning("Duplicate GML name detected.");
                    };
                }
            }
        
        }

        public HashSet<string> FindAllAreaNames()
        {
            var areaNames = new HashSet<string>();
            foreach(var names in gmls.Select(pair => pair.Value.FloodingAreaNames))
            {
                areaNames.UnionWith(names);
            }

            return areaNames;
        }

        public void Filter(FilterParameter parameter)
        {
            foreach (var gml in gmls.Values)
            {
                gml.Filter(parameter);
            }
        }

        private SampleGml GetGml(string gmlName)
        {
            return gmls[gmlName];
        }

        public SampleCityObject GetCityObject(string gmlName, string cityObjName)
        {
            return GetGml(gmlName).GetCityObject(cityObjName);
        }

        public SampleAttribute GetAttribute(string gmlFileName, string cityObjectId)
        {
            if (gmls.TryGetValue(gmlFileName, out SampleGml gml))
            {
                return gml.GetAttribute(cityObjectId);
            }

            Debug.LogWarning("gml not found.");
            return null;
        }
    
        /// <summary>
        /// 色分け処理
        /// </summary>
        public void ColorCity(ColorCodeType type, string areaName, Color[] heightColorTable, Color[] floodingRankColorTable)
        {
            foreach (var keyValue in gmls)
            {
                Color[] colorTable = null;
                switch (type)
                {
                    case ColorCodeType.Height:
                        colorTable = heightColorTable;
                        break;
                    case ColorCodeType.FloodingRank:
                        colorTable = floodingRankColorTable;
                        break;
                    default:
                        break;
                }

                keyValue.Value.ColorGml(type, colorTable, areaName);
            }
        }
    
    }
}
