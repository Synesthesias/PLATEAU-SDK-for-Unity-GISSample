using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using System;
using GISSample.Misc.CameraPositionMemory;

namespace GISSample.Misc
{
    /// <summary>
    /// 実行時の機能である<see cref="ISubComponent"/>をここにまとめて、UpdateやOnEnable等を呼び出します。
    /// </summary>

    public enum SubMenuUxmlType
    {
        Menu = -1,
        CameraList,
        CameraEdit,
        WalkMode,
    }

    public class GISSampleSubComponents : MonoBehaviour
    {
        private List<ISubComponent> subComponents;
        // 現在開かれているサブメニュー機能
        private SubMenuUxmlType subMenuUxmlType = SubMenuUxmlType.Menu;
        // サブメニューのuxmlを管理するする配列
        VisualElement[] subMenuUxmls;

        private void Awake()
        {
            var uiRoot = new UIDocumentFactory().CreateWithUxmlName("GlobalNavi_Main");
            // GlobalNavi_Main.uxmlのSortOrderを設定
            GameObject.Find("GlobalNavi_Main").GetComponent<UIDocument>().sortingOrder = 1;

            // サブメニューのuxmlを生成して非表示
            subMenuUxmls = new VisualElement[Enum.GetNames(typeof(SubMenuUxmlType)).Length - 1];
            for (int i = 0; i < subMenuUxmls.Length; i++)
            {
                subMenuUxmls[i] = new UIDocumentFactory().CreateWithUxmlName(((SubMenuUxmlType)i).ToString());
                subMenuUxmls[i].style.display = DisplayStyle.None;
            }

            // MainCameraを取得
            GameObject mainCamera = Camera.main.gameObject;

            // MainCameraにCinemachineBrainがアタッチされていない場合は追加
            if (mainCamera.GetComponent<CinemachineBrain>() == null)
            {
                mainCamera.AddComponent<CinemachineBrain>();
            }

            //俯瞰視点用のカメラの生成と設定
            GameObject mainCam = new GameObject("PointOfViewCamera");
            CinemachineVirtualCamera mainCamVC = mainCam.AddComponent<CinemachineVirtualCamera>();
            mainCamVC.m_Lens.FieldOfView = 60;
            mainCamVC.m_Lens.NearClipPlane = 0.3f;
            mainCamVC.m_Lens.FarClipPlane = 1000;

            //歩行者視点用のオブジェクトの生成と設定
            GameObject walker = new GameObject("Walker");
            CharacterController characterController = walker.AddComponent<CharacterController>();
            characterController.slopeLimit = 90;
            characterController.stepOffset = 0.3f;
            characterController.skinWidth = 0.05f;

            //歩行者視点用のカメラの生成と設定
            GameObject walkerCam = new GameObject("WalkerCamera");
            CinemachineVirtualCamera walkerCamVC = walkerCam.AddComponent<CinemachineVirtualCamera>();
            walkerCamVC.m_Lens.FieldOfView = 60;
            walkerCamVC.m_Lens.NearClipPlane = 0.3f;
            walkerCamVC.m_Lens.FarClipPlane = 1000;
            walkerCamVC.Priority = 9;
            walkerCamVC.m_StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Never;
            walkerCamVC.AddCinemachineComponent<CinemachineTransposer>();
            walkerCamVC.AddCinemachineComponent<CinemachinePOV>();
            CinemachineInputProvider walkerCamInput = walkerCam.AddComponent<CinemachineInputProvider>();
            walkerCamInput.XYAxis = InputActionReference.Create(new DefaultInputActions().Player.Look);
            walkerCam.SetActive(false);
            walkerCam.SetActive(true);
            walkerCamVC.Follow = walker.transform;

            var landscapeCamera = new LandscapeCamera(mainCamVC, walkerCamVC, walker);
            var walkerMoveByUserInput = new WalkerMoveByUserInput(walkerCamVC, walker);
            var cameraPositionMemory = new CameraPositionMemory.CameraPositionMemory(mainCamVC, walkerCamVC, landscapeCamera);

            var saveSystem = new SaveSystem(uiRoot);

            // 必要な機能をここに追加します
            subComponents = new List<ISubComponent>
            {
                new GlobalNaviHeader(uiRoot, subMenuUxmls),
                new CameraMoveByUserInput(mainCamVC),
                new GISSampleCameraUI(landscapeCamera, uiRoot,subMenuUxmls),
                walkerMoveByUserInput,
                new CameraPositionMemoryUI(cameraPositionMemory, subMenuUxmls, walkerMoveByUserInput,saveSystem, uiRoot),
            };
        }

        private void Start()
        {
            foreach (var c in subComponents)
            {
                c.Start();
            }
        }

        private void OnEnable()
        {
            foreach (var c in subComponents)
            {
                c.OnEnable();
            }
        }

        private void Update()
        {
            foreach (var c in subComponents)
            {
                c.Update(Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            foreach (var c in subComponents)
            {
                c.OnDisable();
            }
        }

        public SubMenuUxmlType GetSubMenuUxmlType()
        {
            return subMenuUxmlType;
        }

        public void SetSubMenuUxmlType(SubMenuUxmlType type)
        {
            subMenuUxmlType = type;
        }
    }
}