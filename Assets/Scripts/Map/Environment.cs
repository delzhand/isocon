using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Palette {
    GREENFIELD,
    BLUECAVE,
    ARKENDEPTH,
    PYROFLOW,
    ICECASTLE,
}

public enum Background {
    SUNSHINE,
    NIGHTTIME,
    SEASIDE,
    ARKENDEPTH,
    SANDSTORM,
    FANTASIA,
    DEEPHEAT,
    CLOUDTOP
}

public class Environment : MonoBehaviour
{
    private static Background background = Background.SUNSHINE;
    private static Palette palette = Palette.GREENFIELD;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private static Color FromRGB(int r, int g, int b) {
        return new Color(r/255f, g/255f, b/255f);
    }

    public static void SetBackground(Background background) {
        Environment.background = background;

        Dictionary<Background,(Color, Color)> gradients = new Dictionary<Background, (Color, Color)>();
        gradients.Add(Background.SUNSHINE, (Environment.FromRGB(177, 214, 128), Environment.FromRGB(38, 113, 156)));
        gradients.Add(Background.NIGHTTIME, (Environment.FromRGB(0, 0, 0), Environment.FromRGB(0, 38, 60)));
        gradients.Add(Background.SEASIDE, (Environment.FromRGB(38, 113, 156), Environment.FromRGB(234, 215, 129)));
        gradients.Add(Background.SANDSTORM, (Environment.FromRGB(224, 221, 160), Environment.FromRGB(161, 161, 148)));
        gradients.Add(Background.ARKENDEPTH, (Environment.FromRGB(0, 0, 0), Environment.FromRGB(0, 108, 0)));
        gradients.Add(Background.FANTASIA, (Environment.FromRGB(8, 0, 209), Environment.FromRGB(0, 108, 221)));
        gradients.Add(Background.DEEPHEAT, (Environment.FromRGB(68, 0, 0), Environment.FromRGB(104, 60, 21)));

        MeshRenderer mr = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
        mr.material.SetColor("_Color1", gradients[background].Item1);
        mr.material.SetColor("_Color2", gradients[background].Item2);
    }

    public static Background GetBackground() {
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

        Block.SetColor("top1", palettes[palette].Item1);
        Block.SetColor("top2", DarkenColor(palettes[palette].Item1, .2f));
        Block.SetColor("side1", palettes[palette].Item2);
        Block.SetColor("side2", DarkenColor(palettes[palette].Item2, .2f));
    }

    public static Color DarkenColor(Color originalColor, float percentage)
    {
        float darkenAmount = Mathf.Clamp01(percentage);
        Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
        return darkenedColor;
    }

}
