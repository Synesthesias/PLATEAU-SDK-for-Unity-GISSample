using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay
{
    public class AttributeUi : MonoBehaviour
    {
        private UIDocument uiDoc;
        private Button closeButton;
        private SceneManager sceneManager;
        
        /// <summary>
        /// 選択中のCityObject
        /// </summary>
        private SampleCityObject selectedCityObject;

        private void Awake()
        {
            uiDoc = GetComponent<UIDocument>();
            closeButton = uiDoc.rootVisualElement.Q<Button>("attr-open-close-button");
            sceneManager = FindObjectOfType<SceneManager>();
        }

        private void OnEnable()
        {
            // UI Documentをdisableにするとイベントがなくなることに注意してください。ここで再登録します。
            closeButton.clicked += Close;
        }
        
        public void Close()
        {
            selectedCityObject = null;
            sceneManager.RecolorFlooding();
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

        /// <summary>
        /// 都市オブジェクトを選択して色を付けます。
        /// </summary>
        public void SelectCityObj(SampleCityObject cityObj, Color selectedColor)
        {
            selectedCityObject = cityObj;
            selectedCityObject.SetMaterialColorAndShow(selectedColor);
            
        }
        
    }
}