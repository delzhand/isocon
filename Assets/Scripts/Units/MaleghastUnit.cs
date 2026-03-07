using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

[Serializable]
public class MaleghastUnitToken : UnitData
{
    private readonly static string TypeName = "Maleghast Unit";
    private static JSONNode GameData;

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
        return JsonUtility.FromJson<Icon2x0MobUnit>(json);
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
        Modal.AddTokenField("TokenSearchField");

        string maleghastData = Preferences.Current.MaleghastFile;
        Modal.AddFileField("RulesFile", "Maleghast Data", maleghastData, "rules", (evt) =>
        {
            Preferences.Current.MaleghastFile = evt.newValue;
            UI.Modal.Q<DropdownField>("PlayerColor").choices = GetHouses();
        });


        Modal.AddSearchField("UnitTypeField", "Unit Type", "", GetUnits().ToArray());
        Modal.AddDropdownField("PlayerColor", "Player Color", "House Default", GetHouses().ToArray());

        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "TokenSearchField", "RulesFile", "UnitType", "PlayerColor", "UnitTypeField"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string houseJob = SearchField.GetValue(UI.Modal.Q("UnitType"));
        string house = houseJob.Split("/")[0];
        string job = houseJob.Split("/")[1];
        // string colorValue = UI.Modal.Q<DropdownField>("PlayerColor").value;
        // // Color color = ColorUtility.GetColor(FindHouse(house)["color"]);
        // // if (colorValue != "House Default")
        // // {
        // //     color = ColorUtility.GetColor(FindHouse(colorValue)["color"]);
        // // }


        // JSONNode jobdata = FindJob(house, job);
        // // data.Type = job["type"];
        // // data.Move = job["move"];
        // // data.MaxHP = job["hp"];
        // // data.CurrentHP = job["hp"];
        // // data.Defense = job["def"];
        // // data.Conditions = new string[] { };
        // // data.Tokens = new string[] { };
        // // string upgrades = job["upgrades"];
        // // if (upgrades != null)
        // // {
        // //     data.Upgrades = upgrades.Split("|");
        // // }
        // // string traits = job["traits"];
        // // if (traits != null)
        // // {
        // //     data.Traits = traits.Split("|");
        // // }
        // // string actAbilities = job["actAbilities"];
        // // if (actAbilities != null)
        // // {
        // //     data.ActAbilities = actAbilities.Split("|");
        // // }
        // // string soulAbilities = job["soulAbilities"];
        // // if (soulAbilities != null)
        // // {
        // //     data.SoulAbilities = soulAbilities.Split("|");
        // // }
        // // string initConditions = job["conditions"];
        // // if (initConditions != null && initConditions.Length > 0)
        // // {
        // //     data.Conditions = initConditions.Split("|");
        // // }

        // MaleghastUnitToken t = new()
        // {
        //     System = "Maleghast Unit",
        //     Job = job,
        //     House = house,
        //     PType = jobdata["type"],
        //     Move = jobdata["move"],
        //     MaxHP = jobdata["hp"],
        //     CurrentHP = jobdata["hp"],
        //     Defense = jobdata["def"],
        //     TokenMeta = TokenLibrary.GetSelectedMeta()
        // };

        // AddToken.FinalizeToken(t.Serialize());
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

    // public override MenuItem[] GetTokenMenuItems(bool placed)
    // {
    //     MenuItem[] baseItems = base.GetTokenMenuItems(placed);

    //     List<MenuItem> items = new();
    //     // if (Hits > 0)
    //     // {
    //     //     items.Add(new MenuItem("TakeHit", "Take Hit", (evt) =>
    //     //     {
    //     //         Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, "TakeHit");
    //     //         SelectionMenu.Hide();
    //     //     }));
    //     // }
    //     // if (Hits < 2)
    //     // {
    //     //     items.Add(new MenuItem("RestoreHit", "Restore Hit", (evt) =>
    //     //     {
    //     //         Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, "RestoreHit");
    //     //         SelectionMenu.Hide();
    //     //     }));
    //     // }

    //     return baseItems.Concat(items.ToArray()).ToArray();
    // }

    // public override void HandleCommand(string command, TokenData tokenData)
    // {
    //     Token token = tokenData.GetToken();
    //     base.HandleCommand(command, tokenData);
    //     // if (command == "TakeHit")
    //     // {
    //     //     if (Hits > 0)
    //     //     {
    //     //         Hits -= 1;
    //     //         PopoverText.Create(token, $"-/1|_HIT", Color.white);
    //     //     }
    //     //     UpdateGraphic(tokenData);
    //     // }
    //     // if (command == "RestoreHit")
    //     // {
    //     //     if (Hits < 2)
    //     //     {
    //     //         Hits += 1;
    //     //         PopoverText.Create(token, $"+/1|_HIT", Color.white);
    //     //     }
    //     //     UpdateGraphic(tokenData);
    //     // }
    // }
    // public override void UpdateOverhead(TokenData tokenData)
    // {
    //     VisualElement o = tokenData.OverheadElement;
    //     o.Q<Label>("Pips").text = SymbolString("▰", CurrentHP, MaxHP);
    // }

    // #region Private Logic functions

    // private void UpdateGraphic(TokenData tokenData)
    // {
    //     Token token = tokenData.GetToken();
    //     token.SetDefeated(CurrentHP <= 0);
    // }

    // private static JSONNode FindHouse(string searchHouse)
    // {
    //     JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
    //     foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray)
    //     {
    //         if (house["name"] == searchHouse)
    //         {
    //             return house;
    //         }
    //     }
    //     return null;
    // }

    private static JSONNode FindJob(string searchHouse, string searchJob)
    {
        JSONNode gamedata = GetData();
        foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray)
        {
            foreach (JSONNode unit in house["units"].AsArray)
            {
                if (unit["name"] == searchJob)
                {
                    return unit;
                }
            }
        }
        return null;
    }

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
    // #endregion
}