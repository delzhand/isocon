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

        var dialog = Modal2.CreateContext("PrimaryDialog");
        var contents = Modal2.Contents("PrimaryDialog");
        contents.Clear();

        Modal2.AddDialogHeader("Add Terrain Effect");

        Modal2.AddTextField("EffectName", "Effect Name", "");
        Modal2.AddSelectField("VisualMarker", "Visual Marker", "None", StringUtility.CreateArray("None", "Spiky", "Wavy", "Hole", "Hand", "Skull", "Blocked", "Corners", "Border").ToList<string>());
        Modal2.AddComboboxField("Color", "Color", "None", StringUtility.CreateArray("Black", "White", "Yellow", "Red", "Blue", "Green").ToList<string>());

        var footer = Modal2.AddDialogFooter();
        var confirm = new ShunDialogClose();
        confirm.SetVariant(ButtonVariant.Primary);
        confirm.text = "Apply Effect";
        confirm.clicked += () =>
        {
            ConfirmAddEffect();
            dialog.Close();
        };
        footer.Add(confirm);

        Modal2.Open("Add Terrain Effect");
    }

    private static void ConfirmAddEffect()
    {
        Modal2.ReadContext("PrimaryDialog");
        string effect = Modal2.GetTextFieldValue("EffectName");
        string marker = Modal2.GetSelectFieldValue("VisualMarker");
        string color = Modal2.GetComboboxFieldValue("Color");

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
