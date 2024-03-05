using System;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    public struct FloodingRank
    {
        public int Rank;

        private FloodingRank(int rank)
        {
            Rank = rank;
        }
            
        public float Height
        {
            get
            {
                return Rank switch
                {
                    1 => 0.5f,
                    2 => 3f,
                    3 => 5f,
                    4 => 10f,
                    5 => 20f,
                    _ => 0f
                };
            }
        }

        public static FloodingRank FromString(string str)
        {
            return str switch
            {
                "0.5m未満" => new FloodingRank(1),
                "0.5m以上3m未満" => new FloodingRank(2),
                "3m以上5m未満" => new FloodingRank(3),
                "5m以上10m未満" => new FloodingRank(4),
                "10m以上20m未満" => new FloodingRank(5),
                _ => throw new ArgumentOutOfRangeException($"Unknown value: {str}")
            };
        }
    }
}