using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow.MenuUiPart
{
    /// <summary>
    /// 「カメラ位置を記憶」「カメラ位置を復元」ボタンに関する処理です。
    /// </summary>
    public class CameraPositionMemoryUi
    {
        private readonly CameraPositionMemory cameraPositionMemory;
        private readonly Button saveButton;
        private readonly Button restoreButton;
        private float timeToResetSaveButton;
        private float timeToResetRestoreButton;
        private readonly string initialSaveButtonText;
        private readonly string initialRestoreButtonText;
        
        /// <summary>
        /// ボタンを押してから、ボタンのテキストを「記憶しました」や「復元しました」に変更する秒数。この秒数の後はもとに戻る。
        /// FIXME: 実際はこの秒数よりも早く切り替わってしまう気がする
        /// </summary>
        private const float TimeToChangeText = 3f;
        
        public CameraPositionMemoryUi(CameraPositionMemory cameraPositionMemory, VisualElement menuUiRoot)
        {
            this.cameraPositionMemory = cameraPositionMemory;
            
            saveButton = menuUiRoot.Q<Button>("CameraPositionSave");
            restoreButton = menuUiRoot.Q<Button>("CameraPositionRestore");
            saveButton.clicked += SaveButtonPushed;
            restoreButton.clicked += RestoreButtonPushed;
            restoreButton.visible = false;
            initialSaveButtonText = saveButton.text;
            initialRestoreButtonText = restoreButton.text;
        }

        public void Update()
        {
            // ボタンを押してテキストが変わった後、一定時間でテキストを戻す
            timeToResetSaveButton -= Time.deltaTime;
            timeToResetRestoreButton -= Time.deltaTime;
            if (timeToResetSaveButton is <= 0f and >= -100f)
            {
                saveButton.text = initialSaveButtonText;
                timeToResetSaveButton = -999f;
            }
            if (timeToResetRestoreButton is <= 0f and >= -100f)
            {
                restoreButton.text = initialRestoreButtonText;
                timeToResetRestoreButton = -999f;
            }
        }

        /// <summary>
        /// 「カメラ位置を記憶」ボタンが押された時、記憶してボタンのテキストを変える
        /// </summary>
        private void SaveButtonPushed()
        {
            cameraPositionMemory.Save();
            saveButton.text = "記憶しました";
            timeToResetSaveButton = TimeToChangeText;
            restoreButton.visible = true;
        }

        /// <summary>
        /// 「カメラ位置を復元」ボタンが押された時、復元してボタンのテキストを変える
        /// </summary>
        private void RestoreButtonPushed()
        {
            cameraPositionMemory.Restore();
            restoreButton.text = "復元しました";
            timeToResetRestoreButton = TimeToChangeText;
        }
        
        
    }
}
