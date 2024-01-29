using PlateauToolkit.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// GISサンプル実行時のメニューパネルから天候変更の操作を受け取り、天候を変更します。
    /// </summary>
    public class WeatherController
    {
        private UIDocument menuUi;
        private EnvironmentController environmentController;

        public WeatherController(UIDocument menuUi)
        {
            this.menuUi = menuUi;
            var menuRoot = menuUi.rootVisualElement;
            menuRoot.Q<Slider>("RainSlider").RegisterValueChangedCallback(OnRainSliderChanged);
            menuRoot.Q<Slider>("SnowSlider").RegisterValueChangedCallback(OnSnowSliderChanged);
            menuRoot.Q<Slider>("CloudySlider").RegisterValueChangedCallback(OnCloudySliderChanged);
            menuRoot.Q<Slider>("CloudIntensitySlider").RegisterValueChangedCallback(OnCloudIntensitySliderChanged);
            this.environmentController = Object.FindObjectOfType<EnvironmentController>();
        }
        

        private void OnRainSliderChanged(ChangeEvent<float> e)
        {
            environmentController.Rain = e.newValue;
        }

        private void OnSnowSliderChanged(ChangeEvent<float> e)
        {
            environmentController.Snow = e.newValue;
        }

        private void OnCloudySliderChanged(ChangeEvent<float> e)
        {
            environmentController.Cloud = e.newValue;
        }

        private void OnCloudIntensitySliderChanged(ChangeEvent<float> e)
        {
            environmentController.CloudIntensity = e.newValue;
        }
        
    }
}