using UnityEngine;

namespace GISSample.Misc
{
    /// <summary>
    /// 常にMainCameraを向くようにします。
    /// </summary>
    public class LookAtMainCamera : MonoBehaviour
    {
        private Camera mainCam;
        private void Start()
        {
            mainCam = Camera.main;
        }

        private void Update()
        {
            transform.LookAt(mainCam.transform.position);
        }
    }
}
