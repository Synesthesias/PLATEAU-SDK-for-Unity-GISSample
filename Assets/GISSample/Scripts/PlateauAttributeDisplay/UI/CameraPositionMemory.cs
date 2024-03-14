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
            LoadPersistenceDataOrDefault();
        }

        
        /// カメラスロットに関して、永続化データがあればそれを利用、なければデフォルト値にする
        public void LoadPersistenceDataOrDefault()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (!SlotData.TryLoadPersistenceData(i, out var slotData))
                {
                    slotData =  new SlotData(false, DefaultSlotName(i));
                }
                else
                {
                    Debug.Log("save loaded");
                }
                data[i] = slotData;
            }
        }

        public static string DefaultSlotName(int slotId) => "スロット" + (slotId + 1);
        
        public void Save(int slotId)
        {
            var trans = camera.transform;
            var slotData = new SlotData(trans.position, trans.rotation, true, GetName(slotId));
            this.data[slotId] = slotData;
            slotData.Persist(slotId);
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
            slotData.Persist(slotId);
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

        /// <summary> スロットデータを永続的に保存します。 </summary>
        public void Persist(int slotId)
        {
            PlayerPrefs.SetFloat(PersistenceKey.PositionX(slotId), Position.x);
            PlayerPrefs.SetFloat(PersistenceKey.PositionY(slotId), Position.y);
            PlayerPrefs.SetFloat(PersistenceKey.PositionZ(slotId), Position.z);
            PlayerPrefs.SetFloat(PersistenceKey.RotationX(slotId), Rotation.x);
            PlayerPrefs.SetFloat(PersistenceKey.RotationY(slotId), Rotation.y);
            PlayerPrefs.SetFloat(PersistenceKey.RotationZ(slotId), Rotation.z);
            PlayerPrefs.SetFloat(PersistenceKey.RotationW(slotId), Rotation.w);
            PlayerPrefs.SetInt(PersistenceKey.IsSaved(slotId), IsSaved ? 1 : 0);
            PlayerPrefs.SetString(PersistenceKey.Name(slotId), Name);
            PlayerPrefs.Save();
            Debug.Log("saved");
        }
        
        /// <summary> 永続化したスロットデータをロードします </summary>
        public static bool TryLoadPersistenceData(int slotId, out SlotData outSlotData)
        {
            if (!PlayerPrefs.HasKey(PersistenceKey.IsSaved(slotId)))
            {
                outSlotData = new SlotData(false, CameraPositionMemory.DefaultSlotName(slotId));
                Debug.Log($"slot{slotId} : no data");
                return false;
            }
            var posX = PlayerPrefs.GetFloat(PersistenceKey.PositionX(slotId));
            var posY = PlayerPrefs.GetFloat(PersistenceKey.PositionY(slotId));
            var posZ = PlayerPrefs.GetFloat(PersistenceKey.PositionZ(slotId));
            var rotX = PlayerPrefs.GetFloat(PersistenceKey.RotationX(slotId));
            var rotY = PlayerPrefs.GetFloat(PersistenceKey.RotationY(slotId));
            var rotZ = PlayerPrefs.GetFloat(PersistenceKey.RotationZ(slotId));
            var rotW = PlayerPrefs.GetFloat(PersistenceKey.RotationW(slotId));
            bool isSaved = PlayerPrefs.GetInt(PersistenceKey.IsSaved(slotId)) != 0;
            string name = PlayerPrefs.GetString(PersistenceKey.Name(slotId));
            outSlotData = new SlotData(
                new Vector3(posX, posY, posZ),
                new Quaternion(rotX, rotY, rotZ, rotW),
                isSaved, name);
            Debug.Log($"slot{slotId} : persistence data found.");
            return true;
        }

        /// <summary> スロットデータを永続化するときのキーです </summary>
        private class PersistenceKey
        {
            private const string KeyBase = "CameraSlot_";
            private static readonly string keyPosition = KeyBase + "Position_";
            private static readonly string keyRotation = KeyBase + "Rotation_";
            private static readonly string keyIsSaved = KeyBase + "IsSaved_";
            private static readonly string keyName = KeyBase + "Name_";

            public static string PositionX(int slotId) => keyPosition + "X" + slotId;
            public static string PositionY(int slotId) => keyPosition + "Y" + slotId;
            public static string PositionZ(int slotId) => keyPosition + "Z" + slotId;
            public static string RotationX(int slotId) => keyRotation + "X" + slotId;
            public static string RotationY(int slotId) => keyRotation + "Y" + slotId;
            public static string RotationZ(int slotId) => keyRotation + "Z" + slotId;
            public static string RotationW(int slotId) => keyRotation + "W" + slotId;
            public static string IsSaved(int slotId) => keyIsSaved + slotId;
            public static string Name(int slotId) => keyName + slotId;
        }
    }
}
