using UnityEngine;

namespace PLATEAU.Samples
{
    /// <summary>
    /// カメラ制御クラス
    /// 
    /// 車のカメラ用リグに追従します。
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField, Tooltip("カメラ位置ターゲット")] Transform positionTarget;
        [SerializeField, Tooltip("注視点ターゲット")] Transform lookAtTarget;

        [SerializeField, Tooltip("追従のスムージングパラメータ")] float smoothing = 6.0f;

        private void FixedUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, positionTarget.position, Time.deltaTime * smoothing);        
            transform.LookAt(lookAtTarget);
        }
    }
}
