// using System;
// using System.Collections.Generic;
// using System.Linq;
// using SimpleJSON;
// using UnityEngine;
// using UnityEngine.UIElements;

// [Serializable]
// public class MaleghastUnitToken : UnitToken
// {
//     #region Registration
//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     private static void Register()
//     {
//         UnitTokenRegistry.RegisterSystem("Maleghast Unit");
//         UnitTokenRegistry.RegisterInterfaceCallback("Maleghast Unit", DeserializeAsInterface);
//         UnitTokenRegistry.RegisterSimpleCallback("Maleghast Unit|AddTokenModal", AddTokenModal);
//     }
//     public override string Serialize()
//     {
//         return JsonUtility.ToJson(this);
//     }
//     public static IUnitToken DeserializeAsInterface(string json)
//     {
//         return JsonUtility.FromJson<MaleghastUnitToken>(json);
//     }
//     #endregion

//     #region Stats
//     public string Job;
//     public string House;
//     public string PType;
//     public int CurrentHP;
//     public int MaxHP;
//     public int Move;
//     public int Defense;
//     public int Armor;
//     #endregion

//     #region Creation
//     public static void AddTokenModal()
//     {
//         Modal.AddMarkup("Description", "Maleghast Unit tokens have stats automatically derived from ruledata.");
//         Modal.AddTokenField("TokenSearchField");

//         // JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
//         List<string> units = new();
//         List<string> houses = new();
//         foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray)
//         {
//             houses.Add(house["name"]);
//             foreach (JSONNode unit in house["units"].AsArray)
//             {
//                 string houseJob = $"{house["name"]}/{unit["name"]}";
//                 units.Add(houseJob.Replace("\"", ""));
//             }
//         }
//         Modal.AddSearchField("UnitType", "Unit Type", "", units.ToArray());
//         Modal.AddDropdownField("PlayerColor", "Player Color", "House Default", houses.ToArray());

//         Modal.AddPreferredButton("Create Token", CreateClicked);
//         Modal.AddButton("Cancel", Modal.CloseEvent);

//         // Necessary to ensure fields are in order and can be cleared when changing type dropdown
//         AddToken.OrderFields(StringUtility.CreateArray("Description", "TokenSearchField", "UnitType", "PlayerColor"));
//     }

//     private static void CreateClicked(ClickEvent evt)
//     {
//         if (!TokenLibrary.TokenSelected())
//         {
//             Toast.AddError("A token has not been selected");
//             return;
//         }

//         string houseJob = SearchField.GetValue(UI.Modal.Q("UnitType"));
//         string house = houseJob.Split("/")[0];
//         string job = houseJob.Split("/")[1];
//         string colorValue = UI.Modal.Q<DropdownField>("PlayerColor").value;
//         Color color = ColorUtility.GetColor(FindHouse(house)["color"]);
//         if (colorValue != "House Default")
//         {
//             color = ColorUtility.GetColor(FindHouse(colorValue)["color"]);
//         }


//         JSONNode jobdata = FindJob(house, job);
//         // data.Type = job["type"];
//         // data.Move = job["move"];
//         // data.MaxHP = job["hp"];
//         // data.CurrentHP = job["hp"];
//         // data.Defense = job["def"];
//         // data.Conditions = new string[] { };
//         // data.Tokens = new string[] { };
//         // string upgrades = job["upgrades"];
//         // if (upgrades != null)
//         // {
//         //     data.Upgrades = upgrades.Split("|");
//         // }
//         // string traits = job["traits"];
//         // if (traits != null)
//         // {
//         //     data.Traits = traits.Split("|");
//         // }
//         // string actAbilities = job["actAbilities"];
//         // if (actAbilities != null)
//         // {
//         //     data.ActAbilities = actAbilities.Split("|");
//         // }
//         // string soulAbilities = job["soulAbilities"];
//         // if (soulAbilities != null)
//         // {
//         //     data.SoulAbilities = soulAbilities.Split("|");
//         // }
//         // string initConditions = job["conditions"];
//         // if (initConditions != null && initConditions.Length > 0)
//         // {
//         //     data.Conditions = initConditions.Split("|");
//         // }

//         MaleghastUnitToken t = new()
//         {
//             System = "Maleghast Unit",
//             Job = job,
//             House = house,
//             PType = jobdata["type"],
//             Move = jobdata["move"],
//             MaxHP = jobdata["hp"],
//             CurrentHP = jobdata["hp"],
//             Defense = jobdata["def"],
//             TokenMeta = TokenLibrary.GetSelectedMeta()
//         };

//         AddToken.FinalizeToken(t.Serialize());
//     }
//     #endregion

//     public override string Label()
//     {
//         return Job;
//     }

//     public override string GetOverheadAsset()
//     {
//         return "UI/TableTop/Overheads/PipCounter";
//     }

//     public override MenuItem[] GetTokenMenuItems(bool placed)
//     {
//         MenuItem[] baseItems = base.GetTokenMenuItems(placed);

//         List<MenuItem> items = new();
//         // if (Hits > 0)
//         // {
//         //     items.Add(new MenuItem("TakeHit", "Take Hit", (evt) =>
//         //     {
//         //         Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, "TakeHit");
//         //         SelectionMenu.Hide();
//         //     }));
//         // }
//         // if (Hits < 2)
//         // {
//         //     items.Add(new MenuItem("RestoreHit", "Restore Hit", (evt) =>
//         //     {
//         //         Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, "RestoreHit");
//         //         SelectionMenu.Hide();
//         //     }));
//         // }

//         return baseItems.Concat(items.ToArray()).ToArray();
//     }

//     public override void HandleCommand(string command, TokenData tokenData)
//     {
//         Token token = tokenData.GetToken();
//         base.HandleCommand(command, tokenData);
//         // if (command == "TakeHit")
//         // {
//         //     if (Hits > 0)
//         //     {
//         //         Hits -= 1;
//         //         PopoverText.Create(token, $"-/1|_HIT", Color.white);
//         //     }
//         //     UpdateGraphic(tokenData);
//         // }
//         // if (command == "RestoreHit")
//         // {
//         //     if (Hits < 2)
//         //     {
//         //         Hits += 1;
//         //         PopoverText.Create(token, $"+/1|_HIT", Color.white);
//         //     }
//         //     UpdateGraphic(tokenData);
//         // }
//     }
//     public override void UpdateOverhead(TokenData tokenData)
//     {
//         VisualElement o = tokenData.OverheadElement;
//         o.Q<Label>("Pips").text = SymbolString("▰", CurrentHP, MaxHP);
//     }

//     #region Private Logic functions

//     private void UpdateGraphic(TokenData tokenData)
//     {
//         Token token = tokenData.GetToken();
//         token.SetDefeated(CurrentHP <= 0);
//     }

//     private static JSONNode FindHouse(string searchHouse)
//     {
//         JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
//         foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray)
//         {
//             if (house["name"] == searchHouse)
//             {
//                 return house;
//             }
//         }
//         return null;
//     }

//     private static JSONNode FindJob(string searchHouse, string searchJob)
//     {
//         JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
//         foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray)
//         {
//             if (house["name"] == searchHouse)
//             {
//                 foreach (JSONNode unit in house["units"].AsArray)
//                 {
//                     if (unit["name"] == searchJob)
//                     {
//                         return unit;
//                     }
//                 }
//             }
//         }
//         return null;
//     }
//     #endregion
// }