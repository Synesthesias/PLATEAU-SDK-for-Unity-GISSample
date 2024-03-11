using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.UI
{
    /// <summary>
    /// カメラの位置を覚えておいて復元します。
    /// </summary>
    public class CameraPositionMemory
    {
        private Camera camera;
        private Vector3 position;
        private Quaternion rotation;
        
        public bool IsPositionSaved { get; private set; }
        
        public CameraPositionMemory(Camera camera)
        {
            this.camera = camera;
        }
        
        public void Save()
        {
            IsPositionSaved = true;
            var trans = camera.transform;
            position = trans.position;
            rotation = trans.rotation;
        }

        public void Restore()
        {
            camera.transform.SetPositionAndRotation(position, rotation);
        }
    }
}
