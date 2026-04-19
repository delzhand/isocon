using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UIElements;

public class TileMenu
{
    public static void ShowMenu(Block b)
    {
        SelectionMenu.Open(new Vector2(0, 0), b.transform);
        MenuItem[] defaultItems = GetTileMenuItems(b);
        foreach (MenuItem m in defaultItems)
        {
            SelectionMenu.AddItem(m.Label, m.Action, null);
        }
    }

    public static MenuItem[] GetTileMenuItems(Block b)
    {
        List<MenuItem> items = new();

        if (!b.Selected)
        {
            items.Add(new MenuItem("Select Tile", () => { b.Select(); }));
        }
        else
        {
            items.Add(new MenuItem("Deselect Tile", () => { b.Select(); }));
        }
        if (TerrainController.GridType == "Square")
        {
            items.Add(new MenuItem("Select Adjacent", () => { AddAdjacent(b); }));
            items.Add(new MenuItem("Select Neighbors", () => { AddNeighbors(b); }));
        }
        // if (StateManager.Find().SubState.TypeName() == "TileMarkingState")
        // {
        //     items.Add(new MenuItem("QuickSelect", "Exit Quick Select Mode", () =>
        //     {
        //         StateManager.Find().ChangeSubState(new NeutralState());
        //         Player.Self().ClearOp();
        //     }));
        // }
        // else
        // {
        //     items.Add(new MenuItem("QuickSelect", "Quick Select Mode", () => { StateManager.Find().ChangeSubState(new TileMarkingState()); }));
        // } // @todo
        if (b.Selected)
        {
            items.Add(new MenuItem("Deselect Tile", () => { b.Select(); }));
        }
        items.Add(new MenuItem("Deselect All", ClickDeselectAll));
        items.Add(new MenuItem("Add Effect", () => ClickAddEffect(null)));
        items.Add(new MenuItem("Clear Effects from Tile", () => ClickClearTile(b)));
        items.Add(new MenuItem("Clear Effects from All", ClickClearMap));
        items.Add(new MenuItem("Clear Effects from Selection", ClickClearSelection));

        List<string> effects = new();
        foreach (var block in Block.GetSelected())
        {
            block.Marks.ForEach(effect =>
            {
                string effectName = effect.Split("::")[0];
                if (!effects.Contains(effectName))
                {
                    if (effectName.Length == 0)
                    {
                        effectName = "unnamed effect";
                    }
                    items.Add(new MenuItem($"Remove {effectName}", () =>
                    {
                        Player.Self().CmdRequestMapSetValue(SelectedBlockNames(), "RemoveEffect", effect);
                        SelectionMenu.Hide();
                    }));
                    effects.Add(effectName);
                }
            });
        }
        return items.ToArray();
    }

    private static void AddAdjacent(Block block)
    {
        Block[] neighbors = TerrainController.FindAdjacent(block);
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (!neighbors[i].Selected)
            {
                neighbors[i]?.Select();
            }
        }
    }

    private static void AddNeighbors(Block block)
    {
        Block[] neighbors = TerrainController.FindNeighbors(block, 3);
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (!neighbors[i].Selected)
            {
                neighbors[i]?.Select();
            }
        }
    }

    public static void ClickAddEffect(Block b)
    {
        AddTerrainEffect.OpenModal(b);
        SelectionMenu.Hide();
    }

    public static void ClickDeselectAll()
    {
        Block.DeselectAll();
        SelectionMenu.Hide();
    }

    public static void ClickClearTile(Block b)
    {
        Player.Self().CmdRequestMapSetValue(new string[] { b.name }, "ClearEffects", "None");
        SelectionMenu.Hide();
    }

    public static void ClickClearSelection()
    {
        Player.Self().CmdRequestMapSetValue(SelectedBlockNames(), "ClearEffects", "None");
        SelectionMenu.Hide();
    }

    public static void ClickClearMap()
    {
        Player.Self().CmdRequestMapSetValue(AllBlockNames(), "ClearEffects", "None");
        SelectionMenu.Hide();
    }

    private static string[] AllBlockNames()
    {
        List<string> blocks = new List<string>();
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < objects.Length; i++)
        {
            blocks.Add(objects[i].name);
        }
        return blocks.ToArray();
    }

    public static string[] SelectedBlockNames()
    {
        List<string> blocks = new();
        foreach (Block b in Block.GetSelected())
        {
            blocks.Add(b.name);
        }
        return blocks.ToArray();
    }
}
