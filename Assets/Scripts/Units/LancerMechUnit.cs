using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IsoconUILibrary;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class LancerMechUnit : UnitData
{
    #region Registration
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        UnitTokenRegistry.RegisterSystem("Lancer Mech");
        UnitTokenRegistry.RegisterInterfaceCallback("Lancer Mech", DeserializeAsInterface);
        UnitTokenRegistry.RegisterSimpleCallback("Lancer Mech|AddTokenModal", AddTokenModal);
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
    public static IUnitData DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<LancerMechUnit>(json);
    }
    #endregion

    #region Stats
    public string Name;
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
    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "Lancer Mech tokens have primary HP, Structure, Stress, and Heat stats by default.");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddDropdownField("ShapeField", "Shape", "Hex 1", UnitData.HexShapeOptions());
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "NameField", "ShapeField", "ColorField"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string name = UI.Modal.Q<TextField>("NameField").value;
        string shape = UI.Modal.Q<DropdownField>("ShapeField").value;
        string color = UI.Modal.Q<DropdownField>("ColorField").value;
        LancerMechUnit t = new()
        {
            Type = "Lancer Mech",
            Name = name,
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
            Shape = shape,
            Color = ColorUtility.GetCommonColor(color),
            TokenMeta = TokenLibrary.GetSelectedMeta()
        };
        AddToken.FinalizeToken(t.Serialize());
    }
    #endregion

    public override string Label()
    {
        return Name;
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/LancerMech";
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("CoreStats", "Alter Stats", (evt) => { AlterStatModal(); }));
        items.Add(new MenuItem("Damage", "Damage HP/Shield", (evt) => { NumberPicker.TokenCommand("Damage", false); }));
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.TokenCommand("ModHP"); }));
        items.Add(new MenuItem("ModShield", "Modify Shield", (evt) => { NumberPicker.TokenCommand("ModShield"); }));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
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
                PopoverText.Create(tokenData.GetToken(), $"/{plus}{diff}|_HP", Color.white);
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
                PopoverText.Create(tokenData.GetToken(), $"/{plus}{diff}|_OVERSHIELD", Color.white);
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
            LancerMechUnit lmu = JsonUtility.FromJson<LancerMechUnit>(json);
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
        else
        {
            Debug.Log(command);
        }

    }

    public override void UpdatePanel(TokenData tokenData, string elementName)
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

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;
        o.Q<Label>("Structure").text = SymbolString("◆", Structure, MaxStructure);
        o.Q<Label>("Stress").text = SymbolString("●", Stress, MaxStress);
        o.Q<Label>("Heat").text = SymbolString("▰", Heat, MaxHeat);
    }

    public override void InitPanel(string elementName, bool selected)
    {
        base.InitPanel(elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement container = new();
        container.style.flexDirection = FlexDirection.Row;
        container.style.position = Position.Absolute;
        int mr = 8;

        VisualElement hpBar = UI.CreateFromTemplate("UI/TableTop/IconHPBar");
        hpBar.style.marginRight = mr;
        hpBar.name = "MainHPBar";
        hpBar.Q<ProgressBar>("HpBar").style.minWidth = 100;

        container.Add(hpBar);

        TokenData data = selected ? Token.GetSelected().Data : null;

        VisualElement structure = UI.CreateFromTemplate("UI/TableTop/LancerCoreStat");
        structure.style.marginRight = mr;
        structure.name = "Structure";
        structure.Q<Label>("StatName").text = "STRUCTURE";
        structure.Q<Label>("Pips").style.color = ColorUtility.GetColor("#FF0093");
        structure.Q<Label>("Pips").text = SymbolString("◆", Structure, MaxStructure);
        if (data != null)
        {
            structure.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestTokenDataCommand(data.Id, "ModStructure|1"); });
            structure.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestTokenDataCommand(data.Id, "ModStructure|-1"); });
        }
        else
        {
            UI.ToggleDisplay(structure.Q<Button>("Increment"), false);
            UI.ToggleDisplay(structure.Q<Button>("Decrement"), false);
        }
        container.Add(structure);

        VisualElement stress = UI.CreateFromTemplate("UI/TableTop/LancerCoreStat");
        stress.style.marginRight = mr;
        stress.name = "Stress";
        stress.Q<Label>("StatName").text = "STRESS";
        stress.Q<Label>("Pips").style.color = ColorUtility.GetColor("#FF7300");
        stress.Q<Label>("Pips").text = SymbolString("●", Stress, MaxStress);
        if (data != null)
        {
            stress.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestTokenDataCommand(data.Id, "ModStress|1"); });
            stress.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestTokenDataCommand(data.Id, "ModStress|-1"); });
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
            heat.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestTokenDataCommand(data.Id, "ModHeat|1"); });
            heat.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestTokenDataCommand(data.Id, "ModHeat|-1"); });
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
        Modal.Reset("Alter Core Stats");
        Modal.AddNumberNudgerField("MaxHP", "Max HP", MaxHP, 0);
        Modal.AddNumberNudgerField("MaxHeat", "Max Heat", MaxHeat, 1);
        Modal.AddNumberNudgerField("Stress", "Max Stress", MaxStress, 1);
        Modal.AddNumberNudgerField("Struct", "Max Structure", MaxStructure, 1);
        Modal.AddNumberNudgerField("Attack", "Attack Bonus", Attack, 0);
        Modal.AddNumberNudgerField("Tech", "Tech Attack", TechAttack, 0);
        Modal.AddNumberNudgerField("Armor", "Armor", Armor, 0);
        Modal.AddNumberNudgerField("EDef", "E-Defense", EDefense, 0);
        Modal.AddNumberNudgerField("Evade", "Evade", Evade, 0);
        Modal.AddNumberNudgerField("Speed", "Speed", Speed, 0);
        Modal.AddNumberNudgerField("Save", "Save Target", SaveTarget, 0);
        Modal.AddNumberNudgerField("Sensor", "Sensor Range", SensorRange, 0);
        Modal.AddColumns("Stats", 2);
        Modal.MoveToColumn("Stats_0", "MaxHP");
        Modal.MoveToColumn("Stats_0", "MaxHeat");
        Modal.MoveToColumn("Stats_0", "Struct");
        Modal.MoveToColumn("Stats_0", "Stress");
        Modal.MoveToColumn("Stats_0", "Attack");
        Modal.MoveToColumn("Stats_0", "Tech");

        Modal.MoveToColumn("Stats_1", "Armor");
        Modal.MoveToColumn("Stats_1", "EDef");
        Modal.MoveToColumn("Stats_1", "Evade");
        Modal.MoveToColumn("Stats_1", "Speed");
        Modal.MoveToColumn("Stats_1", "Save");
        Modal.MoveToColumn("Stats_1", "Sensor");

        Modal.AddPreferredButton("Save", (evt) =>
        {
            MaxHP = UI.Modal.Q<NumberNudger>("MaxHP").value;
            MaxHeat = UI.Modal.Q<NumberNudger>("MaxHeat").value;
            MaxStress = UI.Modal.Q<NumberNudger>("Stress").value;
            MaxStructure = UI.Modal.Q<NumberNudger>("Struct").value;
            Attack = UI.Modal.Q<NumberNudger>("Attack").value;
            TechAttack = UI.Modal.Q<NumberNudger>("Tech").value;
            Armor = UI.Modal.Q<NumberNudger>("Armor").value;
            EDefense = UI.Modal.Q<NumberNudger>("EDef").value;
            Evade = UI.Modal.Q<NumberNudger>("Evade").value;
            Speed = UI.Modal.Q<NumberNudger>("Speed").value;
            SaveTarget = UI.Modal.Q<NumberNudger>("Save").value;
            SensorRange = UI.Modal.Q<NumberNudger>("Sensor").value;
            string serialized = Serialize();

            Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, $"UpdateStats|{serialized}");
            Modal.Close();
            this.InitPanel("LeftTokenPanel", true);
        });
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }
}