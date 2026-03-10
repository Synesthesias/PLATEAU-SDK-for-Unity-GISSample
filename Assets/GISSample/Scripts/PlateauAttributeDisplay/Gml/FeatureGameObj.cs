using PLATEAU.CityGML;
using PLATEAU.Util;
using UnityEngine;
using Material = UnityEngine.Material;
using Object = UnityEngine.Object;
using Texture = UnityEngine.Texture;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// GISサンプルにおいて、都市モデルのゲームオブジェクト1つに関する情報を格納するクラスです。
    /// </summary>
    public class FeatureGameObj
    {
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
        public Texture[] InitialTextures { get; set; }

        /// <summary> 色分けによって色が塗られたときのマテリアルを用意しておきます。色分けのたびにマテリアルをnewするのは重いためです。 </summary>
        public Material[] ColoredMaterials { get; set; }
        

        /// <summary> 色分け時に使うマテリアル </summary>
        private static readonly Material MaterialForColorBldg = Resources.Load<Material>("ColorByAttributesOpaqueMaterial");

        private static readonly Material MaterialForColorFld =
            Resources.Load<Material>("ColorByAttributesTransparentMaterial");

        private static readonly int ShaderPropIdBaseMap = Shader.PropertyToID("_BaseMap");

        private GameObject gameObj;

        /// <summary> 対象となるゲームオブジェクトです。 </summary>
        /// タイル更新時にnullになるため再取得する必要がある
        public GameObject GameObj
        {
            get
            {
                if (gameObj == null)
                {
                    if (parentGml.Tile != null)
                    {
                        if (parentGml.Tile.LoadedObject == null)
                        {
                            //Debug.LogError($"parentGml.Tile.LoadedObject is null {gameObjPath}");
                            return null;
                        }

                        gameObj = parentGml.Tile?.LoadedObject?.transform?.GetTransformFromPath(gameObjPath)?.gameObject; // タイル更新によって消えている場合はPathからGameObjectを取得し直す
                    }
                }

                return gameObj;
            }
        }

        /// <summary>
        /// GameObjのRenderer
        /// 取得時にnullなら各データを初期化
        /// タイル更新時にnullになるため再取得する必要がある
        /// </summary>
        public Renderer Renderer
        {
            get
            {
                if (renderer == null && GameObj != null)
                {
                    renderer = GameObj.GetComponent<Renderer>(); // タイル更新によって消えている場合は取得し直す

                    if(renderer == null)
                        return null;

                    ClearResources();

                    // 開始時のマテリアルを記憶。ただし編集に耐えるようコピーしておきます
                    var srcMaterials = renderer.materials;
                    int matCount = srcMaterials.Length;
                    NormalMaterials = new Material[matCount];
                    ColoredMaterials = new Material[matCount];
                    var srcColorMat = isFlooding ? MaterialForColorFld : MaterialForColorBldg;

                    // 開始時のマテリアル/Textureを記憶。(Normal/Color material, Texture)
                    InitialTextures = new Texture[matCount];
                    for (int i = 0; i < matCount; i++)
                    {
                        // Original Materials
                        NormalMaterials[i] = new Material(srcMaterials[i]);

                        // Texture
                        var mat = NormalMaterials[i];
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

                        // 色分け用マテリアル
                        var srcMat = isFlooding ? MaterialForColorFld : MaterialForColorBldg;
                        ColoredMaterials[i] = new Material(srcColorMat);
                    }

                    RestoreInitialMaterials();
                }
                return renderer;
            }
        }
        private Renderer renderer;

        /// <summary>
        /// 表示すべきかどうかを格納します。
        /// この結果は<see cref="ApplyFilter"/>で適用します。
        /// </summary>s
        public FeatureObjFilter Filter { get; set; }

        private string gameObjPath;
        private SampleGml parentGml;

        private SemanticCityObject parentSemantic;

        private bool isFlooding;

        private int lod;

        internal SemanticCityObject ParentSemantic => parentSemantic;
        internal int Lod => lod;

        public FeatureGameObj(GameObject gameObj, bool isFlooding, SampleGml parent, SemanticCityObject parentSemantic, int lod)
        {
            this.gameObj = gameObj;
            this.parentGml = parent;
            this.isFlooding = isFlooding;
            this.parentSemantic = parentSemantic;
            this.lod = lod;

            if(parent.Tile != null)
                this.gameObjPath = gameObj?.transform?.GetPathToParent(parent?.Tile?.Address);

            Filter = new FeatureObjFilter(isFlooding);
            if (Renderer == null)
            {
                Debug.LogWarning("renderer is not found.");
                return;
            }
        }

        /// <summary>
        /// ShouldActiveの結果を適用します。
        /// </summary>
        public void ApplyFilter()
        {
            if(GameObj == null) return;
            GameObj?.SetActive(Filter.ShouldActive());
        }

        /// <summary>
        /// Texture ON/OFF
        /// NormalMaterials / ColoredMaterials 切替はSetMaterialColorで行う (マテリアル色優先のため）
        /// </summary>
        /// <param name="on"></param>
        public void TextureOnOff(bool on)
        {
            var materials = NormalMaterials;
            int matCount = materials.Length;
            for (int i = 0; i < matCount; i++)
            {
                var mat = materials[i];
                if (on)
                    SetMainTexture(mat, InitialTextures[i]);
                else
                    SetMainTexture(mat, null);
            }
        }
        private void SetMainTexture(Material mat, Texture tex)
        {
            if (mat.HasTexture(ShaderPropIdBaseMap))
            {
                // Toolkitのシェーダーの場合
                mat.SetTexture(ShaderPropIdBaseMap, tex);
            }
            else
            {
                mat.mainTexture = null;
            }
        }

        /// <summary>
        /// マテリアル色変更
        /// </summary>
        /// <param name="color"></param>
        public void SetMaterialColor(Color color)
        {
            var renderer = Renderer;
            if (renderer == null) return;
            var materials = ColoredMaterials;
            foreach (var mat in materials)
            {
                mat.color = color;
            }

            renderer.materials = materials;
        }

        public void RestoreInitialMaterials()
        {
            if (Renderer == null) return;
            Renderer.materials = NormalMaterials;
        }

        /// <summary>
        /// 生成したリソースを破棄 (メモリリーク防止）
        /// </summary>
        private void ClearResources()
        {
            if (NormalMaterials != null)
            {
                for (int i = 0; i < NormalMaterials.Length; i++)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(NormalMaterials[i]);
#else
                    Object.Destroy(NormalMaterials[i]);
#endif
                }
                NormalMaterials = null;
            }

            if (InitialTextures  != null)
            {
                InitialTextures = null;
            }

            if (ColoredMaterials != null)
            {
                for (int i = 0; i < ColoredMaterials.Length; i++)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(ColoredMaterials[i]);
#else
                    Object.Destroy(ColoredMaterials[i]);
#endif
                }
                ColoredMaterials = null;
            }
        }
    }
}
