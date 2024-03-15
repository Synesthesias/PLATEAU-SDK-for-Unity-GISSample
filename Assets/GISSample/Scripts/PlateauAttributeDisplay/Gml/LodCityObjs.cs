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
        public readonly SortedDictionary<int, FeatureGameObj> LodToFeatureObj = new();

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
            if (!LodToFeatureObj.TryAdd(lod, featureGameObj))
            {
                Debug.LogError($"Failed to add {cityObjectTrans.name} for lod {lod}");
            }
        }

        public int MaxLodExist => LodToFeatureObj.Keys.Max();
        

        public void SetMaterialColor(Color color)
        {
            foreach (var feature in LodToFeatureObj.Values)
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

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            return LodToFeatureObj.Values;
        }
    }
}
