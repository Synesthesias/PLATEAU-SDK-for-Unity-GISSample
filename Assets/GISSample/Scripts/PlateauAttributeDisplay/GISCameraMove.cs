using GISSample.PlateauAttributeDisplay.UI;
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
        private bool isMouseDraggingFromNonUi;

        private Vector2 horizontalMoveByKeyboard;
        private Vector2 verticalMoveByKeyboard;
        private const float MoveSpeedByKeyboard = 400f;
        
        
        /// <summary>
        /// カメラのTransform
        /// </summary>
        private readonly Transform cameraTransform;

        private readonly GisUiController gisUiController;

        public GISCameraMove(GisUiController gisUiController)
        {
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("Main camera is not found.");
                return;
            }
            cameraTransform = mainCam.transform;
            this.gisUiController = gisUiController;
        }
        
        /// <summary>
        /// マウスでのカメラ水平移動
        /// </summary>
        public void OnHorizontalMoveCameraByMouse(InputAction.CallbackContext context)
        {
            
            if (context.performed && isMouseDraggingFromNonUi)
            {
                // 左右同時押下時は上下移動を優先
                if (Mouse.current.rightButton.isPressed) return;

                var delta = context.ReadValue<Vector2>();
                MoveCameraHorizontal(delta);
            }
        }

        /// <summary>
        /// キーボードでのカメラ水平移動
        /// </summary>
        public void OnHorizontalMoveCameraByKeyboard(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                var delta = context.ReadValue<Vector2>();
                horizontalMoveByKeyboard = delta;
            }else if (context.canceled)
            {
                horizontalMoveByKeyboard = Vector2.zero;
            }
        }

        /// <summary>
        /// カメラ水平移動
        /// </summary>
        private void MoveCameraHorizontal(Vector2 delta)
        {
            var dir = new Vector3(delta.x, 0.0f, delta.y);
            var rotY = cameraTransform.eulerAngles.y;
            dir = Quaternion.Euler(new Vector3(0.0f, rotY, 0.0f)) * dir;
            cameraTransform.position -= dir;
        }

        /// <summary>
        /// マウスでのカメラ上下左右移動
        /// </summary>
        public void OnVerticalMoveCameraByMouse(InputAction.CallbackContext context)
        {
            if (context.performed && isMouseDraggingFromNonUi)
            {
                var delta = context.ReadValue<Vector2>();
                MoveCameraVertical(delta * -1);
            }
        }

        /// <summary>
        /// キーボードでのカメラ上下移動
        /// </summary>
        public void OnVerticalMoveCameraByKeyboard(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                var delta = Vector2.up * context.ReadValue<float>();
                verticalMoveByKeyboard = delta;
            }
            else
            {
                verticalMoveByKeyboard = Vector2.zero;
            }
        }

        /// <summary>
        /// カメラ上下左右移動
        /// </summary>
        private void MoveCameraVertical(Vector2 delta)
        {
            var dir = new Vector3(delta.x, delta.y, 0.0f);
            var rotY = cameraTransform.eulerAngles.y;
            dir = Quaternion.Euler(new Vector3(0.0f, rotY, 0.0f)) * dir;
            cameraTransform.position += dir;
        }
        
        public void Update()
        {
            float moveFactor = MoveSpeedByKeyboard * Time.deltaTime;
            MoveCameraHorizontal(moveFactor * horizontalMoveByKeyboard);
            MoveCameraVertical(moveFactor * verticalMoveByKeyboard);
        }

        /// <summary>
        /// カメラ回転
        /// </summary>
        /// <param name="context"></param>
        public void OnRotateCamera(InputAction.CallbackContext context)
        {
            if (context.performed && isMouseDraggingFromNonUi)
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
            if (context.performed && !GisUiController.IsMousePositionInUiRect())
            {
                var delta = context.ReadValue<float>();
                var dir = delta * Vector3.forward;
                dir = cameraTransform.rotation * dir;
                cameraTransform.position += dir;
            }
        }

        /// <summary>
        /// クリックでの選択操作
        /// </summary>
        public void OnSelectObject(InputAction.CallbackContext context)
        {
            gisUiController.OnSelectObject(context);
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
                isMouseDraggingFromNonUi = !GisUiController.IsMousePositionInUiRect();
            }

            if (context.canceled)
            {
                isMouseDraggingFromNonUi = false;
            }
        }
    }
}