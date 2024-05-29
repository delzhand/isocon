using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using IsoconUILibrary;
using SimpleJSON;
using Random = UnityEngine.Random;
using Unity.Mathematics;

public class Icon_v1_5 : GameSystem
{

    public static int PartyResolve = 0;

    public override string SystemName()
    {
        return "ICON 1.5";
    }

    public override string GetSystemVars()
    {
        return $"{RoundNumber}|{PartyResolve}";
    }

    public override void SetSystemVars(string vars)
    {
        RoundNumber = int.Parse(vars.Split("|")[0]);
        UI.System.Q<Label>("TurnNumber").text = RoundNumber.ToString();
        PartyResolve = int.Parse(vars.Split("|")[1]);
    }

    public override string GetOverheadAsset()
    {
        return "UITemplates/GameSystem/IconOverhead";
    }

    public override string TurnAdvanceMessage()
    {
        return "Increase the round counter, gain +1 group resolve, and reset ended turns?";
    }

    public override void Setup()
    {
        base.Setup();
        SetupPanel("SelectedTokenPanel", true);
        SetupPanel("FocusedTokenPanel", false);
    }

    private void SetupPanel(string elementName, bool editable)
    {
        VisualElement panel = UI.System.Q(elementName);
        VisualElement unitPanel = UI.CreateFromTemplate("UITemplates/GameSystem/IconUnitPanel");

        unitPanel.Q("Damage").Q<Label>("Label").text = "DMG/FRAY";
        unitPanel.Q("Range").Q<Label>("Label").text = "RNG";
        unitPanel.Q("Speed").Q<Label>("Label").text = "SPD/DASH";
        unitPanel.Q("Defense").Q<Label>("Label").text = "DEF";
        unitPanel.Q<Button>("AlterVitals").RegisterCallback<ClickEvent>(AlterVitalsModal);
        unitPanel.Q<Button>("AlterStatus").RegisterCallback<ClickEvent>(AlterStatusModal);
        panel.Q("Data").Add(unitPanel);
        panel.Q("ExtraInfo").Add(new Label() { name = "Class" });
        panel.Q("ExtraInfo").Add(new Label() { name = "Job" });
        panel.Q("ExtraInfo").Add(new Label() { name = "Elite", text = "Elite" });
    }

    public override void Teardown()
    {
        TeardownPanel("SelectedTokenPanel");
        TeardownPanel("FocusedTokenPanel");
    }

    private void TeardownPanel(string elementName)
    {
        VisualElement panel = UI.System.Q(elementName);
        panel.Q("ExtraInfo").Clear();
        panel.Q("Data").Clear();
    }

    private static void AlterVitalsModal(ClickEvent evt)
    {
        Modal.Reset("Alter Vitals");
        Modal.AddIntField("Number", "Value", 0);
        UI.Modal.Q("Number").AddToClassList("big-number");
        Modal.AddContentButton("Damage", "Damage HP/VIG", (evt) => AlterVitals("Damage"));
        Modal.AddColumns("VitalColumns", 2);

        Modal.AddContentButton("ReduceHP", "Reduce HP", (evt) => AlterVitals("LoseHP"));
        Modal.AddContentButton("ReduceVIG", "Reduce VIG", (evt) => AlterVitals("LoseVIG"));
        Modal.AddContentButton("ReduceRES", "Reduce RES", (evt) => AlterVitals("LoseRES"));
        Modal.AddContentButton("ReducePRES", "Reduce P-RES", (evt) => AlterVitals("LosePRES"));
        Modal.MoveToColumn("VitalColumns_0", "ReduceHP");
        Modal.MoveToColumn("VitalColumns_0", "ReduceVIG");
        Modal.MoveToColumn("VitalColumns_0", "ReduceRES");
        Modal.MoveToColumn("VitalColumns_0", "ReducePRES");

        Modal.AddContentButton("RecoverHP", "Recover HP", (evt) => AlterVitals("GainHP"));
        Modal.AddContentButton("RecoverVIG", "Recover VIG", (evt) => AlterVitals("GainVIG"));
        Modal.AddContentButton("RecoverRES", "Recover RES", (evt) => AlterVitals("GainRES"));
        Modal.AddContentButton("RecoverPRES", "Recover P-RES", (evt) => AlterVitals("GainPRES"));
        Modal.MoveToColumn("VitalColumns_1", "RecoverHP");
        Modal.MoveToColumn("VitalColumns_1", "RecoverVIG");
        Modal.MoveToColumn("VitalColumns_1", "RecoverRES");
        Modal.MoveToColumn("VitalColumns_1", "RecoverPRES");

        Modal.AddSeparator();

        Modal.AddColumns("WoundColumns", 2);
        Modal.AddContentButton("AddWound", "Add Wound", (evt) => AlterVitals("GainWound"));
        Modal.AddContentButton("RemoveWound", "Remove Wound", (evt) => AlterVitals("LoseWound"));
        Modal.MoveToColumn("WoundColumns_0", "AddWound");
        Modal.MoveToColumn("WoundColumns_1", "RemoveWound");
        Modal.AddPreferredButton("Complete", Modal.CloseEvent);
    }

    private static void AlterVitals(string cmd)
    {
        int val = UI.Modal.Q<IntegerField>("Number").value;
        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelected().Data.Id, $"{cmd}|{val}");
    }

    private static void AlterStatusModal(ClickEvent evt)
    {
        Modal.Reset("Add Status");

        Modal.AddDropdownField("Type", "Type", "ICON Preset", StringUtility.CreateArray("ICON Preset", "Custom"), (evt) => AddStatusModalEvaluateConditions());

        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> statuses = new();
        foreach (JSONNode s in gamedata["Icon1_5"]["StatusEffects"].AsArray)
        {
            statuses.Add(s["Name"]);
        }
        Modal.AddSearchField("PregenStatuses", "Status", "", statuses.ToArray());
        Modal.AddTextField("Name", "Name", "");
        Modal.AddDropdownField("Color", "Color", "Gray", StringUtility.CreateArray("Gray", "Green", "Red", "Blue", "Purple", "Yellow", "Orange"));
        Modal.AddDropdownField("Modifier", "Modifier Type", "None", StringUtility.CreateArray("None", "Number"));
        Modal.AddPreferredButton("Add", AddStatus);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        AddStatusModalEvaluateConditions();
    }

    private static void AddStatusModalEvaluateConditions()
    {
        bool pregenStatus = UI.Modal.Q<DropdownField>("Type").value == "ICON Preset";
        bool name = !pregenStatus;
        bool color = !pregenStatus;
        bool modifier = !pregenStatus;

        UI.ToggleDisplay(UI.Modal.Q("PregenStatuses"), pregenStatus);
        UI.ToggleDisplay(UI.Modal.Q("Name"), name);
        UI.ToggleDisplay(UI.Modal.Q("Color"), color);
        UI.ToggleDisplay(UI.Modal.Q("Modifier"), modifier);
    }

    private static void AddStatus(ClickEvent evt)
    {
        Icon1_5Condition condition = new();
        string type = UI.Modal.Q<DropdownField>("Type").value;
        if (type == "ICON Preset")
        {
            condition.Name = SearchField.GetValue(UI.Modal.Q("PregenStatuses"));
            JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
            foreach (JSONNode j in gamedata["Icon1_5"]["StatusEffects"])
            {
                if (j["Name"] == condition.Name)
                {
                    condition.Color = j["Color"];
                    condition.ModifierType = j["ModifierType"];
                }
            }
            condition.NumValue = 0;
        }
        else
        {
            // Get from parameters
            condition.Name = UI.Modal.Q<TextField>("Name").value;
            condition.Color = UI.Modal.Q<DropdownField>("Color").value;
            condition.ModifierType = UI.Modal.Q<DropdownField>("Modifier").value;
            condition.NumValue = 0;
        }

        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelected().Data.Id, $"GainStatus|{JsonUtility.ToJson(condition)}");
        Modal.Close();
    }

    public override void GameDataSetValue(string value)
    {
        FileLogger.Write($"Game system changed - {value}");
        if (value == "IncrementTurn")
        {
            RoundNumber++;
            PartyResolve++;
            UI.System.Q<Label>("TurnNumber").text = RoundNumber.ToString();
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("TokenData"))
            {
                TokenData data = g.GetComponent<TokenData>();
                TokenDataSetValue(data.Id, "StartTurn");
            }
        }
    }

    public override string[] GetEffectList()
    {
        return new string[] { "Difficult", "Pit", "Dangerous", "Impassable", "Interactive", "Demon Slayer/Flash Step - Afterimage", "Demon Slayer/Six Hells Trigram", "Demon Slayer/Heroic Six Hells Trigram", "Fool/Party Favor", "Freelancer/Showdown - Quench", "Freelancer/Warding Bolts", "Shade/Shadow Cloud (Blinded+ exc Caster)", "Harvester/Plant", "Harvester/Blood Grove", "Harvester/Mote of Life (Blessing))", "Harvester/Mote of Life (Regen)", "Spellblade/Lightning Spike 1", "Spellblade/Lightning Spike 2", "Spellblade/Lightning Spike 3", "Spellblade/Lightning Spike 4", "Spellblade/Lightning Spike 5", "Spellblade/Lightning Spike 6", "Stormbender/Selkie", "Stormbender/Salt Sprite", "Stormbender/Pit", "Stormbender/Tsunami", "Stormbender/Tsunami - Stormlash", "Stormbender/Dangerous", "Stormbender/Geyser I", "Stormbender/Gust", "Stormbender/Gust I", "Stormbender/Gust II", "Stormbender/Waterspout", "Stormbender/Waterspout - Hurricane", "Stormbender/Waterspout I", "Stormbender/Waterspout I - Hurricane", "Stormbender/Waterspout II", "Stormbender/Waterspout II - Hurricane" };
    }

    public override void AddTokenModal()
    {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> playerJobs = new();
        foreach (JSONNode pjob in gamedata["Icon1_5"]["PlayerJobs"].AsArray)
        {
            playerJobs.Add(pjob);
        }
        List<string> foeClasses = new();
        foreach (JSONNode fclass in gamedata["Icon1_5"]["FoeClasses"].AsArray)
        {
            foeClasses.Add(fclass);
        }

        base.AddTokenModal();
        Modal.AddDropdownField("Type", "Type", "Player", new string[] { "Player", "Foe", "Object" }, (evt) => AddTokenModalEvaluateConditions());
        Modal.AddSearchField("PlayerJob", "Job", "Stalwart/Bastion", playerJobs.ToArray());
        Modal.AddDropdownField("FoeClass", "Class", foeClasses[0], foeClasses.ToArray(), (evt) => AddTokenModalEvaluateConditions());
        Modal.AddTextField("FoeJob", "Job", "");
        Modal.AddToggleField("Elite", "Elite", false);
        Modal.AddDropdownField("LegendHP", "Legend HP Multiplier", "x4", new string[] { "x2", "x3", "x4", "x5", "x6", "x7", "x8" });
        Modal.AddDropdownField("Size", "Size", "1x1", new string[] { "1x1", "2x2", "3x3" });
        Modal.AddIntField("ObjectHP", "Object HP", 1);
        Modal.AddIntField("CloneCount", "Clone Count", 1);

        AddTokenModalEvaluateConditions();
    }

    private static void AddTokenModalEvaluateConditions()
    {
        bool playerJob = UI.Modal.Q<DropdownField>("Type").value == "Player";
        bool foeClass = UI.Modal.Q<DropdownField>("Type").value == "Foe";
        bool foeJob = UI.Modal.Q<DropdownField>("Type").value == "Foe";
        bool elite = foeClass && !StringUtility.CheckInList(UI.Modal.Q<DropdownField>("FoeClass").value, "Legend", "Mob");
        bool legendHP = foeClass && UI.Modal.Q<DropdownField>("FoeClass").value == "Legend";
        bool size = foeClass;
        bool objectHP = UI.Modal.Q<DropdownField>("Type").value == "Object";
        bool cloneCount = UI.Modal.Q<DropdownField>("Type").value == "Object" || UI.Modal.Q<DropdownField>("FoeClass").value == "Mob";

        UI.ToggleDisplay(UI.Modal.Q("PlayerJob"), playerJob);
        UI.ToggleDisplay(UI.Modal.Q("FoeClass"), foeClass);
        UI.ToggleDisplay(UI.Modal.Q("FoeJob"), foeJob);
        UI.ToggleDisplay(UI.Modal.Q("Elite"), elite);
        UI.ToggleDisplay(UI.Modal.Q("LegendHP"), legendHP);
        UI.ToggleDisplay(UI.Modal.Q("Size"), size);
        UI.ToggleDisplay(UI.Modal.Q("ObjectHP"), objectHP);
        UI.ToggleDisplay(UI.Modal.Q("CloneCount"), cloneCount);
    }

    public override void CreateToken()
    {
        string name = UI.Modal.Q<TextField>("NameField").value;
        var tokenMeta = TokenLibrary.GetSelectedMeta();

        string type = UI.Modal.Q<DropdownField>("Type").value;
        string playerJob = SearchField.GetValue(UI.Modal.Q("PlayerJob"));
        string foeClass = UI.Modal.Q<DropdownField>("FoeClass").value;
        string foeJob = UI.Modal.Q<TextField>("FoeJob").value;
        bool elite = UI.Modal.Q<Toggle>("Elite").value;
        int legendHP = int.Parse(UI.Modal.Q<DropdownField>("LegendHP").value[1..]);
        int objectHP = UI.Modal.Q<IntegerField>("ObjectHP").value;
        int size = int.Parse(UI.Modal.Q<DropdownField>("Size").value[..1]);
        int count = UI.Modal.Q<IntegerField>("CloneCount").value;

        int hpMultiplier = 1;
        Icon1_5Data data = new()
        {
            Type = type
        };

        if (type == "Player")
        {
            data.Class = playerJob.Split("/")[0];
            data.Job = playerJob.Split("/")[1];
            data.Elite = false;
        }
        else if (type == "Foe")
        {
            data.Class = foeClass;
            data.Job = foeJob;
            data.Elite = elite;
            if (elite)
            {
                hpMultiplier = 2;
            }
            else if (foeClass == "Legend")
            {
                hpMultiplier = legendHP;
            }
        }
        else
        {

            hpMultiplier = objectHP;
        }
        InitSystemData(data, hpMultiplier);

        Color color = GetColor(data.Class);

        if ((type == "Object" || foeClass == "Mob") && count > 1)
        {
            for (int i = 0; i < count; i++)
            {
                string cloneName = $"{name} {StringUtility.ConvertIntToAlpha(i + 1)}";
                Player.Self().CmdCreateToken("Icon v1.5", tokenMeta, cloneName, size, color, JsonUtility.ToJson(data));
            }
        }
        else
        {
            Player.Self().CmdCreateToken("Icon v1.5", tokenMeta, name, size, color, JsonUtility.ToJson(data));
        }
    }

    private static Color GetColor(string job)
    {
        switch (GetStatColor(job))
        {
            case "Red":
                return ColorUtility.NormalizeRGB(238, 34, 12);
            case "Blue":
                return ColorUtility.NormalizeRGB(0, 162, 255);
            case "Yellow":
                return ColorUtility.NormalizeRGB(254, 174, 0);
            case "Green":
                return ColorUtility.NormalizeRGB(97, 216, 54);
            case "Purple":
                return ColorUtility.NormalizeRGB(202, 85, 239);
            case "Gray":
                return ColorUtility.NormalizeRGB(146, 146, 146);
        }
        return Color.black;
    }

    private static string GetStatColor(string job)
    {
        string statColor = "Gray";
        switch (job)
        {
            case "Wright":
            case "Artillery":
                statColor = "Blue";
                break;
            case "Vagabond":
            case "Skirmisher":
                statColor = "Yellow";
                break;
            case "Stalwart":
            case "Heavy":
                statColor = "Red";
                break;
            case "Leader":
            case "Mendicant":
                statColor = "Green";
                break;
            case "Legend":
                statColor = "Purple";
                break;
        }
        return statColor;
    }

    public override MenuItem[] GetTokenMenuItems(TokenData data)
    {
        MenuItem[] baseItems = base.GetTokenMenuItems(data);

        List<MenuItem> items = new();
        items.Add(new MenuItem("AttackRoll", "Attack Roll", AttackRollClicked));
        items.Add(new MenuItem("SaveRoll", "Save Roll", SaveRollClicked));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override MenuItem[] GetTileMenuItems()
    {
        List<MenuItem> items = new();
        if (Block.GetSelected().Length > 0)
        {
            items.Add(new MenuItem("SelectSmallBlast", "To Small Blast", SelectSmallBlastClicked));
            items.Add(new MenuItem("SelectMedBlast", "To Medium Blast", SelectMedBlastClicked));
            items.Add(new MenuItem("SelectLargeBlast", "To Large Blast", SelectLargeBlastClicked));
        }
        return items.ToArray();
    }

    public static void SelectSmallBlastClicked(ClickEvent evt)
    {
        List<Block> blocksToAppend = new();
        foreach (var b in Block.GetSelected())
        {
            foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetCardinallyAdjacent(new Vector2Int(b.Coordinate.x, b.Coordinate.y))))
            {
                blocksToAppend.Add(block);
            }
        }
        foreach (var b in blocksToAppend)
        {
            b.SelectAppend(true);
        }
        SelectionMenu.Hide();
    }

    public static void SelectMedBlastClicked(ClickEvent evt)
    {
        List<Block> blocksToAppend = new();
        foreach (var b in Block.GetSelected())
        {
            foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetCardinallyAdjacent(new Vector2Int(b.Coordinate.x, b.Coordinate.y))))
            {
                blocksToAppend.Add(block);
            }
            foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetDiagonallyAdjacent(new Vector2Int(b.Coordinate.x, b.Coordinate.y))))
            {
                blocksToAppend.Add(block);
            }
        }
        foreach (var b in blocksToAppend)
        {
            b.SelectAppend(true);
        }
        SelectionMenu.Hide();
    }

    public static void SelectLargeBlastClicked(ClickEvent evt)
    {
        SelectSmallBlastClicked(evt);
        SelectSmallBlastClicked(evt);
    }


    private void AttackRollClicked(ClickEvent evt)
    {
        Modal.Reset("Attack Roll");
        Modal.AddNumberNudgerField("BoonField", "Boons", 0, 0);
        Modal.AddNumberNudgerField("CurseField", "Curses", 0, 0);
        Modal.AddPreferredButton("Roll", AttackRoll);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private void SaveRollClicked(ClickEvent evt)
    {
        Modal.Reset("Save Roll");
        Modal.AddNumberNudgerField("BoonField", "Boons", 0, 0);
        Modal.AddNumberNudgerField("CurseField", "Curses", 0, 0);
        Modal.AddPreferredButton("Roll", SaveRoll);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private void AttackRoll(ClickEvent evt)
    {
        BoonCurseRoll("Attack");
        Token.DeselectAll();
    }

    private void SaveRoll(ClickEvent evt)
    {
        BoonCurseRoll("Save");
        Token.DeselectAll();
    }

    private void BoonCurseRoll(string label)
    {
        string name = Token.GetSelected().Data.Name;
        int boon = UI.Modal.Q<NumberNudger>("BoonField").value;
        int curse = UI.Modal.Q<NumberNudger>("CurseField").value;
        int balance = boon - curse;
        int x = 1 + Random.Range(0, 20);
        string rollString = $"{x}";
        int mod = 0;
        List<int> mods = new();
        for (int i = 0; i < math.abs(balance); i++)
        {
            int y = 1 + Random.Range(0, 6);
            mods.Add(y);
            mod = math.max(mod, y);
        }
        if (mod > 0 && balance > 0)
        {
            x += mod;
            rollString += $"+boon({String.Join(",", mods.ToArray())})";
        }
        if (mod > 0 && balance < 0)
        {
            x -= mod;
            rollString += $"-curse({String.Join(",", mods.ToArray())})";
        }
        Player.Self().CmdShareDiceRoll($"{name}'s {label} ({Player.Self().Name})", $"{x}", rollString, 20);
        Modal.Close();
    }

    public override void UpdateData(TokenData data)
    {
        base.UpdateData(data);
        Icon1_5Data mdata = JsonUtility.FromJson<Icon1_5Data>(data.SystemData);

        data.OverheadElement.Q<ProgressBar>("VigorBar").value = mdata.Vigor;
        data.OverheadElement.Q<ProgressBar>("VigorBar").highValue = mdata.MaxHP;
        UI.ToggleDisplay(data.OverheadElement.Q("VigorBar"), mdata.Vigor > 0);

        data.OverheadElement.Q<ProgressBar>("HpBar").value = mdata.CurrentHP;
        data.OverheadElement.Q<ProgressBar>("HpBar").highValue = mdata.MaxHP;

        UI.ToggleDisplay(data.OverheadElement.Q("Wound1"), mdata.Wounds >= 1);
        UI.ToggleDisplay(data.OverheadElement.Q("Wound2"), mdata.Wounds >= 2);
        UI.ToggleDisplay(data.OverheadElement.Q("Wound3"), mdata.Wounds >= 3);

        UI.ToggleDisplay(data.OverheadElement.Q("HpBar"), mdata.CurrentHP > 0);
    }

    private static void InitSystemData(Icon1_5Data data, int hpMultiplier)
    {

        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        JSONNode stats = gamedata["Icon1_5"]["Stats"][GetStatColor(data.Class)];

        data.MaxHP = stats["MaxHP"] * hpMultiplier;
        if (data.Type == "Object")
        {
            data.MaxHP = hpMultiplier;
        }
        data.CurrentHP = data.MaxHP;
        data.Vigor = 0;
        data.Wounds = 0;

        data.Damage = stats["Damage"];
        data.Fray = stats["Fray"];
        data.Range = stats["Range"];
        data.Speed = stats["Speed"];
        data.Dash = stats["Dash"];
        data.Defense = stats["Defense"];
    }

    public override void TokenDataSetValue(string tokenId, string value)
    {
        base.TokenDataSetValue(tokenId, value);
        TokenData data = TokenData.Find(tokenId);
        if (data.Destroyed)
        {
            return;
        }
        Icon1_5Data sysdata = JsonUtility.FromJson<Icon1_5Data>(data.SystemData);
        sysdata.Change(value, data.WorldObject.GetComponent<Token>(), data.Placed);
        data.SystemData = JsonUtility.ToJson(sysdata);
        data.NeedsRedraw = true;
    }

    public override void UpdateTokenPanel(string tokenId, string elementName)
    {
        TokenData data = TokenData.Find(tokenId);
        UI.ToggleActiveClass(elementName, data != null);
        if (!data)
        {
            return;
        }

        data.UpdateTokenPanel(elementName);
        Icon1_5Data sysdata = JsonUtility.FromJson<Icon1_5Data>(data.SystemData);

        VisualElement panel = UI.System.Q(elementName);

        UI.ToggleDisplay(panel.Q("Data"), data.WorldObject.GetComponent<Token>().State == TokenState.Inspecting);

        panel.Q("ClassBackground").style.borderTopColor = data.Color;
        panel.Q("ClassBackground").style.borderRightColor = data.Color;
        panel.Q("ClassBackground").style.borderBottomColor = data.Color;
        panel.Q("ClassBackground").style.borderLeftColor = data.Color;

        panel.Q<Label>("Class").text = sysdata.Class;
        panel.Q<Label>("Class").style.backgroundColor = data.Color;
        panel.Q<Label>("Job").text = sysdata.Job;
        panel.Q<Label>("Job").style.backgroundColor = data.Color;
        UI.ToggleDisplay(panel.Q("Job"), sysdata.Job.Length > 0);
        UI.ToggleDisplay(panel.Q("ExtraInfo"), sysdata.Type != "Object");


        panel.Q("Elite").style.backgroundColor = ColorUtility.NormalizeRGB(202, 85, 239);
        UI.ToggleDisplay(panel.Q("Elite"), sysdata.Elite);

        panel.Q("IconHPBar").Q<Label>("CHP").text = $"{sysdata.CurrentHP}";
        panel.Q("IconHPBar").Q<Label>("MHP").text = $"/{sysdata.MaxHP}";
        panel.Q("IconHPBar").Q<ProgressBar>("HpBar").value = sysdata.CurrentHP;
        panel.Q("IconHPBar").Q<ProgressBar>("HpBar").highValue = sysdata.MaxHP;

        panel.Q("IconHPBar").Q<Label>("VIG").text = $"+{sysdata.Vigor}";
        panel.Q("IconHPBar").Q<ProgressBar>("VigorBar").value = sysdata.Vigor;
        panel.Q("IconHPBar").Q<ProgressBar>("VigorBar").highValue = sysdata.MaxHP;
        UI.ToggleDisplay(panel.Q("VigorBar"), sysdata.Vigor > 0);
        UI.ToggleDisplay(panel.Q("VIG"), sysdata.Vigor > 0);

        UI.ToggleDisplay(panel.Q("Wound1"), sysdata.Wounds >= 1);
        UI.ToggleDisplay(panel.Q("Wound2"), sysdata.Wounds >= 2);
        UI.ToggleDisplay(panel.Q("Wound3"), sysdata.Wounds >= 3);

        panel.Q("IconResolveBar").Q<Label>("ResolveNum").text = $"{sysdata.Resolve}";
        panel.Q("IconResolveBar").Q<ProgressBar>("ResolveBar").value = sysdata.Resolve + Icon_v1_5.PartyResolve;
        panel.Q("IconResolveBar").Q<ProgressBar>("ResolveBar").highValue = 6;

        panel.Q("IconResolveBar").Q<Label>("PartyResolveNum").text = $"+{Icon_v1_5.PartyResolve}";
        panel.Q("IconResolveBar").Q<ProgressBar>("PartyResolveBar").value = Icon_v1_5.PartyResolve;
        panel.Q("IconResolveBar").Q<ProgressBar>("PartyResolveBar").highValue = 6;
        UI.ToggleDisplay(panel.Q("IconResolveBar"), sysdata.Type == "Player");
        UI.ToggleDisplay(panel.Q("PartyResolveBar"), Icon_v1_5.PartyResolve > 0);
        UI.ToggleDisplay(panel.Q("PartyResolveNum"), Icon_v1_5.PartyResolve > 0);

        panel.Q("Damage").Q<Label>("Value").text = $"{sysdata.Defense}/{sysdata.Fray}";
        panel.Q("Range").Q<Label>("Value").text = $"{sysdata.Range}";
        panel.Q("Speed").Q<Label>("Value").text = $"{sysdata.Speed}/{sysdata.Dash}";
        panel.Q("Defense").Q<Label>("Value").text = $"{sysdata.Defense}";
        UI.ToggleDisplay(panel.Q("Stats"), sysdata.Type != "Object");

        if (data.NeedsRedraw)
        {
            data.NeedsRedraw = false;
            panel.Q("Conditions").Q("List").Clear();
            foreach (Icon1_5Condition condition in sysdata.Status)
            {
                VisualElement template = UI.CreateFromTemplate("UITemplates/GameSystem/ConditionTemplate");
                string label = $"{condition.Name}";
                if (condition.ModifierType == "Number")
                {
                    label = $"{label} ({condition.NumValue})";
                }
                template.Q<Label>("Name").text = label;
                if (elementName == "SelectedTokenPanel")
                {
                    template.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) =>
                    {
                        Player.Self().CmdRequestTokenDataSetValue(tokenId, $"IncrementStatus|{condition.Name}");
                    });
                    template.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) =>
                    {
                        Player.Self().CmdRequestTokenDataSetValue(tokenId, $"DecrementStatus|{condition.Name}");
                    });
                    template.Q<Button>("Remove").RegisterCallback<ClickEvent>((evt) =>
                    {
                        Player.Self().CmdRequestTokenDataSetValue(tokenId, $"LoseStatus|{condition.Name}");
                    });
                    UI.ToggleDisplay(template.Q("Increment"), condition.ModifierType == "Number");
                    UI.ToggleDisplay(template.Q("Decrement"), condition.ModifierType == "Number");
                    UI.ToggleDisplay(template.Q("Remove"), !condition.Locked);
                }
                else
                {
                    UI.ToggleDisplay(template.Q("Increment"), false);
                    UI.ToggleDisplay(template.Q("Decrement"), false);
                    UI.ToggleDisplay(template.Q("Remove"), false);
                }
                template.Q("Wrapper").style.backgroundColor = condition.GetColorFromName();
                panel.Q("Conditions").Q("List").Add(template);
            }
        }
    }
}

[Serializable]
public class Icon1_5Condition
{
    public string Name;
    public string ModifierType;
    public string Color;
    public int NumValue;
    public bool Locked;

    public Color GetColorFromName()
    {
        switch (Color)
        {
            case "Green":
                return ColorUtility.GetColor("339A2A");
            case "Red":
                return ColorUtility.GetColor("A23431");
            case "Blue":
                return ColorUtility.GetColor("4C4CD7");
            case "Purple":
                return ColorUtility.GetColor("9E198D");
            case "Yellow":
                return ColorUtility.GetColor("9A9939");
            case "Orange":
                return ColorUtility.GetColor("BC840B");
            default:
                return ColorUtility.GetColor("727272");
        }
    }
}

[Serializable]
public class Icon1_5Data
{
    public int CurrentHP;
    public int MaxHP;
    public int Vigor;
    public int Resolve;
    public string Type;
    public string Job;
    public string Class;
    public bool Elite;
    public int HPMultiplier;
    public int Wounds;
    public int Damage;
    public int Fray;
    public int Range;
    public int Speed;
    public int Dash;
    public int Defense;
    public Icon1_5Condition[] Status;

    public void Change(string value, Token token, bool placed)
    {
        if (value.StartsWith("GainWound"))
        {
            Wounds++;
            Wounds = Math.Min(Wounds, 3);
            int woundMaxHP = MaxHP / 4 * (4 - Wounds);
            CurrentHP = Math.Min(CurrentHP, woundMaxHP);
        }
        if (value.StartsWith("LoseWound"))
        {
            Wounds--;
            Wounds = Math.Max(Wounds, 0);
        }
        if (value.StartsWith("GainHP"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            int woundMaxHP = MaxHP / 4 * (4 - Wounds);
            if (CurrentHP + diff > woundMaxHP)
            {
                diff = woundMaxHP - CurrentHP;
            }
            if (diff > 0)
            {
                CurrentHP += diff;
                if (placed)
                {
                    PopoverText.Create(token, $"/+{diff}|_HP", Color.white);
                }
                token.SetDefeated(CurrentHP <= 0);
            }
        }
        if (value.StartsWith("LoseHP"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP - diff < 0)
            {
                diff = CurrentHP;
            }
            if (diff > 0)
            {
                CurrentHP -= diff;
                if (placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                }
                token.SetDefeated(CurrentHP <= 0);
            }
        }
        if (value.StartsWith("GainVIG"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            if (Vigor + diff > MaxHP / 4)
            {
                diff = MaxHP / 4 - Vigor;
            }
            if (diff > 0)
            {
                Vigor += diff;
                if (placed)
                {
                    PopoverText.Create(token, $"/+{diff}|_VIG", Color.white);
                }
            }
        }
        if (value.StartsWith("LoseVIG"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            if (Vigor - diff < 0)
            {
                diff = Vigor;
            }
            if (diff > 0)
            {
                Vigor -= diff;
                if (placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_VIG", Color.white);
                }
            }
        }
        if (value.StartsWith("GainRES"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            // if (diff + Resolve > 6) {
            //     diff = 6 - Resolve;
            // }
            if (diff > 0)
            {
                Resolve += diff;
                if (placed)
                {
                    PopoverText.Create(token, $"/+{diff}|_RES", Color.white);
                }
            }
        }
        if (value.StartsWith("GainPRES"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            Icon_v1_5.PartyResolve += diff;
        }
        if (value.StartsWith("LoseRES"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            Resolve = Math.Max(0, Resolve - diff);
        }
        if (value.StartsWith("LosePRES"))
        {
            int diff = int.Parse(value.Split("|")[1]);
            if (diff > Icon_v1_5.PartyResolve)
            {
                diff = Icon_v1_5.PartyResolve;
            }
            Icon_v1_5.PartyResolve -= diff;
        }
        if (value.StartsWith("Damage"))
        {
            int diff = int.Parse(value.Split("|")[1]);
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
                if (placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_VIG", Color.white);
                }
            }
            else if (diff > Vigor && Vigor > 0)
            {
                // Vig zeroed and HP damage
                CurrentHP -= (diff - Vigor);
                Vigor = 0;
                if (placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP/VIG", Color.white);
                }
            }
            else if (Vigor <= 0)
            {
                // HP damage only
                CurrentHP -= diff;
                if (placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                }
            }
        }
        if (value.StartsWith("LoseStatus"))
        {
            string[] parts = value.Split("|");
            RemoveCondition(parts[1]);
            if (placed)
            {
                PopoverText.Create(token, $"/-|_{parts[1].ToUpper()}", Color.white);
            }
        }
        if (value.StartsWith("GainStatus"))
        {
            string[] parts = value.Split("|");
            Icon1_5Condition condition = JsonUtility.FromJson<Icon1_5Condition>(parts[1]);
            AddCondition(condition);
            if (placed)
            {
                PopoverText.Create(token, $"/+|_{condition.Name.ToUpper()}", Color.white);
            }
        }
        if (value.StartsWith("IncrementStatus"))
        {
            string[] parts = value.Split("|");
            CounterCondition(parts[1], 1);
            if (placed)
            {
                PopoverText.Create(token, $"/+1|_{parts[1].ToUpper()}", Color.white);
            }
        }
        if (value.StartsWith("DecrementStatus"))
        {
            string[] parts = value.Split("|");
            CounterCondition(parts[1], -1);
            if (placed)
            {
                PopoverText.Create(token, $"/-1|_{parts[1].ToUpper()}", Color.white);
            }
        }
        OnChange(token);
    }

    private void OnChange(Token token)
    {
        token.SetDefeated(CurrentHP <= 0);
        if (CurrentHP <= 0)
        {
            RemoveCondition("Bloodied");
            AddCondition(new Icon1_5Condition()
            {
                Name = "Defeated",
                ModifierType = "None",
                Color = "Red",
                Locked = true
            });
        }
        else if (CurrentHP <= MaxHP / 2)
        {
            RemoveCondition("Defeated");
            AddCondition(new Icon1_5Condition()
            {
                Name = "Bloodied",
                ModifierType = "None",
                Color = "Red",
                Locked = true
            });
        }
        else
        {
            RemoveCondition("Bloodied");
            RemoveCondition("Defeated");
        }
    }

    private void AddCondition(Icon1_5Condition c)
    {
        RemoveCondition(c.Name);
        List<Icon1_5Condition> statusList = Status.ToList();
        statusList.Add(c);
        Status = statusList.ToArray();
    }

    private void RemoveCondition(string name)
    {
        List<Icon1_5Condition> statusList = Status.ToList();
        List<Icon1_5Condition> newStatusList = new();
        foreach (Icon1_5Condition condition in statusList)
        {
            if (name != condition.Name)
            {
                newStatusList.Add(condition);
            }
        }
        Status = newStatusList.ToArray();
    }

    private void CounterCondition(string name, int num)
    {
        List<Icon1_5Condition> statusList = Status.ToList();
        foreach (Icon1_5Condition condition in statusList)
        {
            if (name == condition.Name)
            {
                condition.NumValue += num;
            }
        }
        Status = statusList.ToArray();
    }
}

