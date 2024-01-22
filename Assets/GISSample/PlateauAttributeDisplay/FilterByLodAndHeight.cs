using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// LODと高さによって都市モデルのON/OFFを切り替えます。
    /// </summary>
    public class FilterByLodAndHeight
    {
        /// <summary>
        /// フィルターパラメータ
        /// </summary>
        private FilterParameter filterParameter;

        private UIDocument menuUi;
        
        /// <summary>
        /// 高さフィルターのスライダー
        /// </summary>
        private MinMaxSlider heightSlider;

        /// <summary>
        /// LODフィルターのスライダー
        /// </summary>
        private MinMaxSlider lodSlider;

        /// <summary>
        /// 高さフィルターのラベル
        /// </summary>
        private Label heightValueLabel;

        /// <summary>
        /// LODフィルターのラベル
        /// </summary>
        private Label lodValueLabel;
        
        private readonly Dictionary<string, SampleGml> gmls;

        public FilterByLodAndHeight(UIDocument menuUi, Dictionary<string, SampleGml> gmls)
        {
            this.menuUi = menuUi;
            this.gmls = gmls;
            
            heightSlider = menuUi.rootVisualElement.Q<MinMaxSlider>("HeightSlider");
            heightSlider.RegisterValueChangedCallback(OnHightSliderValueChanged);

            lodSlider = menuUi.rootVisualElement.Q<MinMaxSlider>("LodSlider");
            lodSlider.RegisterValueChangedCallback(OnLodSliderValueChanged);

            heightValueLabel = menuUi.rootVisualElement.Q<Label>("HeightValue");

            lodValueLabel = menuUi.rootVisualElement.Q<Label>("LodValue");
            
            var param = GetFilterParameterFromSliders();
            Filter(param);
            UpdateFilterText(param);
        }
        
        /// <summary>
        /// 高さフィルタースライダーの値変更イベントコールバック
        /// </summary>
        /// <param name="e"></param>
        public void OnHightSliderValueChanged(ChangeEvent<Vector2> e)
        {
            filterParameter = GetFilterParameterFromSliders();
            Filter(filterParameter);
            UpdateFilterText(filterParameter);
        }
        
        /// <summary>
        /// LODフィルタースライダーの値変更イベントコールバック
        /// </summary>
        /// <param name="e"></param>
        public void OnLodSliderValueChanged(ChangeEvent<Vector2> e)
        {
            lodSlider.value = new Vector2(Mathf.Round(e.newValue.x), Mathf.Round(e.newValue.y));

            filterParameter = GetFilterParameterFromSliders();
            Filter(filterParameter);
            UpdateFilterText(filterParameter);
        }
        
        /// <summary>
        /// フィルター処理
        /// </summary>
        /// <param name="parameter"></param>
        private void Filter(FilterParameter parameter)
        {
            foreach (var keyValue in gmls)
            {
                keyValue.Value.Filter(parameter);
            }
        }
        
        /// <summary>
        /// フィルターのテキストを更新
        /// </summary>
        /// <param name="parameter"></param>
        private void UpdateFilterText(FilterParameter parameter)
        {
            heightValueLabel.text = $"{parameter.MinHeight:F1} to {parameter.MaxHeight:F1}";
            lodValueLabel.text = $"{parameter.MinLod:D} to {parameter.MaxLod:D}";
        }
        
        /// <summary>
        /// フィルターパラメータを取得
        /// UIのスライダーの状態からフィルターパラメータを作成します。
        /// </summary>
        /// <returns>フィルターパラメータ</returns>
        private FilterParameter GetFilterParameterFromSliders()
        {
            return new FilterParameter
            {
                MinHeight = heightSlider.value.x,
                MaxHeight = heightSlider.value.y,
                MinLod = (int)lodSlider.value.x,
                MaxLod = (int)lodSlider.value.y,
            };
        }
    }
}