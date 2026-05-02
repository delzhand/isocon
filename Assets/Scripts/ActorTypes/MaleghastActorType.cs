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

[Serializable]
public class MaleghastActorType : ActorType
{
    private readonly static string TypeName = "Maleghast Unit";

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
    public static void AddActorModal()
    {
        // Copy the static asset to the user folder        
        TextAsset baseline = Resources.Load<TextAsset>("Text/maleghast");
        string path = Preferences.Current.DataPath;
        string filename = $"{path}/maleghast_data/base.json";
        System.IO.File.WriteAllText(filename, baseline.text);


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
        return "UI/TableTop/Overheads/PipCounter";
    }

    public override void UpdateOverhead(ActorData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<Label>("Pips").text = SymbolString("■", CurrentHP, MaxHP);
        UI.ToggleDisplay(o, CurrentHP > 0 && tokenData.Placed);
    }

    public override void UpdatePanel(ActorData tokenData, string elementName)
    {
        base.UpdatePanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);

        Label mainHPLabel = panel.Q<Label>("MainHPLabel");
        mainHPLabel.text = SymbolString("■", CurrentHP, MaxHP);
    }

    public override void InitPanel(ActorData actorData, string elementName, bool selected)
    {
        base.InitPanel(actorData, elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        if (selected)
        {
            VisualElement hppips = PipsBar("MainHPLabel", "■", CurrentHP, MaxHP, Color.red,
                (evt) => { Player.Self().CmdRequestActorCommand(actorData.Id, "ModHP|-1"); },
                (evt) => { Player.Self().CmdRequestActorCommand(actorData.Id, "ModHP|1"); }
            );
            panel.Q("Bars").Add(hppips);
        }
        else
        {
            Label l = new();
            l.name = "MainHPLabel";
            l.text = SymbolString("■", CurrentHP, MaxHP);
            l.style.color = Color.red;
            l.style.unityTextOutlineColor = Color.white;
            l.style.unityTextOutlineWidth = 1;
            l.style.fontSize = 26;
            panel.Q("Bars").Add(l);
        }

        VisualElement s1 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s1.Q<Label>("Label").text = "MOVE/DEF";
        s1.Q<Label>("Value").text = $"{Move}/{Defense}+";
        panel.Q("Stats").Add(s1);

        List<string> actions = new();
        foreach (string s in ActAbilities)
        {
            if (s.Substring(0, 1) == "=")
            {
                actions.Add(s.Substring(1));
            }
        }
        VisualElement acts = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        acts.Q<Label>("Label").text = $"ACT: {String.Join(" | ", actions)}";
        acts.Q<Label>("Value").text = "";
        panel.Q("Bars").Add(acts);
        acts.SendToBack();


        foreach (string s in Traits)
        {
            VisualElement template = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
            template.Q<Label>("Label").text = $"TRAIT: {s.Substring(1)}";
            template.Q<Label>("Value").text = "";
            panel.Q("Stats").Add(template);
        }

        panel.Q("Pills").Add(Pill.InitStatic("HousePill", $"{House} {PType}", actorData.Color));
        panel.Q("Pills").Q("HousePill").SendToBack();
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
                tag.Name = "Turn Ended";
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
                PopoverText.Create(token, $"/{plus}{diff}|_HP", Color.white);
                UpdateGraphic(tokenData);
            }
        }
        else if (command.StartsWith("UpdateStats"))
        {
            string json = command.Split("|")[1];
            MaleghastActorType lmu = JsonUtility.FromJson<MaleghastActorType>(json);
            MaxHP = lmu.MaxHP;
            Defense = lmu.Defense;
            Move = lmu.Move;
            PopoverText.Create(token, $"_STAT|_CHANGE", Color.white);
        }

    }

    private void UpdateGraphic(ActorData tokenData)
    {
        Actor token = tokenData.GetActor();
        token.SetDefeated(CurrentHP <= 0);
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
            houses.Add(unit["house"]);
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