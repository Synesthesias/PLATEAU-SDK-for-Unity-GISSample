using System.Collections.Generic;
using System.Linq;
using GISSample.PlateauAttributeDisplay.Gml;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
    public class MenuUi : MonoBehaviour
    {
        private HashSet<string> FloodingAreaNames { get; set; }
        private GisUiController gisUiController;
        private UIDocument uiDoc;
    
        public MinMaxSlider heightSlider;
        public MinMaxSlider lodSlider;
        private Label heightValueLabel;
        private Label lodValueLabel;
        public Slider rainSlider;
        public Slider snowSlider;
        public Slider cloudSlider;
        public Slider cloudIntensitySlider;

        public VisualElement RootVisualElement => uiDoc.rootVisualElement;
    
        /// <summary>
        /// 色分けグループ
        /// </summary>
        public RadioButtonGroup colorCodeGroup;

        public void Init(GisUiController gisUiControllerArg, SceneManager sceneManager, HashSet<string> floodingAreaNames)
        {
            FloodingAreaNames = floodingAreaNames;
            uiDoc = GetComponent<UIDocument>();
            gisUiController = gisUiControllerArg;
            var menuRoot = uiDoc.rootVisualElement;
            colorCodeGroup = menuRoot.Q<RadioButtonGroup>("ColorCodeGroup");
            colorCodeGroup.RegisterValueChangedCallback(gisUiControllerArg.OnColorCodeGroupValueChanged);

            var uiRoot = uiDoc.rootVisualElement;
            heightSlider = uiRoot.Q<MinMaxSlider>("HeightSlider");
            lodSlider = uiRoot.Q<MinMaxSlider>("LodSlider");
            heightValueLabel = uiRoot.Q<Label>("HeightValue");
            lodValueLabel = uiRoot.Q<Label>("LodValue");
        
            if (FloodingAreaNames.Count > 0)
            {
                var choices = colorCodeGroup.choices.ToList();
                choices.AddRange(FloodingAreaNames);
                colorCodeGroup.choices = choices;
            }

            rainSlider = uiRoot.Q<Slider>("RainSlider");
            snowSlider = uiRoot.Q<Slider>("SnowSlider");
            cloudSlider = uiRoot.Q<Slider>("CloudySlider");
            cloudIntensitySlider = uiRoot.Q<Slider>("CloudIntensitySlider");
        }
    
        /// <summary>
        /// フィルターのテキストを更新
        /// </summary>
        /// <param name="parameter"></param>
        public void UpdateFilterText(FilterParameter parameter)
        {
            heightValueLabel.text = $"{parameter.MinHeight:F1} to {parameter.MaxHeight:F1}";
            lodValueLabel.text = $"{parameter.MinLod:D} to {parameter.MaxLod:D}";
        }
    
        public void RegisterHeightSliderChangedCallback(EventCallback<ChangeEvent<Vector2>> callback)
        {
            heightSlider.RegisterValueChangedCallback(callback);
        }

        public void RegisterLodSliderChangedCallback(EventCallback<ChangeEvent<Vector2>> callback)
        {
            lodSlider.RegisterValueChangedCallback(callback);
        }
    }
}
