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

    public static VisualElement Create(string name, Color initial) {
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

        SetRGB(initial, element);
        UpdatePreview(element);
        SetHex(initial, element);

        return element;
    }

    private static void SliderChange(ChangeEvent<int> evt) {
        Color c = FromSliders();
        SetHex(c);
        UpdatePreview();
        MapEdit.ColorChanged();
        // onColorChange?.Invoke(c);
    }

    private static void SetRGB(Color c, VisualElement element = null) {
        element ??= Modal.Find();
        element.Q<SliderInt>("EditGreen").value = Mathf.RoundToInt(c.g * 255);
        element.Q<SliderInt>("EditRed").value = Mathf.RoundToInt(c.r * 255);
        element.Q<SliderInt>("EditBlue").value = Mathf.RoundToInt(c.b * 255);
    }

    public static Color FromSliders(VisualElement element = null) {
        element ??= Modal.Find();
        int r = element.Q<SliderInt>("EditRed").value;
        int g = element.Q<SliderInt>("EditGreen").value;
        int b = element.Q<SliderInt>("EditBlue").value;
        return new Color(r/255f, g/255f, b/255f);
    }

    private static void SetHex(Color c, VisualElement element = null) {
        element ??= Modal.Find();
        element.Q<TextField>("EditColorHex").value = ColorUtility.ColorToHex(c);
    }

    private static void UpdatePreview(VisualElement element = null) {
        element ??= Modal.Find();
        element.Q("Preview").style.backgroundColor = FromSliders(element);
    }
}
