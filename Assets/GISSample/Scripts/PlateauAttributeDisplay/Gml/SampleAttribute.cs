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

        public FloodingAreaInfo GetFloodingAreaInfoByTitle(FloodingTitle floodingTitle)
        {
            var infos = GetFloodingAreaInfos();
            var index = infos.FindIndex(info => info.FloodingTitle.Equals(floodingTitle));
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
            // ケース1: fldデータのケースであり、属性情報のキー "gml:name" に "○○川流域～" と書いてあり、 "uro:floodingRiskAttribute" に浸水ランクが書いてあるケース
            if (attrs.TryGetValue("uro:WaterBodyRiverFloodingRiskAttribute", out var floodingRiskAttributeVal))
            {
                if (floodingRiskAttributeVal.AttributesMapValue.TryGetValue("uro:description", out var description))
                {
                    if (!description.StringValue.Contains("流域"))
                    {
                        return;
                    }

                    string adminName = "";
                    if (floodingRiskAttributeVal.AttributesMapValue.TryGetValue("uro:adminType", out var adminAttr))
                    {
                        adminName = adminAttr.StringValue;
                    }

                    string scaleName = "";
                    if (floodingRiskAttributeVal.AttributesMapValue.TryGetValue("uro:scale", out var scaleAttr))
                    {
                        scaleName = scaleAttr.StringValue;
                    }
                
                    if (floodingRiskAttributeVal.AttributesMapValue.TryGetValue("uro:rank", out var rankVal))
                    {
                        var rankStr = rankVal.StringValue;
                        FloodingRank rank = FloodingRank.FromString(rankStr);
                        var floodingInfo =  new FloodingAreaInfo(new FloodingTitle(description.StringValue, adminName, scaleName), rank);
                        infos.Add(floodingInfo);
                    }
                }
            }
            // ケース2: bldgデータに洪水情報があるケースであり、キー "uro:rank" に浸水ランクが書いてあり、 "uro:description" に "○○川" と書いてあるケース
            if (attrs.TryGetValue("uro:BuildingHighTideRiskAttribute", out var highTideAttribute))
            {
                var flood = GetBuildingFloodingAttr(highTideAttribute.AttributesMapValue);
                if(flood != null) infos.Add(flood);
            }
            // ケース3: ケース2の親キーが違うバージョン。ケース2と3が両方実行されることもありうる。
            if (attrs.TryGetValue("uro:BuildingRiverFloodingRiskAttribute", out var floodingAttribute))
            {
                var flood = GetBuildingFloodingAttr(floodingAttribute.AttributesMapValue);
                if(flood != null) infos.Add(flood);
            }
            // ケース4: ケース3の親キーの2のバージョン。
            if (attrs.TryGetValue("uro:BuildingRiverFloodingRiskAttribute2", out var floodingAttribute2))
            {
                var flood = GetBuildingFloodingAttr(floodingAttribute2.AttributesMapValue);
                if(flood != null) infos.Add(flood);
            }
        }

        private FloodingAreaInfo GetBuildingFloodingAttr(CityObjectList.Attributes parentAttrs)
        {
            if (parentAttrs.TryGetValue("uro:rank", out var floodingBuildingRank))
            {
                string adminName = "";
                if (parentAttrs.TryGetValue("uro:adminType", out var adminAttr))
                {
                    adminName = adminAttr.StringValue;
                }

                string scaleName = "";
                if (parentAttrs.TryGetValue("uro:scale", out var scaleAttr))
                {
                    scaleName = scaleAttr.StringValue;
                }
                    
                if (parentAttrs.TryGetValue("uro:description", out var floodingDescription))
                {
                    // 神田川bldgデータが少なすぎてあまり見られないのでスキップ
                    // bool shouldSkip = floodingDescription.StringValue.Contains("神田川");
                    // if (!shouldSkip)
                    // {
                    var floodingInfo = new FloodingAreaInfo(new FloodingTitle(floodingDescription.StringValue, adminName, scaleName),
                        FloodingRank.FromString(floodingBuildingRank.StringValue));
                    return floodingInfo;
                    // }
                        
                }
            }

            return null;
        }
    }
}