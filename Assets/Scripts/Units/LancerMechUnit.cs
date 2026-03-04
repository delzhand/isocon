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
    public int Stress;
    public int Structure;
    public int Heat;
    public int MaxHeat;
    public int Armor;
    #endregion

    #region Creation
    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "Lancer Mech tokens have primary HP, Structure, Stress, and Heat stats by default.");
        Modal.AddTokenField("TokenSearchField");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", StringUtility.CreateArray("Square 1x1", "Square 2x2", "Square 3x3", "Hex 1", "Hex 2", "Hex 3"));
        Modal.AddDropdownField("ColorField", "Color", "Black", ColorUtility.CommonColors());
        Modal.AddIntField("MaxHPField", "Max HP", 10);
        Modal.AddIntField("MaxHeatField", "Heat Cap", 4);
        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "TokenSearchField", "NameField", "ShapeField", "ColorField", "MaxHPField", "MaxHeatField"));
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
            System = "Lancer Mech",
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
        items.Add(new MenuItem("GainHP", "HP Up", (evt) => { NumberPicker.NumberCommand("HpUp"); }));
        items.Add(new MenuItem("LoseHP", "HP Down", (evt) => { NumberPicker.NumberCommand("HpDown"); }));
        items.Add(new MenuItem("HeatUp", "Heat Up", (evt) => { DirectCommand("HeatUp|1"); }));
        items.Add(new MenuItem("HeatDown", "Heat Down", (evt) => { DirectCommand("HeatDown|1"); }));
        items.Add(new MenuItem("StressUp", "Stress Up", (evt) => { DirectCommand("StressUp|1"); }));
        items.Add(new MenuItem("StressDown", "Stress Down", (evt) => { DirectCommand("StressDown|1"); }));
        items.Add(new MenuItem("StructureUp", "Structure Up", (evt) => { DirectCommand("StructureUp|1"); }));
        items.Add(new MenuItem("StructureDown", "Structure Down", (evt) => { DirectCommand("StructureDown|1"); }));

        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, TokenData tokenData)
    {
        base.Command(command, tokenData);
        if (command.StartsWith("HpUp|"))
        {
            GainHP(command, tokenData);
        }
        if (command.StartsWith("HpDown|"))
        {
            LoseHP(command, tokenData);
        }

        if (command.StartsWith("HeatUp|"))
        {
            GainHeat(command, tokenData);
        }
        if (command.StartsWith("StructureDown|"))
        {
            LoseStructure(command, tokenData);
        }
        if (command.StartsWith("StressDown|"))
        {
            LoseStress(command, tokenData);
        }
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }

    }

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;
        o.Q<Label>("Structure").text = SymbolString("◆", Structure, 4);
        o.Q<Label>("Stress").text = SymbolString("▼", Stress, 4);
        o.Q<Label>("Heat").text = SymbolString("▰", Heat, MaxHeat);
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