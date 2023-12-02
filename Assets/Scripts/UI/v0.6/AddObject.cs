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

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmAddEffect);
        confirm.AddToClassList("preferred");

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(CloseModal);

        Modal.AddButton(confirm);
        Modal.AddButton(cancel);
    }

    private static void ConfirmAddEffect(ClickEvent evt) {
        string value = Modal.Find().Q("ImageSearchField").Q<TextField>("SearchInput").value;
        Debug.Log(value);

        Modal.Close();
        
        // List<GameObject> selectedBlocks = Block.GetAllSelected();
        // List<string> blockNames = new();
        // for (int i = 0; i < selectedBlocks.Count; i++) {
        //     blockNames.Add(selectedBlocks[i].name);
        // }
        // Player.Self().CmdRequestMapSetValue(blockNames.ToArray(), "Effect", value);
    }

    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

}
