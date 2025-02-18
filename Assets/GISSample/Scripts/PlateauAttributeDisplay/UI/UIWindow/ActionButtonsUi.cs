using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
	public class ActionButtonsUi : MonoBehaviour
	{
		private UIDocument uiDoc;
		private Button helpButton;
		private Button quitButton;
		private Toggle walkerToggle;
		private Button vehicleButton;

		public event Action<bool> OnWalkerToggle;
		public event Action OnVehicleButtonClicked;

		public void SetWalkerToggleEnabled(bool enabled)
		{
			walkerToggle.SetEnabled(enabled);
		}

		public void SetVehicleToggleEnabled(bool enabled)
		{
			vehicleButton.SetEnabled(enabled);
		}

		public void SetWalkerToggleOff()
		{
			walkerToggle.SetValueWithoutNotify(false);
		}

		// Start is called before the first frame update
		private void Start()
		{
			uiDoc = GetComponent<UIDocument>();

			helpButton = uiDoc.rootVisualElement.Q<Button>("HelpButton");
			Transform userGuideTrans = transform.parent.Find("UserGuideUI");
			if (userGuideTrans != null)
			{
				UserGuideUi userGuideUi = userGuideTrans.GetComponent<UserGuideUi>();
				helpButton.clicked += userGuideUi.OnOpenCloseButtonClick;
			}

			quitButton = uiDoc.rootVisualElement.Q<Button>("QuitButton");
			quitButton.clicked += () =>
			{
				Debug.Log("Quitting App.");
				Application.Quit();
			};

			walkerToggle = uiDoc.rootVisualElement.Q<Toggle>("WalkCameraToggle");
			walkerToggle.RegisterValueChangedCallback((evt) =>
			{
				OnWalkerToggle?.Invoke(evt.newValue);
			});

			vehicleButton = uiDoc.rootVisualElement.Q<Button>("VehicleCameraButton");
			vehicleButton.clicked += () =>
			{
				OnVehicleButtonClicked?.Invoke();
			};
			vehicleButton.SetEnabled(false);
		}
	}
}