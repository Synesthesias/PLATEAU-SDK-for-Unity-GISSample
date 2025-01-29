using System.Collections.Generic;
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
        private readonly RadioButtonGroup radioButtonBuilding;

        /// <summary> 色分け選択ラジオボタン（浸水区域） </summary>
        private readonly RadioButtonGroup radioButtonFld;

        /// <summary> 浸水色分けの選択肢（建物） </summary>
        private readonly FloodingTitleSet floodingTitlesBldg;

        /// <summary> 浸水色分けの選択肢（浸水区域） </summary>
        private readonly FloodingTitleSet floodingTitlesFld;
        
        private readonly ColorChangerByAttribute colorChangerByAttribute;

        public ColorByAttrUi(VisualElement menuRoot, FloodingTitleSet floodingTitlesBldg, FloodingTitleSet floodingTitlesFld, ColorChangerByAttribute colorChangerByAttribute)
        {
            this.floodingTitlesBldg = floodingTitlesBldg;
            this.floodingTitlesFld = floodingTitlesFld;
            this.colorChangerByAttribute = colorChangerByAttribute;
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
            FloodingTitle selectedFloodingTitleBldg;
            BuildingColorType selectedBuildingColorType;
            // valueは
            // 0: 色分けなし
            // 1: 高さ
            // 2～: 浸水ランク
            if (e.newValue < 0)
            {
                return;
            }
            if (e.newValue < 2)
            {
                selectedBuildingColorType = (BuildingColorType)e.newValue;
                selectedFloodingTitleBldg = null;
            }
            else
            {
                selectedBuildingColorType = BuildingColorType.FloodingRank;
                selectedFloodingTitleBldg = floodingTitlesBldg.GetByTitleString(radioButtonBuilding.choices.ElementAt(e.newValue));
            }

            colorChangerByAttribute.ChangeBuildings(selectedBuildingColorType, selectedFloodingTitleBldg);
        }

        private void OnRadioButtonFldChanged(ChangeEvent<int> e)
        {
            FloodingTitle selectedFloodingTitleFld;
            // valueは
            // 0: 表示なし
            // 1～: 浸水ランク
            if (e.newValue <= 0)
            {
                selectedFloodingTitleFld = null;
            }
            else
            {
                selectedFloodingTitleFld =
                    floodingTitlesFld.GetByTitleString(radioButtonFld.choices.ElementAt(e.newValue));
            }
            colorChangerByAttribute.ChangeFlooding(selectedFloodingTitleFld);
        }
        
    }
}