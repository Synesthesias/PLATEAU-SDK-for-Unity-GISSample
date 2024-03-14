using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow.MenuUiPart
{
    /// <summary>
    /// カメラ位置の記憶と復元に関するボタンです。
    /// ボタン押下時のメイン処理は <see cref="CameraPositionMemory"/>に委譲します。
    /// </summary>
    public class CameraPositionMemoryUi
    {
        private readonly CameraPositionMemory cameraPositionMemory;
        private readonly ButtonWithClickMessage[] saveButtons; // 添字はスロットID
        private readonly ButtonWithClickMessage[] restoreButtons; // 添字はスロットID
        private readonly Button saveRestButton;
        private float timeToResetSaveButton;
        private float timeToResetRestoreButton;
        private readonly RenameCameraSlotUi renameCameraSlotUi;

        private const string UiNameCameraRoot = "CameraPosition";
        private const string UiNameCameraSaveButton = "CameraSaveSlot";
        private const string UiNameCameraRestoreButton = "CameraRestoreSlot";
        private const string UiNameSlotRenameButton = "CameraRenameSlot";
        private const string UiNameResetSaveButton = "ResetCameraSaveButton";
        
        public CameraPositionMemoryUi(CameraPositionMemory cameraPositionMemory, VisualElement menuUiRoot, RenameCameraSlotUi renameCameraSlotUi)
        {
            this.cameraPositionMemory = cameraPositionMemory;
            this.renameCameraSlotUi = renameCameraSlotUi;

            int slotCount = CameraPositionMemory.SlotCount;
            saveButtons = new ButtonWithClickMessage[slotCount];
            restoreButtons = new ButtonWithClickMessage[slotCount];
            var cameraUiRoot = menuUiRoot.Q(UiNameCameraRoot);
            var cameraTab = new TabUi(cameraUiRoot);

            // 保存ボタンの機能を構築
            for (int i = 0; i < slotCount; i++)
            {
                string saveButtonName = UiNameCameraSaveButton + (i+1);
                int slotIndex = i;
                var buttonUi = cameraUiRoot.Q<Button>(saveButtonName);
                saveButtons[i] = new ButtonWithClickMessage(
                    buttonUi,
                    "保存しました！",
                    () => OnClickedSaveButton(slotIndex) // ここに i を渡してはいけないことに注意
                );
                
            }

            // 復元ボタンの機能を構築
            for (int i = 0; i < slotCount; i++)
            {
                string restoreButtonName = UiNameCameraRestoreButton + (i+1);
                int slotIndex = i;
                var buttonUi = cameraUiRoot.Q<Button>(restoreButtonName);
                restoreButtons[i] = new ButtonWithClickMessage(
                    buttonUi,
                    "復元しました！",
                    () => OnClickedRestoreButton(slotIndex)
                );
            }
            
            // 名前変更ボタンの機能を構築
            for (int i = 0; i < slotCount; i++)
            {
                string renameButtonName = UiNameSlotRenameButton + (i + 1);
                int slotIndex = i;
                var buttonUis = cameraUiRoot.Query<Button>(renameButtonName);
                buttonUis.ForEach(button =>
                {
                    button.clicked += () => OnClickedRenameButton(slotIndex);
                });
            }

            saveRestButton = menuUiRoot.Q<Button>(UiNameResetSaveButton);
            saveRestButton.clicked += OnClickedResetSaveButton; 
            
            UpdateButtonState();
        }

        public void Update()
        {
            foreach (var b in saveButtons)
            {
                b.Update();
            }

            foreach (var b in restoreButtons)
            {
                b.Update();
            }
        }

        /// <summary>
        /// 「カメラ位置を記憶」ボタンが押された時、記憶してボタンのテキストを変える
        /// </summary>
        private void OnClickedSaveButton(int slotId)
        {
            cameraPositionMemory.Save(slotId);
            UpdateButtonState();
        }

        /// <summary>
        /// 「カメラ位置を復元」ボタンが押された時、復元してボタンのテキストを変える
        /// </summary>
        private void OnClickedRestoreButton(int slotId)
        {
            cameraPositionMemory.Restore(slotId);
            UpdateButtonState();
        }

        private void OnClickedRenameButton(int slotId)
        {
            renameCameraSlotUi.Open(slotId, cameraPositionMemory.GetName(slotId));
        }

        /// <summary>
        /// 保存データがあるかどうかによってボタンのテキストとスタイルを切り替えます
        /// </summary>
        public void UpdateButtonState()
        {
            int slotCount = CameraPositionMemory.SlotCount;
            
            // 保存ボタンのテキスト変更
            for (int i = 0; i < slotCount; i++)
            {
                string text = cameraPositionMemory.GetName(i);
                if (cameraPositionMemory.IsSaved(i))
                {
                    text += "(上書き)";
                }
                else
                {
                    text += "(新規)";
                }

                saveButtons[i].NormalButtonText = text;
            }
            
            // 復元ボタンのテキスト変更
            for (int i = 0; i < slotCount; i++)
            {
                string text = cameraPositionMemory.GetName(i);
                var button = restoreButtons[i];
                var buttonUi = button.Button;
                if (cameraPositionMemory.IsSaved(i))
                {
                    buttonUi.SetEnabled(true);
                }
                else
                {
                    text += "(未保存)";
                    buttonUi.SetEnabled(false);
                }
                button.NormalButtonText = text;
            }
        }

        /// <summary>
        /// 「カメラ保存をリセット」ボタンが押された時
        /// </summary>
        private void OnClickedResetSaveButton()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            cameraPositionMemory.LoadPersistenceDataOrDefault();
            UpdateButtonState();
        }
    }
}
