using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AddObject
{
    public static void OpenModal(ClickEvent evt)
    {

        Modal.Reset("Add Object");
        string[] imageOptions = AddToken.GetImageOptions();
        if (imageOptions.Length > 0)
        {
            Modal.AddSearchField("ImageSearchField", "Add Object", "", AddToken.GetImageOptions());
            Modal.AddToggleField("SuppressTileEffects", "Suppress Tile Effects", false);
            Modal.AddDropdownField("ObjectHeight", "Height", "1", new string[] { "1", "2", "3", "4", "5" });
            Modal.AddPreferredButton("Confirm", ConfirmAddEffect);
        }
        else
        {
            string path = Preferences.Current.DataPath;
            Modal.AddLabel($"No images were found. Token images must be added to {path}/tokens (this can be changed in configuration).", "error-message");
        }
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void ConfirmAddEffect(ClickEvent evt)
    {
        string value = UI.Modal.Q("ImageSearchField").Q<TextField>("SearchInput").value;
        Modal.Close();
    }
}
