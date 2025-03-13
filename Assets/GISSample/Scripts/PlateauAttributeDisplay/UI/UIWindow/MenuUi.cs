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
        private Toggle floatingTextSwitchToggle;
        private Toggle textureSwitchToggle;
        public CameraPositionMemoryUi CameraPositionMemoryUi { get; private set; }

		private ResolutionMonitor resolutionMonitor;

		public void Init(SceneManager sceneManager, FloodingTitleSet floodingTitlesBldg, FloodingTitleSet floodingTitlesFld, RenameCameraSlotUi renameCameraSlotUi, CameraPositionMemory cameraPositionMemory)
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
            floatingTextSwitchToggle = uiRoot.Q<Toggle>("FloatingTextSwitchToggle");
            textureSwitchToggle = uiRoot.Q<Toggle>("TextureSwitchToggle");
            floatingTextSwitchToggle.RegisterValueChangedCallback((e) =>
            {
                sceneManager.FloatingTextList.SetActive(e.newValue);
            });
            textureSwitchToggle.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue)
                {
                    sceneManager.TextureSwitcher.SetTextureOn();
                }
                else
                {
                    sceneManager.TextureSwitcher.SetTextureOff();
                }
            });
            CameraPositionMemoryUi = new CameraPositionMemoryUi(cameraPositionMemory, uiRoot, renameCameraSlotUi);
			resolutionMonitor = transform.parent.GetComponent<ResolutionMonitor>();
			if (resolutionMonitor != null)
			{
				ResolutionChanged(Screen.width, Screen.height);
				resolutionMonitor.OnResolutionChanged += ResolutionChanged;
			}
		}

        public void Update()
        {
            CameraPositionMemoryUi.Update();
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

		private void ResolutionChanged(int width, int height)
		{
			if (uiDoc == null)
			{
				uiDoc = GetComponent<UIDocument>();
			}
			VisualElement window = uiDoc.rootVisualElement.Q<VisualElement>("ScrollView");
			if (window != null)
			{
				float bottom = 15f;
				float marginBottom = (10f + bottom) / height * 100f;
				if (marginBottom < (10f + bottom) / 1080f * 100f)
				{
					marginBottom = (10f + bottom) / 1080f * 100f;
				}

				float windowHeight = 98f - marginBottom;
				if (windowHeight > 98f)
				{
					windowHeight = 98f;
				}
				else if (windowHeight < 0)
				{
					windowHeight = 0;
				}
                Debug.Log("windowHeight: " + windowHeight);
				window.style.height = new StyleLength(Length.Percent(windowHeight));
			}
		}
	}
}
