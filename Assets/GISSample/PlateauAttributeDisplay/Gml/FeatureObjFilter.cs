using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureObjFilter
{
    /// <summary>
    /// 高さやLODでのフィルタリングで、表示すべきならtrue,すべきでないならfalse。
    /// filters[0]は高さによるフィルタ、[1]はLODでのフィルタ。[2]は洪水情報かどうか、[3]は非表示の水害情報のみfalse。
    /// </summary>
    private readonly bool[] filters;
    
    private const int FilterIndexHeight = 0;
    private const int FilterIndexLod = 1;
    private const int FilterIndexFeatureIsFlooding = 2;
    private const int FilterIndexIsSelectedFlooding = 3;

    public FeatureObjFilter(bool isFlooding)
    {
        filters = new bool[] { true, true, isFlooding, false };
    }

    /// <summary>
    /// フィルタリングの結果、表示するべきかどうかを返します
    /// </summary>
    public bool ShouldActive()
    {
        bool isHeightInRange = filters[FilterIndexHeight];
        bool isLodInRange = filters[FilterIndexLod];
        bool isFeatureFlooding = filters[FilterIndexFeatureIsFlooding];
        bool isSelectedFlooding = filters[FilterIndexIsSelectedFlooding];
        if (isFeatureFlooding)
        {
            return isSelectedFlooding;
        }
        else
        {
            return isHeightInRange && isLodInRange;
        }
    }


    public void SetHeightFilter(bool shouldActive) => filters[FilterIndexHeight] = shouldActive;
    public void SetLodFilter(bool shouldActive) => filters[FilterIndexLod] = shouldActive;
    public void SetFloodingFilter(bool shouldActive) => filters[FilterIndexIsSelectedFlooding] = shouldActive;
}
