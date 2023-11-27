using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using System.Data.Common;
using IsoconUILibrary;

public class Icon_v1_5 : GameSystem
{
    public int TurnNumber = 1;
    public int PartyResolve = 0;


    public override string SystemName()
    {
        return "ICON 1.5";
    }

    public override string GetTokenData() {
        return Icon_v1_5TokenDataRaw.ToJson();
    }

    public override void TokenDataSetValue(TokenData data, string label, int value)
    {
        (data as Icon_v1_5TokenData).Change(label, value);
    }

    public override void TokenDataSetValue(TokenData data, string label, string value)
    {
        (data as Icon_v1_5TokenData).Change(label, value);
    }

    public override void GameDataSetValue(string label, int value) {
        // switch (label) {
        //     case "TurnNumber":
        //         TurnNumber = value;
        //         UI.System.Q<Label>("TurnNumber").text = $"Turn {TurnNumber}";
        //         foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
        //             Icon_v1_5TokenData data = g.GetComponent<Icon_v1_5TokenData>();
        //             if (data.CheckCondition("TurnEnded")) {
        //                 data.Change("Status", "Turn Ended|neu");
        //             }
        //         }
        //         break;
        //     case "PartyResolve":
        //         PartyResolve = value;
        //         Token selected = TokenController.GetSelected();
        //         if (selected != null) {
        //             TokenData selectedData = selected.onlineDataObject.GetComponent<TokenData>();
        //             Player.Self().CmdRequestTokenDataSetValue(selectedData, "PartyResolve", PartyResolve);
        //         }
        //         break;
        // }
    }

    public override Texture2D GetGraphic(string json) {
        Icon_v1_5TokenDataRaw raw = JsonUtility.FromJson<Icon_v1_5TokenDataRaw>(json);
        return TextureSender.LoadImageFromFile(raw.GraphicHash, true);
    }

    public override void TokenDataSetup(GameObject g, string json, string id) {
        g.GetComponent<Icon_v1_5TokenData>().TokenDataSetup(json, id);
    }

    public override GameObject GetDataPrefab() {
        return Instantiate(Resources.Load<GameObject>("Prefabs/Icon_v1_5TokenData"));
    }

    public override void Setup() {
        AddTokenSetup();
        EditTokenSetup();
        SetJobOptions("Stalwart");
        UI.ToggleDisplay("Icon1_5TurnInfo", true);
        UI.ToggleDisplay("AlterHPMenuItem", true);
        UI.ToggleDisplay(UI.System.Q("IconV1_5Stats"), true);
        UI.System.Q<Button>("NewTurnButton").RegisterCallback<ClickEvent>((evt) => {
            Player.Self().CmdRequestGameDataSetValue("TurnNumber", TurnNumber+1);
            Player.Self().CmdRequestGameDataSetValue("PartyResolve", PartyResolve+1);
        });
    }

    public override void Teardown() {
        AddTokenTeardown();
        UI.ToggleDisplay(UI.System.Q("Icon1_5EditPanel"), false);
        UI.ToggleDisplay(UI.System.Q("IconV1_5Stats"), false);
        UI.ToggleDisplay("AlterHPMenuItem", false);
    }

    public override void SyncEditValues(TokenData data)
    {
        Icon_v1_5TokenData Data = data as Icon_v1_5TokenData;
        UI.System.Q<Label>("e_CurrentHP").text = $"{Data.CurrentHP}";
        UI.System.Q<SliderInt>("e_CurrentHPSlider").highValue = Data.MaxHP;
        UI.System.Q<SliderInt>("e_CurrentHPSlider").value = Data.CurrentHP;

        UI.System.Q<Label>("e_Vigor").text = $"{Data.Vigor}";
        UI.System.Q<SliderInt>("e_VigorSlider").highValue = Data.MaxHP;
        UI.System.Q<SliderInt>("e_VigorSlider").value = Data.Vigor;

        UI.System.Q<NumberNudger>("e_Wounds").SetValueWithoutNotify(Data.Wounds);
        UI.System.Q<NumberNudger>("e_Resolve").SetValueWithoutNotify(Data.Resolve);
        UI.System.Q<NumberNudger>("e_PartyResolve").SetValueWithoutNotify((GameSystem.Current() as Icon_v1_5).PartyResolve);
        UI.System.Q<NumberNudger>("e_Aether").SetValueWithoutNotify(Data.Aether);
        UI.System.Q<NumberNudger>("e_Vigilance").SetValueWithoutNotify(Data.Vigilance);
        UI.System.Q<NumberNudger>("e_Blessings").SetValueWithoutNotify(Data.Blessings);

        UI.System.Q<Toggle>("e_StackedDie").SetValueWithoutNotify(Data.StatusesToString().Contains("Stacked Die"));
        UI.System.Q<TextField>("e_Marked").SetValueWithoutNotify(Data.Marked);
        UI.System.Q<TextField>("e_Marked").SetValueWithoutNotify(Data.Hatred);
        UI.System.Q<DropdownField>("e_Stance").SetValueWithoutNotify(Data.Stance);
    }

    public override void UpdateTokenPanel(GameObject data, string elementName)
    {
        // data.GetComponent<Icon_v1_5TokenData>().UpdateSelectedTokenPanel();
    }

    public override string GetEditPanelName()
    {
        return "Icon1_5EditPanel";
    }

    private void AddTokenSetup() {
        VisualElement root = UI.System.Q("AddTokenSystem").Q("Icon_v1_5");
        UI.ToggleDisplay(root, true);
        List<string> classes = GetClasses();
        root.Q<DropdownField>("ClassDropdown").choices = classes;
        root.Q<DropdownField>("ClassDropdown").value = classes[0];
        root.Q<DropdownField>("ClassDropdown").RegisterValueChangedCallback(SetJobOptions);
    }

    private void AddTokenTeardown() {
        foreach(VisualElement child in UI.System.Q("AddTokenSystem").Children()) {
            UI.ToggleDisplay(child, false);
        }
        VisualElement root = UI.System.Q("AddTokenSystem");
        root.Q<DropdownField>("ClassDropdown").UnregisterValueChangedCallback(SetJobOptions);
    }

    private void EditTokenSetup() {

        VisualElement panel = UI.System.Q("Icon1_5EditPanel");
        
        #region HP
        panel.Q<SliderInt>("e_CurrentHPSlider").RegisterValueChangedCallback((evt) => {
            panel.Q<Label>("e_CurrentHP").text = evt.newValue.ToString();
        });
        panel.Q<SliderInt>("e_CurrentHPSlider").RegisterCallback<MouseEnterEvent>((evt) => {
            TokenEditPanel.HPOld = panel.Q<SliderInt>("e_CurrentHPSlider").value;
        });
        panel.Q<SliderInt>("e_CurrentHPSlider").RegisterCallback<MouseLeaveEvent>((evt) => {
            int HPNew = panel.Q<SliderInt>("e_CurrentHPSlider").value;
            int HPDiff = -(TokenEditPanel.HPOld - HPNew);
            if (HPDiff != 0) {
                Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "CurrentHP", HPNew);
            }
        });
        #endregion

        #region Vigor
        panel.Q<SliderInt>("e_VigorSlider").RegisterValueChangedCallback((evt) => {
            panel.Q<Label>("e_Vigor").text = evt.newValue.ToString();
        });
        panel.Q<SliderInt>("e_VigorSlider").RegisterCallback<MouseEnterEvent>((evt) => {
            TokenEditPanel.HPOld = panel.Q<SliderInt>("e_VigorSlider").value;
        });
        panel.Q<SliderInt>("e_VigorSlider").RegisterCallback<MouseLeaveEvent>((evt) => {
            int HPNew = panel.Q<SliderInt>("e_VigorSlider").value;
            int HPDiff = -(TokenEditPanel.HPOld - HPNew);
            if (HPDiff != 0) {
                Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Vigor", HPNew);
            }
        });        
        #endregion

        #region Wounds
        panel.Q<NumberNudger>("e_Wounds").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Wounds", Math.Clamp(evt, 0, 4));
        });
        #endregion

        #region Resolve
        panel.Q<NumberNudger>("e_Resolve").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Resolve", evt);
        });
        panel.Q<NumberNudger>("e_PartyResolve").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestGameDataSetValue("PartyResolve", evt);
            // Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "PartyResolve", evt);
        });        
        #endregion

        #region ClassFeatures
        panel.Q<NumberNudger>("e_Aether").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Aether", evt);
        });
        panel.Q<NumberNudger>("e_Vigilance").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Vigilance", evt);
        }); 
        panel.Q<NumberNudger>("e_Blessings").AddValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Blessings", evt);
        });
        panel.Q<Toggle>("e_StackedDie").RegisterValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Status", "Stacked Die|pos");
        });        
        panel.Q<DropdownField>("e_Stance").RegisterValueChangedCallback((evt) => {
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Stance", evt.newValue);
        });
        #endregion

        #region Status
        panel.Q<Button>("ToggleBuffButton").RegisterCallback<ClickEvent>((evt) => {
            string status = panel.Q<DropdownField>("e_BuffDropdown").value;
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Status", $"{status}|pos");
        });

        panel.Q<Button>("ToggleDebuffButton").RegisterCallback<ClickEvent>((evt) => {
            string status = panel.Q<DropdownField>("e_DebuffDropdown").value;
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Status", $"{status}|neg");
        });

        panel.Q<TextField>("e_Marked").RegisterCallback<BlurEvent>((evt) => {
            string s = panel.Q<TextField>("e_Marked").value;
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Marked", s);
        });

        panel.Q<TextField>("e_Hatred").RegisterCallback<BlurEvent>((evt) => {
            string s = panel.Q<TextField>("e_Hatred").value;
            Player.Self().CmdRequestTokenDataSetValue(TokenEditPanel.Data, "Hatred", s);
        });
        #endregion
    }

    private void SetJobOptions(ChangeEvent<string> evt) {
        SetJobOptions(evt.newValue);
    }

    private void SetJobOptions(string jclass) {
        List<string> jobs = GetJobs(jclass);
        UI.System.Q<DropdownField>("JobDropdown").choices = jobs;
        UI.System.Q<DropdownField>("JobDropdown").value = jobs[0];

        UI.ToggleDisplay("EliteToggle", IsFoe(jclass));
        UI.ToggleDisplay("SizeDropdown", IsFoe(jclass) || jclass == "Object");
        UI.ToggleDisplay("LegendHPDropdown", jclass == "Legend");
        UI.ToggleDisplay("ObjectHPField", jclass == "Object");
    }

    private static bool IsFoe(string jclass) {
        return jclass switch
        {
            "Stalwart" or "Wright" or "Mendicant" or "Vagabond" or "Object" => false,
            _ => true,
        };
    }

    private List<string> GetClasses() {
        return new string[]{"Stalwart", "Vagabond", "Mendicant", "Wright", "Heavy", "Skirmisher","Leader","Artillery","Legend","Mob","Object"}.ToList();
    }

    private List<string> GetJobs(string jclass) {
        return jclass switch
        {
            "Stalwart" => new string[] { "Bastion", "Demon Slayer", "Knave", "Colossus" }.ToList(),
            "Vagabond" => new string[] { "Shade", "Freelancer", "Fool", "Warden" }.ToList(),
            "Mendicant" => new string[] { "Seer", "Chanter", "Sealer", "Harvester" }.ToList(),
            "Wright" => new string[] { "Enochian", "Geomancer", "Spellblade", "Stormbender" }.ToList(),
            "Heavy" => new string[] { "Warrior", "Soldier", "Impaler", "Greatsword", "Brute", "Knuckle", "Sentinel", "Crusher", "Berserker", "Sledge" }.ToList(),
            "Skirmisher" => new string[] { "Pepperbox", "Hunter", "Fencer", "Assassin", "Hellion", "Skulk", "Shadow", "Arsonist" }.ToList(),
            "Leader" => new string[] { "Errant", "Priest", "Commander", "Aburer", "Diviner", "Greenseer", "Judge", "Saint", "Cantrix" }.ToList(),
            "Artillery" => new string[] { "Blaster", "Seismatist", "Storm Caller", "Rift Dancer", "Disruptor", "Chaos Wright", "Scourer", "Sapper", "Justicar", "Sniper", "Alchemist" }.ToList(),
            "Legend" => new string[] { "Demolisher", "Nocturnal", "Master", "Razer" }.ToList(),
            "Mob" => new string[] { "Mob" }.ToList(),
            "Object" => new string[] { "Destructible" }.ToList(),
            _ => new string[] { }.ToList(),
        };
    }

}
