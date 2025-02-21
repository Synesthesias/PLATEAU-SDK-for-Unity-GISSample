using PlateauToolkit.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
    public class TimeUi : MonoBehaviour
    {
        private Slider timeSlider;
        private EnvironmentController environmentController;

		private ResolutionMonitor resolutionMonitor;

		public void Init()
        {
            var rootUi = GetComponent<UIDocument>().rootVisualElement;
            timeSlider = rootUi.Q<Slider>("time-slider");
            timeSlider.RegisterValueChangedCallback(OnTimeChanged);
            environmentController = FindObjectOfType<EnvironmentController>();

			resolutionMonitor = transform.parent.GetComponent<ResolutionMonitor>();
			if (resolutionMonitor != null)
			{
				ResolutionChanged(Screen.width, Screen.height);
				resolutionMonitor.OnResolutionChanged += ResolutionChanged;
			}
		}

        private void OnTimeChanged(ChangeEvent<float> evt)
        {
            environmentController.m_TimeOfDay = evt.newValue;
        }
		private void ResolutionChanged(int width, int height)
		{
			var rootUi = GetComponent<UIDocument>().rootVisualElement;
			if (rootUi != null)
			{
				float marginBottom = 10f / height * 100f;
				if (marginBottom < 1000f / 1080f)
				{
					marginBottom = 1000f / 1080f;
				}
				rootUi.style.bottom = new StyleLength(Length.Percent(marginBottom));
			}
		}
	}
}
