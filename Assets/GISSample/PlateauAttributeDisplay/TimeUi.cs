using System.Collections;
using System.Collections.Generic;
using PlateauToolkit.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class TimeUi : MonoBehaviour
{
    private VisualElement timeHeader;
    private Slider timeSlider;
    private EnvironmentController environmentController;
    private void Start()
    {
        var rootUi = GetComponent<UIDocument>().rootVisualElement;
        timeSlider = rootUi.Q<Slider>("time-slider");
        timeSlider.RegisterValueChangedCallback(OnTimeChanged);
        environmentController = FindObjectOfType<EnvironmentController>();
        timeHeader = rootUi.Q("time-header");
    }

    private void OnTimeChanged(ChangeEvent<float> evt)
    {
        environmentController.TimeOfDay = evt.newValue;
    }

    public bool IsMouseInWindow(Vector2 mousePos)
    {
        return timeHeader.worldBound.Contains(mousePos);
    }
}
