
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShunUI;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UIElements;

public interface IActorType
{
    string TypeId();
    string Serialize();
    string Label();
    string GetOverheadAsset();
    List<MenuItem> GetMenuItems(bool placed);
    void Command(string command, ActorData actorData);
    void UpdateOverhead(ActorData actorData);
    void UpdatePanel(ActorData actorData, string elementName);
    void InitPanel(ActorData actorData, string elementName, bool selected = false);
    void InitOverhead(ActorData actorData);
    void Disconnect();
}

[Serializable]
public abstract class ActorType : IActorType
{
    public string Type;
    public List<ActorTag> Tags;
    public List<ActorBar> Bars;
    public List<ActorStat> Stats;

    public string TypeId()
    {
        return Type;
    }

    public virtual string Label()
    {
        throw new NotImplementedException();
    }

    public virtual string Serialize()
    {

        throw new NotImplementedException();
    }

    public virtual void Disconnect()
    {

    }

    public virtual string GetOverheadAsset()
    {
        return null;
    }

    public virtual List<MenuItem> GetMenuItems(bool placed)
    {
        List<MenuItem> items = new();

        var configToken = new MenuItem("Configure Token", null);
        items.Add(configToken);
        configToken.Children.Add(new MenuItem("Change Size/Shape", ReshapeModal));
        configToken.Children.Add(new MenuItem("Change Color", RecolorModal));

        var configActor = new MenuItem("Configure Actor", null);
        items.Add(configActor);
        configActor.Children.Add(new MenuItem("Change Name", RenameModal));
        configActor.Children.Add(new MenuItem("Add Stat/Bar", AddStatModal));
        if (Stats.Count > 0 || Bars.Count > 0)
        {
            configActor.Children.Add(new MenuItem("Edit Stats/Bars", EditStatBarModal));
        }

        var changeValue = new MenuItem("Change Values", null);
        items.Add(changeValue);
        changeValue.Children.Add(new MenuItem("Add Status/Resource", AddTagModal));
        foreach (ActorBar bar in Bars)
        {
            changeValue.Children.Add(new MenuItem($"Modify {bar.Name}", () =>
            {
                NumberPicker.ActorCommand($"ModBar|{bar.Name}");
            }));
        }

        var other = new MenuItem("Other", null);
        items.Add(other);
        other.Children.Add(new MenuItem("Clone", CloneConfirm));
        other.Children.Add(new MenuItem("Delete", DeleteConfirm));
        if (placed)
        {
            other.Children.Add(new MenuItem("Remove", () =>
            {
                Actor.GetSelected().Remove();
            }));
            other.Children.Add(new MenuItem("Flip Left/Right", () =>
            {
                Actor.GetSelected().Flip();
            }));
        }

        return items;
    }

    protected MenuItem FindParent(string label, List<MenuItem> list)
    {
        foreach (MenuItem m in list)
        {
            if (m.Label == label)
            {
                return m;
            }
        }
        throw new Exception("No such item");
    }

    protected static void ClickFlip()
    {
        Actor.GetSelected().transform.Find("Offset/Avatar/Cutout/Cutout Quad").Rotate(new Vector3(0, 180, 0));
        Actor.Deselect();
    }

    protected static void ClickRemove()
    {
        Actor.GetSelected().Remove();
        Actor.Deselect();
    }

    protected static void DeleteConfirm()
    {
        ActorData data = Actor.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal2.Confirm("PrimaryDialog", $"Are you sure you want to delete {name}? This action cannot be undone.", () =>
        {
            Actor.Deselect();
            Player.Self().CmdRequestDeleteActor(data.Id);
        });
        SelectionMenu.Hide();
    }

    private static void AddTagModal()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddInlineTextField("TagName", "Name", "");
        Modal2.AddInlineComboboxField("ColorField", "Color", "Gray", ColorUtility.CommonColors().ToList<string>());
        Modal2.AddSwitchField("HasNumberField", "Use Counter?", false);
        Modal2.AddInlineIntField("TagValue", "Initial Value", 0);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Add", AddTagSubmit);
        Modal2.Open("Add Tag");
    }

    private static void AddTagModalEvaluateConditions()
    {
        bool hasNumberValue = UI.Modal.Q<Toggle>("HasNumberField").value;
        UI.ToggleDisplay(UI.Modal.Q("TagValue"), hasNumberValue);
    }


    private static void AddTagSubmit()
    {
        Modal2.ReadContext("PrimaryDialog");
        string tagName = Modal2.GetTextFieldValue("TagName");
        int tagValue = Modal2.GetIntFieldValue("TagValue");
        string colorValue = Modal2.GetComboboxFieldValue("ColorField");
        bool hasNumber = Modal2.GetSwitchFieldValue("HasNumberField");
        ActorTag tag = new();
        tag.Name = tagName;
        tag.Value = tagValue;
        tag.HasNumber = hasNumber;
        tag.Color = ColorUtility.GetCommonColor(colorValue);
        Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"AddTag|{JsonUtility.ToJson(tag)}");
    }

    private void EditStatBarModal()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Edit Stats");

        foreach (ActorBar bar in Bars)
        {
            Modal2.AddSwitchField(bar.Name, $"Bar: {bar.Name}", true);
        }
        foreach (ActorStat stat in Stats)
        {
            Modal2.AddSwitchField(stat.Name, $"Stat: {stat.Name}", true);
        }
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Save", EditStatBarSubmit);
        Modal2.Open("Edit Stat Bar");
    }

    private void EditStatBarSubmit()
    {
        Modal2.ReadContext("PrimaryDialog");
        foreach (ActorBar bar in Bars)
        {
            bool keep = Modal2.GetSwitchFieldValue(bar.Name);
            if (!keep)
            {
                Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"RemoveBar|{bar.Name}");
            }
        }
        foreach (ActorStat stat in Stats)
        {
            bool keep = Modal2.GetSwitchFieldValue(stat.Name);
            if (!keep)
            {
                Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"RemoveStat|{stat.Name}");
            }
        }
    }

    private static void AddStatModal()
    {
        SelectionMenu.Hide();
        ActorData data = Actor.GetSelected().Data;
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Add Stat or Bar");

        Modal2.AddInlineTextField("StatName", "Stat Name", "");
        Modal2.AddInlineIntField("StatValue", "Stat Value", 0);
        var bar = Modal2.AddSwitchField("IsBar", "Display as Bar", false);
        var max = Modal2.AddInlineIntField("MaxValue", "Max Value", 0);
        var color = Modal2.AddInlineComboboxField("Color", "Bar Color", "Green", ColorUtility.CommonColors().ToList<string>());

        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Add Stat", () =>
        {
            AddStatSubmit();
        });

        modalConditionBool(max, false);
        modalConditionBool(color, false);
        bar.Q<ShunSwitch>().onValueChanged += (val) =>
        {
            modalConditionBool(max, val);
            modalConditionBool(color, val);
        };

        Modal2.Open("Add Stat");
    }

    private static void modalConditionBool(VisualElement e, bool show)
    {
        UI.ToggleDisplay(e, show);
    }

    private static void AddStatSubmit()
    {
        Modal2.ReadContext("PrimaryDialog");
        string name = Modal2.GetTextFieldValue("StatName");
        int value = Modal2.GetIntFieldValue("StatValue");
        string color = Modal2.GetComboboxFieldValue("Color");
        bool isBar = Modal2.GetSwitchFieldValue("IsBar");

        if (isBar)
        {
            ActorBar bar = new();
            bar.Name = name;
            bar.Value = value;
            bar.MaxValue = value;
            bar.Color = ColorUtility.GetCommonColor(color);
            Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"AddBar|{JsonUtility.ToJson(bar)}");
        }
        else
        {
            ActorStat stat = new();
            stat.Name = name;
            stat.Value = value;
            Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"AddStat|{JsonUtility.ToJson(stat)}");
        }
    }

    protected static void CloneConfirm()
    {
        ActorData data = Actor.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal2.Confirm("PrimaryDialog", $"Are you sure you want to clone {name}?", () =>
        {
            data.Placed = false; // set this to false only long enough to create the clone, easier than cloning the object
            string json = JsonUtility.ToJson(data.Persist());
            data.Placed = true;
            Player.Self().CmdCreateActor(json);
        });
        SelectionMenu.Hide();
    }

    public static string[] ShapeOptions()
    {
        return StringUtility.CreateArray("Square 1/2", "Square 1x1", "Square 2x2", "Square 3x3", "Square 4x4", "Hex 1/2", "Hex 1", "Hex 2", "Hex 3", "Hex 4");
    }

    public static string[] SquareShapeOptions()
    {
        return StringUtility.CreateArray("Square 1/2", "Square 1x1", "Square 2x2", "Square 3x3", "Square 4x4");
    }

    public static string[] HexShapeOptions()
    {
        return StringUtility.CreateArray("Hex 1/2", "Hex 1", "Hex 2", "Hex 3", "Hex 4");
    }

    private static void ReshapeModal()
    {
        SelectionMenu.Hide();
        ActorData data = Actor.GetSelected().Data;
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Change Size/Shape");
        Modal2.AddInlineComboboxField("Reshape", "New Shape", data.Shape, ShapeOptions().ToList<string>());
        Modal2.AddAlert("Resizing", "Changing between a size that occupies a tile center and a tile intersection requires manual position adjustment", AlertVariant.Attention);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Update", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            string newShape = Modal2.GetComboboxFieldValue("Reshape");
            Player.Self().CmdRequestActorCommand(data.Id, $"Reshape|{newShape}");
        });
        Modal2.Open("Reshape");
    }

    private static void RecolorModal()
    {
        SelectionMenu.Hide();
        ActorData data = Actor.GetSelected().Data;
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Change Color");
        Modal2.AddInlineComboboxField("Recolor", "New Color", "Black", ColorUtility.CommonColors().ToList<string>());
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Update", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            string newColor = Modal2.GetComboboxFieldValue("Recolor");
            Player.Self().CmdRequestActorCommand(data.Id, $"Recolor|{newColor}");
        });
        Modal2.Open("Recolor");
    }

    protected virtual void RenameModal()
    {
        SelectionMenu.Hide();
        ActorData data = Actor.GetSelected().Data;
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Edit Name");
        Modal2.AddInlineTextField("Name", "New Name", data.Name);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Confirm", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            string newName = Modal2.GetTextFieldValue("Name");
            Player.Self().CmdRequestActorCommand(data.Id, $"Rename|{newName}");
        });
        Modal2.Open("Rename");
    }

    public virtual void Command(string value, ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        if (value.StartsWith("AddTag"))
        {
            string[] parts = value.Split("|");
            ActorTag tag = JsonUtility.FromJson<ActorTag>(parts[1]);
            Tags.Add(tag);
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/+|_{tag.Name.ToUpper()}", Color.white);
            }
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("IncrementTag"))
        {
            string[] parts = value.Split("|");
            CounterTag(parts[1], 1);
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/+1|_{parts[1].ToUpper()}", Color.white);
            }
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("DecrementTag"))
        {
            string[] parts = value.Split("|");
            CounterTag(parts[1], -1);
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/-1|_{parts[1].ToUpper()}", Color.white);
            }
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("RemoveTag"))
        {
            string[] parts = value.Split("|");
            RemoveTag(parts[1]);
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/-|_{parts[1].ToUpper()}", Color.white);
            }
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("RemoveStat"))
        {
            string[] parts = value.Split("|");
            int i = Stats.FindIndex(a => a.Name == parts[1]);
            Stats.RemoveAt(i);
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("RemoveBar"))
        {
            string[] parts = value.Split("|");
            int i = Bars.FindIndex(a => a.Name == parts[1]);
            Bars.RemoveAt(i);
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("AddBar"))
        {
            string[] parts = value.Split("|");
            ActorBar bar = JsonUtility.FromJson<ActorBar>(parts[1]);
            Bars.Add(bar);
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("AddStat"))
        {
            string[] parts = value.Split("|");
            ActorStat stat = JsonUtility.FromJson<ActorStat>(parts[1]);
            Stats.Add(stat);
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("ModBar"))
        {
            ModBar(value, tokenData);
            Actor.RebuildPanels = true;
        }
        if (value.StartsWith("Reshape"))
        {
            string[] parts = value.Split("|");
            tokenData.Shape = parts[1];
            tokenData.SetShape();
        }
        if (value.StartsWith("Recolor"))
        {
            string[] parts = value.Split("|");
            tokenData.Color = ColorUtility.GetCommonColor(parts[1]);
            tokenData.SetColor();
        }
    }

    public virtual void UpdateOverhead(ActorData tokenData)
    {
    }

    public virtual void UpdatePanel(ActorData tokenData, string elementName)
    {
        VisualElement panel = UI.System.Q(elementName);
        foreach (var child in panel.Children())
        {
            UI.ToggleDisplay(child, false);
        }
        UI.ToggleDisplay(panel.Q("DefaultActorPanel"), true);
    }

    public virtual void InitOverhead(ActorData actorData)
    {

    }

    public virtual void InitPanel(ActorData actorData, string elementName, bool selected = false)
    {
        VisualElement panel = UI.System.Q(elementName);
        foreach (var child in panel.Children())
        {
            UI.ToggleDisplay(child, false);
        }
        UI.ToggleDisplay(panel.Q("DefaultActorPanel"), true);
        panel.Q("Pills").Clear();
        panel.Q("Stats").Clear();
        panel.Q("Bars").Clear();
        foreach (ActorBar bar in Bars)
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
        foreach (ActorStat stat in Stats)
        {
            VisualElement statt = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
            statt.Q<Label>("Label").text = stat.Name;
            statt.Q<Label>("Value").text = $"{stat.Value}";
            panel.Q("Stats").Add(statt);
        }
        foreach (ActorTag tag in Tags)
        {
            if (tag.HasNumber)
            {
                panel.Q("Pills").Add(Pill.InitNumber(tag.Name, tag.Name, tag.Value, 0, tag.Color, true));
            }
            else
            {
                panel.Q("Pills").Add(Pill.InitRemovable(tag.Name, tag.Name, tag.Color, true));
            }
        }
    }

    public static string SymbolString(string character, int value, int max)
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

    protected VisualElement PipsBar(string name, string symbol, int current, int max, Color color, EventCallback<ClickEvent> minusAction, EventCallback<ClickEvent> plusAction)
    {
        VisualElement container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.style.marginBottom = 0;
        container.style.paddingBottom = 0;

        Label minus = new();
        minus.name = "Minus";
        minus.text = "-";
        minus.style.color = Color.white;
        minus.style.fontSize = 26;
        minus.style.marginTop = 0; // line height oddities with the minus symbol
        minus.style.marginBottom = 0;
        minus.style.paddingBottom = 0;
        minus.RegisterCallback<ClickEvent>(minusAction);

        Label pips = new();
        pips.name = name;
        pips.text = SymbolString(symbol, current, max);
        pips.style.color = color;
        pips.style.unityTextOutlineColor = Color.white;
        pips.style.unityTextOutlineWidth = 1;
        pips.style.fontSize = 26;
        pips.style.marginBottom = 0;
        pips.style.paddingBottom = 0;


        Label plus = new();
        plus.name = "Plus";
        plus.text = "+";
        plus.style.color = Color.white;
        plus.style.fontSize = 26;
        plus.style.marginBottom = 0;
        plus.style.paddingBottom = 0;
        plus.RegisterCallback<ClickEvent>(plusAction);

        container.Add(minus);
        container.Add(pips);
        container.Add(plus);

        return container;
    }

    public void DirectCommand(string command)
    {
        Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, command);
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

    private void ModBar(string command, ActorData tokenData)
    {
        Actor actor = tokenData.GetActor();
        string name = command.Split("|")[1];
        int index = Bars.FindIndex(a => a.Name == name);
        ActorBar bar = Bars[index];
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
            PopoverText.Create(actor, popoverText, Color.white);
        }
        Bars[index] = bar;
    }

    protected int Clamped(int min, int value, int max)
    {
        return Math.Max(min, Math.Min(value, max));
    }
}

[Serializable]
public class ActorTag
{
    public string Name;
    public int Value;
    public Color Color;
    public bool HasNumber;
}

[Serializable]
public class ActorBar
{
    public string Name;
    public int Value;
    public int MaxValue;
    public Color Color;
}

[Serializable]
public class ActorStat
{
    public string Name;
    public int Value;
}
