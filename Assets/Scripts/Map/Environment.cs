using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum Palette {
    GREENFIELD,
    BLUECAVE,
    ARKENDEPTH,
    PYROFLOW,
    ICECASTLE,
}

public enum BackgroundGradient {
    FANTASIA,
    SUNSHINE,
    NIGHTTIME,
    SEASIDE,
    ARKENDEPTH,
    SANDSTORM,
    DEEPHEAT,
    CLOUDTOP
}

public class Environment : MonoBehaviour
{
    private static BackgroundGradient background = BackgroundGradient.FANTASIA;
    private static Palette palette = Palette.GREENFIELD;
    
    // Start is called before the first frame update
    void Start()
    {
        setup();
    }

    private void setup() {
        UI.System.Q<EnumField>("BackgroundEnum").RegisterValueChangedCallback((evt) => {
            SetBackground((BackgroundGradient)evt.newValue);
        });

        UI.System.Q<EnumField>("PaletteEnum").RegisterValueChangedCallback((evt) => {
            SetPalette((Palette)evt.newValue);
        });
    }


    public static Color FromRGB(int r, int g, int b) {
        return new Color(r/255f, g/255f, b/255f);
    }

    public static Color FromHex(string hex) {
        hex = hex.Replace("#", "").ToUpper();
        if (hex.Length != 6)
        {
            Debug.LogError("Invalid hex color format. Please use the format '#RRGGBB'.");
            return Color.white; // Default to white color
        }

        string rHex = hex.Substring(0, 2);
        string gHex = hex.Substring(2, 2);
        string bHex = hex.Substring(4, 2);

        byte r = byte.Parse(rHex, System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(gHex, System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(bHex, System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    public static void SetBackground(BackgroundGradient background) {
        Environment.background = background;

        Dictionary<BackgroundGradient,(Color, Color)> gradients = new Dictionary<BackgroundGradient, (Color, Color)>();
        gradients.Add(BackgroundGradient.SUNSHINE, (Environment.FromRGB(177, 214, 128), Environment.FromRGB(38, 113, 156)));
        gradients.Add(BackgroundGradient.NIGHTTIME, (Environment.FromRGB(0, 0, 0), Environment.FromRGB(0, 38, 60)));
        gradients.Add(BackgroundGradient.SEASIDE, (Environment.FromRGB(38, 113, 156), Environment.FromRGB(234, 215, 129)));
        gradients.Add(BackgroundGradient.SANDSTORM, (Environment.FromRGB(224, 221, 160), Environment.FromRGB(161, 161, 148)));
        gradients.Add(BackgroundGradient.ARKENDEPTH, (Environment.FromRGB(0, 0, 0), Environment.FromRGB(0, 108, 0)));
        gradients.Add(BackgroundGradient.FANTASIA, (Environment.FromHex("#474571"), Environment.FromHex("#001022")));
        gradients.Add(BackgroundGradient.DEEPHEAT, (Environment.FromRGB(68, 0, 0), Environment.FromRGB(104, 60, 21)));

        SetBackgroundColors(gradients[background].Item1,  gradients[background].Item2);
    }

    public static BackgroundGradient GetBackground() {
        return Environment.background;
    }

    public static Palette GetPalette() {
        return Environment.palette;
    }

    public static void SetPalette(Palette palette) {
        Environment.palette = palette;
        Dictionary<Palette,(Color, Color)> palettes = new Dictionary<Palette, (Color, Color)>();
        // Item1 = top
        // Item2 = side
        palettes.Add(Palette.GREENFIELD, (Environment.FromRGB(62, 113, 62), Environment.FromRGB(111, 90, 60)));
        palettes.Add(Palette.BLUECAVE, (Environment.FromRGB(62, 76, 84), Environment.FromRGB(62, 76, 113)));
        palettes.Add(Palette.ARKENDEPTH, (Environment.FromRGB(92, 92, 92), Environment.FromRGB(106, 154, 10)));
        palettes.Add(Palette.PYROFLOW, (Environment.FromRGB(35, 35, 35), Environment.FromRGB(149, 22, 32)));
        palettes.Add(Palette.ICECASTLE, (Environment.FromRGB(60, 130, 200), Environment.FromRGB(176, 176, 176)));

        SetTileColors(palettes[palette].Item1, palettes[palette].Item2);
    }

    public static Color DarkenColor(Color originalColor, float percentage)
    {
        float darkenAmount = Mathf.Clamp01(percentage);
        Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
        return darkenedColor;
    }

    public static void SetTileColors(Color top, Color sides) {
        Block.SetColor("top1", top);
        Block.SetColor("top2", DarkenColor(top, .2f));
        Block.SetColor("side1", sides);
        Block.SetColor("side2", DarkenColor(sides, .2f));
    }

    public static void SetBackgroundColors(Color bottom, Color top) {
        MeshRenderer mr = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
        mr.material.SetColor("_Color1", bottom);
        mr.material.SetColor("_Color2", top);
    }

}
