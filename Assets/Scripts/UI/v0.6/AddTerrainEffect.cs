using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AddTerrainEffect
{
    public static void OpenModal(ClickEvent evt) {
        Modal.Reset("Add Terrain Effect");
        Modal.AddSearchField("SearchField", "Add Terrain Effect", "", GameSystem.Current().GetEffectList());
        Modal.AddDropdownField("VisualMarker", "Visual Marker", "None", StringUtility.Arr("None", "Spiky", "Wavy", "Hole", "Hand", "Skull", "Blocked", "Corners"));
        Modal.AddPreferredButton("Confirm", ConfirmAddEffect);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void ConfirmAddEffect(ClickEvent evt) {
        string effect = UI.Modal.Q("SearchField").Q<TextField>("SearchInput").value;
        string marker = UI.Modal.Q<DropdownField>("VisualMarker").value;
        Modal.Close();
        
        List<Block> selected = Block.GetSelected().ToList();
        List<string> blockNames = new();
        selected.ForEach(block => {
            blockNames.Add(block.name);
        });
        Player.Self().CmdRequestMapSetValue(blockNames.ToArray(), "Effect", $"{effect}::{marker}");
    }
}
