using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public int Structure;
    public int Heat;
    public int MaxHeat;
    public int Armor;
    public int Attack;
    public int TechAttack;
    public int Speed;
    public int Evade;
    public int EDefense;
    public int SensorRange;
    #endregion

    #region Creation
    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "Lancer Mech tokens have primary HP, Structure, Stress, and Heat stats by default.");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddDropdownField("ShapeField", "Shape", "Hex 1", UnitData.HexShapeOptions());
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddIntField("MaxHPField", "Max HP", 10);
        Modal.AddIntField("MaxHeatField", "Heat Cap", 4);
        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "NameField", "ShapeField", "ColorField", "MaxHPField", "MaxHeatField"));
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
        int maxHP = UI.Modal.Q<IntegerField>("MaxHPField").value;
        int maxHeat = UI.Modal.Q<IntegerField>("MaxHeatField").value;
        string color = UI.Modal.Q<DropdownField>("ColorField").value;
        LancerMechUnit t = new()
        {
            Type = "Lancer Mech",
            Name = name,
            MaxHP = maxHP,
            CurrentHP = maxHP,
            Structure = 4,
            Stress = 4,
            MaxHeat = maxHeat,
            Heat = 0,
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
        items.Add(new MenuItem("Damage", "Damage HP/Shield", (evt) => { NumberPicker.TokenCommand("Damage", false); }));
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.TokenCommand("ModHP"); }));
        items.Add(new MenuItem("ModShield", "Modify Shield", (evt) => { NumberPicker.TokenCommand("ModShield"); }));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        base.Command(command, tokenData);
        if (command.StartsWith("ModShield"))
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
            Structure = Clamped(0, Structure + changeValue, 4);
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
            Stress = Clamped(0, Stress + changeValue, 4);
            int diff = Stress - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_STRESS", Color.white);
            }
        }
        // if (command.StartsWith("HeatUp|"))
        // {
        //     GainHeat(command, tokenData);
        // }
        // if (command.StartsWith("StructureDown|"))
        // {
        //     LoseStructure(command, tokenData);
        // }
        // if (command.StartsWith("StressDown|"))
        // {
        //     LoseStress(command, tokenData);
        // }
        // if (command.StartsWith("Rename|"))
        // {
        //     Name = command.Split("|")[1];
        // }
        else
        {
            Debug.Log(command);
        }

    }

    public override void UpdatePanel(TokenData tokenData, string elementName)
    {
        MaxHP = 16;
        MaxHeat = 6;
        Speed = 4;
        Evade = 8;
        EDefense = 8;
        SensorRange = 10;
        Armor = 0;
        Overshield = 4;

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

        panel.Q("Structure").Q<Label>("Pips").text = SymbolString("◆", Structure, 4);
        panel.Q("Stress").Q<Label>("Pips").text = SymbolString("●", Stress, 4);
        panel.Q("Heat").Q<Label>("Pips").text = SymbolString("▰", Heat, MaxHeat);
    }

    public override void UpdateOverhead(TokenData tokenData)
    {
        MaxHP = 16;
        MaxHeat = 6;
        Speed = 4;
        Evade = 8;
        EDefense = 8;
        SensorRange = 10;
        Armor = 0;

        VisualElement o = tokenData.OverheadElement;
        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;
        o.Q<Label>("Structure").text = SymbolString("◆", Structure, 4);
        o.Q<Label>("Stress").text = SymbolString("●", Stress, 4);
        o.Q<Label>("Heat").text = SymbolString("▰", Heat, MaxHeat);
    }

    public override void InitPanel(string elementName, bool selected)
    {
        MaxHP = 16;
        MaxHeat = 6;
        Speed = 4;
        Evade = 8;
        EDefense = 8;
        SensorRange = 10;
        Armor = 0;

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
        structure.Q<Label>("Pips").text = SymbolString("◆", Structure, 4);
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
        stress.Q<Label>("Pips").text = SymbolString("●", Stress, 4);
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
        stats.Add($"ARMOR|{Armor}");
        stats.Add($"E-DEFENSE|{EDefense}");
        stats.Add($"EVADE|{Evade}");
        stats.Add($"SPEED|{Speed}");
        stats.Add($"SENSOR|{SensorRange}");
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


    private void GainHeat(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        int diff = int.Parse(command.Split("|")[1]);
        if (Heat + diff > MaxHeat)
        {
            diff = MaxHeat - Heat;
        }
        if (diff > 0)
        {
            Heat += diff;
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/+{diff}|_HEAT", Color.white);
            }
        }
    }

    private void LoseStress(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        int diff = int.Parse(command.Split("|")[1]);
        if (Stress - diff < 0)
        {
            diff = Stress;
        }
        if (diff > 0)
        {
            Stress -= diff;
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/-{diff}|_STRESS", Color.white);
            }
        }
    }

    private void LoseStructure(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        int diff = int.Parse(command.Split("|")[1]);
        if (Structure - diff < 0)
        {
            diff = Structure;
        }
        if (diff > 0)
        {
            Structure -= diff;
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/-{diff}|_STRUCT", Color.white);
            }
        }
    }

    private void GainHP(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        int diff = int.Parse(command.Split("|")[1]);
        if (CurrentHP + diff > MaxHP)
        {
            diff = MaxHP - CurrentHP;
        }
        if (diff > 0)
        {
            CurrentHP += diff;
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/+{diff}|_HP", Color.white);
            }
        }
    }

    private void LoseHP(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        int diff = int.Parse(command.Split("|")[1]);
        if (CurrentHP - diff < 0)
        {
            diff = CurrentHP;
        }
        if (diff > 0)
        {
            CurrentHP -= diff;
            if (tokenData.Placed)
            {
                PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
            }
        }
    }
}