using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AddObject
{
    public static void OpenModal(ClickEvent evt) {

        VisualElement imageSearchField = SearchField.Create(AddToken.GetImageOptions(), "Add Object");
        imageSearchField.name = "ImageSearchField";

        Toggle blockEffects = new Toggle();
        blockEffects.name = "BlockEffects";
        blockEffects.label = "Block Terrain Effects";
        blockEffects.AddToClassList("no-margin");
        blockEffects.focusable = false;

        DropdownField height = new DropdownField();
        height.name = "ObjectHeight";
        height.label = "Height";
        height.choices = new List<string>();
        for (int i = 0; i <= 3; i++) {
            height.choices.Add(i.ToString());
        }
        height.focusable = false;
        height.AddToClassList("no-margin");

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmAddEffect);
        confirm.AddToClassList("preferred");

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(CloseModal);

        Modal.Reset("Add Object");
        Modal.AddContents(imageSearchField);
        Modal.AddContents(height);
        Modal.AddContents(blockEffects);
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
