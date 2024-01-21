using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AddTerrainEffect
{
    public static void OpenModal(ClickEvent evt) {
        Modal.Reset("Add Terrain Effect");
        Modal.AddSearchField("SearchField", "Effect Name", "", GameSystem.Current().GetEffectList());
        Modal.AddDropdownField("VisualMarker", "Visual Marker", "None", StringUtility.Arr("None", "Spiky", "Wavy", "Hole", "Hand", "Skull", "Blocked", "Corners", "Border"));
        Modal.AddDropdownField("Color", "Color", "None", StringUtility.Arr("Black", "White", "Yellow", "Red", "Blue", "Green"));

        Modal.AddPreferredButton("Confirm", ConfirmAddEffect);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void ConfirmAddEffect(ClickEvent evt) {
        string effect = UI.Modal.Q("SearchField").Q<TextField>("SearchInput").value;
        string marker = UI.Modal.Q<DropdownField>("VisualMarker").value;
        string color = UI.Modal.Q<DropdownField>("Color").value;
        Modal.Close();
        
        List<Block> selected = Block.GetSelected().ToList();
        List<string> blockNames = new();
        selected.ForEach(block => {
            blockNames.Add(block.name);
        });
        Player.Self().CmdRequestMapSetValue(blockNames.ToArray(), "Effect", $"{effect}::{marker}::{color}");
    }

    public static void ClearAll() {
        List<string> blocks = new();
        foreach(Block b in Block.GetSelected()) {
            blocks.Add(b.name);
        }
        Player.Self().CmdRequestMapSetValue(blocks.ToArray(), "Effect", "None");

    }
}
