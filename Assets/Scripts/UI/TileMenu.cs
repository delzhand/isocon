using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TileMenu
{
    public static void ShowMenu(Block b)
    {
        SelectionMenu.Reset("TILE MENU", new Vector2(30, 0), b.transform);
        MenuItem[] defaultItems = GetTileMenuItems();
        foreach (MenuItem m in defaultItems)
        {
            SelectionMenu.AddItem(m.Name, m.Label, m.OnClick);
        }
        MenuItem[] systemItems = GameSystem.Current().GetTileMenuItems();
        foreach (MenuItem m in systemItems)
        {
            SelectionMenu.AddItem(m.Name, m.Label, m.OnClick);
        }

    }

    public static MenuItem[] GetTileMenuItems()
    {
        List<MenuItem> items = new();
        if (Block.GetSelected().Length > 0)
        {
            items.Add(new MenuItem("AddEffect", "Add Effect", ClickAddEffect));
            items.Add(new MenuItem("DeselectAll", "Deselect All", ClickDeselectAll));
            items.Add(new MenuItem("ClearEffects", "Clear Effects", ClickClearEffects));

            List<string> effects = new();
            foreach (var block in Block.GetSelected())
            {
                block.Marks.ForEach(effect =>
                {
                    string effectName = effect.Split("::")[0];
                    if (!effects.Contains(effectName))
                    {
                        items.Add(new MenuItem($"Remove_{effectName}", $"Remove {effectName}", (evt) =>
                        {
                            Player.Self().CmdRequestMapSetValue(SelectedBlockNames(), "RemoveEffect", effect);
                            SelectionMenu.Hide();
                        }));
                        effects.Add(effectName);
                    }
                });
            }
        }
        items.Add(new MenuItem("ClearMap", "Clear Map", ClickClearMap));
        return items.ToArray();
    }

    public static void ClickAddEffect(ClickEvent evt)
    {
        AddTerrainEffect.OpenModal(evt);
        SelectionMenu.Hide();
    }

    public static void ClickDeselectAll(ClickEvent evt)
    {
        Block.DeselectAll();
        SelectionMenu.Hide();
    }

    public static void ClickClearEffects(ClickEvent evt)
    {
        Player.Self().CmdRequestMapSetValue(SelectedBlockNames(), "Effect", "None");
        SelectionMenu.Hide();
    }

    public static void ClickClearMap(ClickEvent evt)
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
