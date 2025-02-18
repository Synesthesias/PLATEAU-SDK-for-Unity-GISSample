using UnityEngine.InputSystem;
using UnityEngine;
using Cinemachine;

namespace GISSample.PlateauAttributeDisplay
{
    public class WalkerMoveByUserInput : GISSampleInputActions.IWalkerMoveActions
    {
        private readonly CinemachineVirtualCamera camera;
        private readonly GameObject mainCam;
        public static bool IsActive = false;
        private readonly GameObject walker;
        private GISSampleInputActions.WalkerMoveActions input;
        private Vector2 deltaWASD;
        private float deltaUpDown;
        private readonly CustomCinemachineInputProvider inputProviderComponent;
        private CameraMoveData cameraMoveSpeedData;

        public Vector2 DeltaWASD { get => deltaWASD; set => deltaWASD = value; }
        public float DeltaUpDown { get => deltaUpDown; set => deltaUpDown = value; }
        public float WalkerMoveSpeedMultiplier { get; set; } = 1.0f;
        public float CameraOffsetY
        {
            get
            {
                var transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
                return transposer.m_FollowOffset.y;
            }
        }

        public WalkerMoveByUserInput(CinemachineVirtualCamera camera, GameObject walker)
        {
            this.camera = camera;
            this.walker = walker;
            mainCam = Camera.main.gameObject;
            inputProviderComponent = camera.GetComponent<CustomCinemachineInputProvider>();
            inputProviderComponent.enabled = false;
        }
        public void OnEnable()
        {
            input = new GISSampleInputActions.WalkerMoveActions(new GISSampleInputActions());
            input.SetCallbacks(this);
            input.Enable();
        }

        public void OnDisable()
        {
            input.Disable();
        }

        public void Start()
        {
            cameraMoveSpeedData = Resources.Load<CameraMoveData>("CameraMoveSpeedData");
            inputProviderComponent.SensitivityMultiplier = cameraMoveSpeedData.walkerRotateScale;
        }

        public void Update(float deltaTime)
        {
            if (IsActive)
            {
                Debug.Log($"deltaWASD: {deltaWASD}, deltaUpDown: {deltaUpDown}");
                var transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
                walker.GetComponent<CharacterController>().Move(9.8f * deltaTime * Vector3.down);
                MoveUpDown(cameraMoveSpeedData.walkerOffsetYSpeed * deltaUpDown * deltaTime, transposer);
                MoveWASD(WalkerMoveSpeedMultiplier * cameraMoveSpeedData.walkerMoveSpeed * deltaTime * deltaWASD);
            }
        }

        /// <summary>
        /// InputActionsからカメラのWASD移動のキーボード操作を受け取り、歩行者カメラを移動します。
        /// </summary>
        /// <param name="context"></param>
        public void OnWASD(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                var delta = context.ReadValue<Vector2>();
                deltaWASD = delta;
            }
            else if (context.canceled)
            {
                deltaWASD = Vector2.zero;
            }
        }

        /// <summary>
        /// InputActionsからカメラのEQキーボード入力を受け取り、歩行者カメラの高度を変更します。
        /// </summary>
        /// <param name="context"></param>
        public void OnUpDown(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                var delta = context.ReadValue<float>();
                deltaUpDown = delta;
            }
            else if (context.canceled)
            {
                deltaUpDown = 0f;
            }
        }

        /// <summary>
        /// InputActionsから右クリックを受け取り、右クリックされているときのみ歩行者カメラを回転できるようにする。
        /// </summary>
        /// <param name="context"></param>
        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                inputProviderComponent.enabled = true;
            }
            else if (context.canceled)
            {
                inputProviderComponent.enabled = false;
            }
        }

        /// <summary>
        /// 歩行者カメラ移動
        /// </summary>
        /// <param name="moveDelta"></param>
        public void MoveWASD(Vector2 moveDelta)
        {
            var dir = new Vector3(moveDelta.x, 0.0f, moveDelta.y);
            var rot = mainCam.transform.eulerAngles;
            dir = Quaternion.Euler(new Vector3(0.0f, rot.y, rot.z)) * dir;
            walker.GetComponent<CharacterController>().Move(dir);
        }

        /// <summary>
        /// 歩行者カメラ高さ変更
        /// </summary>
        /// <param name="moveDelta"></param>
        /// <param name="trans"></param>
        private void MoveUpDown(float moveDelta, CinemachineTransposer trans)
        {
            if (trans != null)
            {
                Vector3 currentOffset = trans.m_FollowOffset;
                currentOffset.y += moveDelta;
                if (currentOffset.y < 0.5f)
                {
                    ResetOffsetY(trans);
                }
                else
                {
                    trans.m_FollowOffset = currentOffset;
                }
            }
            else
            {
                Debug.LogError("CinemachineTransposer component not found");
            }
        }

        /// <summary>
        /// 歩行者カメラ高さリセット
        /// </summary>
        /// <param name="trans"></param>
        private void ResetOffsetY(CinemachineTransposer trans)
        {
            if (trans != null)
            {
                Vector3 currentOffset = trans.m_FollowOffset;
                currentOffset.y = 0.5f;
                trans.m_FollowOffset = currentOffset;
            }
            else
            {
                Debug.LogError("CinemachineTransposer component not found");
            }
        }
    }
}
