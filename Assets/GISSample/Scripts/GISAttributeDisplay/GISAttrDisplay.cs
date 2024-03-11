using UnityEngine;
using UnityEngine.UI;

namespace GISSample.GISAttributeDisplay
{
    /// <summary>
    /// 国土数値情報を示すビューの1つです。
    /// 宙に浮かぶ文字ウィンドウと、その下に突き刺さる円柱の色を制御します。
    /// </summary>
    public class GISAttrDisplay : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Image background;
        [SerializeField] private MeshRenderer pillar;
        public void SetContent(string content)
        {
            text.text = content;
        }

        public void SetColor(Color bgColor, Color textColor, Color pillarColor)
        {
            background.color = bgColor;
            text.color = textColor;
            var mat = pillar.sharedMaterial;
            mat.color = pillarColor;
            pillar.sharedMaterial = mat;
        }
    }
}
