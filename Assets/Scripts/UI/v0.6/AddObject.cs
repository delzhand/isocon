using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AddObject
{
    public static void OpenModal(ClickEvent evt) {

        Modal.Reset("Add Object");
        Modal.AddSearchField("ImageSearchField", "Add Object", "", AddToken.GetImageOptions());
        Modal.AddToggleField("SuppressTileEffects", "Suppress Tile Effects", false);
        Modal.AddDropdownField("ObjectHeight", "Height", "1", new string[]{"1", "2", "3", "4", "5"});
        Modal.AddPreferredButton("Confirm", ConfirmAddEffect);
        Modal.AddButton("Cancel", CloseModal);
    }

    private static void ConfirmAddEffect(ClickEvent evt) {
        string value = Modal.Find().Q("ImageSearchField").Q<TextField>("SearchInput").value;
        Modal.Close();
    }

    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

}
