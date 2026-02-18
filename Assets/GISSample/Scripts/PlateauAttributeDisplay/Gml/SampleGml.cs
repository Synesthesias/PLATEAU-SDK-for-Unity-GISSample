using PLATEAU.DynamicTile;
using System.Collections.Generic;
using UnityEngine;
using PLATEAU.Util;
using System.Collections;
using System;
using UnityEngine.UIElements;

namespace GISSample.PlateauAttributeDisplay.Gml
{
    /// <summary>
    /// フィルターパラメータ
    /// </summary>
    public struct FilterParameter
    {
        /// <summary>
        /// 最小高さ
        /// </summary>
        public float MinHeight;

        /// <summary>
        /// 最大高さ
        /// </summary>
        public float MaxHeight;

        /// <summary>
        /// 最小LOD
        /// </summary>
        public int MinLod;

        /// <summary>
        /// 最大LOD
        /// </summary>
        public int MaxLod;
    }


    /// <summary>
    /// 色分けタイプ
    /// </summary>
    public enum BuildingColorType
    {
        /// <summary>
        /// なし
        /// </summary>
        None,

        /// <summary>
        /// 高さ
        /// </summary>
        Height,

        /// <summary>
        /// 浸水ランク
        /// </summary>
        FloodingRank,
    }

    public class TileInfo
    {
        public PLATEAUDynamicTile Tile { get; }

        public string ChildName { get; }
        public TileInfo(PLATEAUDynamicTile tile, GameObject go)
        {
            Tile = tile;
            ChildName = go.name;
        }
    }


    /// <summary>
    /// GMLファイル1つに対応するゲームオブジェクトをサンプル上で扱いやすくしたラッパーです。
    /// </summary>
    public class SampleGml
    {
        public CityObjDict cityObjDict;
        public FloodingTitleSet FloodingTitles { get; private set; }
        public bool IsFlooding { get; private set; }

        public bool IsCached { get; set; } = false; // キャッシュから取得されたか・新規生成されたか (Tile用)

        public PLATEAUDynamicTile Tile => tileInfo != null ? tileInfo.Tile : null;

        public string ChildName => tileInfo != null ? tileInfo.ChildName : string.Empty;
        private TileInfo tileInfo;

        public SampleGml()
        {
        }

        public void Initialize(GameObject gmlGameObjArg)
        {
            FloodingTitles = new FloodingTitleSet();
            IsFlooding = gmlGameObjArg.name.Contains("fld");
            cityObjDict = new CityObjDict();
            cityObjDict.Initialize(gmlGameObjArg, this);
            FloodingTitles = cityObjDict.FindAllFloodingTitles();
        }

        public void InitializeTile(PLATEAUDynamicTile tile)
        {
            tileInfo = new TileInfo(tile, tile.LoadedObject);
            Initialize(tile.LoadedObject);
        }

        public IEnumerable<SemanticCityObject> GetCityObjects()
        {
            return cityObjDict.SemanticCityObjs();
        }

        public SemanticCityObject GetCityObject(string cityObjId)
        {
            return cityObjDict.Get(cityObjId);
        }

        public SampleAttribute GetAttribute(string cityObjID)
        {
            var cityObj = GetCityObject(cityObjID);
            return cityObj.Attribute;
        }

        public IEnumerable<FeatureGameObj> FeatureGameObjs()
        {
            return cityObjDict.FeatureGameObjs();
        }

        public IEnumerable<SemanticCityObject> SemanticCityObjs()
        {
            return cityObjDict.SemanticCityObjs();
        }
    }

}
