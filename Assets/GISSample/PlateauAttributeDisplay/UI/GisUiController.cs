using System.Collections.Generic;
using System.Linq;
using GISSample.PlateauAttributeDisplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GisUiController : MonoBehaviour
{
    private MenuUi menuUi;
    public MenuUi MenuUi => menuUi;
    [SerializeField, Tooltip("操作説明")] private UIDocument userGuideUi;
    private AttributeUi attrUi;
    private TimeUi timeUi;

    private SceneManager sceneManager;
    
    

    [SerializeField, Tooltip("選択中オブジェクトの色")] private Color selectedColor;
    [SerializeField, Tooltip("色分け（高さ）の色テーブル")] private Color[] heightColorTable;
    [SerializeField, Tooltip("色分け（浸水ランク）の色テーブル")] private Color[] floodingRankColorTable;
    
    
    /// <summary>
    /// 色分けタイプ
    /// </summary>
    private ColorCodeType colorCodeType;
    
    /// <summary>
    /// 浸水エリア名（色分け用）
    /// </summary>
    private string floodingAreaName;

    public void Init(SceneManager sceneManagerArg)
    {
        sceneManager = sceneManagerArg;
        menuUi = GetComponentInChildren<MenuUi>();
        attrUi = GetComponentInChildren<AttributeUi>();
        timeUi = FindObjectOfType<TimeUi>();

        attrUi.Close();
        userGuideUi.gameObject.SetActive(true);
        menuUi.Init(this, sceneManagerArg);
        
        
        ColorCity(colorCodeType, floodingAreaName);
        
        
    }
    
    /// <summary>
    /// オブジェクトのピック
    /// マウスの位置からレイキャストしてヒットしたオブジェクトのTransformを返します。
    /// </summary>
    /// <returns>Transform</returns>
    private Transform PickObject()
    {
        var cam = Camera.main;
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        // 一番手前のオブジェクトを選びます。
        float nearestDistance = float.MaxValue;
        Transform nearestTransform = null;
        foreach (var hit in Physics.RaycastAll(ray))
        {
            var hitTrans = hit.transform;
            if (hitTrans.name.Contains("Cesium")) continue;
            if (hit.distance <= nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestTransform = hitTrans;
            }
        }

        return nearestTransform;
    }
    
    
    /// <summary>
    /// オブジェクト選択
    /// </summary>
    /// <param name="context"></param>
    public void OnSelectObject(InputAction.CallbackContext context)
    {
        if (context.performed && !IsMousePositionInUiRect())
        {
            var trans = PickObject();
            if (trans == null)
            {
                attrUi.Close();
                return;
            };

            // 前回選択中のオブジェクトの色を戻すために色分け処理を実行
            RecolorFlooding();

            // 選択されたオブジェクトの色を変更
            var nameKey = trans.parent.parent.name;
            if (nameKey.Contains("Cesium")) return;
            attrUi.SelectCityObj(sceneManager.gmls.GetCityObject(nameKey, trans.name), selectedColor);

            attrUi.Open();

            var data = GetAttribute(nameKey, trans.name);
            attrUi.SetAttributes(data);

                
        }
    }
    
    
    /// <summary>
    /// マウスの位置がUI上にあるかどうか
    /// </summary>
    /// <returns></returns>
    public bool IsMousePositionInUiRect()
    {
        var pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        foreach (var r in raycastResults)
        {
            if (r.gameObject.name == "GISSamplePanelSettings")
            {
                return true;
            }
        }

        return false;
    }
    
    
    /// <summary>
    /// 属性情報を取得
    /// </summary>
    /// <param name="gmlFileName">GMLファイル名</param>
    /// <param name="cityObjectID">CityObjectID</param>
    /// <returns>属性情報</returns>
    private SampleAttribute GetAttribute(string gmlFileName, string cityObjectID)
    {
        return sceneManager.gmls.GetAttribute(gmlFileName, cityObjectID);
    }
    
    /// <summary>
    /// 色分け処理
    /// </summary>
    public void ColorCity(ColorCodeType type, string areaName)
    {
        sceneManager.gmls.ColorCity(type, areaName, heightColorTable, floodingRankColorTable);
    }
    
    /// <summary>
    /// 色分け選択変更イベントコールバック
    /// </summary>
    /// <param name="e"></param>
    public void OnColorCodeGroupValueChanged(ChangeEvent<int> e)
    {
        // valueは
        // 0: 色分けなし
        // 1: 高さ
        // 2～: 浸水ランク
        if (e.newValue < 2)
        {
            colorCodeType = (ColorCodeType)e.newValue;
            floodingAreaName = null;
        }
        else
        {
            colorCodeType = ColorCodeType.FloodingRank;
            floodingAreaName = menuUi.colorCodeGroup.choices.ElementAt(e.newValue);
        }

        RecolorFlooding();
    }

    public void RecolorFlooding()
    {
        ColorCity(colorCodeType, floodingAreaName);
    }

    

    
}
