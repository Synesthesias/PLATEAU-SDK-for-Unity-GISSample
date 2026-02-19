using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PLATEAU.CityInfo;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// GISサンプルにおいて、1つのGMLファイルに含まれる地物の辞書です。
    /// キーはIDです。
    /// </summary>
    public class CityObjDict
    {
        //private readonly Dictionary<string, SemanticCityObject> dict;
        public  Dictionary<string, SemanticCityObject> dict;
        private SampleGml parentGml;
        internal SampleGml ParentGml => parentGml;

        public FloodingTitleSet FloodingTitleSet {  get; private set; }

        /// <summary>
        /// GML相当のゲームオブジェクトの子をもとに<see cref="CityObjDict"/>を構築します。
        /// </summary>
        public CityObjDict() {}

        public void Initialize(GameObject gmlGameObj, SampleGml parentGml)
        {
            dict = new();
            this.parentGml = parentGml;
            if (gmlGameObj.GetComponent<PLATEAUCityObjectGroup>() != null)
            {
                var id = gmlGameObj.name;
                if (dict.ContainsKey(id))
                {
                    // Debug.LogWarning("Duplicate CityObject id detected.");
                }
                else
                {
                    var cityObjComponent = gmlGameObj.GetComponent<PLATEAUCityObjectGroup>();
                    if (cityObjComponent != null)
                    {
                        dict[id] = new SemanticCityObject(cityObjComponent, this);

                        Debug.Log($"Created SemanticCityObject for id: {id}");
                    }
                }

                if (dict.TryGetValue(id, out var o))
                {
                    o.AddCityObjectForLod(gmlGameObj.transform.parent, gmlGameObj.transform, parentGml.IsFlooding, parentGml);
                }
                return;
            }

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
                        o.AddCityObjectForLod(lodTransform, cityObjectTransform, parentGml.IsFlooding, parentGml);
                    }
                }
            }
        }

        public IEnumerator GenerateFloodingTitleSet()
        {
            Debug.Log($"<color=cyan>CityObjDict GenerateFloodingTitleSet ({parentGml.Tile.Address})</color>");

            FloodingTitleSet = new FloodingTitleSet();
            foreach (var cityObj in dict.Values)
            {
                foreach (var flood in cityObj.Attribute.GetFloodingAreaInfos(parentGml.IsFlooding))
                {
                    FloodingTitleSet.Add(flood.FloodingTitle);
                    yield return null;
                }
            }
        }

        public bool IsFlooding => parentGml.IsFlooding;

        public FloodingTitleSet FindAllFloodingTitles()
        {
            var floodingNames = new FloodingTitleSet();
            foreach (var cityObj in dict.Values)
            {
                foreach (var flood in cityObj.Attribute.GetFloodingAreaInfos(parentGml.IsFlooding))
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
            return dict.Values;
        }

    }
}
