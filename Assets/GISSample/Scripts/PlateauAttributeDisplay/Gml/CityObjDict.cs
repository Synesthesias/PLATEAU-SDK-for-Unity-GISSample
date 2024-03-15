using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// GISサンプルにおいて、1つのGMLファイルに含まれる地物の辞書です。
    /// キーはIDです。
    /// </summary>
    public class CityObjDict
    {
        private readonly Dictionary<string, SemanticCityObject> dict;
        private readonly SampleGml parentGml;

        /// <summary>
        /// GML相当のゲームオブジェクトの子をもとに<see cref="CityObjDict"/>を構築します。
        /// </summary>
        public CityObjDict(GameObject gmlGameObj, SampleGml parentGml)
        {
            dict = new();
            this.parentGml = parentGml;
            foreach (Transform lodTransform in gmlGameObj.transform)
            {
                foreach (Transform cityObjectTransform in lodTransform)
                {
                    var id = cityObjectTransform.name;
                    if (dict.ContainsKey(id))
                    {
                        // Debug.LogWarning("Duplicate CityObject id detected.");
                    }
                    else
                    {
                        var cityObjComponent = cityObjectTransform.GetComponent<PLATEAUCityObjectGroup>();
                        if (cityObjComponent != null)
                        {
                            dict[id] = new SemanticCityObject(cityObjComponent, this);
                        }
                    
                    }

                    if (dict.TryGetValue(id, out var o))
                    {
                        o.AddCityObjectForLod(lodTransform, cityObjectTransform, parentGml.IsFlooding);
                    }

                    
                }
            }
        }

        public bool IsFlooding => parentGml.IsFlooding;

        public FloodingTitleSet FindAllFloodingTitles()
        {
            var floodingNames = new FloodingTitleSet();
            foreach (var cityObj in dict.Values)
            {
                foreach (var flood in cityObj.Attribute.GetFloodingAreaInfos())
                {
                    floodingNames.Add(flood.FloodingTitle);
                }
            }

            return floodingNames;
        }

        public SemanticCityObject Get(string cityObjName)
        {
            return dict[cityObjName];
        }

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            return dict.Values.SelectMany(cityObj => cityObj.FeatureGameObjs());
        }

        public IEnumerable<SemanticCityObject> SemanticCityObjs()
        {
            foreach (var semanticObj in dict.Values)
            {
                yield return semanticObj;
            }
        }
    }
}
