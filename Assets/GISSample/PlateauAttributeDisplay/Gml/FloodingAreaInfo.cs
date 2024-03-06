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
        public readonly FloodingTitle FloodingTitle;

        /// <summary>
        /// 浸水ランク
        /// </summary>
        public FloodingRank Rank;

        public FloodingAreaInfo(FloodingTitle floodingTitle, FloodingRank rank)
        {
            FloodingTitle = floodingTitle;
            Rank = rank;
        }
        
    }
}