using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Text;

[Serializable]
public class MaleghastUnit : UnitData
{
    private readonly static string TypeName = "Maleghast Unit";

    #region Registration
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        UnitTokenRegistry.RegisterSystem($"{TypeName}");
        UnitTokenRegistry.RegisterInterfaceCallback($"{TypeName}", DeserializeAsInterface);
        UnitTokenRegistry.RegisterSimpleCallback($"{TypeName}|AddTokenModal", AddTokenModal);
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
    public static IUnitData DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<MaleghastUnit>(json);
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
    public int Armor;
    public string[] Upgrades;
    public string[] Traits;
    public string[] ActAbilities;
    public string[] SoulAbilities;
    #endregion

    #region Creation
    public static void AddTokenModal()
    {
        // Copy the static asset to the user folder        
        TextAsset baseline = Resources.Load<TextAsset>("Text/maleghast");
        string path = Preferences.Current.DataPath;
        string filename = $"{path}/maleghast_data/base.json";
        if (!Directory.Exists(path + "/maleghast_data"))
        {
            Directory.CreateDirectory(path + "/maleghast_data");
        }
        System.IO.File.WriteAllText(filename, baseline.text);

        Modal.AddMarkup("Description", "Maleghast Unit tokens have stats automatically derived from ruledata.");

        string maleghastData = Preferences.Current.MaleghastFile;
        Modal.AddFileField("RulesFile", "Data Override", maleghastData, "rules", (evt) =>
        {
            Preferences.Current.MaleghastFile = evt.newValue;
            UI.Modal.Q<DropdownField>("PlayerColor").choices = GetHouses();
            SearchField.ChangeOptions(UI.Modal.Q("UnitTypeField"), GetUnits().ToArray());
        });
        Modal.AddHelpText("RulesHelp", "If blank, stats will be derived from the core rules + updates. You can change these values or add homebrew units by copying and editing the maleghast_data/base.json file in the data directory.");


        Modal.AddSearchField("UnitTypeField", "Unit Type", "", GetUnits().ToArray());
        Modal.AddDropdownField("PlayerColor", "Player Color", "House Default", GetHouses().ToArray());

        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "RulesFile", "RulesHelp", "UnitType", "PlayerColor", "UnitTypeField"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string houseJob = SearchField.GetValue(UI.Modal.Q("UnitTypeField"));
        string house = houseJob.Split("/")[0];
        string job = houseJob.Split("/")[1];
        string colorValue = UI.Modal.Q<DropdownField>("PlayerColor").value;
        JSONNode jobdata = GetJob(job);
        Color color = ColorUtility.GetColor(jobdata["color"]);
        if (colorValue != "House Default")
        {
            color = GetHouseColor(colorValue);
        }

        MaleghastUnit t = new()
        {
            Type = TypeName,
            Job = job,
            House = house,
            PType = jobdata["type"],
            Move = jobdata["move"],
            MaxHP = jobdata["hp"],
            CurrentHP = jobdata["hp"],
            Defense = jobdata["def"],
            TokenMeta = TokenLibrary.GetSelectedMeta(),
            Color = color
        };

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
                UnitTag ut = new()
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
                    UnitTag ut = new()
                    {
                        Name = s.Split("#")[0],
                        Color = t.Color,
                        HasNumber = true,
                        Value = int.Parse(s.Split("#")[1])
                    };
                    t.Tags.Add(ut);
                }
                else
                {
                    UnitTag ut = new()
                    {
                        Name = s,
                        Color = t.Color,
                    };
                    t.Tags.Add(ut);
                }
            }
        }


        AddToken.FinalizeToken(t.Serialize());
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

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<Label>("Pips").text = SymbolString("■", CurrentHP, MaxHP);
        UI.ToggleDisplay(o, CurrentHP > 0 && tokenData.Placed);
    }

    public override void UpdatePanel(TokenData tokenData, string elementName)
    {
        base.UpdatePanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);

        Label mainHPLabel = panel.Q<Label>("MainHPLabel");
        mainHPLabel.text = SymbolString("■", CurrentHP, MaxHP);
    }

    public override void InitPanel(string elementName, bool selected)
    {
        base.InitPanel(elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        Label l = new();
        l.name = "MainHPLabel";
        l.text = SymbolString("■", CurrentHP, MaxHP);
        l.style.color = Color.red;
        l.style.unityTextOutlineColor = Color.white;
        l.style.unityTextOutlineWidth = 1;
        l.style.fontSize = 26;
        panel.Q("Bars").Add(l);

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

        panel.Q("Pills").Add(Pill.InitStatic("HousePill", $"{House} {PType}", Color));
        panel.Q("Pills").Q("HousePill").SendToBack();
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        List<MenuItem> items = new();
        if (placed)
        {
            items.Add(new MenuItem("Remove", "Remove", ClickRemove));
            items.Add(new MenuItem("Flip", "Flip", ClickFlip));
        }
        // items.Add(new MenuItem("Reshape", "Reshape", ClickReshape));
        items.Add(new MenuItem("Clone", "Clone", ClickClone));
        items.Add(new MenuItem("Delete", "Delete", ClickDelete));
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.TokenCommand("ModHP"); }));

        if (House == "CARCASS" && !HasTag("Loaded"))
        {
            items.Add(new MenuItem("Reload", "Reload", (evt) => DirectCommand("Reload")));
        }

        return items.ToArray();
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
                PopoverText.Create(token, $"/{plus}{diff}|_HP", Color.white);
                UpdateGraphic(tokenData);
            }
        }
        if (command == "Reload")
        {
            if (!HasTag("Loaded"))
            {
                UnitTag tag = new()
                {
                    Name = "Loaded",
                    Color = GetHouseColor("CARCASS")
                };
                Tags.Add(tag);
                PopoverText.Create(token, $"_RELOADED", Color.white);
                Token.RebuildPanels = true;
            }
        }
    }

    private void UpdateGraphic(TokenData tokenData)
    {
        Token token = tokenData.GetToken();
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

    #endregion
}