using PLATEAU.Samples;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// GIS Sampleのカメラ移動です
    /// </summary>
    public class GISCameraMove : GISSampleInputActions.IGISSampleActions
    {
        /// <summary>
        /// カメラ操作が有効かどうか
        /// ドラッグの起点がUI上の場合はカメラ操作できないようにするための判定用フラグです。
        /// </summary>
        public bool isCameraControllActive = false;
        
        
        /// <summary>
        /// カメラのTransform
        /// </summary>
        private Transform cameraTransform;

        private SceneManager sceneManager;

        public GISCameraMove(SceneManager sceneManager)
        {
            cameraTransform = Camera.main.transform;
            this.sceneManager = sceneManager;
        }
        
        /// <summary>
        /// カメラ水平移動
        /// </summary>
        /// <param name="context"></param>
        public void OnHorizontalMoveCamera(InputAction.CallbackContext context)
        {
            if (context.performed && isCameraControllActive)
            {
                // 左右同時押下時は上下移動を優先
                if (Mouse.current.rightButton.isPressed) return;

                var delta = context.ReadValue<Vector2>();
                var dir = new Vector3(delta.x, 0.0f, delta.y);
                var rotY = cameraTransform.eulerAngles.y;
                dir = Quaternion.Euler(new Vector3(0.0f, rotY, 0.0f)) * dir;
                cameraTransform.position -= dir;
            }
        }

        /// <summary>
        /// カメラ上下移動
        /// </summary>
        /// <param name="context"></param>
        public void OnVerticalMoveCamera(InputAction.CallbackContext context)
        {
            if (context.performed && isCameraControllActive)
            {
                var delta = context.ReadValue<Vector2>();
                var dir = new Vector3(delta.x, delta.y, 0.0f);
                var rotY = cameraTransform.eulerAngles.y;
                dir = Quaternion.Euler(new Vector3(0.0f, rotY, 0.0f)) * dir;
                cameraTransform.position -= dir;
            }
        }

        /// <summary>
        /// カメラ回転
        /// </summary>
        /// <param name="context"></param>
        public void OnRotateCamera(InputAction.CallbackContext context)
        {
            if (context.performed && isCameraControllActive)
            {
                // 左右同時押下時は上下移動を優先
                if (Mouse.current.leftButton.isPressed) return;

                var delta = context.ReadValue<Vector2>();

                var euler = cameraTransform.rotation.eulerAngles;
                euler.x -= delta.y;
                euler.x = Mathf.Clamp(euler.x, 0.0f, 90.0f);
                euler.y += delta.x;
                cameraTransform.rotation = Quaternion.Euler(euler);
            }
        }

        /// <summary>
        /// カメラ前後移動
        /// </summary>
        /// <param name="context"></param>
        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            if (context.performed && !sceneManager.IsMousePositionInUiRect())
            {
                var delta = context.ReadValue<float>();
                var dir = delta * Vector3.forward;
                dir = cameraTransform.rotation * dir;
                cameraTransform.position += dir;
            }
        }

        public void OnSelectObject(InputAction.CallbackContext context)
        {
            sceneManager.OnSelectObject(context);
        }

        /// <summary>
        /// マウスクリックイベントコールバック
        /// ドラッグの起点がUI上の場合カメラ操作させないようにするため、
        /// このタイミングで判定しています。
        /// </summary>
        /// <param name="context"></param>
        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isCameraControllActive = !sceneManager.IsMousePositionInUiRect();
            }

            if (context.canceled)
            {
                isCameraControllActive = false;
            }
        }
        
        
    }
}