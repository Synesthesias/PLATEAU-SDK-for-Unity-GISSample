using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// ある地物を、意味的に意味のある単位でまとめたものです。
    /// 具体的には、LODと<see cref="FeatureGameObj"/>の対応関係をまとめた<see cref="LodCityObjs"/>と、属性情報を保持します。
    /// 例えば建物ならば、建物1つに関する情報(LOD0～3, 属性情報)をこのクラスが保持します。
    /// </summary>
    public class SemanticCityObject
    {
        public LodCityObjs LodCityObjs { get; }

        public SampleAttribute Attribute { get; }
        private CityObjDict parentDict;

        public SemanticCityObject(PLATEAUCityObjectGroup cityObjComponent, CityObjDict parentDict)
        {
            Attribute = new SampleAttribute(cityObjComponent.PrimaryCityObjects.First().AttributesMap);
            LodCityObjs = new LodCityObjs();
            this.parentDict = parentDict;
        }

        public int MaxLodExist => LodCityObjs.MaxLodExist;
        public bool IsFlooding => parentDict.IsFlooding;
        

        public void AddCityObjectForLod(Transform lodTrans, Transform cityObjectTrans, bool isFlooding)
        {
            LodCityObjs.Add(lodTrans, cityObjectTrans, isFlooding);
        }
        

        public void SetMaterialColor(Color color)
        {
            LodCityObjs.SetMaterialColor(color);
        }

        public void ChangeToDefaultState()
        {
            foreach (var featureObj in FeatureGameObjs())
            {
                featureObj.Filter.SetFloodingFilter(false);
                featureObj.ApplyFilter();
                featureObj.RestoreInitialMaterials();
            }
        }

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            return LodCityObjs.FeatureGameObjs();
        }
    }
}