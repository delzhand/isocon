using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorSidebar : MonoBehaviour
{

    public static Color EditColor = Color.black;

    public delegate void ColorChangeCallback(Color newColor);
    public ColorChangeCallback onColorChange;

    // Start is called before the first frame update
    void Start()
    {
        UI.System.Q<Button>("CloseColorSidebar").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("ColorSidebar", false);
        });

        UI.System.Q<TextField>("EditColorHex").RegisterValueChangedCallback<string>((evt) => {
            try {
                Color c = FromHex(evt.newValue);
                SetRGB(c);
                onColorChange?.Invoke(c);
            }
            catch (Exception e) {
                Debug.LogWarning(e);
            }
        });

        UI.System.Q<SliderInt>("EditRed").RegisterValueChangedCallback<int>(SliderChange);
        UI.System.Q<SliderInt>("EditGreen").RegisterValueChangedCallback<int>(SliderChange);
        UI.System.Q<SliderInt>("EditBlue").RegisterValueChangedCallback<int>(SliderChange);
    }

    private void SliderChange(ChangeEvent<int> evt) {
        Color c = FromSliders();
        SetHex(c);
        onColorChange?.Invoke(c);
    }

    public static void SetColor(Color c) {
        EditColor = c;
        SetRGB(c);
        SetHex(c);
    }

    private static void SetRGB(Color c) {
        UI.System.Q<SliderInt>("EditRed").value = Mathf.RoundToInt(c.r * 255);
        UI.System.Q<SliderInt>("EditGreen").value = Mathf.RoundToInt(c.g * 255);
        UI.System.Q<SliderInt>("EditBlue").value = Mathf.RoundToInt(c.b * 255);
    }

    private static void SetHex(Color c) {
        UI.System.Q<TextField>("EditColorHex").value = ColorToHex(c);
    }

    public static string ColorToHex(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static Color FromHex(string hex) {
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

    public static Color FromSliders() {
        int r = UI.System.Q<SliderInt>("EditRed").value;
        int g = UI.System.Q<SliderInt>("EditGreen").value;
        int b = UI.System.Q<SliderInt>("EditBlue").value;
        return new Color(r/255f, g/255f, b/255f);
    }

    public void ClearColorChangeListeners()
    {
        onColorChange = null;
    }

    public static Color DarkenColor(Color originalColor, float percentage)
    {
        float darkenAmount = Mathf.Clamp01(percentage);
        Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
        return darkenedColor;
    }
}
