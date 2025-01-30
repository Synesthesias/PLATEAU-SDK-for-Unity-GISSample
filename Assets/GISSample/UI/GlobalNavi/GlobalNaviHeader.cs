using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;
using System;

namespace GISSample.Misc
{
    /// <summary>
    /// GlobalNaviのヘッダー処理
    /// </summary>
    public class GlobalNaviHeader : ISubComponent
    {
        // メニューボタングループ
        private readonly GroupBox menuGroup;
        // サブメニューパネル
        private readonly VisualElement subMenuElement;
        // 設定ボタン
        private readonly Button settingButton;
        // 設定パネル
        private readonly VisualElement settingElement;
        //機能変更トグルボタン
        private readonly VisualElement functionContainer;
        // メニューボタングループ名前
        private const string UImenuGroup = "Menu";
        // サブメニューパネル名前
        private const string UISubMenuElement = "SubMenu";
        // 設定ボタン名前
        private const string UISettingButton = "Button_Setting";
        // 設定パネル名前
        private const string UISettingElement = "SettingPanel";
        // 機能変更トグルボタン名前
        private const string UIFunctionContainer = "FunctionContainer";

        // サブメニューのuxmlを格納する配列
        public VisualElement[] subMenuUxmls;

        // 最後に開いたサブメニューuxml
        private VisualElement lastSubMenuUxml;

        public GlobalNaviHeader(VisualElement uiRoot, VisualElement[] subMenuUxmls)
        {
            this.subMenuUxmls = subMenuUxmls;
            menuGroup = uiRoot.Q<GroupBox>(UImenuGroup);
            subMenuElement = uiRoot.Q<VisualElement>(UISubMenuElement);
            settingButton = uiRoot.Q<Button>(UISettingButton);
            settingElement = uiRoot.Q<VisualElement>(UISettingElement);
            functionContainer = uiRoot.Q<VisualElement>(UIFunctionContainer);
            this.subMenuUxmls = subMenuUxmls;

            // サブメニューを非表示
            foreach (var child in subMenuElement.Children())
            {
                child.style.display = DisplayStyle.None;
            }

            // 設定パネルを非表示
            settingElement.style.display = DisplayStyle.None;

            // メニューボタンリスト
            var menuButtons = menuGroup.Children();

            // メニューボタンの値が変更されたとき
            menuButtons.ToList().ForEach(r =>
            {
                var button = r as RadioButton;
                button.RegisterValueChangedCallback(evt =>
                {
                    if (button.value)
                    {
                        // tabIndexに対応するサブメニューを表示
                        ShowSubMenuPanel(button.tabIndex);

                        // メニュー切り替え時はサブメニューの最初の要素であるuxmlを表示
                        if (button.tabIndex >= 0) // tabIndexが-1(Menu)の場合は何も表示しない)
                        {
                            var subMenu = subMenuElement.Children().ElementAt(button.tabIndex);
                            var subMenuButton = subMenu.Children().ElementAt(0) as RadioButton;
                            subMenuButton.value = true;
                        }
                    }
                });
            });

            // サブメニューボタンの値が変更されたとき
            subMenuElement.Children().ToList().ForEach(g =>
            {
                var groupBox = g as GroupBox;
                groupBox.Children().ToList().ForEach(r =>
                {
                    var button = r as RadioButton;
                    button.RegisterValueChangedCallback(evt =>
                    {
                        if (button.value)
                        {
                            // tabIndexに該当するuxmlを表示
                            if (functionContainer.Q<Toggle>("Toggle_WalkMode").value && button.tabIndex == (int)SubMenuUxmlType.CameraList)
                            {
                                SetSubMenuUxml((int)SubMenuUxmlType.WalkMode);
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].style.display = DisplayStyle.Flex;
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<TemplateContainer>("Panel_WalkViewRegister").style.display = DisplayStyle.None;
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<TemplateContainer>("Panel_WalkViewEditor").style.display = DisplayStyle.None;
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<VisualElement>("Title_CameraRegist").style.display = DisplayStyle.None;
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<VisualElement>("Title_WalkController").style.display = DisplayStyle.Flex;
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<TemplateContainer>("Panel_WalkController").style.display = DisplayStyle.Flex;
                                lastSubMenuUxml = subMenuUxmls[(int)SubMenuUxmlType.WalkMode];
                            }
                            else if (functionContainer.Q<Toggle>("Toggle_WalkMode").value && button.tabIndex == (int)SubMenuUxmlType.CameraEdit)
                            {
                                SetSubMenuUxml((int)SubMenuUxmlType.WalkMode);
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].style.display = DisplayStyle.Flex;
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<TemplateContainer>("Panel_WalkViewRegister").style.display = DisplayStyle.Flex;
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<TemplateContainer>("Panel_WalkViewEditor").style.display = DisplayStyle.None;
                                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<VisualElement>("Title_CameraRegist").style.display = DisplayStyle.Flex;
                                lastSubMenuUxml = subMenuUxmls[(int)SubMenuUxmlType.WalkMode];
                            }
                            else
                            {
                                SetSubMenuUxml(button.tabIndex);
                                subMenuUxmls[button.tabIndex].style.display = DisplayStyle.Flex;
                                lastSubMenuUxml = subMenuUxmls[button.tabIndex];
                            }
                        }
                    });
                });
            });

            // 設定ボタンがクリックされたとき
            settingButton.clicked += () =>
            {
                // 設定パネルの表示/非表示を切り替え
                if (settingElement.style.display == DisplayStyle.Flex)
                {
                    settingElement.style.display = DisplayStyle.None;
                }
                else
                {
                    settingElement.style.display = DisplayStyle.Flex;
                }
            };

            //歩行者モードトグルボタンが押されたら変更する
            functionContainer.Q<Toggle>("Toggle_WalkMode").RegisterValueChangedCallback(OnToggleWalkModeValueChanged);

            //保存、ロードした時にセッティングパネルを閉じる処理(新見)
            var saveButton = settingElement.Q<Button>("SaveButton");
            saveButton.clicked += () =>
            {
                settingElement.style.display = DisplayStyle.None;
            };
            var loadButton = settingElement.Q<Button>("LoadButton");
            loadButton.clicked += () =>
            {
                settingElement.style.display = DisplayStyle.None;
            };
        }

        // サブメニューを選択するパネルを表示する
        private void ShowSubMenuPanel(int id)
        {
            // 最後に開いたサブメニューのuxmlを非表示
            if (lastSubMenuUxml != null)
            {
                lastSubMenuUxml.style.display = DisplayStyle.None;
                lastSubMenuUxml = null;
            }
            foreach (var subMenu in subMenuElement.Children())
            {
                // 全てのサブメニューを非表示
                subMenu.style.display = DisplayStyle.None;

                // サブメニューのラジオボタンを最初の要素以外falseにする
                foreach (var subMenuButton in subMenu.Children())
                {
                    var button = subMenuButton as RadioButton;
                    button.value = false;
                }
            }

            // 指定のサブメニューを表示
            if (id >= 0) // tabIndexが-1(Menu)の場合は何も表示しない
            {
                subMenuElement.Children().ElementAt(id).style.display = DisplayStyle.Flex;
            }
        }

        // サブメニューのuxmlを表示する
        private void SetSubMenuUxml(int id)
        {
            subMenuUxmls[id].style.display = DisplayStyle.Flex;

            // 最後に開いたサブメニューのuxmlを非表示
            if (lastSubMenuUxml != null)
            {
                lastSubMenuUxml.style.display = DisplayStyle.None;
                lastSubMenuUxml = null;
            }

            // 現在開かれているサブメニューのuxmlを設定
            var type = (SubMenuUxmlType)Enum.ToObject(typeof(SubMenuUxmlType), id);
            var subComponents = GameObject.FindObjectOfType<GISSampleSubComponents>();
            if (subComponents != null)
            {
                subComponents.SetSubMenuUxmlType(type);
            }
            lastSubMenuUxml = subMenuUxmls[id];
        }

        private void OnToggleWalkModeValueChanged(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                // ShowSubMenuPanel(2);
                SetSubMenuUxml((int)SubMenuUxmlType.WalkMode);
                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].style.display = DisplayStyle.Flex;
                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<TemplateContainer>("Panel_WalkViewRegister").style.display = DisplayStyle.None;
                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<TemplateContainer>("Panel_WalkViewEditor").style.display = DisplayStyle.None;
                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<VisualElement>("Title_CameraRegist").style.display = DisplayStyle.None;
                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<VisualElement>("Title_WalkController").style.display = DisplayStyle.None;
                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].Q<TemplateContainer>("Panel_WalkController").style.display = DisplayStyle.None;
                lastSubMenuUxml = subMenuUxmls[(int)SubMenuUxmlType.WalkMode];
            }
            else
            {
                subMenuUxmls[(int)SubMenuUxmlType.WalkMode].style.display = DisplayStyle.None;
                menuGroup.Q<RadioButton>("MenuMain").value = true;
                ShowSubMenuPanel(-1);
            }
        }

        public void Update(float deltaTime)
        {
        }
        public void OnEnable()
        {
        }
        public void OnDisable()
        {
        }
        public void Start()
        {
        }
    }
}
