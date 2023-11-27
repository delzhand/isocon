using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AddTerrainEffect
{
    public static void OpenModal(ClickEvent evt) {

        VisualElement searchField = SearchField.Create(GameSystem.Current().GetEffectList(), "Add Terrain Effect");
        searchField.name = "SearchField";

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmAddEffect);
        confirm.AddToClassList("preferred");

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(CloseModal);

        Modal.Reset("Add Terrain Effect");
        Modal.AddContents(searchField);
        Modal.AddButton(confirm);
        Modal.AddButton(cancel);
    }

    private static void ConfirmAddEffect(ClickEvent evt) {
        string value = Modal.Find().Q("SearchField").Q<TextField>("SearchInput").value;
        Modal.Close();
        
        List<GameObject> selectedBlocks = Block.GetAllSelected();
        List<string> blockNames = new();
        for (int i = 0; i < selectedBlocks.Count; i++) {
            blockNames.Add(selectedBlocks[i].name);
        }
        Player.Self().CmdRequestMapSetValue(blockNames.ToArray(), "Effect", value);
    }

    private static void CloseModal(ClickEvent evt) {
        Modal.Close();
    }

}
