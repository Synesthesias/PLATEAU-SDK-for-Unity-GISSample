using PlateauToolkit.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
    public class TimeUi : MonoBehaviour
    {
        private Slider timeSlider;
        private EnvironmentController environmentController;

        public void Init()
        {
            var rootUi = GetComponent<UIDocument>().rootVisualElement;
            timeSlider = rootUi.Q<Slider>("time-slider");
            timeSlider.RegisterValueChangedCallback(OnTimeChanged);
            environmentController = FindObjectOfType<EnvironmentController>();
        }

        private void OnTimeChanged(ChangeEvent<float> evt)
        {
            environmentController.m_TimeOfDay = evt.newValue;
        }
    }
}
