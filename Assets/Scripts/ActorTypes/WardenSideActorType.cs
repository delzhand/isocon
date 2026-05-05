using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class WardenSideActorType : WardenBase
{
    private readonly static string TypeName = "WARDEN Side Character";

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
        return JsonUtility.FromJson<WardenSideActorType>(json);
    }
    #endregion

    #region Stats
    public string Name;
    public int MaxHP;
    public int CurrentHP;
    public int MaxSP;
    public int CurrentSP;
    public int TempHP;
    public int Reaction;
    public int MaxReaction;
    public int AP;
    public int MaxAP;
    public int Armor;
    public int Speed;
    public int Major;
    public int Minor;
    public int Level;
    public string Features;
    public string WeaponName;
    public int WeaponDice;
    public string WeaponDieType;
    public string WeaponModifier;
    #endregion

    #region Creation
    public static void AddActorModal()
    {
        var contents = Modal2.Contents("PrimaryDialog");
        var typeContainer = contents.Q("ActorTypeContainer");
        typeContainer.Clear();
        contents.Q("CreateActor")?.RemoveFromHierarchy();


        var name = Modal2.AddInlineTextField("Name", "Name", "");
        Modal2.MoveToContainer(name, typeContainer);

        var shape = Modal2.AddInlineComboboxField("ShapeField", "Shape", "Square 1/1", ActorType.ShapeOptions().ToList<string>());
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

        string name = Modal2.GetTextFieldValue("Name");
        string shape = Modal2.GetComboboxFieldValue("ShapeField");
        string color = Modal2.GetComboboxFieldValue("ColorField");
        WardenSideActorType t = new()
        {
            Type = "WARDEN Player",
            Name = name,
            MaxHP = 10,
            CurrentHP = 10,
            MaxSP = 10,
            CurrentSP = 10,
            MaxAP = 2,
            AP = 2,
            MaxReaction = 1,
            Reaction = 1,
            Speed = 5,
            Major = 2,
            Minor = 0

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
        return $"{Name}";
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/WARDEN";
    }

    public override List<MenuItem> GetMenuItems(bool placed)
    {
        var baseItems = base.GetMenuItems(placed);

        var changeValues = FindParent("Change Values", baseItems);
        changeValues.Children.Add(new MenuItem("Change Level", () => { NumberPicker.ActorCommand("ChangeLevel", false); }));
        changeValues.Children.Add(new MenuItem("Alter Core Stats", AlterStatModal));
        changeValues.Children.Add(new MenuItem("Damage HP", () => { NumberPicker.ActorCommand("Damage", false); }));
        changeValues.Children.Add(new MenuItem("Modify HP", () => { NumberPicker.ActorCommand("ModHP"); }));
        changeValues.Children.Add(new MenuItem("Modify SP", () => { NumberPicker.ActorCommand("ModSP"); }));
        changeValues.Children.Add(new MenuItem("Modify TempHP", () => { NumberPicker.ActorCommand("ModShield"); }));
        changeValues.Children.Add(new MenuItem("Change Equipment", ChangeEquipment));
        changeValues.Children.Add(new MenuItem("Refresh Turn", RefreshTurn));
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
        else if (command.StartsWith("ChangeLevel"))
        {
            int changeValue = int.Parse(command.Split("|")[1]);
            Level = changeValue;
            if (Level == 0) { Major = 2; Minor = 0; WeaponDice = 2; }
            else if (Level == 1) { Major = 3; Minor = 0; WeaponDice = 2; }
            else if (Level == 2) { Major = 4; Minor = 1; WeaponDice = 2; }
            else if (Level == 3) { Major = 6; Minor = 4; WeaponDice = 3; }
            else if (Level == 4) { Major = 7; Minor = 5; WeaponDice = 3; }
            else if (Level == 5) { Major = 9; Minor = 6; WeaponDice = 4; }
            else if (Level == 6) { Major = 10; Minor = 7; WeaponDice = 4; }
            else if (Level == 7) { Major = 11; Minor = 8; WeaponDice = 4; }
            else if (Level == 8) { Major = 13; Minor = 10; WeaponDice = 5; }
            else if (Level == 9) { Major = 14; Minor = 11; WeaponDice = 5; }
            else if (Level == 10) { Major = 15; Minor = 12; WeaponDice = 5; }
            else if (Level == 11) { Major = 16; Minor = 13; WeaponDice = 5; }
            else if (Level == 12) { Major = 17; Minor = 14; WeaponDice = 5; }
            string serialized = Serialize();

            Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"UpdateStats|{serialized}");
            this.InitPanel(Actor.GetSelected().Data, "LeftTokenPanel", true);
        }
        else if (command.StartsWith("ModSP"))
        {
            int original = CurrentSP;
            int changeValue = int.Parse(command.Split("|")[1]);
            CurrentSP = Clamped(0, CurrentSP + changeValue, MaxHP);
            int diff = CurrentSP - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(tokenData.GetActor(), $"/{plus}{diff}|_SP", Color.white);
            }
        }
        else if (command.StartsWith("ModShield"))
        {
            int original = TempHP;
            int changeValue = int.Parse(command.Split("|")[1]);
            TempHP = Clamped(0, TempHP + changeValue, 1000);
            int diff = TempHP - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(tokenData.GetActor(), $"/{plus}{diff}|_TempHP", Color.white);
            }
        }
        else if (command.StartsWith("Damage"))
        {
            int diff = int.Parse(command.Split("|")[1]);
            if (TempHP + CurrentHP - diff < 0)
            {
                diff = TempHP + CurrentHP;
            }
            if (diff <= 0)
            {
                return;
            }
            if (diff < TempHP)
            {
                // Vig damage only
                TempHP -= diff;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_TEMPHP", Color.white);
                }
            }
            else if (diff > TempHP && TempHP > 0)
            {
                // Vig zeroed and HP damage
                CurrentHP -= (diff - TempHP);
                TempHP = 0;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP/TEMPHP", Color.white);
                }
            }
            else if (TempHP <= 0)
            {
                // HP damage only
                CurrentHP -= diff;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                }
            }
        }
        else if (command.StartsWith("ModAP"))
        {
            int original = AP;
            int changeValue = int.Parse(command.Split("|")[1]);
            AP = Clamped(0, AP + changeValue, MaxAP);
            int diff = AP - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_AP", Color.white);
            }
        }
        else if (command.StartsWith("ModReaction"))
        {
            int original = Reaction;
            int changeValue = int.Parse(command.Split("|")[1]);
            Reaction = Clamped(0, Reaction + changeValue, MaxReaction);
            int diff = Reaction - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_Reaction", Color.white);
            }
        }
        else if (command.StartsWith("UpdateStats"))
        {
            string json = command.Split("|")[1];
            WardenSideActorType lmu = JsonUtility.FromJson<WardenSideActorType>(json);
            MaxHP = lmu.MaxHP;
            MaxSP = lmu.MaxSP;
            MaxReaction = lmu.MaxReaction;
            MaxAP = lmu.MaxAP;
            Major = lmu.Major;
            Minor = lmu.Minor;
            Level = lmu.Level;
            Features = lmu.Features;
            Armor = lmu.Armor;
            Speed = lmu.Speed;
            WeaponName = lmu.WeaponName;
            WeaponDice = lmu.WeaponDice;
            WeaponDieType = lmu.WeaponDieType;
            WeaponModifier = lmu.WeaponModifier;
            PopoverText.Create(token, $"_STAT|_CHANGE", Color.white);
        }
        else if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
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
        mainHPBar.Q<Label>("VIG").text = $"+{TempHP}";
        mainHPBar.Q<ProgressBar>("VigorBar").value = TempHP;
        mainHPBar.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(mainHPBar.Q("VigorBar"), TempHP > 0);
        UI.ToggleDisplay(mainHPBar.Q("VIG"), TempHP > 0);
        UI.ToggleDisplay(mainHPBar.Q("Wound1"), false);
        UI.ToggleDisplay(mainHPBar.Q("Wound2"), false);
        UI.ToggleDisplay(mainHPBar.Q("Wound3"), false);


        VisualElement RESBar = panel.Q("Bars").Q("RESBar");
        RESBar.Q<Label>("StatLabel").text = "SP";
        RESBar.Q<Label>("CHP").text = $"{CurrentSP}";
        RESBar.Q<Label>("MHP").text = $"/{MaxSP}";
        RESBar.Q<ProgressBar>("HpBar").value = CurrentSP;
        RESBar.Q<ProgressBar>("HpBar").highValue = MaxSP;

        panel.Q("AP").Q<Label>("Pips").text = SymbolString("◆", AP, MaxAP);
        panel.Q("Reaction").Q<Label>("Pips").text = SymbolString("●", Reaction, MaxReaction);
    }

    public override void UpdateOverhead(ActorData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;
        o.Q<Label>("AP").text = SymbolString("◆", AP, MaxAP);
        o.Q<Label>("Reaction").text = SymbolString("●", Reaction, MaxReaction);
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

        VisualElement resBar = UI.CreateFromTemplate("UI/TableTop/SimpleHPBar");
        resBar.name = "RESBar";
        resBar.style.marginRight = mr;
        resBar.style.marginLeft = ml;
        resBar.Q<ProgressBar>("HpBar").style.minWidth = 100;
        resBar.Query(null, "unity-progress-bar__progress").First().style.backgroundColor = Color.cyan;
        resBar.Query(null, "unity-progress-bar__background").First().style.backgroundColor = ColorUtility.DarkenColor(Color.cyan, .5f);
        container.Add(resBar);

        ActorData data = selected ? Actor.GetSelected().Data : null;

        VisualElement ap = UI.CreateFromTemplate("UI/TableTop/LancerCoreStat");
        ap.style.marginRight = mr;
        ap.style.marginLeft = ml;
        ap.name = "AP";
        ap.Q<Label>("StatName").text = "ACTION POINTS";
        ap.Q<Label>("Pips").style.color = ColorUtility.GetColor("#FF0093");
        ap.Q<Label>("Pips").text = SymbolString("◆", AP, MaxAP);
        if (data != null)
        {
            ap.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModAP|1"); });
            ap.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModAP|-1"); });
        }
        else
        {
            UI.ToggleDisplay(ap.Q<Button>("Increment"), false);
            UI.ToggleDisplay(ap.Q<Button>("Decrement"), false);
        }
        container.Add(ap);

        VisualElement reaction = UI.CreateFromTemplate("UI/TableTop/LancerCoreStat");
        reaction.style.marginRight = mr;
        reaction.style.marginLeft = ml;
        reaction.name = "Reaction";
        reaction.Q<Label>("StatName").text = "REACTION";
        reaction.Q<Label>("Pips").style.color = ColorUtility.GetColor("#FF7300");
        reaction.Q<Label>("Pips").text = SymbolString("●", Reaction, MaxReaction);
        if (data != null)
        {
            reaction.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModReaction|1"); });
            reaction.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => { Player.Self().CmdRequestActorCommand(data.Id, "ModReaction|-1"); });
        }
        else
        {
            UI.ToggleDisplay(reaction.Q<Button>("Increment"), false);
            UI.ToggleDisplay(reaction.Q<Button>("Decrement"), false);
        }
        container.Add(reaction);

        /*VisualElement VSpeed = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        VSpeed.Q<Label>("Label").text = "Speed";
        VSpeed.Q<Label>("Label").style.minWidth = 80;
        VSpeed.Q<Label>("Value").text = $"{Speed}";
        panel.Q("Bars").Add(VSpeed);
      */
        panel.Q("Bars").Add(container);

        List<string> stats = new();
        stats.Add($"MAJOR|+{Major}");
        stats.Add($"MINOR|+{Minor}");
        stats.Add($"SPEED|{Speed}");
        stats.Add($"ARMOR|{Armor}");
        stats.Add($"WEAPON|{WeaponName}");
        stats.Add($"DAMAGE|{WeaponDice}{WeaponDieType}{WeaponModifier}");
        stats.Add($"LEVEL|{Level}");
        stats.Add($"FEATURES|{Features}");
        foreach (string s in stats)
        {
            VisualElement sTemplate = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
            sTemplate.Q<Label>("Label").text = s.Split("|")[0];
            sTemplate.Q<Label>("Label").style.minWidth = 80;
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

    private void RefreshTurn()
    {
        SelectionMenu.Hide();
    /*    foreach (ActorTag tag in Tags)
        {
            CounterTag(tag.Name, -1);
            Console.WriteLine("Tag accessed");
        }
      */Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"ModReaction|1");
        Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"ModAP|4");
    }
    
    private void ChangeEquipment()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Change Equipment");

        string[] dicetypes = { "d4", "d6", "d8", "d10", "d12" };

        Modal2.AddInlineTextField("WeaponName", "Weapon Name", $"{WeaponName}");
        Modal2.AddInlineNumberNudgerField("WeaponDice", "Weapon Dice", WeaponDice, 0, 10);
        Modal2.AddInlineComboboxField("DieShapes", "Dice Type", $"{WeaponDieType}", dicetypes.ToList<string>());
        Modal2.AddInlineTextField("WeaponModifier", "Damage Modifier", $"{WeaponModifier}");
        Modal2.AddInlineNumberNudgerField("Armor", "Armor Value", Armor, 0, 10);

        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Save", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            WeaponName = Modal2.GetTextFieldValue("WeaponName");
            WeaponDice = Modal2.GetNumberNudgerFieldValue("WeaponDice");
            WeaponDieType = Modal2.GetComboboxFieldValue("DieShapes");
            WeaponModifier = Modal2.GetTextFieldValue("WeaponModifier");
            Armor = Modal2.GetNumberNudgerFieldValue("Armor");


            string serialized = Serialize();

            Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"UpdateStats|{serialized}");
            this.InitPanel(Actor.GetSelected().Data, "LeftTokenPanel", true);
        });
        Modal2.Open("Change Equipment");
    }

    private void AlterStatModal()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Alter Core Stats");

        Modal2.AddInlineNumberNudgerField("MaxHP", "Max HP", MaxHP, 0, 50);
        Modal2.AddInlineNumberNudgerField("MaxSP", "Max SP", MaxSP, 0, 50);
        Modal2.AddInlineNumberNudgerField("MaxAP", "Max AP", MaxAP, 1, 4);
        Modal2.AddInlineNumberNudgerField("Major", "Major", Major, 0, 20);
        Modal2.AddInlineNumberNudgerField("Minor", "Minor", Minor, 0, 20);
        Modal2.AddInlineTextField("Features", "Tagged Features", $"{Features}");
        Modal2.AddInlineNumberNudgerField("Speed", "Speed", Speed, 0, 100);

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
            MaxSP = Modal2.GetNumberNudgerFieldValue("MaxSP");
            MaxAP = Modal2.GetNumberNudgerFieldValue("MaxAP");
            Major = Modal2.GetNumberNudgerFieldValue("Major");
            Minor = Modal2.GetNumberNudgerFieldValue("Minor");
            Features = Modal2.GetTextFieldValue("Features");
            Speed = Modal2.GetNumberNudgerFieldValue("Speed");
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
        Modal2.AddInlineTextField("Name", "Name", Name);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Confirm", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            string newName = Modal2.GetTextFieldValue("Name").Trim();
            Player.Self().CmdRequestActorCommand(data.Id, $"Rename|{newName}");
        });
    }
}