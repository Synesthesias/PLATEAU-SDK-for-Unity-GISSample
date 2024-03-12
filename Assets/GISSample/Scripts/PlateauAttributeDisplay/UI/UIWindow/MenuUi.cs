using GISSample.PlateauAttributeDisplay.Gml;
using GISSample.PlateauAttributeDisplay.UI.UIWindow.MenuUiPart;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
    public class MenuUi : MonoBehaviour
    {
        public ColorByAttrUi ColorByAttrUi { get; private set; }
        private UIDocument uiDoc;
    
        public MinMaxSlider heightSlider;
        public MinMaxSlider lodSlider;
        private Label heightValueLabel;
        private Label lodValueLabel;
        public Slider rainSlider;
        public Slider snowSlider;
        public Slider cloudSlider;
        public Slider cloudIntensitySlider;
        private Button floatingTextSwitchButton;
        private CameraPositionMemoryUi cameraPositionMemoryUi;


        public void Init(SceneManager sceneManager, FloodingTitleSet floodingTitlesBldg, FloodingTitleSet floodingTitlesFld)
        {
            
            
            uiDoc = GetComponent<UIDocument>();
            var uiRoot = uiDoc.rootVisualElement;
            ColorByAttrUi = new ColorByAttrUi(uiRoot, floodingTitlesBldg, floodingTitlesFld, sceneManager.ColorChangerByAttribute);
            heightSlider = uiRoot.Q<MinMaxSlider>("HeightSlider");
            lodSlider = uiRoot.Q<MinMaxSlider>("LodSlider");
            heightValueLabel = uiRoot.Q<Label>("HeightValue");
            lodValueLabel = uiRoot.Q<Label>("LodValue");
        
            

            rainSlider = uiRoot.Q<Slider>("RainSlider");
            snowSlider = uiRoot.Q<Slider>("SnowSlider");
            cloudSlider = uiRoot.Q<Slider>("CloudySlider");
            cloudIntensitySlider = uiRoot.Q<Slider>("CloudIntensitySlider");
            floatingTextSwitchButton = uiRoot.Q<Button>("FloatingTextSwitch");
            floatingTextSwitchButton.clicked += sceneManager.FloatingTextList.SwitchIsActive;
            cameraPositionMemoryUi = new CameraPositionMemoryUi(sceneManager.CameraPositionMemory, uiRoot);
        }

        public void Update()
        {
            cameraPositionMemoryUi.Update();
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
