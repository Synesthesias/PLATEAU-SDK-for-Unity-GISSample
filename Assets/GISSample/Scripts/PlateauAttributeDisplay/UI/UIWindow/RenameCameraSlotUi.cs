using GISSample.PlateauAttributeDisplay.UI.UIWindow.MenuUiPart;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
    public class RenameCameraSlotUi : MonoBehaviour
    {
        private int slotId;
        private string prevSlotName;

        private UIDocument uiDoc;
        private Label prevSlotNameLabel;
        private TextField nextSlotNameTextField;
        private Button okButton;
        private Button cancelButton;
        private Label labelWarningNameLength;

        private CameraPositionMemory cameraPositionMemory;
        private CameraPositionMemoryUi cameraPositionMemoryUi;

        private const int MaxNameLength = 10;
        
        public void Init(CameraPositionMemory cameraPositionMemoryArg, CameraPositionMemoryUi cameraPositionMemoryUiArg)
        {
            cameraPositionMemory = cameraPositionMemoryArg;
            uiDoc = GetComponent<UIDocument>();
            var uiRoot = uiDoc.rootVisualElement;
            prevSlotNameLabel = uiRoot.Q<Label>("PrevSlotNameLabel");
            nextSlotNameTextField = uiRoot.Q<TextField>("TextFieldSlotName");
            okButton = uiRoot.Q<Button>("OkButton");
            cancelButton = uiRoot.Q<Button>("CancelButton");
            labelWarningNameLength = uiRoot.Q<Label>("LabelWarningNameLength");

            okButton.clicked += OnClickedOkButton;
            cancelButton.clicked += OnClickedCancelButton;
            nextSlotNameTextField.RegisterValueChangedCallback(OnChangedNameField);

            cameraPositionMemoryUi = cameraPositionMemoryUiArg;
            
            HideWindow();
        }

        public void Open(int slotIdArg, string prevSlotNameArg)
        {
            slotId = slotIdArg;
            prevSlotName = prevSlotNameArg;
            prevSlotNameLabel.text = prevSlotNameArg;
            nextSlotNameTextField.value = prevSlotNameArg;
            ShowWindow();
        }

        private void OnClickedOkButton()
        {
            var prevSlot = cameraPositionMemory.GetSlotData(slotId);
            string nextSlotName = nextSlotNameTextField.value;
            cameraPositionMemory.SetSlotData(slotId, new SlotData(prevSlot.Position, prevSlot.Rotation, prevSlot.IsSaved, nextSlotName));
            
            cameraPositionMemoryUi.UpdateButtonState();
            HideWindow();
        }

        private void OnClickedCancelButton()
        {
            HideWindow();
        }

        private void OnChangedNameField(ChangeEvent<string> e)
        {
            // 文字数制限の警告を表示
            int length = e.newValue.Length;
            if (length > MaxNameLength)
            {
                labelWarningNameLength.style.display = DisplayStyle.Flex;
                labelWarningNameLength.text = $"{MaxNameLength}文字以内で入力してください(現在{length}文字)";
                okButton.SetEnabled(false);
            }
            else
            {
                labelWarningNameLength.style.display = DisplayStyle.None;
                okButton.SetEnabled(true);
            }
        }

        private void ShowWindow()
        {
            uiDoc.rootVisualElement.style.display = DisplayStyle.Flex;
            // 名前入力でカメラが動いてしまうのを防ぐ
            GISCameraMove.IsKeyboardActive = false;
        }

        private void HideWindow()
        {
            uiDoc.rootVisualElement.style.display = DisplayStyle.None;
            GISCameraMove.IsKeyboardActive = true;
        }
    }
    
}