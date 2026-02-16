using GISSample.PlateauAttributeDisplay.Gml;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// 「テクスチャの表示切り替え」ボタンが押された時の処理で、テクスチャの有無を切り替えます。
    /// </summary>
    public class TextureSwitcher
    {
        private readonly GmlDictionary gmlDict;
        private readonly GISTileManager tiles;
        private bool isTextureOn = true;
        private static readonly int ShaderPropIdBaseMap = Shader.PropertyToID("_BaseMap");

        public bool IsTextureOn => isTextureOn;

        public TextureSwitcher(GmlDictionary gmlDict, GISTileManager tiles)
        {
            this.gmlDict = gmlDict;
            this.tiles = tiles;
        }

        public void Switch()
        {
            isTextureOn = !isTextureOn;
            if (isTextureOn)
            {
                TurnOnOffTextures(true);
            }
            else
            {
                TurnOnOffTextures(false);
            }
        }

        public void SetCurrentTexture(SampleGml gml)
        {
            CoroutineUtil.RunToEnd(SetCurrentTextureCoroutine(gml));
        }

        public IEnumerator SetCurrentTextureCoroutine(SampleGml gml)
        {
            if (!isTextureOn)
            {
                yield return TurnOnOffTexturesCoroutine(gml, false);
            }
            else
            {
                yield return TurnOnOffTexturesCoroutine(gml, true);
            }
        }

        public void SetTextureOn()
        {
            if (isTextureOn) return;

            isTextureOn = true;
            TurnOnOffTextures(true);
        }

        public void SetTextureOff()
        {
            if (!isTextureOn) return;

            isTextureOn = false;
            TurnOnOffTextures(false); 
        }

        private void TurnOnOffTextures(bool on)
        {
            foreach (var gml in gmlDict.Gmls())
                CoroutineUtil.RunToEnd(TurnOnOffTexturesCoroutine(gml, on));

            if (tiles != null)
            {
                //if (GISTileManager.USE_COROUTINE_FOR_INTERACTION)
                //    tiles.ProcessAllLoadedTiles();
                //else
                //{
                //    //tiles.ClearCoroutineProcess();
                //    foreach (var gml in tiles.Gmls())
                //    {
                //        if(gml.Tile.LoadedObject == null) continue;
                //        CoroutineUtil.RunToEnd(TurnOnOffTexturesCoroutine(gml, on));
                //    }
                //}
                tiles.ProccessInteraction(() => TurnOnOffTextures(tiles.Gmls(), on));
            }
        }

        private void TurnOnOffTextures(IEnumerable<SampleGml> gmls, bool on)
        {
            foreach (var gml in gmls)
            {
                if (gml.Tile.LoadedObject == null) continue;
                CoroutineUtil.RunToEnd(TurnOnOffTexturesCoroutine(gml, on));
            }
        }

        public IEnumerator TurnOnOffTexturesCoroutine(SampleGml gml, bool on)
        {

            int count = 0;
            var semantics = gml.SemanticCityObjs();
            foreach (var semantic in semantics)
            {
                var features = semantic.FeatureGameObjs();

                foreach (var feat in features)
                {
                    var renderer = feat.Renderer; //初期化も同時に行う
                    if (renderer == null) continue; 
                    
                    if (semantic.CurrentColor == Color.white)　//初回実行時にテクスチャが差し変わらないので実装
                        feat.RestoreInitialMaterials();

                    var materials = feat.NormalMaterials;
                    int matCount = materials.Length;
                    for (int i = 0; i < matCount; i++)
                    {
                        var mat = materials[i];
                        if (on)
                            SetMainTexture(mat, feat.InitialTextures[i]);
                        else
                            SetMainTexture(mat, null);
                    }
                }

                count++;
                if (count > CoroutineUtil.YIELD_STEP_FAST)
                {
                    count = 0;
                    yield return null;
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