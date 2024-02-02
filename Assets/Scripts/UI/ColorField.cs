using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorField
{
    public static string CurrentName;

    public static VisualElement Create(string name, Color initial)
    {
        VisualElement element = UI.CreateFromTemplate("UITemplates/ColorSelect");
        CurrentName = name;

        element.Q<TextField>("EditColorHex").RegisterValueChangedCallback<string>((evt) =>
        {
            try
            {
                Color c = ColorUtility.GetColor(evt.newValue);
                SetRGB(c);
                UpdatePreview(c);
                MapEdit.ColorChanged();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        });

        element.Q<SliderInt>("EditRed").RegisterValueChangedCallback<int>(SliderChange);
        element.Q<SliderInt>("EditGreen").RegisterValueChangedCallback<int>(SliderChange);
        element.Q<SliderInt>("EditBlue").RegisterValueChangedCallback<int>(SliderChange);

        SetRGB(initial, element);
        SetHex(initial, element);
        UpdatePreview(initial, element);

        return element;
    }

    private static void SliderChange(ChangeEvent<int> evt)
    {
        Color c = FromSliders();
        SetHex(c);
        UpdatePreview(c);
        MapEdit.ColorChanged();
    }

    private static void SetRGB(Color c, VisualElement element = null)
    {
        element ??= UI.Modal;
        element.Q<SliderInt>("EditGreen").value = Mathf.RoundToInt(c.g * 255);
        element.Q<SliderInt>("EditRed").value = Mathf.RoundToInt(c.r * 255);
        element.Q<SliderInt>("EditBlue").value = Mathf.RoundToInt(c.b * 255);
    }

    public static Color FromSliders(VisualElement element = null)
    {
        element ??= UI.Modal;
        int r = element.Q<SliderInt>("EditRed").value;
        int g = element.Q<SliderInt>("EditGreen").value;
        int b = element.Q<SliderInt>("EditBlue").value;
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    private static void SetHex(Color c, VisualElement element = null)
    {
        element ??= UI.Modal;
        element.Q<TextField>("EditColorHex").value = ColorUtility.GetHex(c);
    }

    private static void UpdatePreview(Color c, VisualElement element = null)
    {
        element ??= UI.Modal;
        element.Q("Preview").style.backgroundColor = c;
    }
}
