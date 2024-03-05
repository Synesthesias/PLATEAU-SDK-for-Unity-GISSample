using System;
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
        /// 高さとLODでのフィルタ
        /// </summary>
        private void Filter(FilterParameter parameter)
        {
            foreach (var semantic in gmls.SemanticCityObjects())
            {
                if (semantic.Attribute.MeasuredHeight.HasValue)
                {
                    // 高さでのフィルタ
                    var measuredHeight = semantic.Attribute.MeasuredHeight.Value;
                    bool heightFilter = measuredHeight >= parameter.MinHeight && measuredHeight <= parameter.MaxHeight;
                    foreach(var feature in semantic.FeatureGameObjs())
                    {
                        feature.Filter.SetHeightFilter(heightFilter);
                    }
                }
                
                // LODでのフィルタ
                int maxLodExist = semantic.MaxLodExist;
                int maxLodToShow = Math.Min(maxLodExist, parameter.MaxLod);
                foreach (var (lod, featureObj) in semantic.LodCityObjs.LodToFeatureObj)
                {
                    featureObj.Filter.SetLodFilter(lod == maxLodToShow && lod >= parameter.MinLod);
                    featureObj.ApplyFilter();
                }
            }
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