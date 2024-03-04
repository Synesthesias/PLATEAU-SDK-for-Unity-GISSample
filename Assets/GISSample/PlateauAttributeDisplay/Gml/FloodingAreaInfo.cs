using PLATEAU.CityInfo;

namespace GISSample.PlateauAttributeDisplay.Gml
{
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
        public FloodingRank Rank;

        public FloodingAreaInfo(string areaName, FloodingRank rank)
        {
            AreaName = areaName;
            Rank = rank;
        }

        public static FloodingAreaInfo CreateFromFldAttrValue(CityObjectList.Attributes.Value floodingRisk,
            string gmlName)
        {
            if (!floodingRisk.AttributesMapValue.TryGetValue("uro:rank", out var rankVal)) return null;
            var rankStr = rankVal.StringValue;
            FloodingRank rank = FloodingRank.FromString(rankStr);
            return new FloodingAreaInfo(gmlName, rank);
        }
    }
}