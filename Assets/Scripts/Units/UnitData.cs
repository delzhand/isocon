
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UIElements;

public interface IUnitData
{
    string Serialize();
    string Label();
    string GetOverheadAsset();
    MenuItem[] GetMenuItems(bool placed);
    void Command(string command, TokenData tokenData);
    void UpdateOverhead(TokenData tokenData);
    void UpdatePanel(TokenData tokenData, string elementName);
    void InitPanel(string elementName, bool selected = false);

}

[Serializable]
public abstract class UnitData : IUnitData
{
    public string Type;
    public TokenMeta TokenMeta;
    public string Shape;
    public Color Color;
    public List<UnitTag> Tags;
    public List<UnitBar> Bars;
    public List<UnitStat> Stats;

    public virtual string Label()
    {
        throw new NotImplementedException();
    }

    public virtual string Serialize()
    {
        throw new NotImplementedException();
    }

    public virtual string GetOverheadAsset()
    {
        return null;
    }

    public virtual MenuItem[] GetMenuItems(bool placed)
    {
        List<MenuItem> items = new();
        if (placed)
        {
            items.Add(new MenuItem("Remove", "Remove", ClickRemove));
            items.Add(new MenuItem("Flip", "Flip", ClickFlip));
        }
        items.Add(new MenuItem("Reshape", "Reshape", ReshapeModal));
        items.Add(new MenuItem("EditName", "Rename", ClickEditName));
        items.Add(new MenuItem("Clone", "Clone", ClickClone));
        items.Add(new MenuItem("Delete", "Delete", ClickDelete));
        items.Add(new MenuItem("AddTag", "Add Tag", AddTagModal));
        items.Add(new MenuItem("AddBar", "Add Bar", AddBarModal));
        items.Add(new MenuItem("AddStat", "Add Stat", AddStatModal));
        if (Stats.Count > 0 || Bars.Count > 0)
        {
            items.Add(new MenuItem("EditStats", "Edit Stats/Bars", EditStatBarModal));
        }
        foreach (UnitBar bar in Bars)
        {
            items.Add(new MenuItem($"Modify{bar.Name}", $"Modify {bar.Name}", (evt) =>
            {
                NumberPicker.TokenCommand($"ModBar|{bar.Name}");
            }));
        }
        return items.ToArray();
    }

    protected static void ClickFlip(ClickEvent evt)
    {
        Token.GetSelected().transform.Find("Offset/Avatar/Cutout/Cutout Quad").Rotate(new Vector3(0, 180, 0));
        Token.Deselect();
    }

    protected static void ClickRemove(ClickEvent evt)
    {
        Token.GetSelected().Remove();
        Token.Deselect();
    }

    protected static void ClickDelete(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal.DoubleConfirm("Delete Token", $"Are you sure you want to delete {name}? This action cannot be undone.", () =>
        {
            Token.Deselect();
            Player.Self().CmdRequestDeleteToken(data.Id);
        });
        SelectionMenu.Hide();
    }

    private static void AddTagModal(ClickEvent evt)
    {
        Modal.Reset("Add Tag");
        Modal.AddTextField("TagName", "Tag Name", "");
        Modal.AddDropdownField("ColorField", "Color", "Gray", ColorUtility.CommonColors());
        Modal.AddToggleField("HasNumberField", "Has Number Value?", false, (evt) => { AddTagModalEvaluateConditions(); });
        Modal.AddIntField("TagValue", "Tag Initial Value", 0);
        Modal.AddPreferredButton("Add", AddTagSubmit);
        Modal.AddButton("Cancel", Modal.CloseEvent);
        AddTagModalEvaluateConditions();

        SelectionMenu.Hide();
    }

    private static void AddTagModalEvaluateConditions()
    {
        bool hasNumberValue = UI.Modal.Q<Toggle>("HasNumberField").value;
        UI.ToggleDisplay(UI.Modal.Q("TagValue"), hasNumberValue);
    }


    private static void AddTagSubmit(ClickEvent evt)
    {
        string tagName = UI.Modal.Q<TextField>("TagName").value;
        int tagValue = UI.Modal.Q<IntegerField>("TagValue").value;
        string colorValue = UI.Modal.Q<DropdownField>("ColorField").value;
        bool hasNumber = UI.Modal.Q<Toggle>("HasNumberField").value;
        UnitTag tag = new();
        tag.Name = tagName;
        tag.Value = tagValue;
        tag.HasNumber = hasNumber;
        tag.Color = ColorUtility.GetCommonColor(colorValue);
        Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"AddTag|{JsonUtility.ToJson(tag)}");
        Modal.Close();
    }

    private static void AddBarModal(ClickEvent evt)
    {
        Modal.Reset("Add Bar");
        Modal.AddTextField("BarName", "Bar Name", "");
        Modal.AddDropdownField("ColorField", "Color", "Red", ColorUtility.CommonColors());
        Modal.AddIntField("BarValue", "Bar Max Value", 0);
        Modal.AddPreferredButton("Add", AddBarSubmit);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        SelectionMenu.Hide();
    }

    private static void AddBarSubmit(ClickEvent evt)
    {
        string barName = UI.Modal.Q<TextField>("BarName").value;
        int barValue = UI.Modal.Q<IntegerField>("BarValue").value;
        string colorValue = UI.Modal.Q<DropdownField>("ColorField").value;
        UnitBar bar = new();
        bar.Name = barName;
        bar.Value = barValue;
        bar.MaxValue = barValue;
        bar.Color = ColorUtility.GetCommonColor(colorValue);
        Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"AddBar|{JsonUtility.ToJson(bar)}");
        Modal.Close();
    }

    private void EditStatBarModal(ClickEvent evt)
    {
        Modal.Reset("Edit Stats/Bars");
        foreach (UnitBar bar in Bars)
        {
            Modal.AddToggleField(bar.Name, $"Bar: {bar.Name}", true);
        }
        foreach (UnitStat stat in Stats)
        {
            Modal.AddToggleField(stat.Name, $"Stat: {stat.Name}", true);
        }
        Modal.AddPreferredButton("Save", EditStatBarSubmit);
        SelectionMenu.Hide();
    }

    private void EditStatBarSubmit(ClickEvent evt)
    {
        foreach (UnitBar bar in Bars)
        {
            bool keep = UI.Modal.Q<Toggle>(bar.Name).value;
            if (!keep)
            {
                Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"RemoveBar|{bar.Name}");
            }
        }
        foreach (UnitStat stat in Stats)
        {
            bool keep = UI.Modal.Q<Toggle>(stat.Name).value;
            if (!keep)
            {
                Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"RemoveStat|{stat.Name}");
            }
        }
        Modal.Close();
    }

    private static void AddStatModal(ClickEvent evt)
    {
        Modal.Reset("Add Stat");
        Modal.AddTextField("StatName", "Stat Name", "");
        Modal.AddIntField("StatValue", "Stat Value", 0);
        Modal.AddPreferredButton("Add", AddStatSubmit);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        SelectionMenu.Hide();
    }

    private static void AddStatSubmit(ClickEvent evt)
    {
        string statName = UI.Modal.Q<TextField>("StatName").value;
        int statValue = UI.Modal.Q<IntegerField>("StatValue").value;
        UnitStat stat = new();
        stat.Name = statName;
        stat.Value = statValue;
        Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"AddStat|{JsonUtility.ToJson(stat)}");
        Modal.Close();
    }

    protected static void ClickClone(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal.DoubleConfirm("Clone Token", $"Are you sure you want to clone {name}?", () =>
        {
            Player.Self().CmdCreateToken(data.SystemData);
            Token.Deselect();
        });
        SelectionMenu.Hide();
    }

    public static string[] ShapeOptions()
    {
        return StringUtility.CreateArray("Square 1x1", "Square 2x2", "Square 3x3", "Square 4x4", "Hex 1", "Hex 2", "Hex 3", "Hex 4");
    }

    public static string[] SquareShapeOptions()
    {
        return StringUtility.CreateArray("Square 1x1", "Square 2x2", "Square 3x3", "Square 4x4");
    }

    public static string[] HexShapeOptions()
    {
        return StringUtility.CreateArray("Hex 1", "Hex 2", "Hex 3", "Hex 4");
    }

    private static void ReshapeModal(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        Modal.Reset("Reshape");
        Modal.AddDropdownField("Reshape", "New Shape", data.Shape, ShapeOptions());
        Modal.AddPreferredButton("Update", (evt) =>
        {
            string newShape = UI.Modal.Q<DropdownField>("Reshape").value;
            Player.Self().CmdRequestTokenDataCommand(data.Id, $"Reshape|{newShape}");
            Modal.Close();
            Token.Deselect();
        });
        Modal.AddButton("Cancel", Modal.CloseEvent);
        SelectionMenu.Hide();
    }

    private static void ClickEditName(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        Modal.Reset("Edit Name");
        Modal.AddTextField("Name", "Name", data.Name);
        Modal.AddPreferredButton("Confirm", (evt) =>
        {
            string newName = UI.Modal.Q<TextField>("Name").value.Trim();
            Player.Self().CmdRequestTokenDataCommand(data.Id, $"Rename|{newName}");
            Modal.Close();
            Token.Deselect();
        });
        Modal.AddButton("Cancel", Modal.CloseEvent);
        SelectionMenu.Hide();
    }

    public virtual void Command(string value, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        if (value.StartsWith("AddTag"))
        {
            string[] parts = value.Split("|");
            UnitTag tag = JsonUtility.FromJson<UnitTag>(parts[1]);
            Tags.Add(tag);
            PopoverText.Create(token, $"/+|_{tag.Name.ToUpper()}", Color.white);
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("IncrementTag"))
        {
            string[] parts = value.Split("|");
            CounterTag(parts[1], 1);
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/+1|_{parts[1].ToUpper()}", Color.white);
            }
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("DecrementTag"))
        {
            string[] parts = value.Split("|");
            CounterTag(parts[1], -1);
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/-1|_{parts[1].ToUpper()}", Color.white);
            }
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("RemoveTag"))
        {
            string[] parts = value.Split("|");
            RemoveTag(parts[1]);
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/-|_{parts[1].ToUpper()}", Color.white);
            }
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("RemoveStat"))
        {
            string[] parts = value.Split("|");
            int i = Stats.FindIndex(a => a.Name == parts[1]);
            Stats.RemoveAt(i);
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("RemoveBar"))
        {
            string[] parts = value.Split("|");
            int i = Bars.FindIndex(a => a.Name == parts[1]);
            Bars.RemoveAt(i);
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("AddBar"))
        {
            string[] parts = value.Split("|");
            UnitBar bar = JsonUtility.FromJson<UnitBar>(parts[1]);
            Bars.Add(bar);
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("AddStat"))
        {
            string[] parts = value.Split("|");
            UnitStat stat = JsonUtility.FromJson<UnitStat>(parts[1]);
            Stats.Add(stat);
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("ModBar"))
        {
            ModBar(value, tokenData);
            Token.RebuildPanels = true;
        }
        if (value.StartsWith("Reshape"))
        {
            string[] parts = value.Split("|");
            Shape = parts[1];
            tokenData.Shape = parts[1];
            tokenData.SetShape();
        }
    }

    public virtual void UpdateOverhead(TokenData tokenData)
    {
    }

    public virtual void UpdatePanel(TokenData tokenData, string elementName)
    {
    }

    public virtual void InitPanel(string elementName, bool selected = false)
    {
        VisualElement panel = UI.System.Q(elementName);
        panel.Q("Pills").Clear();
        panel.Q("Stats").Clear();
        panel.Q("Bars").Clear();
        foreach (UnitBar bar in Bars)
        {
            VisualElement bart = UI.CreateFromTemplate("UI/TableTop/SimpleHPBar");
            bart.Q<Label>("StatLabel").text = bar.Name;
            bart.Q<Label>("CHP").text = $"{bar.Value}";
            bart.Q<Label>("MHP").text = $"/{bar.MaxValue}";
            bart.Q<ProgressBar>("HpBar").value = bar.Value;
            bart.Q<ProgressBar>("HpBar").highValue = bar.MaxValue;
            bart.Query(null, "unity-progress-bar__progress").First().style.backgroundColor = bar.Color;
            bart.Query(null, "unity-progress-bar__background").First().style.backgroundColor = ColorUtility.DarkenColor(bar.Color, .5f);
            panel.Q("Bars").Add(bart);
        }
        foreach (UnitStat stat in Stats)
        {
            VisualElement statt = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
            statt.Q<Label>("Label").text = stat.Name;
            statt.Q<Label>("Value").text = $"{stat.Value}";
            panel.Q("Stats").Add(statt);
        }
        foreach (UnitTag tag in Tags)
        {
            if (tag.HasNumber)
            {
                panel.Q("Pills").Add(Pill.InitNumber(tag.Name, tag.Name, tag.Value, tag.Color, true));
            }
            else
            {
                panel.Q("Pills").Add(Pill.InitRemovable(tag.Name, tag.Name, tag.Color, true));
            }

            // VisualElement pill = UI.CreateFromTemplate("UI/TableTop/Pill");
            // string text = $"{tag.Name}";
            // if (tag.HasNumber)
            // {
            //     text += $"   {tag.Value}";
            //     pill.Q("Decrement").style.color = tag.Color;
            //     pill.Q("Increment").style.color = tag.Color;
            //     pill.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) =>
            //     {
            //         Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"IncrementTag|{tag.Name}");
            //     });
            //     pill.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) =>
            //     {
            //         Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"DecrementTag|{tag.Name}");
            //     });
            // }
            // else
            // {
            //     UI.ToggleDisplay(pill.Q("Increment"), false);
            //     UI.ToggleDisplay(pill.Q("Decrement"), false);
            // }
            // pill.Q<Button>("Remove").RegisterCallback<ClickEvent>((evt) =>
            // {
            //     Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"RemoveTag|{tag.Name}");
            // });
            // pill.Q("Pill").style.backgroundColor = tag.Color;
            // pill.Q("Remove").style.color = tag.Color;
            // pill.Q<Label>("Name").text = text;
            // panel.Q("Pills").Add(pill);
        }
    }

    public string SymbolString(string character, int value, int max)
    {
        StringBuilder sb = new();
        for (int i = 0; i < max; i++)
        {
            if (i == value)
            {
                sb.Append("<color=white>");
            }
            sb.Append(character);
        }
        sb.Append("</color>");
        return sb.ToString();
    }

    public void DirectCommand(string command)
    {
        Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, command);
        SelectionMenu.Hide();
    }

    protected void CounterTag(string name, int num)
    {
        int i = Tags.FindIndex(a => a.Name == name);
        Tags[i].Value += num;
    }

    protected bool HasTag(string name)
    {
        int i = Tags.FindIndex(a => a.Name == name);
        return i >= 0;
    }

    protected void RemoveTag(string name)
    {
        int i = Tags.FindIndex(a => a.Name == name);
        if (i >= 0)
        {
            Tags.RemoveAt(i);
        }
    }

    private void ModBar(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        string name = command.Split("|")[1];
        int index = Bars.FindIndex(a => a.Name == name);
        UnitBar bar = Bars[index];
        int value = int.Parse(command.Split("|")[2]);
        string popoverText = "";
        int diff = Math.Abs(value);

        if (value <= 0)
        {
            if (bar.Value - diff < 0)
            {
                diff = bar.Value;
            }
            bar.Value -= diff;
            if (diff > 0)
            {
                popoverText = $"/-{diff}|_{bar.Name}";
            }
        }
        else
        {
            if (bar.Value + diff > bar.MaxValue)
            {
                diff = bar.MaxValue - bar.Value;
            }
            bar.Value += diff;
            if (diff > 0)
            {
                popoverText = $"/+{diff}|_{bar.Name}";
            }
        }
        if (tokenData.Placed && popoverText?.Length > 0)
        {
            PopoverText.Create(token, popoverText, Color.white);
        }
        Bars[index] = bar;
    }

    protected int Clamped(int min, int value, int max)
    {
        return Math.Max(min, Math.Min(value, max));
    }
}

[Serializable]
public class UnitTag
{
    public string Name;
    public int Value;
    public Color Color;
    public bool HasNumber;
}

[Serializable]
public class UnitBar
{
    public string Name;
    public int Value;
    public int MaxValue;
    public Color Color;
}

[Serializable]
public class UnitStat
{
    public string Name;
    public int Value;
}

[Serializable]
public class UnitMeta
{
    public string Name;
    public string Type;
    public TokenMeta TokenMeta;
    public string Shape;
    public Color Color;
}