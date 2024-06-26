using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ColorEdit
{
    public static Color EditColor = Color.black;

    public delegate void ColorChangeCallback(Color newColor);
    public static ColorChangeCallback onColorChange;


    public static void Setup()
    {
        UI.System.Q("ColorPanel").Q<Button>("Close").RegisterCallback<ClickEvent>((evt) =>
        {
            UI.ToggleDisplay("ColorPanel", false);
        });

        UI.System.Q<TextField>("EditColorHex").RegisterValueChangedCallback<string>((evt) =>
        {
            try
            {
                Color c = ColorUtility.GetColor(evt.newValue);
                SetRGB(c);
                onColorChange?.Invoke(c);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        });

        UI.System.Q<SliderInt>("EditRed").RegisterValueChangedCallback<int>(SliderChange);
        UI.System.Q<SliderInt>("EditGreen").RegisterValueChangedCallback<int>(SliderChange);
        UI.System.Q<SliderInt>("EditBlue").RegisterValueChangedCallback<int>(SliderChange);

    }

    private static void SliderChange(ChangeEvent<int> evt)
    {
        Color c = FromSliders();
        SetHex(c);
        onColorChange?.Invoke(c);
    }

    public static void SetColor(Color c)
    {
        EditColor = c;
        SetRGB(c);
        SetHex(c);
    }

    private static void SetRGB(Color c)
    {
        UI.System.Q<SliderInt>("EditRed").value = Mathf.RoundToInt(c.r * 255);
        UI.System.Q<SliderInt>("EditGreen").value = Mathf.RoundToInt(c.g * 255);
        UI.System.Q<SliderInt>("EditBlue").value = Mathf.RoundToInt(c.b * 255);
    }

    private static void SetHex(Color c)
    {
        UI.System.Q<TextField>("EditColorHex").value = ColorUtility.GetHex(c);
    }

    public static Color FromSliders()
    {
        int r = UI.System.Q<SliderInt>("EditRed").value;
        int g = UI.System.Q<SliderInt>("EditGreen").value;
        int b = UI.System.Q<SliderInt>("EditBlue").value;
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    public static void ClearColorChangeListeners()
    {
        onColorChange = null;
    }

}
