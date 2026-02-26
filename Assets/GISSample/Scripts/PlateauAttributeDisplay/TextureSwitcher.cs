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
            yield return TurnOnOffTexturesCoroutine(gml, isTextureOn);
        }

        public void TurnOnOffTextures(bool on)
        {
            if(on == isTextureOn) return; //すでに同じ状態なら何もしない

            isTextureOn = on; //フラグ更新

            foreach (var gml in gmlDict.Gmls())
                CoroutineUtil.RunToEnd(TurnOnOffTexturesCoroutine(gml, on));

            if (tiles != null)
                tiles.ProccessInteraction(() => TurnOnOffTextures(tiles.Gmls(), on));
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
                    feat.TextureOnOff(on);
                }

                count++;
                if (count > GISTileManager.YIELD_STEP)
                {
                    count = 0;
                    yield return null;
                }
            }
        }
    }
}