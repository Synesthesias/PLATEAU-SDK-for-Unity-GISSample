using System.Collections;
using System.Collections.Generic;
using GISSample.PlateauAttributeDisplay;
using PLATEAU.CityInfo;
using UnityEngine;

/// <summary>
/// GISサンプルにおいて、1つのGMLファイルに含まれる地物の辞書です。
/// キーはIDです。
/// </summary>
public class CityObjDict
{
    private readonly Dictionary<string, SampleCityObject> dict;

    /// <summary>
    /// GML相当のゲームオブジェクトの子をもとに<see cref="CityObjDict"/>を構築します。
    /// </summary>
    public CityObjDict(GameObject gmlGameObj)
    {
        dict = new();
        foreach (Transform lodTransform in gmlGameObj.transform)
        {
            foreach (Transform cityObjectTransform in lodTransform)
            {
                var id = cityObjectTransform.name;
                if (dict.ContainsKey(id))
                {
                    Debug.LogWarning("Duplicate CityObject id detected.");
                }
                else
                {
                    var cityObjComponent = cityObjectTransform.GetComponent<PLATEAUCityObjectGroup>();
                    if (cityObjComponent != null)
                    {
                        dict[id] = new SampleCityObject(id, cityObjComponent);
                    }
                    
                }

                if (dict.TryGetValue(id, out var o))
                {
                    bool isFlooding = gmlGameObj.name.Contains("fld");
                    o.LodCityObjs.Add(lodTransform, cityObjectTransform, isFlooding);
                }

                    
            }
        }
    }

    public HashSet<string> FindAllFloodingAreaNames()
    {
        var floodingNames = new HashSet<string>();
        foreach (var cityObj in dict.Values)
        {
            foreach (var flood in cityObj.Attribute.GetFloodingAreaInfos())
            {
                floodingNames.Add(flood.AreaName);
            }
        }

        return floodingNames;
    }

    public void Filter(FilterParameter parameter)
    {
        foreach (var cityObj in dict.Values)
        {
            cityObj.Filter(parameter);
        }
    }
    
    /// <summary>
    /// 色分け
    /// </summary>
    /// <param name="type">色分けタイプ</param>
    /// <param name="colorTable">色テーブル</param>
    /// <param name="areaName">浸水エリア名</param>
    public void ColorGml(ColorCodeType type, Color[] colorTable, string areaName = null)
    {
        foreach (var cityObj in dict.Values)
        {
            cityObj.ColorCityObj(type, colorTable, areaName);
        }
    }

    public SampleCityObject Get(string cityObjName)
    {
        return dict[cityObjName];
    }
}
