using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class AddTerrainEffect
{
    static Block singleBlock;

    public static void OpenModal(Block b)
    {
        singleBlock = b;

        var dialog = Modal2.SetCurrentDialog("ShunDialog1");
        Modal2.SetCloseAction(StateManager.ToNeutral);
        var dialogContent = Modal2.Contents("ShunDialog1");
        dialogContent.Clear();

        Modal2.AddDialogHeader("Add Terrain Effect");

        Modal2.AddTextField("EffectName", "Effect Name", "");
        Modal2.AddSelectField("VisualMarker", "Visual Marker", "None", StringUtility.CreateArray("None", "Spiky", "Wavy", "Hole", "Hand", "Skull", "Blocked", "Corners", "Border").ToList<string>());
        Modal2.AddComboboxField("Color", "Color", "None", StringUtility.CreateArray("Black", "White", "Yellow", "Red", "Blue", "Green").ToList<string>());

        var footer = Modal2.AddDialogFooter("Cancel", () =>
        {
            dialog.Close();
        });

        var confirm = new ShunDialogClose();
        confirm.SetVariant(ButtonVariant.Primary);
        confirm.text = "Save Config";
        confirm.clicked += () =>
        {
            ConfirmAddEffect();
            dialog.Close();
        };
        footer.Add(confirm);

        dialog.Open();
    }

    private static void ConfirmAddEffect()
    {
        string effect = Modal2.GetTextFieldValue("ShunDialog1", "EffectName");
        string marker = Modal2.GetSelectFieldValue("ShunDialog1", "VisualMarker");
        string color = Modal2.GetComboboxFieldValue("ShunDialog1", "Color");

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
        string command = $"{effect}::{marker}::{color}";
        Player.Self().CmdRequestMapSetValue(blockNames.ToArray(), "AddEffect", command);
        Block.DeselectAll();
    }
}
