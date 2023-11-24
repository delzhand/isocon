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

        SliderInt height = new SliderInt();
        height.name = "ObjectHeight";
        height.label = "Height";
        height.lowValue = 0;
        height.highValue = 3;
        height.AddToClassList("no-margin");

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmAddEffect);
        confirm.AddToClassList("preferred");

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(CloseModal);

        Modal.Reset("Open Map");
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
