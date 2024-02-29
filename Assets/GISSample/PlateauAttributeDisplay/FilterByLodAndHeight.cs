using GISSample.PlateauAttributeDisplay.Gml;
using GISSample.PlateauAttributeDisplay.UI.UIWindow;
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

        private readonly MenuUi menuUi;
        private readonly GmlDictionary gmls;

        public FilterByLodAndHeight(MenuUi menuUi, GmlDictionary gmls)
        {
            this.menuUi = menuUi;
            this.gmls = gmls;

            menuUi.RegisterHeightSliderChangedCallback(OnHeightSliderValueChanged);
            menuUi.RegisterLodSliderChangedCallback(OnLodSliderValueChanged);
            
            
            
            var param = GetFilterParameterFromSliders();
            Filter(param);
            menuUi.UpdateFilterText(param);
        }
        
        /// <summary>
        /// 高さフィルタースライダーの値変更イベントコールバック
        /// </summary>
        /// <param name="e"></param>
        private void OnHeightSliderValueChanged(ChangeEvent<Vector2> e)
        {
            filterParameter = GetFilterParameterFromSliders();
            Filter(filterParameter);
            menuUi.UpdateFilterText(filterParameter);
        }
        
        /// <summary>
        /// LODフィルタースライダーの値変更イベントコールバック
        /// </summary>
        /// <param name="e"></param>
        private void OnLodSliderValueChanged(ChangeEvent<Vector2> e)
        {
            menuUi.lodSlider.value = new Vector2(Mathf.Round(e.newValue.x), Mathf.Round(e.newValue.y));

            filterParameter = GetFilterParameterFromSliders();
            Filter(filterParameter);
            menuUi.UpdateFilterText(filterParameter);
        }
        
        /// <summary>
        /// フィルター処理
        /// </summary>
        /// <param name="parameter"></param>
        private void Filter(FilterParameter parameter)
        {
            gmls.Filter(parameter);
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
                MinHeight = menuUi.heightSlider.value.x,
                MaxHeight = menuUi.heightSlider.value.y,
                MinLod = (int)menuUi.lodSlider.value.x,
                MaxLod = (int)menuUi.lodSlider.value.y,
            };
        }
    }
}