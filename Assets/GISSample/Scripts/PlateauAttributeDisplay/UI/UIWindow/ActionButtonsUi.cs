using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
	public class ActionButtonsUi : MonoBehaviour
	{
		private UIDocument uiDoc;
		private Button helpButton;
		private Button quitButton;

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
		}
	}
}