using System;
using System.Collections.Generic;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;

namespace GISSample.PlateauAttributeDisplay
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

        /// <summary>
        /// 浸水エリア情報
        /// </summary>
        public class FloodingAreaInfo
        {
            /// <summary>
            /// 浸水エリア名
            /// </summary>
            public string AreaName;

            /// <summary>
            /// 浸水ランク
            /// </summary>
            public int Rank;

            public static FloodingAreaInfo CreateFromAttrValue(CityObjectList.Attributes.Value floodingRisk,
                string gmlName)
            {
                if (!floodingRisk.AttributesMapValue.TryGetValue("uro:rank", out var rankVal)) return null;
                var rankStr = rankVal.StringValue;
                int rank = rankStr switch
                {
                    "0.5m未満" => 1,
                    "0.5m以上3m未満" => 2,
                    "3m以上5m未満" => 3,
                    "5m以上10m未満" => 4,
                    "10m以上20m未満" => 5,
                    _ => throw new ArgumentOutOfRangeException($"Unknown value: {rankStr}")
                };
                return new FloodingAreaInfo
                {
                    AreaName = gmlName,
                    Rank = rank
                };
            }
        }

        public readonly double? MeasuredHeight;
        public readonly CityObjectList.Attributes Attributes;
        public readonly string RawText;

        public SampleAttribute(CityObjectList.Attributes attributes)
        {
            Attributes = attributes;
            RawText = Attributes.ToString();

            if (Attributes.TryGetValue("bldg:measuredheight", out var val))
            {
                MeasuredHeight = val.DoubleValue;
            }
            else
            {
                MeasuredHeight = null;
            }
            // MeasuredHeight = Attributes.GetValueOrNull("bldg:measuredheight")?.AsDouble;
        }

        /// <summary>
        /// List化された属性情報を返す
        /// AttributesMap内の全ての情報をListに変換しています。
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<KeyPath, string>> GetKeyValues()
        {
            var keyValues = new List<KeyValuePair<KeyPath, string>>();
            GetKeyValuesInner(Attributes, "", keyValues);

            return keyValues;
        }

        /// <summary>
        /// 浸水エリア情報を返す
        /// </summary>
        /// <returns></returns>
        public List<FloodingAreaInfo> GetFloodingAreaInfos()
        {
            var infos = new List<FloodingAreaInfo>();
            GetFloodingAreaInfosInner(Attributes, infos);

            return infos;
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
            // foreach (var keyValue in attributesMap)
            // {
            //     if (keyValue.Value.Type != AttributeType.AttributeSet) continue;

            // 浸水エリア情報のキー名は不定なので、
            // キー名が"浸水ランク"を含むAttrSetを浸水エリア情報のテーブルとみなします。
            // var attrSet = keyValue.Value.AttributesMapValue;
            // if (attrSet.TryGetValue("浸水ランク", out var floodingVal))
            // {
            //     var info = new FloodingAreaInfo
            //     {
            //         AreaName = keyValue.Key,
            //         Rank = floodingVal.IntValue,
            //     };
            //     infos.Add(info);
            // }

            // if (attrSet.ContainsKey("浸水ランク"))
            // {
            //     var info = new FloodingAreaInfo
            //     {
            //         AreaName = keyValue.Key,
            //         Rank = attrSet["浸水ランク"].AsInt,
            //     };
            //     infos.Add(info);
            // }
            // else
            // {
            //     GetFloodingAreaInfosInner(attrSet, infos);
            // }
            // }

            if (attrs.TryGetValue("gml:name", out var floodingGmlNameVal))
            {
                if (floodingGmlNameVal.StringValue.Contains("浸水"))
                {
                    if (attrs.TryGetValue("uro:floodingRiskAttribute", out var floodingRisk))
                    {
                        var floodingInfo =
                            FloodingAreaInfo.CreateFromAttrValue(floodingRisk, floodingGmlNameVal.StringValue);
                        if (floodingInfo != null) infos.Add(floodingInfo);
                    }
                }
            }
            else if (attrs.TryGetValue("uro:buildingDisasterRiskAttribute", out var floodingRiskBuilding))
            {
                var floodingInfo =
                    FloodingAreaInfo.CreateFromAttrValue(floodingRiskBuilding, "建築物浸水リスク");
                if (floodingInfo != null) infos.Add(floodingInfo);
            }
        }
    }
}