using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Text;
using IsoconUILibrary;
using ShunUI;
using SimpleFileBrowser;
using Unity.VisualScripting;

[Serializable]
public class MaleghastActorType : ActorType
{
    private readonly static string TypeName = "Maleghast Unit";
    private static Dictionary<string, Texture2D> MarkerTextures;
    private static VisualElement MaleghastLeftPanel;
    private static VisualElement MaleghastRightPanel;

    #region Registration
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        ActorTypeRegistry.RegisterSystem($"{TypeName}");
        ActorTypeRegistry.RegisterInterfaceCallback($"{TypeName}", DeserializeAsInterface);
        ActorTypeRegistry.RegisterSimpleCallback($"{TypeName}|AddActorModal", AddActorModal);

        MarkerTextures = new();
        string dir = "Textures/GameSystem/Maleghast";
        MarkerTextures.Add("Activated", Resources.Load<Texture2D>($"{dir}/MarkerActivated"));
        MarkerTextures.Add("Berserk", Resources.Load<Texture2D>($"{dir}/MarkerBerserk"));
        MarkerTextures.Add("Buff", Resources.Load<Texture2D>($"{dir}/MarkerBuff"));
        MarkerTextures.Add("Burst", Resources.Load<Texture2D>($"{dir}/MarkerBurst"));
        MarkerTextures.Add("Deathburst", Resources.Load<Texture2D>($"{dir}/MarkerDeathburst"));
        MarkerTextures.Add("Debt", Resources.Load<Texture2D>($"{dir}/MarkerDebt"));
        MarkerTextures.Add("Debuff", Resources.Load<Texture2D>($"{dir}/MarkerDebuff"));
        MarkerTextures.Add("Doom", Resources.Load<Texture2D>($"{dir}/MarkerDoom"));
        MarkerTextures.Add("Evolve 1", Resources.Load<Texture2D>($"{dir}/MarkerEvolve1"));
        MarkerTextures.Add("Evolve 2", Resources.Load<Texture2D>($"{dir}/MarkerEvolve2"));
        MarkerTextures.Add("Guilt", Resources.Load<Texture2D>($"{dir}/MarkerGuilt"));
        MarkerTextures.Add("Luck", Resources.Load<Texture2D>($"{dir}/MarkerLuck"));
        MarkerTextures.Add("Lunacy", Resources.Load<Texture2D>($"{dir}/MarkerLunacy"));
        MarkerTextures.Add("Madness", Resources.Load<Texture2D>($"{dir}/MarkerMadness"));
        MarkerTextures.Add("Mutation", Resources.Load<Texture2D>($"{dir}/MarkerMutation"));
        MarkerTextures.Add("Petrify", Resources.Load<Texture2D>($"{dir}/MarkerPetrify"));
        MarkerTextures.Add("Plague", Resources.Load<Texture2D>($"{dir}/MarkerPlague"));
        MarkerTextures.Add("Provoke", Resources.Load<Texture2D>($"{dir}/MarkerProvoke"));
        MarkerTextures.Add("Slow", Resources.Load<Texture2D>($"{dir}/MarkerSlow"));
        MarkerTextures.Add("Speed", Resources.Load<Texture2D>($"{dir}/MarkerSpeed"));
        MarkerTextures.Add("Strength", Resources.Load<Texture2D>($"{dir}/MarkerStrength"));
        MarkerTextures.Add("Corpse", Resources.Load<Texture2D>($"{dir}/MarkerCorpse"));
        MarkerTextures.Add("Curseproof", Resources.Load<Texture2D>($"{dir}/MarkerCurseproof"));
        MarkerTextures.Add("Flight", Resources.Load<Texture2D>($"{dir}/MarkerFlight"));
        MarkerTextures.Add("Grapple", Resources.Load<Texture2D>($"{dir}/MarkerGrapple"));
        MarkerTextures.Add("Lurk", Resources.Load<Texture2D>($"{dir}/MarkerLurk"));
        MarkerTextures.Add("Magic Armor", Resources.Load<Texture2D>($"{dir}/MarkerArmorMAG"));
        MarkerTextures.Add("Miracle", Resources.Load<Texture2D>($"{dir}/MarkerMiracle"));
        MarkerTextures.Add("Physical Armor", Resources.Load<Texture2D>($"{dir}/MarkerArmorPHYS"));
        MarkerTextures.Add("Retaliation", Resources.Load<Texture2D>($"{dir}/MarkerRetaliation"));
        MarkerTextures.Add("Reload", Resources.Load<Texture2D>($"{dir}/MarkerReload"));
        MarkerTextures.Add("Super Armor", Resources.Load<Texture2D>($"{dir}/MarkerArmorSUPER2"));


    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
    public static IActorType DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<MaleghastActorType>(json);
    }
    #endregion


    #region Stats
    public string Job;
    public string House;
    public string PType;
    public int CurrentHP;
    public int MaxHP;
    public int Move;
    public int Defense;
    public string[] Upgrades;
    public string[] Traits;
    public string[] ActAbilities;
    public string[] SoulAbilities;
    #endregion

    #region Creation
    private static void WriteBaseData()
    {
        // Copy the static asset to the user folder        
        TextAsset baseline = Resources.Load<TextAsset>("Text/maleghast");
        string path = Preferences.Current.DataPath;
        string filename = $"{path}/maleghast_data/base.json";
        System.IO.File.WriteAllText(filename, baseline.text);
    }

    public static void AddActorModal()
    {
        WriteBaseData();

        var contents = Modal2.Contents("PrimaryDialog");
        var typeContainer = contents.Q("ActorTypeContainer");
        typeContainer.Clear();
        contents.Q("CreateActor")?.RemoveFromHierarchy();

        string maleghastData = Preferences.Current.MaleghastFile;

        var useHomebrew = Modal2.AddSwitchField("UseHomebrew", "Use Homebrew", maleghastData.Length > 0);
        Modal2.MoveToContainer(useHomebrew, typeContainer);

        var homebrewDesc = Modal2.AddLongMarkup("To create homebrew data, locate maleghast_data/base.json in your data directory and make a copy, then select that field below.");
        Modal2.MoveToContainer(homebrewDesc, typeContainer);

        var file = Modal2.AddInlineFileField("Homebrew", "Data Override", maleghastData, FileBrowserType.Maleghast, false, onChange: () =>
        {
            string result = FileBrowser.Result[0];
            Preferences.Current.MaleghastFile = result;
            Modal2.ChangeComboboxOptions("PlayerColor", GetHouses());
            Modal2.ChangeComboboxOptions("UnitTypeField", GetUnits());
        });
        Modal2.MoveToContainer(file, typeContainer);

        modalConditionBool(homebrewDesc, maleghastData.Length > 0);
        modalConditionBool(file, maleghastData.Length > 0);
        useHomebrew.Q<ShunSwitch>().onValueChanged += (val) =>
        {
            modalConditionBool(homebrewDesc, val);
            modalConditionBool(file, val);
        };

        var unit = Modal2.AddInlineComboboxField("UnitTypeField", "Unit Type", "", GetUnits());
        Modal2.MoveToContainer(unit, typeContainer);

        var color = Modal2.AddInlineComboboxField("PlayerColor", "Player Color", "House Default", GetHouses());
        Modal2.MoveToContainer(color, typeContainer);

        var create = new ShunDialogClose();
        create.name = "CreateActor";
        create.text = "Create Actor";
        create.SetVariant(ButtonVariant.Primary);
        create.clicked += () => CreateClicked();
        contents.Q(className: "shun-dialog__footer").Add(create);
    }

    private static void modalConditionBool(VisualElement e, bool show)
    {
        UI.ToggleDisplay(e, show);
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
        string houseJob = Modal2.GetComboboxFieldValue("UnitTypeField");
        if (houseJob.Length == 0)
        {
            Toast.AddError("A unit type was not selected");
            return;
        }
        string house = houseJob.Split("/")[0];
        string job = houseJob.Split("/")[1];
        string colorValue = Modal2.GetComboboxFieldValue("PlayerColor");
        CreateMaleghastUnit(house, job, token, colorValue);
    }

    private static void CreateMaleghastUnit(string house, string job, string token, string colorValue)
    {
        JSONNode jobdata = GetJob(job);
        Color color = ColorUtility.GetColor(jobdata["color"]);
        if (colorValue != "House Default")
        {
            color = GetHouseColor(colorValue);
        }

        MaleghastActorType t = new()
        {
            Type = TypeName,
            Job = job,
            House = house,
            PType = jobdata["type"],
            Move = jobdata["move"],
            MaxHP = jobdata["hp"],
            CurrentHP = jobdata["hp"],
            Defense = jobdata["def"],
        };
        string shape = "Square 1x1";
        if (jobdata["size"] == 2)
        {
            shape = "Square 2x2";
        }

        string upgrades = jobdata["upgrades"];
        if (upgrades != null)
        {
            t.Upgrades = upgrades.Split("|");
        }
        string traits = jobdata["traits"];
        if (traits != null)
        {
            t.Traits = traits.Split("|");
        }
        string actAbilities = jobdata["actAbilities"];
        if (actAbilities != null)
        {
            t.ActAbilities = actAbilities.Split("|");
        }
        string soulAbilities = jobdata["soulAbilities"];
        if (soulAbilities != null)
        {
            t.SoulAbilities = soulAbilities.Split("|");
        }
        t.Tags = new();
        string initConditions = jobdata["conditions"];
        if (initConditions != null && initConditions.Length > 0)
        {
            foreach (string s in initConditions.Split("|"))
            {
                ActorTag ut = new()
                {
                    Name = s,
                    Color = Color.gray
                };
                t.Tags.Add(ut);
            }
        }
        string initTokens = jobdata["tokens"];
        if (initTokens != null && initTokens.Length > 0)
        {
            foreach (string s in initTokens.Split("|"))
            {
                if (s.IndexOf("#") >= 0)
                {
                    ActorTag ut = new()
                    {
                        Name = s.Split("#")[0],
                        Color = color,
                        HasNumber = true,
                        Value = int.Parse(s.Split("#")[1])
                    };
                    t.Tags.Add(ut);
                }
                else
                {
                    ActorTag ut = new()
                    {
                        Name = s,
                        Color = color,
                    };
                    t.Tags.Add(ut);
                }
            }
        }

        ActorPersistence a = new();
        a.Name = t.Label();
        a.Token = TokenLibraryModal.GetToken(token);
        a.Color = color;
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
        return Job;
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/Maleghast";
    }

    public override void UpdateOverhead(ActorData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<Label>("Pips").text = SymbolString("■", CurrentHP, MaxHP);
    }

    public override void UpdatePanel(ActorData actorData, string elementName)
    {
        base.UpdatePanel(actorData, elementName);
        VisualElement panel = UI.System.Q(elementName);
        UI.ToggleDisplay(panel.Q("DefaultActorPanel"), false);

        VisualElement mgPanel = panel.Q("MaleghastActorPanel");
        UI.ToggleDisplay(mgPanel, true);

        mgPanel.Q<Label>("UnitName").text = Job;
        mgPanel.Q<Label>("UnitName").style.backgroundColor = actorData.Color;
        mgPanel.Q<Label>("UnitName").style.color = Color.white;
        mgPanel.Q<Label>("UnitType").text = $"{House} {PType}";
        mgPanel.Q<Label>("UnitType").style.backgroundColor = actorData.Color;
        mgPanel.Q<Label>("UnitType").style.color = Color.white;
        mgPanel.Q("HpBar").Q<Label>("bar").text = SymbolString("█", CurrentHP, MaxHP);
        mgPanel.Q<Label>("MvValue").text = $"{Move}";
        mgPanel.Q<Label>("DfValue").text = $"{Defense}+";

        string armorValue = "—";
        if (HasTag("Physical Armor"))
        {
            armorValue = "PHYS";
        }
        if (HasTag("Magic Armor"))
        {
            armorValue = "MAG";
        }
        if (HasTag("Super Armor"))
        {
            armorValue = "SUPER";
        }
        mgPanel.Q<Label>("ArmValue").text = armorValue;

        StringBuilder actionsValue = new();
        foreach (string s in ActAbilities)
        {
            if (s.Substring(0, 1) == "=")
            {
                actionsValue.AppendLine($"<b>· {s.Substring(1)}</b>");
            }
        }
        mgPanel.Q<Label>("ActValue").text = actionsValue.ToString();

        StringBuilder traitsValue = new();
        foreach (string s in Traits)
        {
            if (s.Substring(0, 1) == "=")
            {
                traitsValue.AppendLine($"· {s.Substring(1)}");
            }
        }
        foreach (string s in Upgrades)
        {
            if (s.Substring(0, 1) == "=")
            {
                traitsValue.AppendLine($"· {s.Substring(1)}");
            }
        }
        mgPanel.Q<Label>("TraitValue").text = traitsValue.ToString();

        StringBuilder soulValue = new();
        int soulAbilityCount = 0;
        foreach (string s in SoulAbilities)
        {
            if (s.Substring(0, 1) == "=")
            {
                soulAbilityCount++;
                soulValue.AppendLine($"<b>· {s.Substring(1)}</b>");
            }
        }
        mgPanel.Q<Label>("SoulValue").text = soulValue.ToString();
        UI.ToggleDisplay(mgPanel.Q("SOUL"), soulAbilityCount > 0);
    }

    public override void InitOverhead(ActorData actorData)
    {
        VisualElement o = actorData.OverheadElement.Q("Markers");
        o.Clear();
        foreach (ActorTag tag in Tags)
        {
            if (MarkerTextures.ContainsKey(tag.Name))
            {
                if (tag.HasNumber)
                {
                    o.Add(buildMarker(tag.Name, tag.Value));
                }
                else
                {
                    o.Add(buildMarker(tag.Name, -1));
                }
            }
        }
    }

    private VisualElement buildMarker(string name, int count)
    {
        var marker = new VisualElement();
        marker.AddToClassList("mg-marker");
        marker.name = name;

        var icon = new VisualElement();
        icon.AddToClassList("mg-marker__icon");
        Texture2D t = MarkerTextures[name];
        icon.style.backgroundImage = t;

        if (count >= 0)
        {
            var counter = new Label($"{count}");
            counter.AddToClassList("mg-marker__count");
            icon.Add(counter);
        }

        marker.Add(icon);
        return marker;
    }

    public override void InitPanel(ActorData actorData, string elementName, bool selected)
    {
        VisualElement panel = UI.System.Q(elementName);
        foreach (var child in panel.Children())
        {
            UI.ToggleDisplay(child, false);
        }
        UI.ToggleDisplay(panel.Q("DefaultActorPanel"), false);

        if (MaleghastLeftPanel == null && MaleghastRightPanel == null)
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UI/Tabletop/Panels/Maleghast");
            MaleghastLeftPanel = template.Instantiate().Q("MaleghastActorPanel");
            MaleghastLeftPanel.Q<Button>("HpDown").clicked += () =>
            {
                Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, "ModHP|-1");
            };
            MaleghastLeftPanel.Q<Button>("HpUp").clicked += () =>
            {
                Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, "ModHP|+1");
            };
            UI.System.Q("LeftTokenPanel").Add(MaleghastLeftPanel);

            MaleghastRightPanel = template.Instantiate().Q("MaleghastActorPanel");
            MaleghastRightPanel.AddToClassList("right");
            UI.System.Q("RightTokenPanel").Add(MaleghastRightPanel);
        }

        VisualElement mgPanel = panel.Q("MaleghastActorPanel");
        UI.ToggleDisplay(mgPanel, true);

        UI.ToggleDisplay(mgPanel.Q("HpUp"), selected);
        UI.ToggleDisplay(mgPanel.Q("HpDown"), selected);

        mgPanel.Q("Pills").Clear();
        foreach (ActorTag tag in Tags)
        {
            if (tag.HasNumber)
            {
                mgPanel.Q("Pills").Add(Pill.InitNumber(tag.Name, tag.Name, tag.Value, 0, tag.Color, true));
            }
            else
            {
                mgPanel.Q("Pills").Add(Pill.InitRemovable(tag.Name, tag.Name, tag.Color, true));
            }
        }
    }

    public override List<MenuItem> GetMenuItems(bool placed)
    {
        var items = base.GetMenuItems(placed);
        var mg = new MenuItem("Maleghast", null);
        items.Add(mg);


        mg.Children.Add(new MenuItem("Add Token", AddTokenModal));
        mg.Children.Add(new MenuItem("Add Status", AddStatusModal));

        mg.Children.Add(new MenuItem("Alter Stats", AlterStatModal));

        if (!HasTag("Turn Ended"))
        {
            mg.Children.Add(new MenuItem("End Turn", () =>
            {
                ActorTag tag = new();
                tag.Name = "Activated";
                tag.Color = ColorUtility.GetCommonColor("gray");
                Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"AddTag|{JsonUtility.ToJson(tag)}");
                SelectionMenu.Hide();
            }));
        }
        else
        {
            mg.Children.Add(new MenuItem("End Turn", false));
        }

        mg.Children.Add(new MenuItem("Reset All Turns", () =>
        {
            Player.Self().CmdRequestAllActorsCommand("RemoveTag|Turn Ended");
            SelectionMenu.Hide();
        }));

        // if (House == "CARCASS")
        // {
        //     var carcass = new MenuItem("CARCASS", null);
        //     items.Add(carcass);
        //     if (!HasTag("Reload"))
        //     {
        //         carcass.Children.Add(new MenuItem("Set Reload", () =>
        //         {
        //             Actor actor = Actor.GetSelected();
        //             ActorTag tag = new();
        //             tag.Name = "Reload";
        //             tag.Color = GetHouseColor("CARCASS");
        //             Player.Self().CmdRequestActorCommand(actor.Data.Id, $"AddTag|{JsonUtility.ToJson(tag)}");

        //         }));
        //     }
        //     else
        //     {
        //         carcass.Children.Add(new MenuItem("Set Reload", false));
        //     }
        // }

        return items;
    }

    private void AddTokenModal()
    {
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddComboboxField("Token", "Token", "", GetTokens());
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Add", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            string token = Modal2.GetComboboxFieldValue("Token");
            Actor actor = Actor.GetSelected();
            if (!HasTag(token))
            {
                ActorTag tag = new();
                tag.Name = token;
                tag.Color = actor.Data.Color;
                tag.HasNumber = true;
                tag.Value = 1;
                Player.Self().CmdRequestActorCommand(actor.Data.Id, $"AddTag|{JsonUtility.ToJson(tag)}");
            }
            else
            {
                Toast.AddError($"{actor.Data.Name} already has {token}");
            }
        });
        Modal2.Open("Add Token");
    }

    private void AddStatusModal()
    {
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddComboboxField("Status", "Status", "", GetStatuses());
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Add", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            string status = Modal2.GetComboboxFieldValue("Status");
            Actor actor = Actor.GetSelected();
            if (!HasTag(status))
            {
                ActorTag tag = new();
                tag.Name = status;
                tag.Color = actor.Data.Color;
                Player.Self().CmdRequestActorCommand(actor.Data.Id, $"AddTag|{JsonUtility.ToJson(tag)}");
            }
            else
            {
                Toast.AddError($"{actor.Data.Name} already has {status}");
            }
        });
        Modal2.Open("Add Token");
    }

    public override void Command(string command, ActorData actorData)
    {
        Actor actor = actorData.GetActor();
        base.Command(command, actorData);
        if (command.StartsWith("ModHP"))
        {
            int original = CurrentHP;
            int changeValue = int.Parse(command.Split("|")[1]);
            CurrentHP = Clamped(0, CurrentHP + changeValue, MaxHP);
            int diff = CurrentHP - original;
            if (diff != 0 && actorData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(actor, $"/{plus}{diff}|_HP", Color.white);
                UpdateGraphic(actorData);
            }
            if (original > 0 && CurrentHP == 0)
            {
                ActorTag tag = new ActorTag();
                tag.Name = "Corpse";
                Tags.Add(tag);
                InitOverhead(actorData);
            }
            if (original == 0 && CurrentHP > 0)
            {
                RemoveTag("Corpse");
                InitOverhead(actorData);
            }
        }
        else if (command.StartsWith("UpdateStats"))
        {
            string json = command.Split("|")[1];
            MaleghastActorType lmu = JsonUtility.FromJson<MaleghastActorType>(json);
            MaxHP = lmu.MaxHP;
            Defense = lmu.Defense;
            Move = lmu.Move;
            PopoverText.Create(actor, $"_STAT|_CHANGE", Color.white);
        }
        if (command.StartsWith("AddTag") || command.StartsWith("IncrementTag") || command.StartsWith("DecrementTag") || command.StartsWith("RemoveTag"))
        {
            InitOverhead(actorData);
        }
    }

    private void UpdateGraphic(ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        token.SetDefeated(CurrentHP <= 0);
    }

    public static void SetupDialog()
    {
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Maleghast Setup");

        var maliceLevels = StringUtility.CreateArray("Spite (0)", "Loathing (4)", "Hatred (7)", "Hell (10)", "Ultrahell (12)").ToList<String>();
        Modal2.AddInlineSelectField("Malice", "Malice", "Spite (0)", maliceLevels);
        Modal2.AddSwitchField("Heresy", "Heresy Allowed", false);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Confirm", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            var malice = Modal2.GetSelectFieldValue("Malice");
            var heresy = Modal2.GetSwitchFieldValue("Heresy");

            GameSystemTag tag = new();
            tag.Name = heresy ? "HERESY ALLOWED" : "HERESY FORBIDDEN";
            tag.Type = "Simple";
            tag.Color = Color.black;
            Player.Self().CmdRequestGameSystemCommand($"AddTag|{JsonUtility.ToJson(tag)}");

            GameSystemTag tag2 = new();
            tag2.Name = malice;
            tag2.Type = "Simple";
            tag2.Color = Color.black;
            Player.Self().CmdRequestGameSystemCommand($"AddTag|{JsonUtility.ToJson(tag2)}");

            GameSystemTag tag3 = new();
            tag3.Name = "Round";
            tag3.Type = "Number";
            tag3.Color = Color.black;
            tag3.Value = 0;
            tag3.MaxValue = 6;
            Player.Self().CmdRequestGameSystemCommand($"AddTag|{JsonUtility.ToJson(tag3)}");


        });

        Modal2.Open("MG Setup");
    }

    public static void BlackMassGeneratorDialog()
    {
        Player.Self().SetOp("Raising a Black Mass");

        WriteBaseData();

        Modal2.CreateContext("PrimaryDialog", true);
        var contents = Modal2.Contents("PrimaryDialog");
        contents.Clear();

        Modal2.AddCloseAction(() =>
        {
            Player.Self().ClearOp();
        });

        Modal2.AddDialogHeader("Black Mass Generator");

        var massContainer = new ShunContainer();
        massContainer.name = "MassContainer";
        massContainer.AddToClassList("shun-dialog__field");

        var house = Modal2.AddInlineComboboxField("House", "House", null, GetHouses());
        house.Q<ShunCombobox>().OnSelect += () =>
        {
            massContainer.Clear();
            string houseValue = contents.Q<ShunCombobox>("House").selectedValue;
            var houseUnits = GetHouseUnits(houseValue);
            foreach (string unit in houseUnits)
            {
                var unitData = GetJob(unit);
                string type = unitData["type"];
                string label = $"{unit}\n<color=grey><size=-1>{type}</size></color>";
                int min = 0;
                int max = 20;
                switch (type)
                {
                    case "Necromancer":
                        min = 1;
                        max = 1;
                        break;
                    case "Scion":
                        max = 3;
                        break;
                    case "Freak":
                    case "Horror":
                    case "Hunter":
                        max = 2;
                        break;
                    case "Tyrant":
                        max = 1;
                        break;
                }

                var wrapper = new ShunContainer();
                wrapper.style.flexDirection = FlexDirection.Row;
                wrapper.style.justifyContent = Justify.SpaceBetween;
                Modal2.MoveToContainer(wrapper, massContainer);


                var unitField = Modal2.AddInlineNumberNudgerField(unit, label, min, min, max);
                Modal2.MoveToContainer(unitField, wrapper);

                var unitTokenField = Modal2.AddTokenField($"{unit}Token", "");
                Modal2.MoveToContainer(unitTokenField, wrapper);
            }
        };

        var color = Modal2.AddInlineComboboxField("PlayerColor", "Player Color", "House Default", GetHouses());

        contents.Add(massContainer);


        var footer = Modal2.AddDialogFooter("Cancel", () =>
        {
            Player.Self().ClearOp();
            Modal2.Close();
        });

        var create = new ShunDialogClose();
        create.name = "CreateMass";
        create.text = "Create Black Mass";
        create.SetVariant(ButtonVariant.Primary);
        create.clicked += () => BlackMassConfirm();
        footer.Add(create);

        Modal2.Open("Black Mass");
    }

    private static void BlackMassConfirm()
    {
        Modal2.ReadContext("PrimaryDialog");
        string house = Modal2.GetComboboxFieldValue("House");
        if (house == null)
        {
            Toast.AddError("No house selected");
            return;
        }
        var houseUnits = GetHouseUnits(house);
        foreach (string unit in houseUnits)
        {
            var unitData = GetJob(unit);
            int count = Modal2.GetNumberNudgerFieldValue($"{unit}");
            string token = Modal2.GetComboboxFieldValue($"{unit}Token");
            if (token.Length == 0 && count > 0)
            {
                Toast.AddError($"A token has not been selected for {unit}");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                string colorValue = Modal2.GetComboboxFieldValue("PlayerColor");
                CreateMaleghastUnit(house, unit, token, colorValue);
            }
        }
        Modal2.Dialog("PrimaryDialog").Close();
        UI.ToggleActiveClass("BottomBar", true);
        Player.Self().ClearOp();
    }


    #region Private Logic functions

    private static JSONNode GetData()
    {
        string maleghastText = Resources.Load<TextAsset>("Text/maleghast").text;
        string dataFile = Preferences.Current.MaleghastFile;
        if (dataFile?.Length > 0)
        {
            if (File.Exists(dataFile))
            {
                maleghastText = File.ReadAllText(dataFile);
            }
            else
            {
                Toast.AddError("Could not find data file override. Please check the value in configuration. Falling back on default Maleghast data.");
            }
        }
        return JSON.Parse(maleghastText);
    }

    private static List<string> GetTokens()
    {
        List<string> tokens = new();
        JSONNode gamedata = GetData();
        foreach (JSONNode token in gamedata["Tokens"].AsArray)
        {
            tokens.Add(token);
        }
        return tokens;
    }

    private static List<string> GetStatuses()
    {
        List<string> tokens = new();
        JSONNode gamedata = GetData();
        foreach (JSONNode token in gamedata["Statuses"].AsArray)
        {
            tokens.Add(token);
        }
        return tokens;
    }

    private static List<string> GetHouses()
    {
        List<string> houses = new();

        JSONNode gamedata = GetData();
        foreach (JSONNode unit in gamedata["Units"].AsArray)
        {
            if (!houses.Contains(unit["house"]))
            {
                houses.Add(unit["house"]);
            }
        }
        return houses;
    }

    private static List<string> GetUnits()
    {
        List<string> units = new();

        JSONNode gamedata = GetData();
        foreach (JSONNode unit in gamedata["Units"].AsArray)
        {
            string houseJob = $"{unit["house"]}/{unit["name"]}";
            units.Add(houseJob.Replace("\"", ""));
        }
        return units;
    }

    private static List<string> GetHouseUnits(string house)
    {
        List<string> units = new();

        JSONNode gamedata = GetData();
        foreach (JSONNode unit in gamedata["Units"].AsArray)
        {
            if (unit["house"] == house)
            {
                units.Add(unit["name"]);
            }
        }
        return units;
    }

    private static JSONNode GetJob(string type)
    {
        JSONNode gamedata = GetData();
        foreach (JSONNode unit in gamedata["Units"].AsArray)
        {
            if (type == unit["name"])
            {
                return unit;
            }
        }
        return null;
    }

    private static Color GetHouseColor(string house)
    {
        JSONNode gamedata = GetData();
        foreach (JSONNode unit in gamedata["Units"].AsArray)
        {
            if (house == unit["house"])
            {
                return ColorUtility.GetColor(unit["color"]);
            }
        }
        return Color.black;
    }

    private void AlterStatModal()
    {
        SelectionMenu.Hide();
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Alter Core Stats");
        Modal2.AddInlineNumberNudgerField("MaxHP", "Max HP", MaxHP, 0, 20);
        Modal2.AddInlineNumberNudgerField("Move", "Move", Move, 0, 10);
        Modal2.AddInlineNumberNudgerField("Defense", "Defense", Defense, 0, 6);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Save", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            MaxHP = Modal2.GetNumberNudgerFieldValue("MaxHP");
            Defense = Modal2.GetNumberNudgerFieldValue("Defense");
            Move = Modal2.GetNumberNudgerFieldValue("Move");
            string serialized = Serialize();

            Player.Self().CmdRequestActorCommand(Actor.GetSelected().Data.Id, $"UpdateStats|{serialized}");
            Modal2.Close();
            this.InitPanel(Actor.GetSelected().Data, "LeftTokenPanel", true);
        });
        Modal2.Open("Alter Stat");
    }

    #endregion
}