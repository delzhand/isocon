using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Palette {
    GREENFIELD,
    BLUECAVE,
}

public enum Background {
    SUNSHINE,
    NIGHTTIME,
    SEASIDE,
    ARKENDEPTHS,
    SANDSTORM,
    FANTASIA,
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
        gradients.Add(Background.ARKENDEPTHS, (Environment.FromRGB(0, 0, 0), Environment.FromRGB(108, 0, 0)));
        gradients.Add(Background.FANTASIA, (Environment.FromRGB(8, 0, 209), Environment.FromRGB(0, 108, 221)));

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
        
    }

}
