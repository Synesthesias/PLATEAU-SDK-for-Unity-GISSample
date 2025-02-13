using GISSample.PlateauAttributeDisplay.Gml;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// 「テクスチャの表示切り替え」ボタンが押された時の処理で、テクスチャの有無を切り替えます。
    /// </summary>
    public class TextureSwitcher
    {
        private readonly GmlDictionary gmlDict;
        private bool isTextureOn = true;
        private static readonly int ShaderPropIdBaseMap = Shader.PropertyToID("_BaseMap");

        public TextureSwitcher(GmlDictionary gmlDict)
        {
            this.gmlDict = gmlDict;
        }

        public void Switch()
        {
            isTextureOn = !isTextureOn;
            if (isTextureOn)
            {
                TurnOnTextures();
            }
            else
            {
                TurnOffTextures();
            }
        }

        public void SetTextureOn()
        {
            if (isTextureOn) return;

            isTextureOn = true;
            TurnOnTextures();
        }

        public void SetTextureOff()
        {
            if (!isTextureOn) return;

            isTextureOn = false;
            TurnOffTextures();
        }

        private void TurnOffTextures()
        {
            foreach (var feat in gmlDict.FeatureGameObjs())
            {
                var materials = feat.NormalMaterials;
                int matCount = materials.Length;
                for (int i = 0; i < matCount; i++)
                {
                    var mat = materials[i];
                    SetMainTexture(mat, null);
                }
                feat.NormalMaterials = materials;
            }
        }

        private void TurnOnTextures()
        {
            foreach (var feat in gmlDict.FeatureGameObjs())
            {
                var materials = feat.NormalMaterials;
                int matCount = materials.Length;
                for (int i = 0; i < matCount; i++)
                {
                    var mat = materials[i];
                    SetMainTexture(mat, feat.InitialTextures[i]);
                }
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
    }
}