using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
    public class WalkControllUI : MonoBehaviour
    {
        private UIDocument uiDoc;
        private VisualElement windowBody;
        private Dictionary<string, Button> walkControlButtons;
        private Dictionary<string, bool> walkControlStates;
        private Label walkControllerHeightLabe;

        public string WalkControllerHeightText
        {
            get => walkControllerHeightLabe.text;
            set => walkControllerHeightLabe.text = value;
        }

        public event Action<string> OnWalkControlPressing;
        public event Action OnWalkControlUp;
        public event Action OnWalkModeQuitClicked;
        public event Action<float> OnWalkSpeedChanged;

        private void Start()
        {
            uiDoc = GetComponent<UIDocument>();
            windowBody = uiDoc.rootVisualElement.Query();

            walkControlButtons = new Dictionary<string, Button>
            {
                { "W", windowBody.Q<Button>("WButton") },
                { "A", windowBody.Q<Button>("AButton") },
                { "S", windowBody.Q<Button>("SButton") },
                { "D", windowBody.Q<Button>("DButton") },
                { "Q", windowBody.Q<Button>("QButton") },
                { "E", windowBody.Q<Button>("EButton") }
            };

            walkControlStates = new Dictionary<string, bool>();
            foreach (var key in walkControlButtons.Keys)
            {
                walkControlStates[key] = false;
            }

            foreach (var kvp in walkControlButtons)
            {
                string controlName = kvp.Key;
                Button button = kvp.Value;

                button.RegisterCallback<PointerDownEvent>(ev =>
                {
                    walkControlStates[controlName] = true;
                    OnWalkControlPressing?.Invoke(controlName);
                }, TrickleDown.TrickleDown);
                button.RegisterCallback<PointerUpEvent>(ev =>
                {
                    walkControlStates[controlName] = false;
                    OnWalkControlUp?.Invoke();
                });
                button.RegisterCallback<PointerLeaveEvent>(ev =>
                {
                    walkControlStates[controlName] = false;
                    OnWalkControlUp?.Invoke();
                });
            }

            var exitButton = windowBody.Q<Button>("ExitButton");
            if (exitButton != null)
            {
                exitButton.clicked += () =>
                {
                    OnWalkModeQuitClicked?.Invoke();
                };
            }

            var speedx1 = windowBody.Q<RadioButton>("Speedx1");
            var speedx2 = windowBody.Q<RadioButton>("Speedx2");
            var speedx3 = windowBody.Q<RadioButton>("Speedx3");

            if (speedx1 != null) speedx1.value = true;
            if (speedx2 != null) speedx2.value = false;
            if (speedx3 != null) speedx3.value = false;

            speedx1?.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue)
                    {
                        if (speedx2 != null) speedx2.value = false;
                        if (speedx3 != null) speedx3.value = false;
                        OnWalkSpeedChanged?.Invoke(1.0f);
                    }
                });
            speedx2?.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue)
                    {
                        if (speedx1 != null) speedx1.value = false;
                        if (speedx3 != null) speedx3.value = false;
                        OnWalkSpeedChanged?.Invoke(2.0f);
                    }
                });
            speedx3?.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue)
                    {
                        if (speedx1 != null) speedx1.value = false;
                        if (speedx2 != null) speedx2.value = false;
                        OnWalkSpeedChanged?.Invoke(3.0f);
                    }
                });

            walkControllerHeightLabe = windowBody.Q<Label>("HeightText");

            CloseWindowBody();
        }

        public void OpenWindowBody()
        {
            windowBody.style.display = DisplayStyle.Flex;
        }

        public void CloseWindowBody()
        {
            windowBody.style.display = DisplayStyle.None;
        }

        private void Update()
        {
            foreach (var kvp in walkControlStates)
            {
                if (kvp.Value)
                {
                    OnWalkControlPressing?.Invoke(kvp.Key);
                }
            }
        }
    }
}