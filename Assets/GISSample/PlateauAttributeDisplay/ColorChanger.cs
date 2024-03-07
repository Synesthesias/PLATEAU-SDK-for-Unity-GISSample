using System;
using GISSample.PlateauAttributeDisplay.Gml;
using UnityEngine;
using UnityEngine.Assertions;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// 高さや水害情報に応じて色分けします。
    /// </summary>
    public class ColorChanger
    {
        private SceneManager sceneManager;
        private const float FloodingHeightMultiplier = 1f;
        
        public ColorChanger(SceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
        }

        public void ChangeColor(ColorCodeType type, FloodingTitle floodingTitle)
        {
            foreach (var gml in sceneManager.Gmls())
            {
                var heightColorTable = sceneManager.GisUiController.heightColorTable;
                var floodingRankColorTable = sceneManager.GisUiController.floodingRankColorTable;

                Color[] colorTable = type switch
                {
                    ColorCodeType.Height => heightColorTable,
                    ColorCodeType.FloodingRank => floodingRankColorTable,
                    ColorCodeType.None => null,
                    _ => throw new ArgumentOutOfRangeException()
                };

                foreach (var semantic in gml.SemanticCityObjs())
                {
                    switch (type)
                    {
                        case ColorCodeType.None:
                            semantic.ChangeToDefaultState();
                            break;
                        case ColorCodeType.Height:
                            ColorByHeight(colorTable, semantic);
                            break;
                        case ColorCodeType.FloodingRank:
                            ColorByFloodingRank(colorTable, floodingTitle, semantic);
                            break;
                        default:
                            throw new ArgumentException();
                    }
                }

            }
        }
        
        
        private void ColorByHeight(Color[] colorTable, SemanticCityObject semantic)
        {
            Assert.AreEqual(6, colorTable.Length, "高さの色分けは6色");

            if (!semantic.Attribute.MeasuredHeight.HasValue)
            {
                semantic.ChangeToDefaultState();
                return;
            }

            var height = semantic.Attribute.MeasuredHeight.Value;
            switch (height)
            {
                case <= 12:
                    semantic.SetMaterialColor(colorTable[0]);
                    break;
                case > 12 and <= 31:
                    semantic.SetMaterialColor(colorTable[1]);
                    break;
                case > 31 and <= 60:
                    semantic.SetMaterialColor(colorTable[2]);
                    break;
                case > 60 and <= 120:
                    semantic.SetMaterialColor(colorTable[3]);
                    break;
                case > 120 and <= 180:
                    semantic.SetMaterialColor(colorTable[4]);
                    break;
                default:
                    semantic.SetMaterialColor(colorTable[5]);
                    break;
            }
        }
        
        private void ColorByFloodingRank(Color[] colorTable, FloodingTitle floodingTitle, SemanticCityObject semantic)
        {
            Assert.AreEqual(5, colorTable.Length, "ランクの色分けは5色");

            var info = semantic.Attribute.GetFloodingAreaInfoByTitle(floodingTitle);
            if(info == null)
            {
                semantic.ChangeToDefaultState();
                return;
            }

            foreach (var featureObj in semantic.FeatureGameObjs())
            {
                featureObj.Filter.SetFloodingFilter(true);
                featureObj.ApplyFilter();
            }
            
            var rank = info.Rank;
            int colorTableIndex = rank.Rank - 1;
            if (colorTableIndex < colorTable.Length)
            {
                // 色つけ
                semantic.SetMaterialColor(colorTable[colorTableIndex]);
                
                // fldをランクに応じて高さを変える
                if (semantic.IsFlooding)
                {
                    foreach (var feature in semantic.FeatureGameObjs())
                    {
                        var trans = feature.GameObj.transform;
                        trans.position = Vector3.up * rank.Height * FloodingHeightMultiplier;
                    }
                }
            }
            else
            {
                semantic.ChangeToDefaultState();
            }
            
        }
        
    }
}
