using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PLATEAU.Samples
{
    /// <summary>
    /// 車制御クラス
    /// </summary>
    public class CarController : MonoBehaviour, GameSampleInputActions.ICarActions
    {
        [SerializeField, Tooltip("最大ステアリング角度(degree)")] float maxAngle = 30.0f;
        [SerializeField, Tooltip("ステアリング角速度(degree/sec)")] float angleSpeed = 30.0f;
        [SerializeField, Tooltip("最大トルク")] float maxTorque = 300.0f;
        [SerializeField, Tooltip("ブレーキトルク")] float brakeTorque = 30000.0f;
        [SerializeField, Tooltip("ホイールプレハブ")] GameObject wheelPrefab;

        [SerializeField, Tooltip("サブステップの速度しきい値")] float criticalSpeed = 5.0f;
        [SerializeField, Tooltip("速度がcriticalSpeed以下の時のサブステップ量")] int stepsBelow = 5;
        [SerializeField, Tooltip("速度がcriticalSpeed以上の時のサブステップ量")] int stepsAbove = 1;

        [SerializeField, Tooltip("エンジン音の最小ピッチ")] float minSePitch = 0.2f;
        [SerializeField, Tooltip("エンジン音の最大ピッチ")] float maxSePitch = 1.0f;
        [SerializeField, Tooltip("エンジン音が最大ピッチになる時のホイールのRPM")] float maxRpmSe = 1.0f;

        [SerializeField, Tooltip("車本体のレンダラー")] Renderer carRenderer;
        [SerializeField, ColorUsage(false, true), Tooltip("車のバックライト点灯時のEmissionColor")] Color backlightEmissionColor;

        /// <summary>
        /// GameSample用InputAction
        /// </summary>
        private GameSampleInputActions inputActions;

        /// <summary>
        /// WheelCollider
        /// </summary>
        private WheelCollider[] wheelColliders;

        /// <summary>
        /// エンジン音のオーディオソース
        /// </summary>
        private AudioSource audioSource;

        /// <summary>
        /// ブレーキランプマテリアル
        /// </summary>
        private Material backlightMaterial;

        /// <summary>
        /// ステアリング角度
        /// </summary>
        private float angle;

        /// <summary>
        /// WheelColliderのトルク
        /// </summary>
        private float torque;

        /// <summary>
        /// WheelColliderのブレーキの強さ
        /// </summary>
        private float brake;

        private void Awake()
        {
            inputActions = new GameSampleInputActions();
            inputActions.Car.SetCallbacks(this);
        }

        private void Start()
        {
            wheelColliders = GetComponentsInChildren<WheelCollider>();

            // 各WheelColliderにインスタンス化したタイヤオブジェクトをアタッチします。
            for (int i = 0; i < wheelColliders.Length; ++i)
            {
                var collider = wheelColliders[i];

                if (wheelPrefab != null)
                {
                    var ws = Instantiate(wheelPrefab);
                    ws.transform.parent = collider.transform;
                }
            }

            audioSource = GetComponent<AudioSource>();

            backlightMaterial = carRenderer.materials[4];
        }

        private void OnEnable()
        {
            inputActions.Enable(); 
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Update()
        {
            UpdateWheels();
            UpdateSound();
            UpdateBacklight();
        }

        /// <summary>
        /// WheelColliderの更新処理
        /// </summary>
        private void UpdateWheels()
        {
            wheelColliders[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

            var desired = maxAngle * inputActions.Car.Steer.ReadValue<float>();
            angle = Mathf.MoveTowards(angle, desired, Time.deltaTime * angleSpeed);
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);

            foreach (var wheelCollider in wheelColliders)
            {
                // ステアリング角度を前輪に反映
                if (wheelCollider.transform.localPosition.z > 0)
                {
                    wheelCollider.steerAngle = angle;
                }

                // 後輪にブレーキ反映
                if (wheelCollider.transform.localPosition.z < 0)
                {
                    wheelCollider.brakeTorque = brake;
                }

                // 前輪にトルク反映
                if (wheelCollider.transform.localPosition.z >= 0)
                {
                    wheelCollider.motorTorque = torque;
                }

                if (wheelPrefab)
                {
                    // WheelColliderの状態をタイヤモデルに反映

                    wheelCollider.GetWorldPose(out Vector3 p, out Quaternion q);

                    var shapeTransform = wheelCollider.transform.GetChild(0);

                    if (wheelCollider.name == "a0l" || wheelCollider.name == "a1l" || wheelCollider.name == "a2l")
                    {
                        shapeTransform.SetPositionAndRotation(p, q * Quaternion.Euler(0, 180, 0));
                    }
                    else
                    {
                        shapeTransform.SetPositionAndRotation(p, q);
                    }
                }
            }
        }

        /// <summary>
        /// エンジン音更新処理
        /// 
        /// WheelColliderのrpmに応じてピッチを変えます。
        /// </summary>
        private void UpdateSound()
        {
            var rpm = Mathf.Abs(wheelColliders[0].rpm);
            var ratio = Mathf.Clamp(rpm / maxRpmSe, 0, 1.0f);
            var currentRatio = (audioSource.pitch - minSePitch) / (maxSePitch - minSePitch);
            ratio = Mathf.Lerp(currentRatio, ratio, 0.1f);

            audioSource.pitch = Mathf.Lerp(minSePitch, maxSePitch, ratio);
        }

        /// <summary>
        /// ブレーキランプ更新処理
        /// 
        /// バックの時にブレーキランプを点灯させます。
        /// </summary>
        private void UpdateBacklight()
        {
            if (torque < 0.0f)
            {
                backlightMaterial.SetColor("_EmissionColor", backlightEmissionColor);
            }
            else
            {
                backlightMaterial.SetColor("_EmissionColor", Color.black);
            }
        }

        /// <summary>
        /// アクセル入力イベントハンドラ
        /// </summary>
        /// <param name="context"></param>
        public void OnAccelerate(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                torque = maxTorque * context.ReadValue<float>();
            }
            else if(context.canceled)
            {
                torque = 0.0f;
            }
        }

        /// <summary>
        /// ブレーキ入力イベントハンドラ
        /// </summary>
        /// <param name="context"></param>
        public void OnBrake(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                brake = brakeTorque * context.ReadValue<float>();
            }
            else if (context.canceled)
            {
                brake = 0.0f;
            }
        }

        /// <summary>
        /// ステアリング入力イベントハンドラ
        /// 
        /// ICarActionsの実装のためだけに用意しているので、何もしません。
        /// Steerの入力はUpdateWheels()で常にReadValueしています。
        /// </summary>
        /// <param name="context"></param>
        public void OnSteer(InputAction.CallbackContext context)
        {
        }
    }
}
