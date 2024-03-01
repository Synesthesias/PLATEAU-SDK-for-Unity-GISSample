using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// ある地物について、LODとゲームオブジェクトの対応を覚えておき、LODを切り替えたりマテリアルを変えたりできるようにします。
    /// </summary>
    public class LodCityObjs
    {
        private readonly SortedDictionary<int, FeatureGameObj> lodToFeatureObj = new();

        /// <summary>
        /// LODとゲームオブジェクトの対応関係を1つ記憶します。
        /// </summary>
        public void Add(Transform lodTrans, Transform cityObjectTrans, bool isFlooding)
        {
            string lodName = lodTrans.name; // "LOD0" "LOD1" "LOD2" "LOD3" のいずれか
            int lod;
            if (!int.TryParse(lodName.Substring(3), out lod)) // "LODn"のnをintに変換
            {
                Debug.LogError("Failed to parse lod name.");
                return;
            }

            var featureGameObj = new FeatureGameObj(cityObjectTrans.gameObject, isFlooding);
            if (!lodToFeatureObj.TryAdd(lod, featureGameObj))
            {
                Debug.LogError($"Failed to add {cityObjectTrans.name} for lod {lod}");
            }
        }

        public void FilterByLod(FilterParameter parameter)
        {
            int maxLodExist = lodToFeatureObj.Keys.Max();
            int maxLodToShow = Math.Min(maxLodExist, parameter.MaxLod);
            foreach (var (lod, featureObj) in lodToFeatureObj)
            {
                featureObj.Filter.SetLodFilter(lod == maxLodToShow && lod >= parameter.MinLod);
            }
        }

        public void FilterByFlooding(bool shouldActive)
        {
            foreach (var featureObj in lodToFeatureObj.Values)
            {
                featureObj.Filter.SetFloodingFilter(shouldActive);
            }
        }

        public void FilterByHeight(bool shouldActive)
        {
            foreach (var featureObj in lodToFeatureObj.Values)
            {
                featureObj.Filter.SetHeightFilter(shouldActive);
            }
        }
    

        public void ApplyFilter()
        {
            foreach (var feature in lodToFeatureObj.Values)
            {
                feature.ApplyFilter();
            }
        }


        public void SetMaterialColor(Color color)
        {
            foreach (var feature in lodToFeatureObj.Values)
            {
                var renderer = feature.Renderer;
                if(renderer == null) continue;
                var coloredMaterials = feature.ColoredMaterials;
                foreach (var mat in coloredMaterials)
                {
                    mat.color = color;
                }

                renderer.materials = coloredMaterials;
            }
        }

        public void RestoreDefaultMaterials()
        {
            foreach (var feature in lodToFeatureObj.Values)
            {
                feature.RestoreInitialMaterials();
            }
        }

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            foreach (var featureObj in lodToFeatureObj.Values)
            {
                yield return featureObj;
            }
        }
    }
}
