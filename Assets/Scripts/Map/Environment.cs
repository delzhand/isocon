using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Environment : MonoBehaviour
{
    public static Color TileTopColor = ColorUtility.GetColor("#3E713E");
    public static Color TileSideColor = ColorUtility.GetColor("#6F5A3C");
    public static Color BgBottomColor = ColorUtility.GetColor("#6d6d6d");
    public static Color BgTopColor = ColorUtility.GetColor("#989898");
    public static Color CurrentPaintTop = ColorUtility.GetColor("#CCBCA7");
    public static Color CurrentPaintSide = ColorUtility.GetColor("#CCBCA7");

    private static Material BackgroundMat;

    private static Color DarkenColor(Color originalColor, float percentage)
    {
        float darkenAmount = Mathf.Clamp01(percentage);
        Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
        return darkenedColor;
    }

    public static void SetTileColors(Color top, Color sides)
    {
        TileTopColor = top;
        TileSideColor = sides;
        BlockRendering.SetSharedMaterialColor("top1", top);
        BlockRendering.SetSharedMaterialColor("top2", DarkenColor(top, .2f));
        BlockRendering.SetSharedMaterialColor("side1", sides);
        BlockRendering.SetSharedMaterialColor("side2", DarkenColor(sides, .2f));
        UI.System.Q("TopBlockColor").style.backgroundColor = TileTopColor;
        UI.System.Q("SideBlockColor").style.backgroundColor = TileSideColor;
    }

    public static void SetBackgroundColors(Color bottom, Color top)
    {
        BgBottomColor = bottom;
        BgTopColor = top;
        MeshRenderer mr = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
        if (BackgroundMat == null)
        {
            BackgroundMat = Resources.Load<Material>($"Materials/Environment/Gradient");
        }
        List<Material> bgmats = new() { BackgroundMat };
        mr.SetMaterials(bgmats);

        mr.material.SetColor("_Color1", top);
        mr.material.SetColor("_Color2", bottom);
        UI.System.Q("BotBgColor").style.backgroundColor = BgBottomColor;
        UI.System.Q("TopBgColor").style.backgroundColor = BgTopColor;
    }
}
