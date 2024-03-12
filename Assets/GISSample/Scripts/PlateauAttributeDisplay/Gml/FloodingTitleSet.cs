using System.Collections.Generic;
using System.Linq;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// 洪水情報のキー（例: ○○川浸水ランク）の集合です。
    /// </summary>
    public class FloodingTitleSet
    {
        private HashSet<FloodingTitle> data = new();

        public bool Add(FloodingTitle floodingTitle)
        {
            return data.Add(floodingTitle);
        }

        public void UnionWith(FloodingTitleSet other)
        {
            data.UnionWith(other.data);
        }

        public int Count => data.Count;

        /// <summary>
        /// 集合のキーの一覧を文字列リストで返します。
        /// </summary>
        public IEnumerable<string> TitleStrings => data.Select(title => title.ToString());

        /// <summary>
        /// キー名から検索して返します。
        /// </summary>
        public FloodingTitle GetByTitleString(string titleStr)
        {
            return data.First(title => title.ToString() == titleStr);
        }
    }
}