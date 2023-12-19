using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Visibility = UnityEngine.UIElements.Visibility;

[System.Serializable]
public class Icon_v1_5TokenDataRaw: TokenDataRaw
{
    public string Class;
    public string Job;
    public bool Elite;
    public int HPMultiplier;

    public static string ToJson() {

        Icon_v1_5TokenDataRaw raw = new Icon_v1_5TokenDataRaw();

        raw.Name = UI.Modal.Q<TextField>("NameField").value;
        Texture2D graphic = TextureSender.CopyLocalImage(UI.Modal.Q("ImageSearchField").Q<TextField>("SearchInput").value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);

        string type = UI.Modal.Q<DropdownField>("Type").value;
        string playerJob = SearchField.GetValue(UI.Modal.Q("PlayerJob"));
        
        if (type == "Player") {
            raw.Class = playerJob.Split("/")[0];
            raw.Job = playerJob.Split("/")[1];
            raw.Elite = false;
            raw.HPMultiplier = 1;
            raw.Size = 1;
        }
        else if (type == "Foe") {
            raw.Class = UI.Modal.Q<DropdownField>("FoeClass").value;
            raw.Job = UI.Modal.Q<TextField>("FoeJob").value;
            raw.HPMultiplier = raw.Class == "Legend" ? int.Parse(UI.Modal.Q<DropdownField>("LegendHP").value.Replace("x", "")) : 1;
            raw.Elite = UI.Modal.Q<Toggle>("Elite").value;
            if (raw.Elite) {
                raw.HPMultiplier = 2;
            }
            raw.Size = int.Parse(UI.Modal.Q<DropdownField>("Size").value[..1]);
        }
        else if (type == "Object") {
            raw.Class = "Object";
            raw.Job = "";
            raw.Elite = false;
            raw.HPMultiplier = UI.Modal.Q<IntegerField>("ObjectHP").value;
            raw.Size = int.Parse(UI.Modal.Q<DropdownField>("Size").value[..1]);
        }

        return JsonUtility.ToJson(raw);
    }
}

public class Icon_v1_5TokenData : TokenData
{
    [SyncVar]
    public string Class;

    [SyncVar]
    public string Job;

    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int Vigor;

    [SyncVar]
    public int Wounds;

    [SyncVar]
    public int Resolve;

    public int MaxHP;
    public int Damage;
    public int Fray;
    public int Range;
    public int Speed;
    public int Dash;
    public int Defense;
    public bool Elite;

    public int Size;

    void Update()
    {
        BaseUpdate();
    }

    public override bool NeedsSetup() {
        return MaxHP == 0;
    }

    public override void UpdateOverheadValues() {
        OverheadElement.Q<ProgressBar>("VigorBar").value = Vigor;
        OverheadElement.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(OverheadElement.Q("VigorBar"), Vigor > 0);

        OverheadElement.Q<ProgressBar>("HpBar").value = CurrentHP;
        OverheadElement.Q<ProgressBar>("HpBar").highValue = MaxHP;

        UI.ToggleDisplay(OverheadElement.Q("Wound1"), Wounds >= 1);
        UI.ToggleDisplay(OverheadElement.Q("Wound2"), Wounds >= 2);
        UI.ToggleDisplay(OverheadElement.Q("Wound3"), Wounds >= 3);
        
        UI.ToggleDisplay(OverheadElement.Q("HpBar"), CurrentHP > 0);
    }

    public override void TokenDataSetup(string json, string id) {
        base.TokenDataSetup(json, id);
        DoTokenDataSetup();
        CurrentHP = MaxHP;
    }

    public override void DoTokenDataSetup() {
        Icon_v1_5TokenDataRaw raw = JsonUtility.FromJson<Icon_v1_5TokenDataRaw>(Json);
        Name = raw.Name;
        GraphicHash = raw.GraphicHash;
        Class = raw.Class;
        Job = raw.Job;
        Elite = raw.Elite;
        Size = raw.Size;
        SetStats(raw.Elite, raw.HPMultiplier);
    }

    public override void CreateOverhead() {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/GameSystem/IconOverhead");
        VisualElement instance = template.Instantiate();
        OverheadElement = instance.Q("Overhead");
        UI.System.Q("Worldspace").Add(OverheadElement);
    }

    public override void CreateWorldToken() {
        base.CreateWorldToken();    
        Color c = UnitColor();
        Material m = Instantiate(Resources.Load<Material>("Materials/Token/BorderBase"));
        m.SetColor("_Border", c);
        TokenObject.transform.Find("Base").GetComponent<DecalProjector>().material = m;
    }

    public override void CreateUI() {
        base.CreateUI();
        Color c = UnitColor();
        UnitBarElement.Q("ClassBackground").style.borderTopColor = c;
        UnitBarElement.Q("ClassBackground").style.borderRightColor = c;
        UnitBarElement.Q("ClassBackground").style.borderBottomColor = c;
        UnitBarElement.Q("ClassBackground").style.borderLeftColor = c;

        ConditionsElement = new VisualElement();
    }

    public override int GetSize()
    {
        return Size;
    }

    private string UnitColorName() {
        switch (Class) {
            case "Wright":
            case "Artillery":
                return "Blue";
            case "Vagabond":
            case "Skirmisher":
                return "Yellow";
            case "Stalwart":
            case "Heavy":
                return "Red";
            case "Leader":
            case "Mendicant":
                return "Green";
            case "Legend":
                return "Purple";
            case "Mob":
                return "Gray";
        }
        return "Black";
    }

    private Color UnitColor() {
        string colorName = UnitColorName();
        return colorName switch
        {
            "Blue" => ColorUtility.ColorFromHex("1A93B7"),
            "Yellow" => ColorUtility.ColorFromHex("C6BB23"),
            "Red" => ColorUtility.ColorFromHex("B72019"),
            "Green" => ColorUtility.ColorFromHex("379317"),
            "Purple" => new Color(.79f, .33f, .94f),
            "Gray" => new Color(.57f, .57f, .57f),
            _ => Color.black
        };
    }

    private void SetStats(bool elite, int hpMultiplier) {
        string color = UnitColorName();
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        MaxHP = gamedata["Icon1_5"]["Stats"][color]["MaxHP"];
        Damage = gamedata["Icon1_5"]["Stats"][color]["Damage"];
        Fray = gamedata["Icon1_5"]["Stats"][color]["Fray"];
        Range = gamedata["Icon1_5"]["Stats"][color]["Range"];
        Speed = gamedata["Icon1_5"]["Stats"][color]["Speed"];
        Dash = gamedata["Icon1_5"]["Stats"][color]["Dash"];
        Defense = gamedata["Icon1_5"]["Stats"][color]["Defense"];
        Elite = elite;
        MaxHP *= hpMultiplier;
        Vigor = 0;
        Wounds = 0;
    }

    public override void Change(string value) {
        FileLogger.Write($"{Name} changed - {value}");
        if (value.StartsWith("GainWound")) {
            Wounds++;
            Wounds = Math.Min(Wounds, 3);
            int woundMaxHP = MaxHP / 4 * (4 - Wounds);
            CurrentHP = Math.Min(CurrentHP, woundMaxHP);
            OnVitalChange();
        }
        if (value.StartsWith("LoseWound")) {
            Wounds--;
            Wounds = Math.Max(Wounds, 0);
            OnVitalChange();
        }
        if (value.StartsWith("GainHP")) {
            int diff = int.Parse(value.Split("|")[1]);
            int woundMaxHP = MaxHP / 4 * (4 - Wounds);
            if (CurrentHP + diff > woundMaxHP) {
                diff = woundMaxHP - CurrentHP;
            }
            if (diff > 0) {
                CurrentHP+=diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/+{diff}|_HP", Color.white);
                TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
            }
            OnVitalChange();
        }
        if (value.StartsWith("LoseHP")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP - diff < 0) {
                diff = CurrentHP;
            }
            if (diff > 0) {
                CurrentHP-=diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-{diff}|_HP", Color.white);
                TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
            }
            OnVitalChange();
        }
        if (value.StartsWith("GainVIG")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (Vigor + diff > MaxHP/4) {
                diff = MaxHP/4 - Vigor;
            }
            if (diff > 0) {
                Vigor+=diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/+{diff}|_VIG", Color.white);
            }
            OnVitalChange();
        }
        if (value.StartsWith("LoseVIG")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (Vigor - diff < 0) {
                diff = Vigor;
            }
            if (diff > 0) {
                Vigor-=diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-{diff}|_VIG", Color.white);
            }
            OnVitalChange();
        }
        if (value.StartsWith("GainRES")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (diff + Resolve > 6) {
                diff = 6 - Resolve;
            }
            if (diff > 0) {
                Resolve+=diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/+{diff}|_RES", Color.white);
            }
            OnVitalChange();
        }
        if (value.StartsWith("GainPRES")) {
            int diff = int.Parse(value.Split("|")[1]);
            Icon_v1_5.PartyResolve+=diff;
            OnVitalChange();
        }
        if (value.StartsWith("LoseRES")) {
            int diff = int.Parse(value.Split("|")[1]);
            Resolve = Math.Max(0, Resolve - diff);
            OnVitalChange();
        }
        if (value.StartsWith("LosePRES")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (diff > Icon_v1_5.PartyResolve) {
                diff = Icon_v1_5.PartyResolve;
            }
            Icon_v1_5.PartyResolve-=diff;
            OnVitalChange();
        }
        if (value.StartsWith("Damage")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (Vigor + CurrentHP - diff < 0) {
                diff = Vigor+CurrentHP;
            }
            if (diff <= 0) {
                return;
            }
            if (diff < Vigor) {
                // Vig damage only
                Vigor -= diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-{diff}|_VIG", Color.white);
            }
            else if (diff > Vigor && Vigor > 0) {
                // Vig zeroed and HP damage
                CurrentHP -= (diff - Vigor);
                Vigor = 0;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-{diff}|_HP/VIG", Color.white);
            }
            else if (Vigor <= 0) {
                // HP damage only
                CurrentHP -= diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-{diff}|_HP", Color.white);
                TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
            }
            OnVitalChange();
        }
        if (value.StartsWith("LoseStatus")) {
            string[] parts = value.Split("|");
            Conditions.Remove(parts[1]);
            PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-|_{parts[1].ToUpper()}", Color.white);
            OnStatusChange();
        }
        if (value.StartsWith("GainStatus")) {
            string[] parts = value.Split("|");
            if (!Conditions.ContainsKey(parts[1])) {
                Conditions.Add(parts[1], new StatusEffect(){Name = parts[1], Type = parts[2], Color = parts[3], Number = int.Parse(parts[4])});
            }
            else {
                Toast.Add($"Condition { parts[1] } is already set on { Name }.");
            }
            PopoverText.Create(TokenObject.GetComponent<Token>(), $"/+|_{parts[1].ToUpper()}", Color.white);
            OnStatusChange();
        }
        if (value.StartsWith("IncrementStatus")) {
            string status = value.Split("|")[1];
            StatusEffect se = Conditions[status];
            se.Number++;
            Conditions[status] = se;
            OnStatusChange();
        }
        if (value.StartsWith("DecrementStatus")) {
            string status = value.Split("|")[1];
            StatusEffect se = Conditions[status];
            se.Number--;
            Conditions[status] = se;
            OnStatusChange();
        }
    }  

    private void OnVitalChange() {
        TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
        if (CurrentHP <= 0) {
            Conditions["Defeated"] = new StatusEffect(){Name = "Defeated", Type = "Simple", Color = "Red", Locked = true};
            if (Conditions.ContainsKey("Bloodied")) {
                Conditions.Remove("Bloodied");
                OnStatusChange();
            }
        }
        else if (CurrentHP <= MaxHP/2) {
            Conditions["Bloodied"] = new StatusEffect(){Name = "Bloodied", Type = "Simple", Color = "Red", Locked = true};
            if (Conditions.ContainsKey("Defeated")) {
                Conditions.Remove("Defeated");
                OnStatusChange();
            }
        }
        else {
            if (Conditions.ContainsKey("Bloodied")) {
                Conditions.Remove("Bloodied");
                OnStatusChange();
            }
            if (Conditions.ContainsKey("Defeated")) {
                Conditions.Remove("Defeated");
                OnStatusChange();
            }
        }
    }

    /**
     * This happens on demand regardless of whether token is selected or focused
     * compare with UpdateTokenPanel, which runs every frame for selected/focused tokens only
     */
    private void OnStatusChange() {
        Color c = Conditions.ContainsKey("Turn Ended") ? ColorUtility.ColorFromHex("#505050") : Color.white;
        UnitBarElement.Q<VisualElement>("Portrait").style.unityBackgroundImageTintColor = c;
        RedrawConditions();
    }

    public override void UpdateTokenPanel(string elementName) {
        base.UpdateTokenPanel(elementName);
        VisualElement panel = UI.System.Q(elementName);

        Color c = UnitColor();
        panel.Q("ClassBackground").style.borderTopColor = c;
        panel.Q("ClassBackground").style.borderRightColor = c;
        panel.Q("ClassBackground").style.borderBottomColor = c;
        panel.Q("ClassBackground").style.borderLeftColor = c;

        panel.Q<Label>("Class").text = Class;
        panel.Q<Label>("Class").style.backgroundColor = c;
        panel.Q<Label>("Job").text = Job;
        panel.Q<Label>("Job").style.backgroundColor = c;

        UI.ToggleDisplay(panel.Q("Elite"), Elite);
        panel.Q("Elite").style.backgroundColor = new Color(.79f, .33f, .94f);

        panel.Q<Label>("CHP").text = $"{ CurrentHP }";
        panel.Q<Label>("MHP").text = $"/{ MaxHP }";
        panel.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q<ProgressBar>("HpBar").highValue = MaxHP;
        

        panel.Q<Label>("VIG").text = $"+{ Vigor }";
        UI.ToggleDisplay(panel.Q("VIG"), Vigor > 0);
        panel.Q<ProgressBar>("VigorBar").value = Vigor;
        panel.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(panel.Q("VigorBar"), Vigor > 0);

        UI.ToggleDisplay(panel.Q("Wound1"), Wounds >= 1);
        UI.ToggleDisplay(panel.Q("Wound2"), Wounds >= 2);
        UI.ToggleDisplay(panel.Q("Wound3"), Wounds >= 3);

        panel.Q<Label>("ResolveNum").text = $"{ Resolve }";
        panel.Q<ProgressBar>("ResolveBar").value = Resolve + Icon_v1_5.PartyResolve;
        
        panel.Q<Label>("PartyResolveNum").text = $"+{ Icon_v1_5.PartyResolve }";
        UI.ToggleDisplay(panel.Q<Label>("PartyResolveNum"), Icon_v1_5.PartyResolve > 0);
        panel.Q<ProgressBar>("PartyResolveBar").value = Icon_v1_5.PartyResolve;

        panel.Q("Damage").Q<Label>("Value").text = $"{ Damage }/{ Fray }";
        panel.Q("Range").Q<Label>("Value").text = $"{ Range }";
        panel.Q("Speed").Q<Label>("Value").text = $"{ Speed }/{ Dash }";
        panel.Q("Defense").Q<Label>("Value").text = $"{ Defense }";

        if (RedrawConditionsElement) {
            panel.Q("Conditions").Q("List").Clear();
            panel.Q("Conditions").Q("List").Add(ConditionsElement);
            RedrawConditionsElement = false;
        }
    }


}
