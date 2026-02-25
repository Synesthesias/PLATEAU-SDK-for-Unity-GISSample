using GISSample.PlateauAttributeDisplay.Gml;
using System;
using System.Collections;
using UnityEngine;

namespace GISSample.PlateauAttributeDisplay
{
    /// <summary>
    /// ColorChangerByAttribute, FilterByLodAndHeight, TextureSwitcher の処理を一括で行う
    /// </summary>
    public class AllUpdateOperations
    {

        private readonly TextureSwitcher textureSwitcher;
        private readonly FilterByLodAndHeight filterByLodAndHeight;
        private readonly ColorChangerByAttribute colorChangerByAttribute;

        public AllUpdateOperations(TextureSwitcher t, FilterByLodAndHeight f, ColorChangerByAttribute c)
        {
            textureSwitcher = t;
            filterByLodAndHeight = f;
            colorChangerByAttribute = c;
        }

        // Height/Lod Filter , Material Color , Texture On/OFFの一括処理
        public IEnumerator HandleOperations(SampleGml gml)
        {
            if (gml.Tile != null && gml.Tile.LoadedObject == null)
                yield break;

            bool isTextureOn = textureSwitcher.IsTextureOn;

            var buildingColorType = colorChangerByAttribute.BuildingColorType;
            var floodingTile = colorChangerByAttribute.FloodingTitle;
            var colorTable = colorChangerByAttribute.GetColorTable(buildingColorType);

            var filterParameter = filterByLodAndHeight.GetFilterParameterFromSliders();
            var semantics = gml.SemanticCityObjs();

            int count = 0;
            foreach (var semantic in semantics)
            {

                // マテリアル色変更
                colorChangerByAttribute.ChangeBuildingsSemantic(semantic, buildingColorType, floodingTile, colorTable);

                // LODでのフィルタ
                int maxLodExist = semantic.MaxLodExist;
                int maxLodToShow = Math.Min(maxLodExist, filterParameter.MaxLod);
                foreach(var feature in semantic.FeatureGameObjs())
                {
                    filterByLodAndHeight.FilterFeatureGameObj(feature, filterParameter, maxLodToShow);


                    // 色がついていない場合は、念のためInitialMaterialに戻す（色がついている場合は、フィルタリング後も色を維持する）
                    if (buildingColorType == BuildingColorType.None)
                        feature.RestoreInitialMaterials();

                    // Texture ON/OFF
                    feature.TextureOnOff(isTextureOn);
                }

                // コルーチンステップ
                count++;
                if (count > GISTileManager.YIELD_STEP)
                {
                    count = 0;
                    yield return null;
                }
            }

            yield return null;

            gml.IsDirty = false; //２回目の処理は行わないフラグ
        }

    }
}
