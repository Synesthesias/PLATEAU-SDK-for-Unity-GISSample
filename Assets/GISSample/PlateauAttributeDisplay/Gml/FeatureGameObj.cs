using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// GISサンプルにおいて、都市モデルのゲームオブジェクト1つに関する情報を格納するクラスです。
    /// </summary>
    public class FeatureGameObj
    {
        /// <summary> 対象となるゲームオブジェクトです。 </summary>
        private GameObject gameObj;

        /// <summary>
        /// 表示すべきかどうかを格納します。
        /// この結果は<see cref="ApplyFilter"/>で適用します。
        /// </summary>
        public FeatureObjFilter Filter { get; set; }

        public FeatureGameObj(GameObject gameObj, bool isFlooding)
        {
            this.gameObj = gameObj;
            Filter = new FeatureObjFilter(isFlooding);
        }

        /// <summary>
        /// ShouldActiveの結果を適用します。
        /// </summary>
        public void ApplyFilter()
        {
            gameObj.SetActive(Filter.ShouldActive());
        }

        public Renderer GetRenderer() => gameObj.GetComponent<Renderer>();
    }
}
