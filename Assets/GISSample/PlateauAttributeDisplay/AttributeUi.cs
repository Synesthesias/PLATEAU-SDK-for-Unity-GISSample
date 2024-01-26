using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay
{
    public class AttributeUi : MonoBehaviour
    {
        private UIDocument uiDoc;
        private Button closeButton;

        private void Awake()
        {
            uiDoc = GetComponent<UIDocument>();
            closeButton = uiDoc.rootVisualElement.Q<Button>("attr-open-close-button");
        }
        public void Close()
        {
            uiDoc.gameObject.SetActive(false);
        }

        public void Open()
        {
            
            uiDoc.gameObject.SetActive(true);
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

                    // 属性情報テーブルの背景色ストライプ
                    var bgColor = elem.style.backgroundColor.value;
                    elem.style.backgroundColor = (i % 2 == 0) 
                        ? bgColor:
                        bgColor + new Color(0.2f, 0.2f, 0.2f);

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

        public bool IsMouseInWindow(Vector2 mousePos)
        {
            var view = uiDoc;
            var viewRect = view.rootVisualElement?.Q<ScrollView>()?.worldBound;
            return viewRect != null && viewRect.Value.Contains(mousePos);
        }
        
    }
}