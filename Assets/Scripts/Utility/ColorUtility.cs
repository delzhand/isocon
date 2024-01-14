using System;
using UnityEngine;

public class ColorUtility
{
    public static Color UIBlue {
        get {
            return ColorFromHex("2c5d87");
        }
    }

    public static Color UIOrange {
        get {
            return ColorFromHex("B78E1A");
        }
    }

    public static Color UIFocusBlue {
        get {
            return new Color(0, .445f, 1);
        }
    }

    public static Color UISelectYellow {
        get {
            return new Color(.914f, 1, 0);
        }
    }

    public static Color UISuccessGreen {
        get {
            return ColorFromHex("2c811b");
        }
    }

    public static Color UIErrorRed {
        get {
            return ColorFromHex("c05050");
        }
    }

    public static string ColorToHex(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static Color NormalizeRGB(int red, int green, int blue) {
        return new Color(red/255f, green/255f, blue/255f);
    }

    public static Color ColorFromHex(string hex) {
        if (hex == null) {
            return Color.white;
        }
        hex = hex.Replace("#", "").ToUpper();
        if (hex.Length != 6)
        {
            throw new Exception("Invalid hex color format. Please use the format '#RRGGBB'.");
        }

        string rHex = hex.Substring(0, 2);
        string gHex = hex.Substring(2, 2);
        string bHex = hex.Substring(4, 2);

        byte r = byte.Parse(rHex, System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(gHex, System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(bHex, System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }

    public static Color DarkenColor(Color originalColor, float percentage)
    {
        float darkenAmount = Mathf.Clamp01(percentage);
        Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
        return darkenedColor;
    }
}
