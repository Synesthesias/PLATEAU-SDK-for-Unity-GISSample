using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

namespace GISSample.Misc.CameraPositionMemory
{
    /// <summary>
    /// カメラの位置を記憶、復元するクラス。
    /// UIは<see cref="CameraPositionMemoryUI"/>が担当します。
    /// </summary>
    public class CameraPositionMemory
    {
        private CinemachineVirtualCamera vcam1;
        private CinemachineVirtualCamera vcam2;
        private int slotsCount = 0;
        private List<SlotData> slots;
        private LandscapeCamera landscapeCamera;

        public CameraPositionMemory(CinemachineVirtualCamera vcam1, CinemachineVirtualCamera vcam2, LandscapeCamera landscapeCamera)
        {
            this.vcam1 = vcam1;
            this.vcam2 = vcam2;
            this.slots = new List<SlotData>();
            this.landscapeCamera = landscapeCamera;
        }

        /// <summary>
        /// カメラの保存データを格納したリストを取得する
        /// </summary>
        /// <returns></returns>
        public List<SlotData> GetSlotDatas()
        {
            return slots;
        }


        /// <summary>
        /// カメラの保存データ数を取得する
        /// </summary>
        /// <returns></returns>
        public int GetSlotCount()
        {
            return slotsCount;
        }

        /// <summary>
        /// カメラの保存データの数を増やす
        /// </summary>
        private void AddSlotCount()
        {
            slotsCount++;
        }

        /// <summary>
        /// カメラの保存データの数を減らす
        /// </summary>
        private void SubtractSlotCout()
        {
            slotsCount--;
        }

        /// <summary>
        /// 現在のカメラ位置を保存する関数
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="name"></param>
        public void Save(int slotId, string name)
        {
            var trans = Camera.main.transform;
            var cameraState = landscapeCamera.GetCameraState();

            if (cameraState == GISSampleCameraState.SelectWalkPoint)
            {
                cameraState = GISSampleCameraState.PointOfView;
            }
            var slotData = new SlotData(trans.position, trans.rotation, true, name, cameraState, vcam2.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y);
            slots.Add(slotData);
            AddSlotCount();
            Debug.Log(GetSlotCount());
            //slotData.Persist(slotId);
            Debug.Log($"pos:{trans.position} rot:{trans.rotation.eulerAngles}");
            Debug.Log($"SlotData pos:{slotData.position} rot:{slotData.rotation.eulerAngles}");
        }

        /// <summary>
        /// 保存したカメラ位置を復元する関数
        /// </summary>
        /// <param name="slotId"></param>
        public void Restore(int slotId)
        {
            var slotData = slots[slotId];
            landscapeCamera.SetCameraState(slotData.cameraState);
            if (slotData.cameraState == GISSampleCameraState.PointOfView)
            {
                vcam1.transform.SetPositionAndRotation(slotData.position, slotData.rotation);
            }
            else if (slotData.cameraState == GISSampleCameraState.Walker)
            {
                landscapeCamera.SetWalkerPos(slotData.position);
                vcam2.transform.position = slotData.position;
                var pov = vcam2.GetCinemachineComponent<CinemachinePOV>();
                var vcam1Euler = slotData.rotation.eulerAngles;
                pov.m_VerticalAxis.Value = vcam1Euler.x > 270 ? vcam1Euler.x - 360 : vcam1Euler.x;
                pov.m_HorizontalAxis.Value = vcam1Euler.y;
                CinemachineTransposer trans = vcam2.GetCinemachineComponent<CinemachineTransposer>();
                Vector3 currentOffset = new Vector3(0.0f, slotData.offSetY, 0.0f);
                if (currentOffset.y < 0.5f)
                {
                    currentOffset.y = 0.5f;
                }
                trans.m_FollowOffset = currentOffset;
            }
            Debug.Log($"RestoreSlotData pos:{slotData.position} rot:{slotData.rotation.eulerAngles}");

        }

        /// <summary>
        /// 保存したカメラ位置を削除する関数
        /// </summary>
        /// <param name="slotId"></param>
        public void Delete(int slotId)
        {
            slots.Remove(slots[slotId]);
            SubtractSlotCout();
        }

        /// <summary>
        /// 保存したカメラ位置の名前を取得する関数
        /// </summary>
        /// <param name="slotId"></param>
        /// <returns></returns>
        public string GetName(int slotId)
        {
            return slots[slotId].name;
        }

        /// <summary>
        /// 保存したカメラデータを更新する関数
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="slotData"></param>
        public void SetSlotData(int slotId, SlotData slotData)
        {
            slots[slotId] = slotData;
        }

        /// <summary>
        /// 保存したカメラデータを取得する関数
        /// </summary>
        /// <param name="slotId"></param>
        /// <returns></returns>
        public SlotData GetSlotData(int slotId)
        {
            return slots[slotId];
        }

        /// <summary>
        /// 歩行者視点カメラの高さを設定する関数
        /// </summary>
        /// <param name="y"></param>
        public void SetOffsetY(float y)
        {
            vcam2.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(0, y, 0);
        }

        /// <summary>
        /// 歩行者視点カメラの高さを戻す関数
        /// </summary>
        /// <returns></returns>
        public float GetOffsetY()
        {
            return vcam2.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y;
        }
    }

    /// <summary>
    /// 内部で使われるカメラの位置を保存する構造体
    /// </summary>
    public struct SlotData
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isSaved;
        public string name;
        public GISSampleCameraState cameraState;
        public float offSetY;
        public SlotData(Vector3 position, Quaternion rotation, bool isSaved, string name, GISSampleCameraState cameraState, float offSetY)
        {
            this.position = position;
            this.rotation = rotation;
            this.isSaved = isSaved;
            this.name = name;
            this.cameraState = cameraState;
            this.offSetY = offSetY;
        }

        public SlotData(bool isSaved, string name)
            : this(Vector3.zero, Quaternion.identity, isSaved, name, GISSampleCameraState.PointOfView, 0.0f)
        {

        }

        /// <summary>
        /// Stringからカメラの状態を管理する列挙型に変換する関数
        /// </summary>
        /// <param name="camState"></param>
        /// <returns></returns>
        public static GISSampleCameraState CameraStateStringToEnum(string camState)
        {
            if (camState == "PointOfView")
            {
                return GISSampleCameraState.PointOfView;
            }
            else if (camState == "Walker")
            {
                return GISSampleCameraState.Walker;
            }
            else
            {
                Debug.LogError("予期しない引数が渡されました");
                return 0;
            }
        }
    }
}
