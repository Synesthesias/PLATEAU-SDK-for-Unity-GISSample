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
        private float timeToResetSaveButton;
        private float timeToResetRestoreButton;
        
        
        public CameraPositionMemoryUi(CameraPositionMemory cameraPositionMemory, VisualElement menuUiRoot)
        {
            this.cameraPositionMemory = cameraPositionMemory;

            int slotCount = CameraPositionMemory.SlotCount;
            saveButtons = new ButtonWithClickMessage[slotCount];
            restoreButtons = new ButtonWithClickMessage[slotCount];
            var cameraUiRoot = menuUiRoot.Q("CameraPosition");
            var cameraTab = new TabUi(cameraUiRoot);

            // 保存ボタンの機能を構築
            for (int i = 0; i < slotCount; i++)
            {
                string saveButtonName = "CameraSaveSlot" + (i+1);
                int slotIndex = i;
                var buttonUi = cameraUiRoot.Q<Button>(saveButtonName);
                saveButtons[i] = new ButtonWithClickMessage(
                    buttonUi,
                    "保存しました！",
                    () => SaveButtonPushed(slotIndex)
                );
                
            }

            // 復元ボタンの機能を構築
            for (int i = 0; i < slotCount; i++)
            {
                string restoreButtonName = "CameraRestoreSlot" + (i+1);
                int slotIndex = i;
                var buttonUi = cameraUiRoot.Q<Button>(restoreButtonName);
                restoreButtons[i] = new ButtonWithClickMessage(
                    buttonUi,
                    "復元しました！",
                    () => RestoreButtonPushed(slotIndex)
                );
            }
            
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
        private void SaveButtonPushed(int slotId)
        {
            cameraPositionMemory.Save(slotId);
            UpdateButtonState();
        }

        /// <summary>
        /// 「カメラ位置を復元」ボタンが押された時、復元してボタンのテキストを変える
        /// </summary>
        private void RestoreButtonPushed(int slotId)
        {
            cameraPositionMemory.Restore(slotId);
            UpdateButtonState();
        }

        /// <summary>
        /// 保存データがあるかどうかによってボタンのテキストとスタイルを切り替えます
        /// </summary>
        private void UpdateButtonState()
        {
            int slotCount = CameraPositionMemory.SlotCount;
            
            // 保存ボタンのテキスト変更
            for (int i = 0; i < slotCount; i++)
            {
                string text = "スロット" + (i+1);
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
                string text = "スロット" + (i + 1);
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
    }
}
