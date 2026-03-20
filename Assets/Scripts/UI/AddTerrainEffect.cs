using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AddTerrainEffect
{
    static Block singleBlock;

    public static void OpenModal(Block b)
    {
        singleBlock = b;
        Modal.Reset("Add Terrain Effect");
        // Modal.AddSearchField("SearchField", "Effect Name", "", GameSystem.Current().GetEffectList());
        Modal.AddDropdownField("VisualMarker", "Visual Marker", "None", StringUtility.CreateArray("None", "Spiky", "Wavy", "Hole", "Hand", "Skull", "Blocked", "Corners", "Border"));
        Modal.AddDropdownField("Color", "Color", "None", StringUtility.CreateArray("Black", "White", "Yellow", "Red", "Blue", "Green"));

        Modal.AddPreferredButton("Confirm", ConfirmAddEffect);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void ConfirmAddEffect(ClickEvent evt)
    {
        string marker = UI.Modal.Q<DropdownField>("VisualMarker").value;
        string color = UI.Modal.Q<DropdownField>("Color").value;
        Modal.Close();

        List<string> blockNames = new();
        if (singleBlock != null)
        {
            blockNames.Add(singleBlock.name);
        }
        else
        {
            List<Block> selected = Block.GetSelected().ToList();
            selected.ForEach(block =>
            {
                blockNames.Add(block.name);
            });
        }
        string command = $"???::{marker}::{color}";
        Debug.Log(command);
        Player.Self().CmdRequestMapSetValue(blockNames.ToArray(), "AddEffect", command);
    }
}
