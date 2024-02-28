using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay
{
    public class UserGuideUi : MonoBehaviour
    {
        private UIDocument uiDoc;
        private bool isWindowBodyOpen;
        private VisualElement windowBody;
        private Button openCloseButton;
        
        private void Start()
        {
            uiDoc = GetComponent<UIDocument>();
            windowBody = uiDoc.rootVisualElement.Q("guide-body");
            openCloseButton = uiDoc.rootVisualElement.Q<Button>("guide-open-close-button");
            CloseWindowBody();
            openCloseButton.clicked += OnOpenCloseButtonClick;
            
        }

        private void OnOpenCloseButtonClick()
        {
            if (isWindowBodyOpen)
            {
                CloseWindowBody();
            }
            else
            {
                OpenWindowBody();
            }
        }

        private void OpenWindowBody()
        {
            isWindowBodyOpen = true;
            windowBody.style.display = DisplayStyle.Flex;
            openCloseButton.text = "閉じる";
        }

        private void CloseWindowBody()
        {
            isWindowBodyOpen = false;
            windowBody.style.display = DisplayStyle.None;
            openCloseButton.text = "開く";
        }
    }
}