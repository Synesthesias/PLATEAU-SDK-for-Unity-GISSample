using System;
using GISSample.PlateauAttributeDisplay.Gml;
using UnityEngine;
using UnityEngine.Assertions;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// 高さや水害情報に応じて色分けします。
    /// </summary>
    public class ColorChangerByAttribute
    {
        private readonly SceneManager sceneManager;
        private const float FloodingHeightMultiplier = 1f;

        /// <summary> 選択中の建物色分けタイプ </summary>
        private BuildingColorType selectedBuildingColorType;
        
        /// <summary> 選択中の浸水タイトル（建物） </summary>
        private FloodingTitle selectedFloodingTitleBldg;

        /// <summary> 選択中の浸水タイトル（浸水区域） 「表示なし」が選択されている時はnull</summary>
        private FloodingTitle selectedFloodingTitleFld;
        
        public ColorChangerByAttribute(SceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
        }

        /// <summary> 色分けなしの状態にします。 </summary>
        public void ChangeToDefault()
        {
            ChangeFlooding(null);
            ChangeBuildings(BuildingColorType.None, null);
        }

        /// <summary> 現在選択中の色で色分けをやり直します。 </summary>
        public void Redraw()
        {
            ChangeBuildings(selectedBuildingColorType, selectedFloodingTitleBldg);
            ChangeFlooding(selectedFloodingTitleFld);
        }

        
        public void ChangeFlooding(FloodingTitle floodingTitleFld)
        {
            selectedFloodingTitleFld = floodingTitleFld;
            var floodingRankColorTable = sceneManager.GisUiController.floodingRankColorTable;

            foreach (var gml in sceneManager.Gmls())
            {
                if (!gml.IsFlooding) continue;
                foreach (var semantic in gml.SemanticCityObjs())
                {
                    if (floodingTitleFld == null)
                    {
                        semantic.ChangeToDefaultState();
                    }
                    else
                    {
                        ColorByFloodingRank(floodingRankColorTable, floodingTitleFld, semantic);
                    }
                }
            }
        } 

        public void ChangeBuildings(BuildingColorType type, FloodingTitle floodingTitleBldg)
        {
            selectedFloodingTitleBldg = floodingTitleBldg;
            selectedBuildingColorType = type;
            var heightColorTable = sceneManager.GisUiController.heightColorTable;
            var floodingRankColorTable = sceneManager.GisUiController.floodingRankColorTable;

            foreach (var gml in sceneManager.Gmls())
            {
                if (gml.IsFlooding) continue;
                Color[] colorTable = type switch
                {
                    BuildingColorType.Height => heightColorTable,
                    BuildingColorType.FloodingRank => floodingRankColorTable,
                    BuildingColorType.None => null,
                    _ => throw new ArgumentOutOfRangeException()
                };

                foreach (var semantic in gml.SemanticCityObjs())
                {
                    switch (type)
                    {
                        case BuildingColorType.None:
                            semantic.ChangeToDefaultState();
                            break;
                        case BuildingColorType.Height:
                            ColorByHeight(colorTable, semantic);
                            break;
                        case BuildingColorType.FloodingRank:
                            ColorByFloodingRank(colorTable, floodingTitleBldg, semantic);
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
