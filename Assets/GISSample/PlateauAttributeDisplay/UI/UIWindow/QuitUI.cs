using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
    public class QuitUI : MonoBehaviour
    {
        private Button quitButton;
        private void Start()
        {
            var ui = GetComponent<UIDocument>();
            quitButton = ui.rootVisualElement.Q<Button>("quit-app-button");
            quitButton.clicked += () =>
            {
                Debug.Log("Quitting App.");
                Application.Quit();
            };
        }
    }
}