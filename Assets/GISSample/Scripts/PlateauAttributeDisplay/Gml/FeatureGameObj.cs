using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// GISサンプルにおいて、都市モデルのゲームオブジェクト1つに関する情報を格納するクラスです。
    /// </summary>
    public class FeatureGameObj
    {
        /// <summary> 対象となるゲームオブジェクトです。 </summary>
        public GameObject GameObj { get; }

        /// <summary>
        /// 通常状態のマテリアルです。
        /// アプリケーション開始時のマテリアルが初期状態として記憶されます。
        /// ただし、テクスチャをオフにするボタンが押下されたときは、これは<see cref="TextureSwitcher"/>によってテクスチャのないマテリアルに置き換わります。
        /// </summary>
        public Material[] NormalMaterials { get; set; }

        /// <summary>
        /// テクスチャのON/OFF機能で、OFFにしたものを元に戻せるようにテクスチャを記憶します。
        /// 添字は renderer.materials の添字に対応します。
        /// </summary>
        public Texture[] InitialTextures { get; }

        /// <summary> 色分けによって色が塗られたときのマテリアルを用意しておきます。色分けのたびにマテリアルをnewするのは重いためです。 </summary>
        public Material[] ColoredMaterials { get; }
        

        public Renderer Renderer { get; }

        /// <summary> 色分け時に使うマテリアル </summary>
        private static readonly Material MaterialForColorBldg = Resources.Load<Material>("ColorByAttributesOpaqueMaterial");

        private static readonly Material MaterialForColorFld =
            Resources.Load<Material>("ColorByAttributesTransparentMaterial");

        private static readonly int ShaderPropIdBaseMap = Shader.PropertyToID("_BaseMap");

        /// <summary>
        /// 表示すべきかどうかを格納します。
        /// この結果は<see cref="ApplyFilter"/>で適用します。
        /// </summary>
        public FeatureObjFilter Filter { get; set; }

        public FeatureGameObj(GameObject gameObj, bool isFlooding)
        {
            this.GameObj = gameObj;
            Filter = new FeatureObjFilter(isFlooding);
            Renderer = gameObj.GetComponent<Renderer>();
            if (Renderer == null)
            {
                Debug.LogWarning("renderer is not found.");
                return;
            }
            
            // 開始時のマテリアルを記憶。ただし編集に耐えるようコピーしておきます
            var srcMaterials = Renderer.materials;
            int matCount = srcMaterials.Length;
            var materials = new Material[matCount];
            for (int i = 0; i < matCount; i++)
            {
                materials[i] = new Material(srcMaterials[i]);
            }
            NormalMaterials = materials;
            
            // 開始時のテクスチャを記録
            InitialTextures = new Texture[matCount];
            for (int i = 0; i < matCount; i++)
            {
                var mat = materials[i];
                Texture tex;
                if (mat.HasTexture(ShaderPropIdBaseMap)) // Toolkitシェーダーの場合
                {
                    tex = mat.GetTexture(ShaderPropIdBaseMap);
                }
                else
                {
                    tex = mat.mainTexture;
                }

                InitialTextures[i] = tex;
            }
                
            // 色分け用マテリアルの初期化
            ColoredMaterials = new Material[matCount];
            var srcMat = isFlooding ? MaterialForColorFld : MaterialForColorBldg;
            for (int i = 0; i < matCount; i++)
            {
                ColoredMaterials[i] = new Material(srcMat);
            }
        }

        /// <summary>
        /// ShouldActiveの結果を適用します。
        /// </summary>
        public void ApplyFilter()
        {
            GameObj.SetActive(Filter.ShouldActive());
        }

        public void RestoreInitialMaterials()
        {
            Renderer.materials = NormalMaterials;
        }
    }
}
