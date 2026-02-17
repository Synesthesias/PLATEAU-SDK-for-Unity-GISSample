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
        private bool isFlooding;

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
                            Debug.LogError($"parentGml.Tile.LoadedObject is null {gameObjPath}");
                            return null;
                        }

                        gameObj = parentGml.Tile?.LoadedObject?.transform?.GetTransformFromPath(gameObjPath)?.gameObject; // タイル更新によって消えている場合はPathからGameObjectを取得し直す
                    }
                }

                return gameObj;
            }
        }
        private GameObject gameObj;

        private string gameObjPath;
        private SampleGml parentGml;

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
                return renderer;
            }
        }
        private Renderer renderer;

        /// <summary>
        /// 表示すべきかどうかを格納します。
        /// この結果は<see cref="ApplyFilter"/>で適用します。
        /// </summary>
        public FeatureObjFilter Filter { get; set; }

        public FeatureGameObj(GameObject gameObj, bool isFlooding, SampleGml parent)
        {
            this.gameObj = gameObj;
            this.parentGml = parent;
            this.isFlooding = isFlooding;

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
            if(NormalMaterials != null)
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

            if(InitialTextures  != null)
            {
                for(int i = 0;i < InitialTextures.Length;i++)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(InitialTextures[i]);
#else
                    Object.Destroy(InitialTextures[i]);
#endif
                }
                InitialTextures = null;
            }

            if(ColoredMaterials  != null)
            {
                for(int i = 0; i < ColoredMaterials.Length; i++)
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
