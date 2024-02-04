using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Config
{
    public static void OpenModal(ClickEvent evt)
    {
        Modal.Reset("Configuration");

#if !UNITY_WEBGL
        string path = Preferences.Current.DataPath;
        Modal.AddTextField("DataPath", "Data Path", path, (evt) =>
        {
            Preferences.SetDataPath(evt.newValue);
        });
#endif

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

        float borderOpacity = Preferences.Current.BlockBorderOpacity;
        Modal.AddFloatSlider("BlockBorderOpacity", "Block Border Minimum", borderOpacity, 1, 0, (evt) =>
        {
            Preferences.SetBlockBorderOpacity(evt.newValue);
        });

        string tokenOutline = Preferences.Current.TokenOutline;
        Modal.AddDropdownField("TokenOutlineField", "Token Outline", tokenOutline, StringUtility.CreateArray("White", "Black", "None"), (evt) =>
        {
            Preferences.SetTokenOutline(evt.newValue);
            Token.SetAllTokenOutlines();
        });

#if !UNITY_WEBGL
        string rulesFile = Preferences.Current.RulesFile;
        Modal.AddDropdownField("RulesFileField", "Rules File", rulesFile, GetAllRuleFiles(), (evt) =>
        {
            Preferences.SetRulesFile(evt.newValue);
        });
        Modal.AddMarkup("RulesDesc", "Rules are applied only when entering solo or host mode. Client mode will use the host's rules.");
#endif

        Modal.AddPreferredButton("Confirm", CloseModal);
    }

    private static void CloseModal(ClickEvent evt)
    {
        Modal.Close();
    }

    public static string[] GetAllRuleFiles()
    {
        List<string> ruleFiles = new List<string>();
        FileUtility.GetFilesRecursively(Preferences.Current.DataPath, "/ruledata", ruleFiles);

        for (int i = 0; i < ruleFiles.Count; i++)
        {
            ruleFiles[i] = ruleFiles[i].Replace("/ruledata/", "");
        }

        return ruleFiles.ToArray();
    }


}
