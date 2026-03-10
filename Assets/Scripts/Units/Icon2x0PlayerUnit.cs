using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Icon2x0PlayerUnit : Icon2x0Base
{
    private readonly static string TypeName = "Icon 2.0 Player";

    public string Name;
    public int CurrentHP;
    public int MaxHP;
    public int Vigor;
    public int Resolve;
    public int PartyResolve = 0;
    public string Job;
    public string Class;
    public int Move;
    public int Defense;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        UnitTokenRegistry.RegisterSystem($"{TypeName}");
        UnitTokenRegistry.RegisterInterfaceCallback($"{TypeName}", DeserializeAsInterface);
        UnitTokenRegistry.RegisterSimpleCallback($"{TypeName}|AddTokenModal", AddTokenModal);
    }


    public override string Label()
    {
        return Name;
    }

    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public static IUnitData DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<Icon2x0PlayerUnit>(json);
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/Icon2";
    }

    public static void AddTokenModal()
    {
        string[] playerJobs = StringUtility.CreateArray(
            "Mendicant/Celestian",
            "Mendicant/Chanter",
            "Mendicant/Chronomancer",
            "Mendicant/Fair Wright",
            "Mendicant/Harvester",
            "Mendicant/Herbalist",
            "Mendicant/Raconteur",
            "Mendicant/Sealer",
            "Mendicant/Seer",
            "Mendicant/Shrine Gaurdian",
            "Mendicant/Yaman",
            "Mendicant/Zephyr",
            "Stalwart/Bastion",
            "Stalwart/Bleak Knight",
            "Stalwart/Breaker",
            "Stalwart/Cleaver",
            "Stalwart/Colossus",
            "Stalwart/Corvidian",
            "Stalwart/Hawk Knight",
            "Stalwart/Knave",
            "Stalwart/Partizan",
            "Stalwart/Slayer",
            "Stalwart/Tactician",
            "Stalwart/Workshop Knight",
            "Vagabond/Blightrunner",
            "Vagabond/Dancer",
            "Vagabond/Dragoon",
            "Vagabond/Fool",
            "Vagabond/Freelancer",
            "Vagabond/Pathfinder",
            "Vagabond/Puppeteer",
            "Vagabond/Rake",
            "Vagabond/Stormscale",
            "Vagabond/Venomist",
            "Vagabond/Warden",
            "Vagabond/Weeping Assassin",
            "Wright/Alchemist",
            "Wright/Auran",
            "Wright/Enochian",
            "Wright/Entropist",
            "Wright/Geomancer",
            "Wright/Mistwalker",
            "Wright/Runesmith",
            "Wright/Snowbinder",
            "Wright/Spellblade",
            "Wright/Stormbender",
            "Wright/Theurgist",
            "Wright/Wayfarer"
        );

        Modal.AddMarkup("Description", "Tokens for ICON 2.0 player characters.");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddSearchField("PlayerJob", "Job", "Stalwart/Bastion", playerJobs);

        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "NameField", "PlayerJob"));

    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string name = UI.Modal.Q<TextField>("NameField").value;
        string playerJob = SearchField.GetValue(UI.Modal.Q("PlayerJob"));
        if (playerJob.Length == 0)
        {
            Toast.AddError("You must select a job");
            return;
        }
        string pclass = playerJob.Split("/")[0];
        string job = playerJob.Split("/")[1];

        Icon2x0PlayerUnit t = new()
        {
            Type = TypeName,
            Name = name,
            Job = job,
            Class = pclass,
            Vigor = 0,
            TokenMeta = TokenLibrary.GetSelectedMeta()
        };

        switch (pclass)
        {
            case "Stalwart":
                t.MaxHP = 40;
                t.CurrentHP = 40;
                t.Move = 4;
                t.Defense = 3;
                t.Color = ColorUtility.GetCommonColor("red");
                break;
            case "Vagabond":
                t.MaxHP = 32;
                t.CurrentHP = 32;
                t.Move = 4;
                t.Defense = 6;
                t.Color = ColorUtility.GetCommonColor("orange");
                break;
            case "Mendicant":
                t.MaxHP = 48;
                t.CurrentHP = 48;
                t.Move = 4;
                t.Defense = 4;
                t.Color = ColorUtility.GetCommonColor("green");
                break;
            case "Wright":
                t.MaxHP = 32;
                t.CurrentHP = 32;
                t.Move = 4;
                t.Defense = 4;
                t.Color = ColorUtility.GetCommonColor("blue");
                break;
        }

        AddToken.FinalizeToken(t.Serialize());
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.TokenCommand("ModHP"); }));
        items.Add(new MenuItem("ModVIG", "Modify Vigor", (evt) => { NumberPicker.TokenCommand("ModVIG"); }));
        items.Add(new MenuItem("ModRES", "Modify Resolve", (evt) => { NumberPicker.TokenCommand("ModRES"); }));
        items.Add(new MenuItem("ModPRES", "Modify Party Resolve", (evt) => { NumberPicker.AllTokensCommand("ModPRES"); }));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, TokenData tokenData)
    {
        base.Command(command, tokenData);
        Token token = tokenData.GetToken();
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
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_VIG", Color.white);
            }
        }
        if (command.StartsWith("ModRES"))
        {
            int original = Resolve;
            int changeValue = int.Parse(command.Split("|")[1]);
            Resolve = Clamped(0, Resolve + changeValue, 6);
            int diff = Resolve - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_RES", Color.white);
            }
        }
        if (command.StartsWith("ModPRES"))
        {
            int original = PartyResolve;
            int changeValue = int.Parse(command.Split("|")[1]);
            PartyResolve = Clamped(0, PartyResolve + changeValue, 6);
            int diff = PartyResolve - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_P-RES", Color.white);
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

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;

        o.Q<ProgressBar>("VigorBar").value = Vigor;
        o.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(o.Q("VigorBar"), Vigor > 0);

        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;

        UI.ToggleDisplay(o, CurrentHP > 0 && tokenData.Placed);
    }

    public override void UpdatePanel(TokenData tokenData, string elementName)
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

        VisualElement RESBar = panel.Q("ResBar");
        RESBar.Q<Label>("ResolveNum").text = $"{Resolve}";
        RESBar.Q<ProgressBar>("ResolveBar").value = Resolve + PartyResolve;
        RESBar.Q<ProgressBar>("ResolveBar").highValue = 6;
        RESBar.Q<Label>("PartyResolveNum").text = $"+{PartyResolve}";
        RESBar.Q<ProgressBar>("PartyResolveBar").value = PartyResolve;
        RESBar.Q<ProgressBar>("PartyResolveBar").highValue = 6;
        UI.ToggleDisplay(panel.Q("PartyResolveBar"), PartyResolve > 0);
        UI.ToggleDisplay(panel.Q("PartyResolveNum"), PartyResolve > 0);

        UI.ToggleDisplay(panel.Q("BloodiedPill"), CurrentHP > 0 && CurrentHP <= MaxHP / 2 && CurrentHP > MaxHP / 4);
        UI.ToggleDisplay(panel.Q("CrisisPill"), CurrentHP > 0 && CurrentHP <= MaxHP / 4);
    }

    public override void InitPanel(string elementName, bool selected)
    {
        base.InitPanel(elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement resBar = UI.CreateFromTemplate("UI/TableTop/IconResolveBar");
        resBar.name = "ResBar";
        resBar.Q<ProgressBar>("ResolveBar").value = Resolve;
        resBar.Q<ProgressBar>("ResolveBar").highValue = 6;
        panel.Q("Bars").Add(resBar);

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

        panel.Q("Pills").Add(Pill.InitStatic("JobPill", Job, Color));
        panel.Q("Pills").Add(Pill.InitStatic("ClassPill", Class, Color));
        panel.Q("Pills").Add(Pill.InitStatic("BloodiedPill", "Bloodied", Color.red));
        panel.Q("Pills").Add(Pill.InitStatic("CrisisPill", "Crisis", Color.red));
    }


    private void UpdateGraphic(TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        token.SetDefeated(CurrentHP <= 0);
    }
}
