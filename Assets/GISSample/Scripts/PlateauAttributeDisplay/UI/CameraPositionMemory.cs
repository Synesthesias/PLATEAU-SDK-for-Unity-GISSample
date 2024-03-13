using UnityEngine;

namespace GISSample.PlateauAttributeDisplay.UI
{
    /// <summary>
    /// カメラの位置を覚えておいて復元します。
    /// UIは<see cref="CameraPositionMemoryUi"/>が担当します。
    /// </summary>
    public class CameraPositionMemory
    {
        private Camera camera;
        public const int SlotCount = 3;
        private SlotData[] data;
        
        
        public CameraPositionMemory(Camera camera)
        {
            this.camera = camera;
            this.data = new SlotData[SlotCount];
        }
        
        public void Save(int slotId)
        {
            var trans = camera.transform;
            this.data[slotId] = new SlotData(trans.position, trans.rotation, true);
        }
        
        public void Restore(int slotId)
        {
            var slotData = data[slotId];
            camera.transform.SetPositionAndRotation(slotData.Position, slotData.Rotation);
        }

        public bool IsSaved(int slotId)
        {
            return data[slotId].IsSaved;
        }
    }

    public struct SlotData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsSaved;

        public SlotData(Vector3 position, Quaternion rotation, bool isSaved)
        {
            Position = position;
            Rotation = rotation;
            IsSaved = isSaved;
        }
    }
}
