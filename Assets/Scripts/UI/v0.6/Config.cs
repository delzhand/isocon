using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Config
{
    public static void OpenModal(ClickEvent evt)
    {
        Modal.Reset("Configuration");

        string path = Preferences.Current.DataPath;
        Modal.AddTextField("DataPath", "Data Path", path, (evt) =>
        {
            Preferences.SetDataPath(evt.newValue);
        });

        string uiScale = Preferences.Current.UIScale;
        List<string> scaleOptions = new();
        for (int i = 75; i <= 250; i += 25)
        {
            scaleOptions.Add(i + "%");
        }
        Modal.AddDropdownField("UIScaleField", "UI Scale", uiScale, scaleOptions.ToArray(), (evt) =>
        {
            Preferences.SetUIScale(evt.newValue);
            float value = float.Parse(evt.newValue.Replace("%", "")) / 100f;
            GameObject.Find("UICanvas/SystemUI").GetComponent<UIDocument>().panelSettings.scale = value;
        });

        float tokenScale = Preferences.Current.TokenScale;
        Modal.AddFloatSlider("TokenScaleField", "Token Scale", tokenScale, 2f, .5f, (evt) =>
        {
            Preferences.SetTokenScale(evt.newValue);
        });

        string tokenOutline = Preferences.Current.TokenOutline;
        Modal.AddDropdownField("TokenOutlineField", "Token Outline", tokenOutline, StringUtility.CreateArray("White", "Black", "None"), (evt) =>
        {
            Preferences.SetTokenOutline(evt.newValue);
        });

        Modal.AddPreferredButton("Confirm", CloseModal);
    }

    private static void CloseModal(ClickEvent evt)
    {
        Modal.Close();
    }

}
