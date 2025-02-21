using System.Linq;
using GISSample.PlateauAttributeDisplay.Gml;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.UI.UIWindow
{
    public class AttributeUi : MonoBehaviour
    {
        private UIDocument uiDoc;
        private Button closeButton;
        private ColorChangerByAttribute colorChangerByAttribute;

        /// <summary>
        /// 選択中のCityObject
        /// </summary>
        private SemanticCityObject selectedSemanticCityObject;

		private ResolutionMonitor resolutionMonitor;

		public void Init(ColorChangerByAttribute colorChangerByAttributeArg)
        {
            colorChangerByAttribute = colorChangerByAttributeArg;
            uiDoc = GetComponent<UIDocument>();
            closeButton = uiDoc.rootVisualElement.Q<Button>("attr-open-close-button");
            closeButton.clicked += Close;
            Close();

			resolutionMonitor = transform.parent.GetComponent<ResolutionMonitor>();
			if (resolutionMonitor != null)
			{
                ResolutionChanged(Screen.width, Screen.height);
				resolutionMonitor.OnResolutionChanged += ResolutionChanged;
			}
		}

        public void Close()
        {
            selectedSemanticCityObject = null;
            colorChangerByAttribute.Redraw();
            uiDoc.rootVisualElement.style.display = DisplayStyle.None;
        }


        public void Open()
        {
            uiDoc.rootVisualElement.style.display = DisplayStyle.Flex;
            var scrollView = GetScrollView();
            var header = scrollView.ElementAt(0);
            scrollView.Clear();
            scrollView.Add(header);
        }

        public void SetAttributes(SampleAttribute data)
        {
            // 属性データに合わせてUIElementを追加
            var elems = data.GetKeyValues()
                .Select((v, i) =>
                {
                    var elem = new VisualElement();
                    elem.AddToClassList("key-value");

                    var keyLabel = new Label(v.Key.Path);
                    keyLabel.AddToClassList("key");
                    elem.Add(keyLabel);

                    var valueLabel = new Label(v.Value);
                    valueLabel.AddToClassList("value");
                    elem.Add(valueLabel);

                    return elem;
                });

            foreach (var elem in elems)
            {
                GetScrollView().Add(elem);
            }
        }

        private ScrollView GetScrollView()
        {
            return uiDoc.rootVisualElement.Q<ScrollView>();
        }

        /// <summary>
        /// 都市オブジェクトを選択して色を付けます。
        /// </summary>
        public void SelectCityObj(SemanticCityObject semanticCityObj, Color selectedColor)
        {
            selectedSemanticCityObject = semanticCityObj;
            selectedSemanticCityObject.SetMaterialColor(selectedColor);

        }
		private void ResolutionChanged(int width, int height)
		{
			if (uiDoc == null)
			{
				uiDoc = GetComponent<UIDocument>();
			}
			VisualElement window = uiDoc.rootVisualElement.Q<VisualElement>("Window");
            if (window != null)
            {
                float windowBottom = 74f;
                float marginBottom = (10f + windowBottom) / height * 100f;
                if (marginBottom < (10f + windowBottom) / 1080f * 100f)
                {
                    marginBottom = (10f + windowBottom) / 1080f * 100f;
                }

                float windowHeight = 100f - marginBottom;
                if (windowHeight > 90.5f)
                {
                    windowHeight = 90.5f;
                }
                else if (windowHeight < 0)
                {
                    windowHeight = 0;
                }

                window.style.height = new StyleLength(Length.Percent(windowHeight));
            }
		}
	}
}