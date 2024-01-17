﻿using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityInfo;
using PLATEAU.Samples;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// シーンマネージャ
    /// カメラ、入力、UIの制御を行います。
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        [SerializeField, Tooltip("初期化中")] private UIDocument initializingUi;
        [SerializeField, Tooltip("メニュー（フィルター、色分け部分）")] private UIDocument menuUi;
        [SerializeField, Tooltip("操作説明")] private UIDocument userGuideUi;
        [SerializeField, Tooltip("属性情報")] private UIDocument attributeUi;

        [SerializeField, Tooltip("選択中オブジェクトの色")] private Color selectedColor;
        [SerializeField, Tooltip("色分け（高さ）の色テーブル")] private Color[] heightColorTable;
        [SerializeField, Tooltip("色分け（浸水ランク）の色テーブル")] private Color[] floodingRankColorTable;


        /// <summary>
        /// InputActions
        /// Assets/GISSample/GISSampleInputActionsから生成されたクラスです。
        /// </summary>
        private GISSampleInputActions inputActions;
        

        /// <summary>
        /// シーン中のPLATEAUInstancedCityModel
        /// 複数の都市データのインポートに対応するため、配列にしています。
        /// </summary>
        private PLATEAUInstancedCityModel[] instancedCityModels;

        /// <summary>
        /// GMLテーブル
        /// 対象GameObjectやGMLの属性情報等の必要な情報をまとめたものです。
        /// </summary>
        private readonly Dictionary<string, SampleGml> gmls = new Dictionary<string, SampleGml>();

        private readonly List<string> floodingAreaNames = new List<string>(); 

        /// <summary>
        /// 選択中のCityObject
        /// </summary>
        private SampleCityObject selectedCityObject;

        /// <summary>
        /// 色分けタイプ
        /// </summary>
        private ColorCodeType colorCodeType;

        /// <summary>
        /// 浸水エリア名（色分け用）
        /// </summary>
        private string floodingAreaName;

        

        /// <summary>
        /// 色分けグループ
        /// </summary>
        private RadioButtonGroup colorCodeGroup;

        private FilterByLodAndHeight filterByLodAndHeight;
        private GISCameraMove gisCameraMove;



        private void Awake()
        {
            inputActions = new GISSampleInputActions();
        }

        private void Start()
        {
            

            attributeUi.gameObject.SetActive(false);
            userGuideUi.gameObject.SetActive(true);
            
            colorCodeGroup = menuUi.rootVisualElement.Q<RadioButtonGroup>("ColorCodeGroup");
            colorCodeGroup.RegisterValueChangedCallback(OnColorCodeGroupValueChanged);

            Initialize();
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void OnDestroy()
        {
            inputActions.Dispose();
        }




        /// <summary>
        /// 初期化処理
        /// 属性情報から必要なデータをまとめます。
        /// </summary>
        /// <returns></returns>
        private void Initialize()
        {
            gisCameraMove = new GISCameraMove(this);
            instancedCityModels = FindObjectsOfType<PLATEAUInstancedCityModel>();
            if (instancedCityModels == null || instancedCityModels.Length == 0)
            {
                return;
            }

            initializingUi.gameObject.SetActive(true);

            foreach (var instancedCityModel in instancedCityModels)
            {
                // インポートしたPLATEAUInstancedCityModelの名前がルートフォルダ名です。
                // var rootDirName = instancedCityModel.name;

                for (int i = 0; i < instancedCityModel.transform.childCount; ++i)
                {
                    // 子オブジェクトの名前はGMLファイル名です。
                    // ロードするときは、一引数に、対応するGameObjectを渡します。
                    var go = instancedCityModel.transform.GetChild(i).gameObject;

                    // サンプルではdemを除外します。
                    if (go.name.Contains("dem")) continue;
                    

                    // ロードしたデータをアプリ用に扱いやすくしたクラスに変換します。
                    var gml = new SampleGml(go);
                    gmls.Add(go.name, gml);
                }
            }

            var areaNames = new HashSet<string>();
            foreach(var names in gmls.Select(pair => pair.Value.FloodingAreaNames))
            {
                areaNames.UnionWith(names);
            }
            floodingAreaNames.AddRange(areaNames);
            floodingAreaNames.Sort();

            if (floodingAreaNames.Count > 0)
            {
                var choices = colorCodeGroup.choices.ToList();
                choices.AddRange(floodingAreaNames);
                colorCodeGroup.choices = choices;
            }
            ColorCode(colorCodeType, floodingAreaName);

            inputActions.GISSample.SetCallbacks(gisCameraMove);

            initializingUi.gameObject.SetActive(false);
            
            filterByLodAndHeight = new FilterByLodAndHeight(menuUi, gmls);
        }

        /// <summary>
        /// 色分け処理
        /// </summary>
        /// <param name="type"></param>
        private void ColorCode(ColorCodeType type, string areaName)
        {
            foreach (var keyValue in gmls)
            {
                Color[] colorTable = null;
                switch (type)
                {
                    case ColorCodeType.Height:
                        colorTable = heightColorTable;
                        break;
                    case ColorCodeType.FloodingRank:
                        colorTable = floodingRankColorTable;
                        break;
                    default:
                        break;
                }

                keyValue.Value.ColorCode(type, colorTable, areaName);
            }
        }

        /// <summary>
        /// 属性情報を取得
        /// </summary>
        /// <param name="gmlFileName">GMLファイル名</param>
        /// <param name="cityObjectID">CityObjectID</param>
        /// <returns>属性情報</returns>
        private SampleAttribute GetAttribute(string gmlFileName, string cityObjectID)
        {
            if (gmls.TryGetValue(gmlFileName, out SampleGml gml))
            {
                if (gml.CityObjects.TryGetValue(cityObjectID, out SampleCityObject city))
                {
                    return city.Attribute;
                }
            }

            return null;
        }

        /// <summary>
        /// オブジェクトのピック
        /// マウスの位置からレイキャストしてヒットしたオブジェクトのTransformを返します。
        /// </summary>
        /// <returns>Transform</returns>
        private Transform PickObject()
        {
            var camera = Camera.main;
            var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            // 一番手前のオブジェクトを選びます。
            float nearestDistance = float.MaxValue;
            Transform nearestTransform = null;
            foreach (var hit in Physics.RaycastAll(ray))
            {
                if (hit.distance <= nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestTransform = hit.transform;
                }
            }

            return nearestTransform;
        }

        /// <summary>
        /// マウスの位置がUI上にあるかどうか
        /// </summary>
        /// <returns></returns>
        public bool IsMousePositionInUiRect()
        {
            var refW = (float)menuUi.panelSettings.referenceResolution.x;
            var scale = refW / Screen.width;
            var mousePos = scale * Mouse.current.position.ReadValue();

            var leftViewRect = menuUi.rootVisualElement.Q<ScrollView>().worldBound;
            var rightView = userGuideUi.gameObject.activeSelf ? userGuideUi : attributeUi;
            var rightViewRect = rightView.rootVisualElement.Q<ScrollView>().worldBound;
            return leftViewRect.Contains(mousePos) || rightViewRect.Contains(mousePos); ;
        }

        /// <summary>
        /// オブジェクト選択
        /// </summary>
        /// <param name="context"></param>
        public void OnSelectObject(InputAction.CallbackContext context)
        {
            if (context.performed && !IsMousePositionInUiRect())
            {
                var trans = PickObject();
                if (trans == null)
                {
                    ColorCode(colorCodeType, floodingAreaName);

                    selectedCityObject = null;

                    userGuideUi.gameObject.SetActive(true);
                    attributeUi.gameObject.SetActive(false);

                    return;
                };

                // 前回選択中のオブジェクトの色を戻すために色分け処理を実行
                ColorCode(colorCodeType, floodingAreaName);

                // 選択されたオブジェクトの色を変更
                selectedCityObject = gmls[trans.parent.parent.name].CityObjects[trans.name];
                selectedCityObject.SetMaterialColor(selectedColor);

                userGuideUi.gameObject.SetActive(false);
                attributeUi.gameObject.SetActive(true);

                var data = GetAttribute(trans.parent.parent.name, trans.name);

                var scrollView = attributeUi.rootVisualElement.Q<ScrollView>();
                var header = scrollView.ElementAt(0);
                scrollView.Clear();
                scrollView.Add(header);

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

                        elem.style.backgroundColor = i % 2 == 0 
                            ? Color.white 
                            : new Color((float)0xED / 0xFF, (float)0xED / 0xFF, (float)0xED / 0xFF);

                        return elem;
                    });

                foreach (var elem in elems)
                {
                    scrollView.Add(elem);
                }
            }
        }

        /// <summary>
        /// 色分け選択変更イベントコールバック
        /// </summary>
        /// <param name="e"></param>
        private void OnColorCodeGroupValueChanged(ChangeEvent<int> e)
        {
            // valueは
            // 0: 色分けなし
            // 1: 高さ
            // 2～: 浸水ランク
            if (e.newValue < 2)
            {
                colorCodeType = (ColorCodeType)e.newValue;
                floodingAreaName = null;
            }
            else
            {
                colorCodeType = ColorCodeType.FloodingRank;
                floodingAreaName = colorCodeGroup.choices.ElementAt(e.newValue);
            }

            ColorCode(colorCodeType, floodingAreaName);
        }

    }
}