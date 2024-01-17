using System;
using System.Linq;
using PLATEAU.CityInfo;
using UnityEngine;
using UnityEngine.Assertions;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// CityObjectのラッパー
    /// </summary>
    public class SampleCityObject
    {
        public readonly string Id;
        public readonly PLATEAUCityObjectGroup CityObjComponent;

        /// <summary>
        /// CityObjectに対応するGameObjectリスト
        /// 配列のインデックスがLODのレベルに対応しています。
        /// </summary>
        public readonly GameObject[] LodObjects;

        public readonly SampleAttribute Attribute;

        public SampleCityObject(string id, PLATEAUCityObjectGroup cityObjComponent)
        {
            Id = id;
            // CityObject = cityObject;
            CityObjComponent = cityObjComponent;
            Attribute = new SampleAttribute(cityObjComponent.PrimaryCityObjects.First().AttributesMap);
            LodObjects = new GameObject[4];
        }

        /// <summary>
        /// フィルタリング
        /// パラメータに応じてオブジェクトを表示、非表示化します。
        /// </summary>
        /// <param name="parameter"></param>
        public void Filter(FilterParameter parameter)
        {
            if (Attribute.MeasuredHeight.HasValue)
            {
                foreach (var lod in LodObjects)
                {
                    if (lod == null) continue;
                    lod.SetActive(false);
                }

                var measuredHeight = Attribute.MeasuredHeight.Value;
                if (measuredHeight < parameter.MinHeight || measuredHeight > parameter.MaxHeight)
                {
                    return;
                }

                try
                {
                    int level = -1;
                    for (int i = 0; i < LodObjects.Length; ++i)
                    {
                        if (LodObjects[i] != null && i >= parameter.MinLod && i <= parameter.MaxLod) level = i;
                    }

                    if (level >= 0 && level <= 3)
                    {
                        LodObjects[level].SetActive(true);
                    }
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 色分け
        /// </summary>
        /// <param name="type"></param>
        /// <param name="colorTable"></param>
        /// <param name="areaName">浸水エリア名</param>
        public void ColorCode(ColorCodeType type, Color[] colorTable, string areaName)
        {
            switch (type)
            {
                case ColorCodeType.None:
                default:
                    SetMaterialColor(Color.white);
                    break;
                case ColorCodeType.Height:
                    ColorCodeByHeight(colorTable);
                    break;
                case ColorCodeType.FloodingRank:
                    ColorCodeByFloodingRank(colorTable, areaName);
                    break;
            }
        }


        private void ColorCodeByHeight(Color[] colorTable)
        {
            Assert.AreEqual(6, colorTable.Length, "高さの色分けは6色");

            if (!Attribute.MeasuredHeight.HasValue)
            {
                SetMaterialColor(Color.white);
                return;
            }

            var height = Attribute.MeasuredHeight.Value;
            if (height <= 12)
            {
                SetMaterialColor(colorTable[0]);
            }
            else if (height > 12 && height <= 31)
            {
                SetMaterialColor(colorTable[1]);
            }
            else if (height > 31 && height <= 60)
            {
                SetMaterialColor(colorTable[2]);
            }
            else if (height > 60 && height <= 120)
            {
                SetMaterialColor(colorTable[3]);
            }
            else if (height > 120 && height <= 180)
            {
                SetMaterialColor(colorTable[4]);
            }
            else
            {
                SetMaterialColor(colorTable[5]);
            }
        }

        private void ColorCodeByFloodingRank(Color[] colorTable, string areaName)
        {
            Assert.AreEqual(5, colorTable.Length, "ランクの色分けは5色");

            var infos = Attribute.GetFloodingAreaInfos();
            var index = infos.FindIndex(info => info.AreaName == areaName);
            if (index < 0)
            {
                SetMaterialColor(Color.white);
                return;
            }

            var info = infos[index];
            switch (info.Rank)
            {
                case 1:
                    SetMaterialColor(colorTable[0]);
                    break;
                case 2:
                    SetMaterialColor(colorTable[1]);
                    break;
                case 3:
                    SetMaterialColor(colorTable[2]);
                    break;
                case 4:
                    SetMaterialColor(colorTable[3]);
                    break;
                case 5:
                    SetMaterialColor(colorTable[4]);
                    break;
                default:
                    SetMaterialColor(Color.white);
                    break;
            }
        }

        public void SetMaterialColor(Color color)
        {
            foreach (var lod in LodObjects)
            {
                if (lod == null) continue;
                if (!lod.TryGetComponent<Renderer>(out var renderer)) continue;

                for (int i = 0; i < renderer.materials.Length; ++i)
                {
                    renderer.materials[i].color = color;
                }
            }
        }
    }
}