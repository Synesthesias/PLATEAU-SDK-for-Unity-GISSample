using PLATEAU.Util;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// GISサンプルにおいて、都市モデルのゲームオブジェクト1つに関する情報を格納するクラスです。
    /// </summary>
    public class FeatureGameObj
    {
        /// <summary> 対象となるゲームオブジェクトです。 </summary>
        private readonly GameObject gameObj;

        /// <summary> アプリケーション開始時のマテリアルを、あとで戻せるように記憶します </summary>
        private readonly Material[] initialMaterials;

        /// <summary> 色分けによって色が塗られたときのマテリアルを用意しておきます。色分けのたびにマテリアルをnewするのは重いためです。 </summary>
        public Material[] ColoredMaterials { get; }

        public Renderer Renderer { get; }

        /// <summary> 色分け時に使うマテリアル </summary>
        private static readonly Material materialForColor = Resources.Load<Material>("ColorByAttributesMaterial");

        /// <summary>
        /// 表示すべきかどうかを格納します。
        /// この結果は<see cref="ApplyFilter"/>で適用します。
        /// </summary>
        public FeatureObjFilter Filter { get; set; }

        public FeatureGameObj(GameObject gameObj, bool isFlooding)
        {
            this.gameObj = gameObj;
            Filter = new FeatureObjFilter(isFlooding);
            Renderer = gameObj.GetComponent<Renderer>();
            if (Renderer == null)
            {
                Debug.LogWarning("renderer is not found.");
            }
            else
            {
                initialMaterials = Renderer.materials;
                
                // 色分け用マテリアルの初期化
                ColoredMaterials = new Material[initialMaterials.Length];
                for (int i = 0; i < ColoredMaterials.Length; i++)
                {
                    ColoredMaterials[i] = new Material(materialForColor);
                }
            }
        }

        /// <summary>
        /// ShouldActiveの結果を適用します。
        /// </summary>
        public void ApplyFilter()
        {
            gameObj.SetActive(Filter.ShouldActive());
        }

        public void RestoreInitialMaterials()
        {
            Renderer.materials = initialMaterials;
        }
    }
}
