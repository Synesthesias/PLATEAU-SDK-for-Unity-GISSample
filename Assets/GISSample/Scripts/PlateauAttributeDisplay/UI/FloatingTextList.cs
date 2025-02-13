using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GISSample.GISAttributeDisplay;
using UnityEngine;

/// <summary>
/// GISサンプルで宙に浮かぶ情報テキストをまとめるものです
/// </summary>
public class FloatingTextList
{
    private readonly GameObject[] floatingParents;
    private bool isActive = true;

    public FloatingTextList()
    {
        // 宙に浮かぶテキストの親には GISAttrDisplayFactoryBase がアタッチされていることが前提
        var factories = Object.FindObjectsByType<GISAttrDisplayFactoryBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        floatingParents = factories.Select(f => f.gameObject).ToArray();
    }

    /// <summary>
    /// 宙に浮かぶテキストの表示と非表示を切り替えます。
    /// </summary>
    public void SwitchIsActive()
    {
        SetActive(!isActive);
    }

    public void SetActive(bool isActiveArg)
    {
        isActive = isActiveArg;
        foreach (var f in floatingParents)
        {
            f.SetActive(isActiveArg);
        }
    }

}
