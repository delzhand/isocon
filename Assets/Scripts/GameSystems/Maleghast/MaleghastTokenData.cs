using System.Collections.Generic;
using System.Linq;
using Mirror;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

[System.Serializable]
public class MaleghastTokenDataRaw: TokenDataRaw
{
    public string HouseJob;
    public string Color;

    public static string ToJson() {
        MaleghastTokenDataRaw raw = new MaleghastTokenDataRaw();
        raw.Name = UI.Modal.Q<TextField>("NameField").value;
        Texture2D graphic = TextureSender.CopyLocalImage(UI.Modal.Q("ImageSearchField").Q<TextField>("SearchInput").value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);
        raw.HouseJob = SearchField.GetValue(UI.Modal.Q("UnitType"));
        raw.Color = UI.Modal.Q<DropdownField>("PlayerColor").value;
        return JsonUtility.ToJson(raw);
    }
}

public class MaleghastTokenData : TokenData
{
    // // Mutable - these variables change during play
    // [SyncVar]
    // public int CurrentHP;
    // [SyncVar]
    // public int Soul;
    // [SyncVar]
    // public int Strength;
    // [SyncVar]
    // public int Vitality;
    // [SyncVar]
    // public int Speed;

    // public string[] Traits;
    // public string[] ActAbilities;
    // public string[] SoulAbilities;

    // // Derived - these values can be initialized from json
    // public string House;  // eg CARCASS
    // public string Type; // eg Thrall
    // public string Job; // eg Gunwight
    // public Color Color;
    // public string Armor;
    // public int MaxHP;
    // public int Move;
    // public int Defense;

    // public int Size;

    // void Update()
    // {
    //     BaseUpdate();
    // }

    // public override bool NeedsSetup()
    // {
    //     return MaxHP == 0;
    // }

    // public override void UpdateOverheadValues() {
    //     OverheadElement.Q<ProgressBar>("HpBar").value = CurrentHP;
    //     OverheadElement.Q<ProgressBar>("HpBar").highValue = MaxHP;
    // }

    // public override void TokenDataSetup(string json, string id) {
    //     base.TokenDataSetup(json, id);
    //     DoTokenDataSetup();
    //     CurrentHP = MaxHP;
    // }

    // public override void DoTokenDataSetup() {
    //     MaleghastTokenDataRaw raw = JsonUtility.FromJson<MaleghastTokenDataRaw>(Json);
    //     Name = raw.Name;
    //     GraphicHash = raw.GraphicHash;
    //     SetStats(raw.HouseJob, raw.Color);
    // }

    // public override void CreateOverhead() {
    //     VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/GameSystem/SimpleOverhead");
    //     VisualElement instance = template.Instantiate();
    //     OverheadElement = instance.Q("Overhead");
    //     UI.System.Q("Worldspace").Add(OverheadElement);
    // }

    // public override void CreateWorldToken() {
    //     base.CreateWorldToken();    
    //     Material m = Instantiate(Resources.Load<Material>("Materials/Token/BorderBase"));
    //     m.SetColor("_Border", Color);
    //     TokenObject.transform.Find("Base").GetComponent<DecalProjector>().material = m;
    // }

    // public override void CreateUI() {
    //     base.CreateUI();
    //     UnitBarElement.Q("ClassBackground").style.borderTopColor = Color;
    //     UnitBarElement.Q("ClassBackground").style.borderRightColor = Color;
    //     UnitBarElement.Q("ClassBackground").style.borderBottomColor = Color;
    //     UnitBarElement.Q("ClassBackground").style.borderLeftColor = Color;
    // }

    // public override int GetSize()
    // {
    //     return Size;
    // }

    // private void SetStats(string houseJob, string color) {
    //     House = houseJob.Split("/")[0];
    //     Job = houseJob.Split("/")[1];

    //     JSONNode gamedata = JSON.Parse(GameSystem.DataJson);

    //     foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray) {
    //         if (color != "House Default" && color == house["name"]) {
    //             Color = ColorUtility.ColorFromHex(house["color"]);
    //         }
    //         if (house["name"] == House) {
    //             if (color == "House Default") {
    //                 Color = ColorUtility.ColorFromHex(house["color"]);
    //             }
    //             foreach (JSONNode unit in house["units"].AsArray) {
    //                 if (unit["name"] == Job) {
    //                     Type = unit["type"];
    //                     Move = unit["move"];
    //                     MaxHP = unit["hp"];
    //                     Defense = unit["def"];
    //                     Armor = unit["armor"];

    //                     // string traits = unit["traits"];
    //                     // Traits = traits.Split(",");

    //                     // string actAbilities = unit["actAbilities"];
    //                     // ActAbilities = actAbilities.Split(",");
                        
    //                     // string upgrades = unit["upgrad"]
    //                     // Traits = "Formation,Thrall";
    //                     // ActAbilities = "OL45,Baton";
    //                     // Upgrades = "Brace,Tactical Reload,Scavenge Ammo";

    //                     Traits = StringUtility.Arr("Formation","Thrall","Brace|0","Tactical Reload|0","Scavenge Ammo|0");
    //                     ActAbilities = StringUtility.Arr("OL45", "Baton");
    //                 }
    //             }
    //         }
    //     }

    //     if (Type == "Tyrant") {
    //         Size = 2;
    //     }
    // }

    // public override void Change(string value)
    // {
    //     FileLogger.Write($"{Name} changed - {value}");
    //     if (value.StartsWith("GainHP")) {
    //         int diff = int.Parse(value.Split("|")[1]);
    //         if (CurrentHP + diff > MaxHP) {
    //             diff = MaxHP - CurrentHP;
    //         }
    //         if (diff > 0) {
    //             CurrentHP+=diff;
    //             PopoverText.Create(TokenObject.GetComponent<Token>(), $"/+{diff}|_HP", Color.white);
    //             TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
    //         }
    //         OnVitalChange();
    //     }
    //     if (value.StartsWith("LoseHP")) {
    //         int diff = int.Parse(value.Split("|")[1]);
    //         if (CurrentHP - diff < 0) {
    //             diff = CurrentHP;
    //         }
    //         if (diff > 0) {
    //             CurrentHP-=diff;
    //             PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-{diff}|_HP", Color.white);
    //             TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
    //         }
    //         OnVitalChange();
    //     }
    //     if (value.StartsWith("LoseStatus")) {
    //         string[] parts = value.Split("|");
    //         Conditions.Remove(parts[1]);
    //         PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-|_{parts[1].ToUpper()}", Color.white);
    //         OnStatusChange();
    //     }
    //     if (value.StartsWith("GainStatus")) {
    //         string[] parts = value.Split("|");
    //         if (!Conditions.ContainsKey(parts[1])) {
    //             Conditions.Add(parts[1], new StatusEffect(){Name = parts[1], Type = parts[2], Color = parts[3], Number = int.Parse(parts[4])});
    //         }
    //         else {
    //             Toast.Add($"Condition { parts[1] } is already set on { Name }.");
    //         }
    //         PopoverText.Create(TokenObject.GetComponent<Token>(), $"/+|_{parts[1].ToUpper()}", Color.white);
    //         OnStatusChange();
    //     }
    //     if (value.StartsWith("IncrementStatus")) {
    //         string status = value.Split("|")[1];
    //         StatusEffect se = Conditions[status];
    //         se.Number++;
    //         Conditions[status] = se;
    //         OnStatusChange();
    //     }
    //     if (value.StartsWith("DecrementStatus")) {
    //         string status = value.Split("|")[1];
    //         StatusEffect se = Conditions[status];
    //         se.Number--;
    //         Conditions[status] = se;
    //         OnStatusChange();
    //     }
    //     if (value.StartsWith("EnableUpgrade")) {
    //         string upgrade = value.Split("|")[1];
    //         for (int i = 0; i < Traits.Length; i++) {
    //             string trait = Traits[i];
    //             if (trait.StartsWith(upgrade)) {
    //                 if (trait.EndsWith("|0")) {
    //                     Traits[i] = Traits[i].Replace("|0", "|1");
    //                 }
    //                 else if (trait.EndsWith("|1")) {
    //                     Traits[i] = Traits[i].Replace("|1", "|0");
    //                 }
    //             }
    //         }
    //     }
    // }  

    // private void OnVitalChange() {
    //     TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
    //     if (CurrentHP <= 0) {
    //         Conditions["Corpse"] = new StatusEffect(){Name = "Corpse", Type = "Simple", Color = "Gray"};
    //     }
    //     else {
    //         if (Conditions.ContainsKey("Corpse")) {
    //             Conditions.Remove("Corpse");
    //         }
    //     }
    // }

    // /**
    //  * This happens on demand regardless of whether token is selected or focused
    //  * compare with UpdateTokenPanel, which runs every frame for selected/focused tokens only
    //  */
    // private void OnStatusChange() {
    //     Color c = Conditions.ContainsKey("Turn Ended") ? ColorUtility.ColorFromHex("#505050") : Color.white;
    //     UnitBarElement.Q<VisualElement>("Portrait").style.unityBackgroundImageTintColor = c;
    // }

    // public override void UpdateTokenPanel(string elementName) {
    //     base.UpdateTokenPanel(elementName);
    //     VisualElement panel = UI.System.Q(elementName);

    //     panel.Q("ClassBackground").style.borderTopColor = Color;
    //     panel.Q("ClassBackground").style.borderRightColor = Color;
    //     panel.Q("ClassBackground").style.borderBottomColor = Color;
    //     panel.Q("ClassBackground").style.borderLeftColor = Color;

    //     panel.Q<Label>("House").text = House;
    //     panel.Q<Label>("House").style.backgroundColor = Color;
    //     panel.Q<Label>("Job").text = Job;
    //     panel.Q<Label>("Job").style.backgroundColor = Color;

    //     panel.Q<Label>("CHP").text = $"{ CurrentHP }";
    //     panel.Q<Label>("MHP").text = $"/{ MaxHP }";
    //     panel.Q<ProgressBar>("HpBar").value = CurrentHP;
    //     panel.Q<ProgressBar>("HpBar").highValue = MaxHP;
        
    //     panel.Q("Defense").Q<Label>("Value").text = $"{ Defense }";
    //     panel.Q("Move").Q<Label>("Value").text = $"{ Move }";
    //     panel.Q("Armor").Q<Label>("Value").text = $"{ Armor.ToUpper() }";
    //     panel.Q("Type").Q<Label>("Value").text = Type;
        
    //     panel.Q("Traits").Q("List").Clear();
    //     foreach (string s in Traits) {
    //         if (!s.EndsWith("|0")) {
    //             panel.Q("Traits").Q("List").Add(new Label(){text = s.Replace("|1", "")});
    //         }
    //     }
    // }

    // private void ReinitUI(string elementName) {
    //     VisualElement panel = UI.System.Q(elementName);
    //     panel.Q("Conditions").Q("List").Clear();

    //     foreach(KeyValuePair<string, StatusEffect> item in Conditions) {
    //         VisualElement e = UI.CreateFromTemplate("UITemplates/GameSystem/ConditionTemplate");
    //         e.Q<Label>("Name").text = item.Key;
    //         Color c = Color.black;
    //         switch (item.Value.Color) {
    //             case "Gray":
    //                 c = ColorUtility.ColorFromHex("7b7b7b");
    //                 break;
    //             case "Green":
    //                 c = ColorUtility.ColorFromHex("248d2e");
    //                 break;
    //             case "Red":
    //                 c = ColorUtility.ColorFromHex("8d2424");
    //                 break;
    //             case "Blue":
    //                 c = ColorUtility.ColorFromHex("24448d");
    //                 break;
    //             case "Purple":
    //                 c = ColorUtility.ColorFromHex("5c159f");
    //                 break;
    //             case "Yellow":
    //                 c = ColorUtility.ColorFromHex("887708");
    //                 break;
    //             case "Orange":
    //                 c = ColorUtility.ColorFromHex("a57519");
    //                 break;
    //         }
    //         e.Q("Wrapper").style.backgroundColor = c;
    //         if (item.Value.Type == "Number") {
    //             e.Q<Label>("Name").text = $"{item.Key} {item.Value.Number}";
    //             e.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => {
    //                 Player.Self().CmdRequestTokenDataSetValue(this, $"IncrementStatus|{ item.Key }");
    //             });
    //             e.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => {
    //                 Player.Self().CmdRequestTokenDataSetValue(this, $"DecrementStatus|{ item.Key }");
    //             });
    //         }
    //         else {
    //             UI.ToggleDisplay(e.Q("Increment"), false);
    //             UI.ToggleDisplay(e.Q("Decrement"), false);
    //         }
    //         e.Q<Button>("Remove").RegisterCallback<ClickEvent>((evt) => {
    //             Player.Self().CmdRequestTokenDataSetValue(this, $"LoseStatus|{ item.Key }");
    //         });
    //         panel.Q("Conditions").Q("List").Add(e);
    //     }
    // }
}
