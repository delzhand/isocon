using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UIElements;

public class TileMenu
{
    public static void ShowMenu(Block b)
    {
        SelectionMenu.Reset("TILE MENU", new Vector2(0, 0), b.transform);
        MenuItem[] defaultItems = GetTileMenuItems(b);
        foreach (MenuItem m in defaultItems)
        {
            SelectionMenu.AddItem(m.Name, m.Label, m.Action, null);
        }
    }

    public static MenuItem[] GetTileMenuItems(Block b)
    {
        List<MenuItem> items = new();
        if (Block.GetSelected().Length == 0)
        {
            b.Select();
        }
        else if (!b.Selected)
        {
            items.Add(new MenuItem("SelectThis", "Select Tile", () => { b.Select(); }));
        }
        if (TerrainController.GridType == "Square")
        {
            items.Add(new MenuItem("AddAdjacent", "Select Adjacent", () => { AddAdjacent(b); }));
            items.Add(new MenuItem("AddNeighbors", "Select Neighbors", () => { AddNeighbors(b); }));
        }
        if (StateManager.Find().SubState.TypeName() == "TileMarkingState")
        {
            items.Add(new MenuItem("QuickSelect", "Exit Quick Select Mode", () =>
            {
                StateManager.Find().ChangeSubState(new NeutralState());
                Player.Self().ClearOp();
            }));
        }
        else
        {
            items.Add(new MenuItem("QuickSelect", "Quick Select Mode", () => { StateManager.Find().ChangeSubState(new TileMarkingState()); }));
        }
        if (b.Selected)
        {
            items.Add(new MenuItem("SelectThis", "Deselect Tile", () => { b.Select(); }));
        }
        items.Add(new MenuItem("DeselectAll", "Deselect All", ClickDeselectAll));
        items.Add(new MenuItem("AddEffect", "Add Effect", () => ClickAddEffect(null)));
        items.Add(new MenuItem("ClearEffects", "Clear Effects from Selection", ClickClearEffects));

        List<string> effects = new();
        foreach (var block in Block.GetSelected())
        {
            block.Marks.ForEach(effect =>
            {
                string effectName = effect.Split("::")[0];
                if (!effects.Contains(effectName))
                {
                    items.Add(new MenuItem($"Remove_{effectName}", $"Remove {effectName}", () =>
                    {
                        Player.Self().CmdRequestMapSetValue(SelectedBlockNames(), "RemoveEffect", effect);
                        SelectionMenu.Hide();
                    }));
                    effects.Add(effectName);
                }
            });
        }
        items.Add(new MenuItem("ClearMap", "Clear Effects from All", ClickClearMap));
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
        StateManager.Find().ChangeSubState(new ModalState());
        SelectionMenu.Hide();
    }

    public static void ClickDeselectAll()
    {
        Block.DeselectAll();
        SelectionMenu.Hide();
    }

    public static void ClickClearEffects()
    {
        Player.Self().CmdRequestMapSetValue(SelectedBlockNames(), "Effect", "None");
        SelectionMenu.Hide();
    }

    public static void ClickClearMap()
    {
        Player.Self().CmdRequestMapSetValue(AllBlockNames(), "Effect", "None");
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
