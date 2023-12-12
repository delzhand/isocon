using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorField
{
    // public delegate void ColorChangeCallback(Color newColor);
    // public static ColorChangeCallback onColorChange;

    public static string CurrentName;

    public static VisualElement Create(string name) {
        VisualElement element = UI.CreateFromTemplate("UITemplates/ColorSelect");
        CurrentName = name;

        element.Q<TextField>("EditColorHex").RegisterValueChangedCallback<string>((evt) => {
            try {
                Color c = ColorUtility.ColorFromHex(evt.newValue);
                SetRGB(c);
                UpdatePreview();
                MapEdit.ColorChanged();
                // onColorChange?.Invoke(c);
            }
            catch (Exception e) {
                Debug.LogWarning(e);
            }
        });

        element.Q<SliderInt>("EditRed").RegisterValueChangedCallback<int>(SliderChange);
        element.Q<SliderInt>("EditGreen").RegisterValueChangedCallback<int>(SliderChange);
        element.Q<SliderInt>("EditBlue").RegisterValueChangedCallback<int>(SliderChange);

        return element;
    }

    private static void SliderChange(ChangeEvent<int> evt) {
        Color c = FromSliders();
        SetHex(c);
        UpdatePreview();
        MapEdit.ColorChanged();
        // onColorChange?.Invoke(c);
    }


    private static void SetRGB(Color c) {
        UI.System.Q("ColorSelect").Q<SliderInt>("EditGreen").value = Mathf.RoundToInt(c.g * 255);
        UI.System.Q("ColorSelect").Q<SliderInt>("EditRed").value = Mathf.RoundToInt(c.r * 255);
        UI.System.Q("ColorSelect").Q<SliderInt>("EditBlue").value = Mathf.RoundToInt(c.b * 255);
    }

    public static Color FromSliders() {
        int r = UI.System.Q("ColorSelect").Q<SliderInt>("EditRed").value;
        int g = UI.System.Q("ColorSelect").Q<SliderInt>("EditGreen").value;
        int b = UI.System.Q("ColorSelect").Q<SliderInt>("EditBlue").value;
        return new Color(r/255f, g/255f, b/255f);
    }

    private static void SetHex(Color c) {
        UI.System.Q("ColorSelect").Q<TextField>("EditColorHex").value = ColorUtility.ColorToHex(c);
    }

    private static void UpdatePreview() {
        UI.System.Q("ColorSelect").Q("Preview").style.backgroundColor = FromSliders();
    }
}
