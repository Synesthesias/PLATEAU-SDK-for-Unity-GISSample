using GISSample.PlateauAttributeDisplay.UI.UIWindow;
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
        private readonly EnvironmentController environmentController;

        public WeatherController(MenuUi menuUi)
        {
            menuUi.rainSlider.RegisterValueChangedCallback(OnRainSliderChanged);
            menuUi.snowSlider.RegisterValueChangedCallback(OnSnowSliderChanged);
            menuUi.cloudSlider.RegisterValueChangedCallback(OnCloudySliderChanged);
            menuUi.cloudIntensitySlider.RegisterValueChangedCallback(OnCloudIntensitySliderChanged);
            this.environmentController = Object.FindObjectOfType<EnvironmentController>();
        }
        

        private void OnRainSliderChanged(ChangeEvent<float> e)
        {
            environmentController.m_Rain = e.newValue;
        }

        private void OnSnowSliderChanged(ChangeEvent<float> e)
        {
            environmentController.m_Snow = e.newValue;
        }

        private void OnCloudySliderChanged(ChangeEvent<float> e)
        {
            environmentController.m_Cloud = e.newValue;
        }

        private void OnCloudIntensitySliderChanged(ChangeEvent<float> e)
        {
            environmentController.m_Cloud = e.newValue;
        }
        
    }
}