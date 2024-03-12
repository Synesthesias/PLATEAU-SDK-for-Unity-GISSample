﻿using System.Collections.Generic;
using System.Linq;
using GISSample.PlateauAttributeDisplay.Gml;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow.MenuUiPart
{
    /// <summary>
    /// PLATEAUモデルをどの属性情報に応じて色分けするか選択するためのUI
    /// </summary>
    public class ColorByAttrUi
    {
        /// <summary> 色分け選択ラジオボタン（建物） </summary>
        private RadioButtonGroup radioButtonBuilding;

        /// <summary> 色分け選択ラジオボタン（浸水区域） </summary>
        private RadioButtonGroup radioButtonFld;
        
        /// <summary> 建築物色分けタイプ </summary>
        public BuildingColorType BuildingColorType { get; private set; }
        
        /// <summary> 選択中の浸水タイトル（建物） </summary>
        public FloodingTitle SelectedFloodingTitleBldg { get; private set; }
        
        /// <summary> 選択中の浸水タイトル（浸水区域） </summary>
        public FloodingTitle SelectedFloodingTitleFld { get; private set; }

        /// <summary> 浸水色分けの選択肢（建物） </summary>
        private FloodingTitleSet floodingTitlesBldg;

        /// <summary> 浸水色分けの選択肢（浸水区域） </summary>
        private FloodingTitleSet floodingTitlesFld;
        
        private readonly ColorChanger colorChanger;

        public ColorByAttrUi(VisualElement menuRoot, FloodingTitleSet floodingTitlesBldg, FloodingTitleSet floodingTitlesFld, ColorChanger colorChanger)
        {
            this.floodingTitlesBldg = floodingTitlesBldg;
            this.floodingTitlesFld = floodingTitlesFld;
            this.colorChanger = colorChanger;
            radioButtonBuilding = menuRoot.Q<RadioButtonGroup>("ColorCodeGroupBuilding");
            radioButtonFld = menuRoot.Q<RadioButtonGroup>("GroupFlooding");
            radioButtonBuilding.RegisterValueChangedCallback(OnRadioButtonBuildingChanged);
            radioButtonFld.RegisterValueChangedCallback(OnRadioButtonFldChanged);
            
            if (this.floodingTitlesBldg.Count > 0)
            {
                AddRadioButtonChoices(radioButtonBuilding, floodingTitlesBldg.TitleStrings);
            }

            if (floodingTitlesFld.Count > 0)
            {
                AddRadioButtonChoices(radioButtonFld, floodingTitlesFld.TitleStrings);
            }
        }

        private static void AddRadioButtonChoices(RadioButtonGroup radioButtonGroup, IEnumerable<string> newChoices)
        {
            var choices = radioButtonGroup.choices.ToList();
            choices.AddRange(newChoices);
            radioButtonGroup.choices = choices;
        }

        /// <summary>
        /// 色分け選択変更イベントコールバック
        /// </summary>
        private void OnRadioButtonBuildingChanged(ChangeEvent<int> e)
        {
            // valueは
            // 0: 色分けなし
            // 1: 高さ
            // 2～: 浸水ランク
            if (e.newValue < 2)
            {
                BuildingColorType = (BuildingColorType)e.newValue;
                SelectedFloodingTitleBldg = null;
            }
            else
            {
                BuildingColorType = BuildingColorType.FloodingRank;
                SelectedFloodingTitleBldg = floodingTitlesBldg.GetByTitleString(radioButtonBuilding.choices.ElementAt(e.newValue));
            }

            ChangeColor();
        }

        private void OnRadioButtonFldChanged(ChangeEvent<int> e)
        {
            // valueは
            // 0: 表示なし
            // 1～: 浸水ランク
            if (e.newValue <= 0)
            {
                SelectedFloodingTitleFld = null;
            }
            else
            {
                SelectedFloodingTitleFld =
                    floodingTitlesFld.GetByTitleString(radioButtonFld.choices.ElementAt(e.newValue));
            }
            ChangeColor();
        }

        public void ChangeColor()
        {
            colorChanger.ChangeColor(BuildingColorType, SelectedFloodingTitleBldg, SelectedFloodingTitleFld);
        }
        
    }
}