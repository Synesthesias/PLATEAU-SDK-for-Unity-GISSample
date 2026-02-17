using GISSample.PlateauAttributeDisplay.Gml;
using GISSample.PlateauAttributeDisplay.UI.UIWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FilterParameter = GISSample.PlateauAttributeDisplay.Gml.FilterParameter;

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

        public FilterParameter FilterParameter => filterParameter;

        private readonly MenuUi menuUi;
        private readonly GmlDictionary gmls;
        private readonly GISTileManager tiles;

        /// <summary>
        /// デフォルト値かどうかの判定用
        /// </summary>
        public bool IsDefaultFilterParameter {
            get  
            {
                return filterParameter.MinHeight == menuUi.heightSlider.lowLimit &&
                        filterParameter.MaxHeight == menuUi.heightSlider.highLimit &&
                        filterParameter.MinLod == menuUi.lodSlider.lowLimit &&
                        filterParameter.MaxLod == menuUi.lodSlider.highLimit;
                       
            } 
        }

        public FilterByLodAndHeight(MenuUi menuUi, GmlDictionary gmls, GISTileManager tiles)
        {
            this.menuUi = menuUi;
            this.gmls = gmls;
            this.tiles = tiles;

            menuUi.RegisterHeightSliderChangedCallback(OnHeightSliderValueChanged);
            menuUi.RegisterLodSliderChangedCallback(OnLodSliderValueChanged);

            var param = GetFilterParameterFromSliders();
            Filter(param);
            menuUi.UpdateFilterText(param);
        }

        public IEnumerator FilterCoroutine(SampleGml gml)
        {
            if (gml.Tile != null && gml.Tile.LoadedObject == null)
                yield break;

            filterParameter = GetFilterParameterFromSliders();
            yield return FilterCoroutine(gml.SemanticCityObjs(), filterParameter);
        }

        public void Filter(SampleGml gml)
        {
            CoroutineUtil.RunToEnd(FilterCoroutine(gml));
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
            Filter(gmls.SemanticCityObjects(), parameter);
            if(tiles != null)
            {
                tiles.ProccessInteraction(() => Filter(tiles.SemanticCityObjects(), parameter));
            } 
        }
        private void Filter(IEnumerable<SemanticCityObject> semantics, FilterParameter parameter)
        {
            CoroutineUtil.RunToEnd(FilterCoroutine(semantics, parameter));
        }

        private IEnumerator FilterCoroutine(IEnumerable<SemanticCityObject> semantics, FilterParameter parameter)
        {
            int count = 0;
            foreach (var semantic in semantics)
            {
                // LODでのフィルタ
                int maxLodExist = semantic.MaxLodExist;
                int maxLodToShow = Math.Min(maxLodExist, parameter.MaxLod);
                foreach (var (lod, featureObj) in semantic.LodCityObjs.LodToFeatureObj)
                {
                    if (semantic.Attribute.MeasuredHeight.HasValue)
                    {
                        // 高さでのフィルタ
                        var measuredHeight = semantic.Attribute.MeasuredHeight.Value;
                        bool heightFilter = measuredHeight >= parameter.MinHeight && measuredHeight <= parameter.MaxHeight;
                        featureObj.Filter.SetHeightFilter(heightFilter); 
                    }

                    featureObj.Filter.SetLodFilter(lod == maxLodToShow && lod >= parameter.MinLod);
                    featureObj.ApplyFilter();
                }

                count++;
                if (count > GISTileManager.YIELD_STEP)
                {
                    count = 0;
                    yield return null;
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