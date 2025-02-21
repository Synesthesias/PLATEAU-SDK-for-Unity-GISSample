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

		private ResolutionMonitor resolutionMonitor;

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

			resolutionMonitor = transform.parent.GetComponent<ResolutionMonitor>();
			if (resolutionMonitor != null)
			{
				ResolutionChanged(Screen.width, Screen.height);
				resolutionMonitor.OnResolutionChanged += ResolutionChanged;
			}
		}

		private void ResolutionChanged(int width, int height)
		{
			if (uiDoc == null)
			{
				uiDoc = GetComponent<UIDocument>();
			}
			VisualElement buttons = uiDoc.rootVisualElement.Q<VisualElement>("Buttons");
			if (buttons != null)
			{
				float marginBottom = 10f / height * 100f;
				if(marginBottom < 1000f / 1080f)
				{
					marginBottom = 1000f / 1080f;
				}
				buttons.style.bottom = new StyleLength(Length.Percent(marginBottom));
			}
		}
	}
}