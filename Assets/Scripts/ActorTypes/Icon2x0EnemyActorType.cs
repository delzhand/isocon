using System;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class Icon2x0EnemyActorType : Icon2x0Base
{
    private readonly static string TypeName = "Icon 2.0 Enemy";

    public string Name;
    public int CurrentHP;
    public int MaxHP;
    public int Vigor;
    public string FoeClass;
    public int Move;
    public int Defense;
    public bool Elite;
    public Color Color;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        ActorTypeRegistry.RegisterSystem($"{TypeName}");
        ActorTypeRegistry.RegisterInterfaceCallback($"{TypeName}", DeserializeAsInterface);
        ActorTypeRegistry.RegisterSimpleCallback($"{TypeName}|AddActorModal", AddActorModal);
    }

    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public static IActorType DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<Icon2x0EnemyActorType>(json);
    }

    public static void AddActorModal()
    {
        var contents = Modal2.Contents("PrimaryDialog");
        var typeContainer = contents.Q("ActorTypeContainer");
        typeContainer.Clear();
        contents.Q("CreateActor")?.RemoveFromHierarchy();

        var name = Modal2.AddInlineTextField("Name", "Actor Name", "Actor");
        Modal2.MoveToContainer(name, typeContainer);

        var shape = Modal2.AddInlineComboboxField("Shape", "Shape", "Square 1x1", ActorType.SquareShapeOptions().ToList<string>());
        Modal2.MoveToContainer(shape, typeContainer);

        var foe = Modal2.AddInlineComboboxField("FoeClass", "Class", "Heavy", StringUtility.CreateArray("Heavy", "Artillery", "Skirmisher", "Leader", "Legend").ToList<string>());
        Modal2.MoveToContainer(foe, typeContainer);

        var elite = Modal2.AddSwitchField("Elite", "Elite", false);
        Modal2.MoveToContainer(elite, typeContainer);

        var legend = Modal2.AddInlineIntField("LegendHP", "HP Multiplier", 1);
        Modal2.MoveToContainer(legend, typeContainer);

        modalConditionLegend(legend);
        modalConditionElite(elite);
        foe.Q<ShunCombobox>().OnSelect += () =>
        {
            modalConditionLegend(legend);
            modalConditionElite(elite);
        };

        var create = new ShunDialogClose();
        create.name = "CreateActor";
        create.text = "Create Actor";
        create.SetVariant(ButtonVariant.Primary);
        create.clicked += () => CreateClicked();
        contents.Q(className: "shun-dialog__footer").Add(create);

    }

    private static void modalConditionElite(VisualElement e)
    {
        Modal2.ReadContext("PrimaryDialog");
        var foeValue = Modal2.GetComboboxFieldValue("FoeClass");
        UI.ToggleDisplay(e, foeValue != "Legend");
    }

    private static void modalConditionLegend(VisualElement e)
    {
        Modal2.ReadContext("PrimaryDialog");
        var foeValue = Modal2.GetComboboxFieldValue("FoeClass");
        UI.ToggleDisplay(e, foeValue == "Legend");
    }

    private static void AddModalEvaluateConditions()
    {
        string foeClass = UI.Modal.Q<DropdownField>("FoeClassField").value;

        VisualElement eliteField = UI.Modal.Q("EliteField");
        VisualElement hpMultiField = UI.Modal.Q("LegendHPField");

        UI.ToggleDisplay(eliteField, foeClass != "Legend");
        UI.ToggleDisplay(hpMultiField, foeClass == "Legend");
    }

    private static void CreateClicked()
    {
        Modal2.ReadContext("PrimaryDialog");
        string token = Modal2.GetComboboxFieldValue("Token");
        if (token == null)
        {
            Toast.AddError("A token has not been selected");
            return;
        }
        string name = Modal2.GetTextFieldValue("Name");
        string shape = Modal2.GetComboboxFieldValue("Shape");
        string foeClass = Modal2.GetComboboxFieldValue("FoeClass");
        int hpMulti = Modal2.GetIntFieldValue("LegendHP");
        bool elite = Modal2.GetSwitchFieldValue("Elite");

        if (elite)
        {
            hpMulti = 2;
        }
        else if (foeClass != "Legend")
        {
            hpMulti = 1;
        }

        Icon2x0EnemyActorType t = new()
        {
            Type = TypeName,
            Name = name,
            Elite = elite,
            FoeClass = foeClass,
        };

        switch (foeClass)
        {
            case "Heavy":
                t.MaxHP = 40 * hpMulti;
                t.CurrentHP = 40 * hpMulti;
                t.Move = 4;
                t.Defense = 3;
                t.Color = ColorUtility.GetCommonColor("red");
                t.Tags = new();
                t.Tags.Add(new ActorTag() { Name = "Guard", Color = ColorUtility.GetCommonColor("blue"), HasNumber = false });
                t.Tags.Add(new ActorTag() { Name = "Armor", Color = ColorUtility.GetCommonColor("blue"), HasNumber = true, Value = 1 });
                break;
            case "Skirmisher":
                t.MaxHP = 28 * hpMulti;
                t.CurrentHP = 28 * hpMulti;
                t.Move = 4;
                t.Defense = 6;
                t.Color = ColorUtility.GetCommonColor("yellow");
                break;
            case "Leader":
                t.MaxHP = 48 * hpMulti;
                t.CurrentHP = 48 * hpMulti;
                t.Move = 4;
                t.Defense = 4;
                t.Color = ColorUtility.GetCommonColor("green");
                break;
            case "Artillery":
                t.MaxHP = 32 * hpMulti;
                t.CurrentHP = 32 * hpMulti;
                t.Move = 4;
                t.Defense = 4;
                t.Color = ColorUtility.GetCommonColor("blue");
                t.Tags = new();
                t.Tags.Add(new ActorTag() { Name = "Aetherwall", Color = ColorUtility.GetCommonColor("blue"), HasNumber = false });
                break;
            case "Legend":
                t.MaxHP = 40 * hpMulti;
                t.CurrentHP = 40 * hpMulti;
                t.Defense = 8;
                t.Move = 5;
                t.Color = ColorUtility.GetCommonColor("purple");
                t.Tags = new();
                t.Tags.Add(new ActorTag() { Name = "Juggernaut", Color = ColorUtility.GetCommonColor("blue"), HasNumber = false });
                break;
        }

        ActorPersistence a = new();
        a.Name = t.Label();
        a.Token = TokenLibraryModal.GetToken(token);
        a.Color = t.Color;
        a.Shape = shape;
        a.Position = Vector3.zero;
        a.Placed = false;
        a.ActorType = JsonUtility.ToJson(t);
        a.ActorTypeId = TypeName;
        string json = JsonUtility.ToJson(a);
        global::AddActorModal.FinalizeToken(json);
    }
    public override string Label()
    {
        return Name;
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/Icon2";
    }

    public override void UpdatePanel(ActorData tokenData, string elementName)
    {
        base.UpdatePanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement mainHPBar = panel.Q("MainHPBar");
        mainHPBar.Q<Label>("CHP").text = $"{CurrentHP}";
        mainHPBar.Q<Label>("MHP").text = $"/{MaxHP}";
        mainHPBar.Q<ProgressBar>("HpBar").value = CurrentHP;
        mainHPBar.Q<ProgressBar>("HpBar").highValue = MaxHP;
        mainHPBar.Q<Label>("VIG").text = $"+{Vigor}";
        mainHPBar.Q<ProgressBar>("VigorBar").value = Vigor;
        mainHPBar.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(mainHPBar.Q("VigorBar"), Vigor > 0);
        UI.ToggleDisplay(mainHPBar.Q("VIG"), Vigor > 0);
        UI.ToggleDisplay(mainHPBar.Q("Wound1"), false);
        UI.ToggleDisplay(mainHPBar.Q("Wound2"), false);
        UI.ToggleDisplay(mainHPBar.Q("Wound3"), false);

        UI.ToggleDisplay(panel.Q("ElitePill"), Elite);
        UI.ToggleDisplay(panel.Q("BloodiedPill"), CurrentHP > 0 && CurrentHP <= MaxHP / 2 && CurrentHP > MaxHP / 4);
        UI.ToggleDisplay(panel.Q("CrisisPill"), CurrentHP > 0 && CurrentHP <= MaxHP / 4);

    }

    public override void UpdateOverhead(ActorData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;

        o.Q<ProgressBar>("VigorBar").value = Vigor;
        o.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(o.Q("VigorBar"), Vigor > 0);

        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;

        UI.ToggleDisplay(o, CurrentHP > 0 && tokenData.Placed);
    }

    public override void InitPanel(ActorData actorData, string elementName, bool selected)
    {
        base.InitPanel(actorData, elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement hpBar = UI.CreateFromTemplate("UI/TableTop/IconHPBar");
        hpBar.name = "MainHPBar";
        hpBar.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q("Bars").Add(hpBar);

        VisualElement s3 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s3.Q<Label>("Label").text = "MOVE";
        s3.Q<Label>("Value").text = $"{Move}";
        panel.Q("Stats").Add(s3);

        VisualElement s4 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s4.Q<Label>("Label").text = "DEF";
        s4.Q<Label>("Value").text = $"{Defense}";
        panel.Q("Stats").Add(s4);

        panel.Q("Pills").Add(Pill.InitStatic("ElitePill", "Elite", Color.purple));
        panel.Q("Pills").Add(Pill.InitStatic("ClassPill", FoeClass, Color));
        panel.Q("Pills").Add(Pill.InitStatic("BloodiedPill", "Bloodied", Color.red));
        panel.Q("Pills").Add(Pill.InitStatic("CrisisPill", "Crisis", Color.red));
    }

    // public override MenuItem[] GetMenuItems(bool placed)
    // {
    //     MenuItem[] baseItems = base.GetMenuItems(placed);

    //     List<MenuItem> items = new();
    //     items.Add(new MenuItem("ModHP", "Modify HP", () => { NumberPicker.ActorCommand("ModHP"); }));
    //     items.Add(new MenuItem("ModVIG", "Modify VIG", () => { NumberPicker.ActorCommand("ModVIG"); }));
    //     return baseItems.Concat(items.ToArray()).ToArray();
    // }

    public override void Command(string command, ActorData tokenData)
    {
        base.Command(command, tokenData);
        Actor token = tokenData.GetActor();
        if (command.StartsWith("ModHP"))
        {
            int original = CurrentHP;
            int changeValue = int.Parse(command.Split("|")[1]);
            CurrentHP = Clamped(0, CurrentHP + changeValue, MaxHP);
            int diff = CurrentHP - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_HP", Color.white);
                UpdateGraphic(tokenData);
            }
        }
        if (command.StartsWith("ModVIG"))
        {
            int original = Vigor;
            int changeValue = int.Parse(command.Split("|")[1]);
            Vigor = Clamped(0, Vigor + changeValue, MaxHP / 4);
            int diff = Vigor - original;
            int maxVigor = (FoeClass == "Legend") ? 15 : MaxHP / 4;
            diff = Math.Min(maxVigor, diff);
            if (FoeClass == "Legend") { }
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_VIG", Color.white);
            }
        }
        if (command.StartsWith("Damage"))
        {
            int diff = int.Parse(command.Split("|")[1]);
            if (Vigor + CurrentHP - diff < 0)
            {
                diff = Vigor + CurrentHP;
            }
            if (diff <= 0)
            {
                return;
            }
            if (diff < Vigor)
            {
                // Vig damage only
                Vigor -= diff;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_VIG", Color.white);
                }
            }
            else if (diff > Vigor && Vigor > 0)
            {
                // Vig zeroed and HP damage
                CurrentHP -= (diff - Vigor);
                Vigor = 0;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP/VIG", Color.white);
                    UpdateGraphic(tokenData);
                }
            }
            else if (Vigor <= 0)
            {
                // HP damage only
                CurrentHP -= diff;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                    UpdateGraphic(tokenData);
                }
            }
        }
    }
    private void UpdateGraphic(ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        token.SetDefeated(CurrentHP <= 0);
    }

}
