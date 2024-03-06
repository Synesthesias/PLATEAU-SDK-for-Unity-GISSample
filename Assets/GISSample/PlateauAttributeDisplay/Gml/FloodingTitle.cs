using System.Text;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// 洪水情報の分類の名前です。
    /// 例： "神田川流域（都道府県管理区間）L2（想定最大規模）浸水ランク"
    /// </summary>
    public class FloodingTitle
    {
        private readonly string riverName; // 例: "神田川流域"
        private readonly string adminName; // 例: "都道府県"
        private readonly string scaleName; // 例： "L2（想定最大規模）"

        private static readonly string[] ExpectingPatterns = { "（想定最大規模）", "（計画規模）" };
        private static readonly string[] RemovingPatternsFromRiver = { "洪水浸水想定区域図", "浸水予想区域図" };

        public FloodingTitle(string riverName, string adminName, string scaleName)
        {
            this.riverName = riverName;
            this.adminName = adminName;
            this.scaleName = scaleName;
            
            // riverNameに "洪水浸水想定区域図" などと書いてあると、川の名前にしては長すぎるしPLATEAU Viewでの表記ともズレるので削除。
            // 加えて建物の浸水名とfldの浸水名が一致するため色分け区分をまとめられる効果もある。
            foreach (var pattern in RemovingPatternsFromRiver)
            {
                if (this.riverName.Contains(pattern))
                {
                    this.riverName = this.riverName.Replace(pattern, "");
                }
            }
            
            // riverNameに "想定最大規模" などと書いてあると scaleName とダブるので、
            // riverNameの "想定最大規模" などを削除して scaleName に移動します。
            foreach (var pattern in ExpectingPatterns)
            {
                if (this.riverName.Contains(pattern))
                {
                    this.riverName = this.riverName.Replace(pattern, "");
                    if (string.IsNullOrEmpty(this.scaleName))
                    {
                        this.scaleName = pattern;
                    }
                }
            }
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(riverName))
            {
                sb.Append(riverName);
            }

            if (!string.IsNullOrEmpty(adminName))
            {
                sb.Append($"（{adminName}管理区間）");
            }

            if (!string.IsNullOrEmpty(scaleName))
            {
                sb.Append(scaleName);
            }

            sb.Append("浸水ランク");
            return sb.ToString();
        }
        
        
        /// <summary>
        /// HashSetに対応するための実装
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var other = (FloodingTitle)obj;
            return riverName == other.riverName && adminName == other.adminName && scaleName == other.scaleName;
        }

        public override int GetHashCode()
        {
            return riverName.GetHashCode() + adminName.GetHashCode() + scaleName.GetHashCode();
        }
    }
}