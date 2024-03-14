using Unity.Mathematics;
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
            for (int i = 0; i < SlotCount; i++)
            {
                data[i] = new SlotData(false, "スロット" + (i + 1));
            }
        }
        
        public void Save(int slotId)
        {
            var trans = camera.transform;
            this.data[slotId] = new SlotData(trans.position, trans.rotation, true, GetName(slotId));
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

        public string GetName(int slotId)
        {
            return data[slotId].Name;
        }

        public void SetSlotData(int slotId, SlotData slotData)
        {
            data[slotId] = slotData;
        }

        public SlotData GetSlotData(int slotId)
        {
            return data[slotId];
        }
    }

    public struct SlotData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsSaved;
        public string Name;

        public SlotData(Vector3 position, Quaternion rotation, bool isSaved, string name)
        {
            Position = position;
            Rotation = rotation;
            IsSaved = isSaved;
            Name = name;
        }

        public SlotData(bool isSaved, string name)
            : this(Vector3.zero, Quaternion.identity, isSaved, name)
        {

        }
    }
}
