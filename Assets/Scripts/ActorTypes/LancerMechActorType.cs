using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class LancerMechActorType : LancerBase
{
    private readonly static string TypeName = "Lancer Mech";

    #region Registration
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
        return JsonUtility.FromJson<LancerMechActorType>(json);
    }
    #endregion

    #region Stats
    public string Callsign;
    public string Pilot;
    public int MaxHP;
    public int CurrentHP;
    public int Overshield;
    public int Stress;
    public int MaxStress;
    public int Structure;
    public int MaxStructure;
    public int Heat;
    public int MaxHeat;
    public int Armor;
    public int Attack;
    public int TechAttack;
    public int Speed;
    public int Evade;
    public int EDefense;
    public int SensorRange;
    public int SaveTarget;
    #endregion

    #region Creation
    public static void AddActorModal()
    {
        var contents = Modal2.Contents("PrimaryDialog");
        var typeContainer = contents.Q("ActorTypeContainer");
        typeContainer.Clear();
        contents.Q("CreateActor")?.RemoveFromHierarchy();


        var callsign = Modal2.AddInlineTextField("Callsign", "Callsign", "");
        Modal2.MoveToContainer(callsign, typeContainer);

        var pilotname = Modal2.AddInlineTextField("PilotName", "Pilot", "");
        Modal2.MoveToContainer(pilotname, typeContainer);

        var shape = Modal2.AddInlineComboboxField("ShapeField", "Shape", "Hex 1", ActorType.ShapeOptions().ToList<string>());
        Modal2.MoveToContainer(shape, typeContainer);

        var color = Modal2.AddInlineComboboxField("ColorField", "Color", "Black", ColorUtility.CommonColors().ToList<string>());
        Modal2.MoveToContainer(color, typeContainer);

        var create = new ShunDialogClose();
        create.name = "CreateActor";
        create.text = "Create Actor";
        create.SetVariant(ButtonVariant.Primary);
        create.clicked += () => CreateClicked();
        contents.Q(className: "shun-dialog__footer").Add(create);
    }

    private static void CreateClicked()
    {
        Modal2.ReadContext("PrimaryDialog");
        string token = Modal2.GetComboboxFieldValue("Token");
        if (token.Length == 0)
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string callsign = Modal2.GetTextFieldValue("Callsign");
        string pilot = Modal2.GetTextFieldValue("PilotName");
        string shape = Modal2.GetComboboxFieldValue("ShapeField");
        string color = Modal2.GetComboboxFieldValue("ColorField");
        LancerMechActorType t = new()
        {
            Type = "Lancer Mech",
            Callsign = callsign,
            Pilot = pilot,
            MaxHP = 10,
            CurrentHP = 10,
            MaxStructure = 4,
            Structure = 4,
            MaxStress = 4,
            Stress = 4,
            MaxHeat = 6,
            Heat = 0,
            Speed = 4,
            Evade = 8,
            EDefense = 8,
            SensorRange = 10,
            SaveTarget = 10,
        };
        ActorPersistence a = new();
        a.Name = t.Label();
        a.Token = TokenLibraryModal.GetToken(token);
        a.Color = ColorUtility.GetCommonColor(color);
        a.Shape = shape;
        a.Position = Vector3.zero;
        a.Placed = false;
        a.ActorType = JsonUtility.ToJson(t);
        a.ActorTypeId = TypeName;
        string json = JsonUtility.ToJson(a);
        global::AddActorModal.FinalizeToken(json);
    }
    #endregion

    public override string Label()
    {
        return $"{Callsign}/{Pilot}";
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/LancerMech";
    }

    public override List<MenuItem> GetMenuItems(bool placed)
    {
        var baseItems = base.GetMenuItems(placed);

        var changeValues = FindParent("Change Values", baseItems);
        changeValues.Children.Add(new MenuItem("Modify HP", () => { NumberPicker.ActorCommand("ModHP"); }));
        changeValues.Children.Add(new MenuItem("Alter Core Stats", AlterStatModal));
        changeValues.Children.Add(new MenuItem("Damage HP/Shield", () => { NumberPicker.ActorCommand("Damage", false); }));
        changeValues.Children.Add(new MenuItem("Modify HP", () => { NumberPicker.ActorCommand("ModHP"); }));
        changeValues.Children.Add(new MenuItem("Modify Shield", () => { NumberPicker.ActorCommand("ModShield"); }));
        return baseItems;
    }

    public override void Command(string command, ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        base.Command(command, tokenData);
        if (command.StartsWith("ModHP"))
        {
            int original = CurrentHP;
            int changeValue = int.Parse(command.Split("|")[1]);
            CurrentHP = Clamped(0, CurrentHP + changeValue, MaxHP);
            int diff = CurrentHP - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(tokenData.GetActor(), $"/{plus}{diff}|_HP", Color.white);
            }
        }
        else if (command.StartsWith("ModShield"))
        {
            int original = Overshield;
            int changeValue = int.Parse(command.Split("|")[1]);
            Overshield = Clamped(0, Overshield + changeValue, 1000);
            int diff = Overshield - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(tokenData.GetActor(), $"/{plus}{diff}|_OVERSHIELD", Color.white);
            }
        }
        else if (command.StartsWith("Damage"))
        {
            int diff = int.Parse(command.Split("|")[1]);
            if (Overshield + CurrentHP - diff < 0)
            {
                diff = Overshield + CurrentHP;
            }
            if (diff <= 0)
            {
                return;
            }
            if (diff < Overshield)
            {
                // Vig damage only
                Overshield -= diff;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_OVERSHIELD", Color.white);
                }
            }
            else if (diff > Overshield && Overshield > 0)
            {
                // Vig zeroed and HP damage
                CurrentHP -= (diff - Overshield);
                Overshield = 0;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP/SHIELD", Color.white);
                }
            }
            else if (Overshield <= 0)
            {
                // HP damage only
                CurrentHP -= diff;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                }
            }
        }
        else if (command.StartsWith("ModHeat"))
        {
            int original = Heat;
            int changeValue = int.Parse(command.Split("|")[1]);
            Heat = Clamped(0, Heat + changeValue, MaxHeat);
            int diff = Heat - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_HEAT", Color.white);
            }
        }
        else if (command.StartsWith("ModStructure"))
        {
            int original = Structure;
            int changeValue = int.Parse(command.Split("|")[1]);
            Structure = Clamped(0, Structure + changeValue, MaxStructure);
            int diff = Structure - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_STRUCTURE", Color.white);
            }
        }
        else if (command.StartsWith("ModStress"))
        {
            int original = Stress;
            int changeValue = int.Parse(command.Split("|")[1]);
            Stress = Clamped(0, Stress + changeValue, MaxStress);
            int diff = Stress - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_STRESS", Color.white);
            }
        }
        else if (command.StartsWith("UpdateStats"))
        {
            string json = command.Split("|")[1];
            LancerMechActorType lmu = JsonUtility.FromJson<LancerMechActorType>(json);
            MaxHP = lmu.MaxHP;
            MaxHeat = lmu.MaxHeat;
            MaxStress = lmu.MaxStress;
            MaxStructure = lmu.MaxStructure;
            Attack = lmu.Attack;
            TechAttack = lmu.TechAttack;
            Armor = lmu.Armor;
            EDefense = lmu.EDefense;
            Evade = lmu.Evade;
            Speed = lmu.Speed;
            SaveTarget = lmu.SaveTarget;
            SensorRange = lmu.SensorRange;
            PopoverText.Create(token, $"_STAT|_CHANGE", Color.white);
        }
        else if (command.StartsWith("Rename|"))
        {
            Callsign = command.Split("|")[1];
            Pilot = command.Split("|")[2];
        }
        else
        {
            Debug.Log(command);
        }

    }

    public override void UpdatePanel(ActorData tokenData, string elementName)
    {
        base.UpdatePanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement mainHPBar = panel.Q("Bars").Q("MainHPBar");
        mainHPBar.Q<Label>("CHP").text = $"{CurrentHP}";
        mainHPBar.Q<Label>("MHP").text = $"/{MaxHP}";
        mainHPBar.Q<ProgressBar>("HpBar").value = CurrentHP;
        mainHPBar.Q<ProgressBar>("HpBar").highValue = MaxHP;
        mainHPBar.Q<Label>("VIG").text = $"+{Overshield}";
        mainHPBar.Q<ProgressBar>("VigorBar").value = Overshield;
        mainHPBar.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(mainHPBar.Q("VigorBar"), Overshield > 0);
        UI.ToggleDisplay(mainHPBar.Q("VIG"), Overshield > 0);
        UI.ToggleDisplay(mainHPBar.Q("Wound1"), false);
        UI.ToggleDisplay(mainHPBar.Q("Wound2"), false);
        UI.ToggleDisplay(mainHPBar.Q("Wound3"), false);

        panel.Q("Structure").Q<Label>("Pips").text = SymbolString("◆", Structure, MaxStructure);
        panel.Q("Stress").Q<Label>("Pips").text = SymbolString("●", Stress, MaxStress);
        panel.Q("Heat").Q<Label>("Pips").text = SymbolString("▰", Heat, MaxHeat);
    }

    public override void UpdateOverhead(ActorData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;
        o.Q<Label>("Structure").text = SymbolString("◆", Structure, MaxStructure);
        o.Q<Label>("Stress").text = SymbolString("●", Stress, MaxStress);
        o.Q<Label>("Heat").text = SymbolString("▰", Heat, MaxHeat);
    }

    public override void InitPanel(ActorData actorData, string elementName, bool selected)
    {
        base.InitPanel(actorData, elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        bool left = elementName == "LeftTokenPanel";

        VisualElement container = new();
        container.style.flexDirection = left ? FlexDirection.Row : FlexDirection.RowReverse;
        container.style.position = Position.Absolute;
        if (!left)
        {
            container.style.right = 0;
        }
        int mr = left ? 8 : 0;
        int ml = left ? 0 : 8;

        VisualElement hpBar = UI.CreateFromTemplate("UI/TableTop/IconHPBar");
        hpBar.style.marginRight = mr;
        hpBar.style.marginLeft = ml;
        hpBar.name = "MainHPBar";
        hpBar.Q<ProgressBar>("HpBar").style.minWidth = 100;

        container.Add(hpBar);

        ActorData data = selected ? Actor.GetSelected().Data : null;

        VisualElement structure = UI.CreateFromTemplate("UI/TableTop/LancerCoreStat");
        structure.style.marginRight = mr;
        structure.style.marginLeft = ml;
        structure.name = "Structure";
        structure.Q<Label>("StatName").text = "STRUCTURE";
        structure.Q<Label>("Pips").style.color = ColorUtility.GetColor("#FF0093");
        structure.Q<Label>("Pips").text = SymbolString("◆", Structure, MaxStructure);
        if (data != null)
        {
            structure.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModStructure|1"); });
            structure.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModStructure|-1"); });
        }
        else
        {
            UI.ToggleDisplay(structure.Q<Button>("Increment"), false);
            UI.ToggleDisplay(structure.Q<Button>("Decrement"), false);
        }
        container.Add(structure);

        VisualElement stress = UI.CreateFromTemplate("UI/TableTop/LancerCoreStat");
        stress.style.marginRight = mr;
        stress.style.marginLeft = ml;
        stress.name = "Stress";
        stress.Q<Label>("StatName").text = "STRESS";
        stress.Q<Label>("Pips").style.color = ColorUtility.GetColor("#FF7300");
        stress.Q<Label>("Pips").text = SymbolString("●", Stress, MaxStress);
        if (data != null)
        {
            stress.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModStress|1"); });
            stress.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModStress|-1"); });
        }
        else
        {
            UI.ToggleDisplay(stress.Q<Button>("Increment"), false);
            UI.ToggleDisplay(stress.Q<Button>("Decrement"), false);
        }
        container.Add(stress);

        VisualElement heat = UI.CreateFromTemplate("UI/TableTop/LancerCoreStat");
        heat.name = "Heat";
        heat.Q<Label>("StatName").text = "HEAT";
        heat.Q<Label>("Pips").style.color = ColorUtility.GetColor("#E4004C");
        heat.Q<Label>("Pips").text = SymbolString("▰", Heat, MaxHeat);
        if (data != null)
        {
            heat.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModHeat|1"); });
            heat.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModHeat|-1"); });
        }
        else
        {
            UI.ToggleDisplay(heat.Q<Button>("Increment"), false);
            UI.ToggleDisplay(heat.Q<Button>("Decrement"), false);
        }
        container.Add(heat);

        panel.Q("Bars").Add(container);

        List<string> stats = new();
        stats.Add($"ATK/TECH|+{Attack}/{TechAttack}");
        stats.Add($"ARMOR/EVADE|{Armor}/{Evade}");
        stats.Add($"E-DEFENSE|{EDefense}");
        stats.Add($"SPEED|{Speed}");
        stats.Add($"SENSOR|{SensorRange}");
        stats.Add($"SAVE|{SaveTarget}");
        foreach (string s in stats)
        {
            VisualElement sTemplate = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
            sTemplate.Q<Label>("Label").text = s.Split("|")[0];
            sTemplate.Q<Label>("Label").style.minWidth = 70;
            sTemplate.Q<Label>("Value").text = s.Split("|")[1];
            panel.Q("Stats").Add(sTemplate);
        }

        // VisualElement s4 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        // s4.Q<Label>("Label").text = "DEF";
        // s4.Q<Label>("Value").text = $"{Defense}";
        // panel.Q("Stats").Add(s4);

        // panel.Q("Pills").Add(Pill.InitStatic("JobPill", Job, Color));
        // panel.Q("Pills").Add(Pill.InitStatic("ClassPill", Class, Color));
        // panel.Q("Pills").Add(Pill.InitStatic("BloodiedPill", "Bloodied", Color.red));
        // panel.Q("Pills").Add(Pill.InitStatic("CrisisPill", "Crisis", Color.red));
    }

    private void AlterStatModal()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Alter Core Stats");

        Modal2.AddInlineNumberNudgerField("MaxHP", "Max HP", MaxHP, 0, 50);
        Modal2.AddInlineNumberNudgerField("MaxHeat", "Max Heat", MaxHeat, 1, 50);
        Modal2.AddInlineNumberNudgerField("Stress", "Max Stress", MaxStress, 1, 50);
        Modal2.AddInlineNumberNudgerField("Struct", "Max Structure", MaxStructure, 1, 50);
        Modal2.AddInlineNumberNudgerField("Attack", "Attack Bonus", Attack, 0, 50);
        Modal2.AddInlineNumberNudgerField("Tech", "Tech Attack", TechAttack, 0, 50);
        Modal2.AddInlineNumberNudgerField("Armor", "Armor", Armor, 0, 50);
        Modal2.AddInlineNumberNudgerField("EDef", "E-Defense", EDefense, 0, 50);
        Modal2.AddInlineNumberNudgerField("Evade", "Evade", Evade, 0, 50);
        Modal2.AddInlineNumberNudgerField("Speed", "Speed", Speed, 0, 50);
        Modal2.AddInlineNumberNudgerField("Save", "Save Target", SaveTarget, 0, 50);
        Modal2.AddInlineNumberNudgerField("Sensor", "Sensor Range", SensorRange, 0, 50);

        // Modal.AddColumns("Stats", 2);
        // Modal.MoveToColumn("Stats_0", "MaxHP");
        // Modal.MoveToColumn("Stats_0", "MaxHeat");
        // Modal.MoveToColumn("Stats_0", "Struct");
        // Modal.MoveToColumn("Stats_0", "Stress");
        // Modal.MoveToColumn("Stats_0", "Attack");
        // Modal.MoveToColumn("Stats_0", "Tech");

        // Modal.MoveToColumn("Stats_1", "Armor");
        // Modal.MoveToColumn("Stats_1", "EDef");
        // Modal.MoveToColumn("Stats_1", "Evade");
        // Modal.MoveToColumn("Stats_1", "Speed");
        // Modal.MoveToColumn("Stats_1", "Save");
        // Modal.MoveToColumn("Stats_1", "Sensor");

        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Save", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            MaxHP = Modal2.GetNumberNudgerFieldValue("MaxHP");
            MaxHeat = Modal2.GetNumberNudgerFieldValue("MaxHeat");
            MaxStress = Modal2.GetNumberNudgerFieldValue("Stress");
            MaxStructure = Modal2.GetNumberNudgerFieldValue("Struct");
            Attack = Modal2.GetNumberNudgerFieldValue("Attack");
            TechAttack = Modal2.GetNumberNudgerFieldValue("Tech");
            Armor = Modal2.GetNumberNudgerFieldValue("Armor");
            EDefense = Modal2.GetNumberNudgerFieldValue("EDef");
            Evade = Modal2.GetNumberNudgerFieldValue("Evade");
            Speed = Modal2.GetNumberNudgerFieldValue("Speed");
            SaveTarget = Modal2.GetNumberNudgerFieldValue("Save");
            SensorRange = Modal2.GetNumberNudgerFieldValue("Sensor");
            string serialized = Serialize();

            Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"UpdateStats|{serialized}");
            this.InitPanel(Actor.GetSelected().Data, "LeftTokenPanel", true);
        });
        Modal2.Open("Alter Stats");
    }

    protected override void RenameModal()
    {
        SelectionMenu.Hide();
        ActorData data = Actor.GetSelected().Data;
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Edit Name");
        Modal2.AddInlineTextField("Name", "Callsign", Callsign);
        Modal2.AddInlineTextField("Pilot", "Pilot Name", Pilot);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Confirm", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            string newName = Modal2.GetTextFieldValue("Name").Trim();
            string newPilotName = Modal2.GetTextFieldValue("Pilot").Trim();
            Player.Self().CmdRequestActorCommand(data.Id, $"Rename|{newName}|{newPilotName}");
        });
    }
}