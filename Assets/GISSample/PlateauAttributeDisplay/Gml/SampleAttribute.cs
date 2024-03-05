using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// 属性情報のラッパー
    /// </summary>
    public class SampleAttribute
    {
        /// <summary>
        /// キーデータ
        /// </summary>
        public struct KeyPath
        {
            /// <summary>
            /// 属性キー
            /// </summary>
            public string Key;

            /// <summary>
            /// ルートのキーから結合したキー
            /// "_"でJoinしています。
            /// </summary>
            public string Path;
        }

        public readonly double? MeasuredHeight;
        private readonly CityObjectList.Attributes attributes;

        public SampleAttribute(CityObjectList.Attributes attributes)
        {
            this.attributes = attributes;

            if (this.attributes.TryGetValue("bldg:measuredheight", out var val))
            {
                MeasuredHeight = val.DoubleValue;
            }
            else
            {
                MeasuredHeight = null;
            }
        }

        /// <summary>
        /// List化された属性情報を返す
        /// AttributesMap内の全ての情報をListに変換しています。
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<KeyPath, string>> GetKeyValues()
        {
            var keyValues = new List<KeyValuePair<KeyPath, string>>();
            GetKeyValuesInner(attributes, "", keyValues);

            return keyValues;
        }

        /// <summary>
        /// 浸水エリア情報を返す
        /// </summary>
        /// <returns></returns>
        public List<FloodingAreaInfo> GetFloodingAreaInfos()
        {
            var infos = new List<FloodingAreaInfo>();
            GetFloodingAreaInfosInner(attributes, infos);

            return infos;
        }

        public FloodingAreaInfo GetFloodingAreaInfoByName(string areaName)
        {
            var infos = GetFloodingAreaInfos();
            var index = infos.FindIndex(info => info.AreaName == areaName);
            if (index < 0)
            {
                return null;
            }

            return infos[index];
        }

        private void GetKeyValuesInner(
            CityObjectList.Attributes attributesMap,
            string parentPath,
            List<KeyValuePair<KeyPath, string>> keyValues
        )
        {
            foreach (var keyValue in attributesMap)
            {
                var path = string.IsNullOrEmpty(parentPath)
                    ? keyValue.Key
                    : string.Join("_", parentPath, keyValue.Key);

                if (keyValue.Value.Type == AttributeType.AttributeSet)
                {
                    GetKeyValuesInner(keyValue.Value.AttributesMapValue, path, keyValues);
                }
                else
                {
                    keyValues.Add(
                        new KeyValuePair<KeyPath, string>(
                            new KeyPath { Key = keyValue.Key, Path = path },
                            keyValue.Value.StringValue
                        )
                    );
                }
            }
        }

        private void GetFloodingAreaInfosInner(CityObjectList.Attributes attrs, List<FloodingAreaInfo> infos)
        {
            // ケース1: fldデータのケースであり、属性情報のキー "gml:name" に "○○川想定浸水～" と書いてあり、 "uro:floodingRiskAttribute" に浸水ランクが書いてあるケース
            if (attrs.TryGetValue("gml:name", out var floodingGmlNameVal))
            {
                if (floodingGmlNameVal.StringValue.Contains("浸水"))
                {
                    if (attrs.TryGetValue("uro:floodingRiskAttribute", out var floodingRisk))
                    {
                        var floodingInfo =
                            FloodingAreaInfo.CreateFromFldAttrValue(floodingRisk, floodingGmlNameVal.StringValue);
                        if (floodingInfo != null) infos.Add(floodingInfo);
                    }
                }
            }
            // ケース2: bldgデータに洪水情報があるケースであり、キー "uro:buildingDisasterRiskAttribute/uro:rank" に浸水ランクが書いてあり、 "uro:description" に "○○川" と書いてあるケース
            else if (attrs.TryGetValue("uro:buildingDisasterRiskAttribute", out var floodingRiskBuilding))
            {
                if (floodingRiskBuilding.AttributesMapValue.TryGetValue("uro:rank", out var floodingBuildingRank))
                {
                    if (attrs.TryGetValue("uro:description", out var floodingDescription))
                    {
                        var floodingInfo = new FloodingAreaInfo(floodingDescription.StringValue,
                            FloodingRank.FromString(floodingBuildingRank.StringValue));
                        infos.Add(floodingInfo);
                    }
                }
                
                
            }
        }
    }
}