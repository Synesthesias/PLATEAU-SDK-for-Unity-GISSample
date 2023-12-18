using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GISAttrDisplay : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private Image background;
    [SerializeField] private MeshRenderer pillar;
    public void SetContent(string content)
    {
        text.text = content;
    }

    public void SetColor(Color bgColor, Color textColor, Color pillarColor)
    {
        background.color = bgColor;
        text.color = textColor;
        var mat = pillar.sharedMaterial;
        mat.color = pillarColor;
        pillar.sharedMaterial = mat;
    }
}
