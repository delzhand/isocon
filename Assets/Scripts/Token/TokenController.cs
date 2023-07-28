using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using System.Linq;
using IsoconUILibrary;

public class TokenController : MonoBehaviour
{
    private static Token selected = null;
    public static Vector3 Size2Offset = new Vector3(0, 0, -.73f);

    // Start is called before the first frame update
    void Start()
    {
        registerCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void registerCallbacks() {

        List<string> classes = getClasses();
        UI.System.Q<DropdownField>("ClassDropdown").choices = classes;
        UI.System.Q<DropdownField>("ClassDropdown").value = classes[0];

        UI.System.Q("AddTokenConfirmButton").RegisterCallback<ClickEvent>((evt) => {
            ReserveController.Adjust();
            CreateNew();
            DisableAddToken();
        });

        UI.System.Q("AddTokenCancelButton").RegisterCallback<ClickEvent>((evt) => {
            DisableAddToken();
        });

        UI.System.Q<DropdownField>("ClassDropdown").RegisterValueChangedCallback<string>((evt) => {
            setJobOptions(evt.newValue);
        });

        UI.System.Q("TokenEditToggle").RegisterCallback<ClickEvent>((evt) => {
            if(UI.System.Q<SlideToggle>("TokenEditToggle").value) {
                EnableFullEdit();
            }
            else {
                DisableFullEdit();
            }
        });

        registerTokenEditCallbacks();

        UI.SetBlocking(UI.System, new string[]{"TokenOptions", "SelectedTokenPanel", "EditTokenPanel"});
    }

    private void registerTokenEditCallbacks() {
        VisualElement panel = UI.System.Q("EditTokenPanel");

        panel.Q<SliderInt>("e_CurrentHPSlider").RegisterValueChangedCallback<int>((evt) => {
            selected.GetComponent<TokenState>().CurrentHP = evt.newValue;
        });

        panel.Q<SliderInt>("e_VigorSlider").RegisterValueChangedCallback<int>((evt) => {
            selected.GetComponent<TokenState>().Vigor = evt.newValue;
        });

        panel.Q<NumberNudger>("e_Wounds").AddValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().Wounds = Math.Clamp(evt, 0, 3);
        });

        panel.Q<NumberNudger>("e_Resolve").AddValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().Resolve = Math.Clamp(evt, 0, 30);
        });

        panel.Q<NumberNudger>("e_GResolve").AddValueChangedCallback((evt) => {
            TokenState.GResolve = Math.Clamp(evt, 0, 30);
        });

        panel.Q<NumberNudger>("e_Aether").AddValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().Aether = Math.Clamp(evt, 0, 30);
        });

        panel.Q<NumberNudger>("e_Blessings").AddValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().Blessings = Math.Clamp(evt, 0, 30);
        });

        panel.Q<NumberNudger>("e_Vigilance").AddValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().Vigilance = Math.Clamp(evt, 0, 30);
        });

        panel.Q<Toggle>("e_StackedDie").RegisterValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().StackedDie = evt.newValue;
        });

        panel.Q<DropdownField>("e_Stance").RegisterValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().Stance = evt.newValue;
        });

        List<string> toggles = new List<string>(){"Counter", "Defiance", "Dodge", "Evasion", "Flying", "Phasing", "Stealth", "Sturdy", "Unstoppable", "Regeneration"};
        for(int i = 0; i < toggles.Count; i++) {
            panel.Q<Toggle>("e_" + toggles[i]).RegisterValueChangedCallback((evt) => {
                Toggle t = evt.target as Toggle;
                string status = t.name.Replace("e_", "");
                TokenState ts = selected.GetComponent<TokenState>();
                if (evt.newValue) {
                    ts.Status.Add(status);
                }
                else {
                    ts.Status.Remove(status);
                }
            });
        }

        List<string> doubleToggles = new List<string>(){"Blind", "Dazed", "Pacified", "Sealed", "Shattered", "Slashed", "Stunned", "Weakened", "Vulnerable"};
        for(int i = 0; i < doubleToggles.Count; i++) {
            panel.Q<DoubleToggle>("e_" + doubleToggles[i]).AddValueChangedCallback((evt) => {
                DoubleToggle t = evt.target as DoubleToggle;
                string status = t.name.Replace("e_", "");
                TokenState ts = selected.GetComponent<TokenState>();
                if (evt.newValue.Item1) {
                    ts.SetStatus(status);
                }
                else {
                    ts.DropStatus(status);
                }
                if (evt.newValue.Item2) {
                    ts.SetStatus(status + "+");
                }
                else {
                    ts.DropStatus(status + "+");
                }
            });
        }

        panel.Q<TextField>("e_Marked").RegisterValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().Mark = evt.newValue;
        });

        panel.Q<TextField>("e_Hatred").RegisterValueChangedCallback((evt) => {
            selected.GetComponent<TokenState>().Hate = evt.newValue;
        });

        panel.Q<TextField>("e_Name").RegisterValueChangedCallback((evt) => {
            selected.name = evt.newValue;
        });

        panel.Q<Button>("e_Duplicate").RegisterCallback<ClickEvent>((evt) => {
            GameObject clone = GameObject.Instantiate(selected.gameObject);
            ReserveSpot openReserve = ReserveSpot.LastReserveSpot();
            openReserve.PlaceAtReserveSpot(clone.GetComponent<Token>());
        });

        panel.Q<Button>("e_Delete").RegisterCallback<ClickEvent>((evt) => {
            GameObject.Destroy(selected.gameObject);
            Deselect();
        });

    }

    private void setJobOptions(string class_) {
        List<string> jobs = getJobs(class_);
        UI.System.Q<DropdownField>("JobDropdown").choices = jobs;
        UI.System.Q<DropdownField>("JobDropdown").value = jobs[0];

        if (IsFoe(class_)) {
            UI.System.Q("EliteToggle").style.display = DisplayStyle.Flex;
            UI.System.Q("SizeDropdown").style.display = DisplayStyle.Flex;
        }
        else {
            UI.System.Q("EliteToggle").style.display = DisplayStyle.None;
            UI.System.Q("SizeDropdown").style.display = DisplayStyle.None;
        }

        if (class_ == "Legend") {
            UI.System.Q("LegendHPDropdown").style.display = DisplayStyle.Flex;
        }
        else {
            UI.System.Q("LegendHPDropdown").style.display = DisplayStyle.None;
        }
    }

    public static void CheckCustomTokens() {
        List<string> avatars = new List<string>{};
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/tokens/");
        if (info.Exists) {
            FileInfo[] fileInfo = info.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++) {
                avatars.Add(fileInfo[i].Name);
                if (i == 0) {
                    UI.System.Q<DropdownField>("GraphicDropdown").value = fileInfo[i].Name;
                }
            }        
        }
        UI.System.Q<DropdownField>("GraphicDropdown").choices = avatars;        
    }

    public static bool IsFoe(string class_) {
        switch (class_) {
            case "Stalwart":
            case "Wright":
            case "Mendicant":
            case "Vagabond":
                return false;
        }
        return true;
    }

    private List<string> getClasses() {
        string version = PlayerPrefs.GetString("IconVersion", "1.5");
        switch (version) {
            case "1.5":
                return new string[]{"Stalwart", "Vagabond", "Mendicant", "Wright", "Heavy", "Skirmisher","Leader","Artillery","Legend","Mob"}.ToList();
        }
        throw new Exception("Invalid data version");
    }

    private List<string> getJobs(string class_) {
        string version = PlayerPrefs.GetString("IconVersion", "1.5");
        switch (version) {
            case "1.5":
                switch (class_) {
                    case "Stalwart":
                        return new string[]{"Bastion","Demon Slayer","Knave","Colossus"}.ToList();
                    case "Vagabond":
                        return new string[]{"Shade","Freelancer","Fool","Warden"}.ToList();
                    case "Mendicant":
                        return new string[]{"Seer","Chanter","Sealer","Harvester"}.ToList();
                    case "Wright":
                        return new string[]{"Enochian","Geomancer","Spellblade","Stormbender"}.ToList();
                    case "Heavy":
                        return new string[]{"Warrior","Soldier","Impaler","Greatsword","Brute","Knuckle","Sentinel","Crusher","Berserker","Sledge"}.ToList();
                    case "Skirmisher":
                        return new string[]{"Pepperbox","Hunter","Fencer","Assassin","Hellion","Skulk","Shadow","Arsonist"}.ToList();
                    case "Leader":
                        return new string[]{"Errant","Priest","Commander","Aburer","Diviner","Greenseer","Judge","Saint","Cantrix"}.ToList();
                    case "Artillery":
                        return new string[]{"Blaster","Seismatist","Storm Caller","Rift Dancer","Disruptor","Chaos Wright","Scourer","Sapper","Justicar","Sniper","Alchemist"}.ToList();
                    case "Legend":
                        return new string[]{"Demolisher","Nocturnal","Master","Razer"}.ToList();
                    case "Mob":
                        return new string[]{"Mob"}.ToList();
                }
                throw new Exception("Invalid class");
        }
        throw new Exception("Invalid data version");
    }

    public static bool IsPositive(string status) {
        status = status.Replace("+", "");
        return getPositiveStatuses().Contains(status);
    }

    public static bool IsNegative(string status) {
        status = status.Replace("+", "");
        return getNegativeStatuses().Contains(status);
    }

    private static List<string> getPositiveStatuses() {
        string version = PlayerPrefs.GetString("IconVersion", "1.5");
        switch (version) {
            case "1.5":
                return new List<string>{"Counter", "Defiance", "Dodge", "Evasion", "Flying", "Phasing", "Stealth", "Sturdy", "Unstoppable", "Regeneration"};
        }
        throw new Exception("Invalid data version");
    }

    private static List<string> getNegativeStatuses() {
        string version = PlayerPrefs.GetString("IconVersion", "1.5");
        switch (version) {
            case "1.5":
                return new List<string>{"Slashed", "Blind", "Dazed", "Pacified", "Sealed", "Shattered", "Stunned", "Weakened", "Vulnerable"};
        }
        throw new Exception("Invalid data version");
    }

    public static void SetStats(TokenState tstate, bool elite = false, int legendHPx = 1) {
        string version = PlayerPrefs.GetString("IconVersion", "1.5");
        switch (version) {
            case "1.5":
                switch(tstate.Class_) {
                    case "Wright":
                    case "Artillery":
                        tstate.Color = "blue";
                        tstate.MaxHP = 32;
                        tstate.Damage = 8;
                        tstate.Fray = 3;
                        tstate.Range = 6;
                        tstate.Speed = 4;
                        tstate.Dash = 2;
                        tstate.Defense = 7;
                        break;
                    case "Vagabond":
                    case "Skirmisher":
                        tstate.Color = "yellow";
                        tstate.MaxHP = 28;
                        tstate.Damage = 10;
                        tstate.Fray = 2;
                        tstate.Range = 4;
                        tstate.Speed = 4;
                        tstate.Dash = 4;
                        tstate.Defense = 10;
                        break;
                    case "Stalwart":
                    case "Heavy":
                        tstate.Color = "red";
                        tstate.MaxHP = 40;
                        tstate.Damage = 6;
                        tstate.Fray = 4;
                        tstate.Range = 3;
                        tstate.Speed = 4;
                        tstate.Dash = 2;
                        tstate.Defense = 6;
                        break;
                    case "Leader":
                    case "Mendicant":
                        tstate.Color = "green";
                        tstate.MaxHP = 40;
                        tstate.Damage = 6;
                        tstate.Fray = 3;
                        tstate.Range = 3;
                        tstate.Speed = 4;
                        tstate.Dash = 2;
                        tstate.Defense = 8;
                        break;
                    case "Legend":
                        tstate.Color = "purple";
                        tstate.MaxHP = 50 * legendHPx;
                        tstate.Damage = 8;
                        tstate.Fray = 3;
                        tstate.Range = 3;
                        tstate.Speed = 4;
                        tstate.Dash = 2;
                        tstate.Defense = 8;
                        break;
                    case "Mob":
                        tstate.Color = "gray";
                        tstate.MaxHP = 2;
                        tstate.Damage = 6;
                        tstate.Fray = 3;
                        tstate.Range = 1;
                        tstate.Speed = 4;
                        tstate.Dash = 2;
                        tstate.Defense = 8;
                        break;
                    default:
                        throw new Exception("Invalid class");
                }
                if (elite) {
                    tstate.MaxHP *= 2;
                }
                else {
                    tstate.MaxHP *= legendHPx;
                }
                tstate.CurrentHP = tstate.MaxHP;
                tstate.Vigor = 0;
                tstate.Wounds = 0;
                break;
            default:
                throw new Exception("Invalid data version");
        }
    }

    public static void CreateNew() {
        TextField nameField = UI.System.Q<TextField>("TokenNameField");
        DropdownField graphicField = UI.System.Q<DropdownField>("GraphicDropdown");
        DropdownField classField = UI.System.Q<DropdownField>("ClassDropdown");
        DropdownField jobField = UI.System.Q<DropdownField>("JobDropdown");
        Toggle eliteField = UI.System.Q<Toggle>("EliteToggle");
        IntegerField legendScale = UI.System.Q<IntegerField>("LegendHPDropdown");
        DropdownField sizeField = UI.System.Q<DropdownField>("SizeDropdown");

        GameObject newToken = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        newToken.name = nameField.value;
        newToken.GetComponent<Token>().CustomCutout("file://" + Application.persistentDataPath + "/tokens/" + graphicField.value);
        newToken.GetComponent<Token>().Deselect();

        TokenState tokenState = newToken.AddComponent<TokenState>();
        tokenState.Job = jobField.value;
        tokenState.Class_ = classField.value;
        tokenState.Elite = eliteField.value;
        SetStats(tokenState);

        if (sizeField.value == "Large (2)") {
            newToken.GetComponent<Token>().Size = 2;
            newToken.transform.Find("Offset").transform.localPosition += Size2Offset;
            newToken.transform.Find("Base").transform.localPosition += Size2Offset;
            newToken.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
            newToken.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2, 2, 4);
        }
        else if (sizeField.value == "Huge (3)") {
            newToken.GetComponent<Token>().Size = 3;
            newToken.transform.Find("Offset").transform.localScale = new Vector3(3, 3, 3);
            newToken.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(3, 3, 4);
        }

        Material m = Instantiate(Resources.Load<Material>("Materials/Token/BorderBase"));
        switch (tokenState.Color) {
            case "red":
                m.SetColor("_Border", new Color(.93f, .13f, .05f));
                break;
            case "blue":
                m.SetColor("_Border", new Color(0, .63f, 1));
                break;
            case "yellow":
                m.SetColor("_Border", new Color(1, .68f, 0));
                break;
            case "green":
                m.SetColor("_Border", new Color(.38f, .85f, .21f));
                break;
            case "purple":
                m.SetColor("_Border", new Color(.79f, .33f, .94f));
                break;
            case "gray":
                m.SetColor("_Border", new Color(.57f, .57f, .57f));
                break;
        }
        newToken.transform.Find("Base").GetComponent<DecalProjector>().material = m;
        newToken.transform.Find("Base").gameObject.SetActive(false);

        ReserveSpot openReserve = ReserveSpot.LastReserveSpot();
        openReserve.PlaceAtReserveSpot(newToken.GetComponent<Token>());
    }

    public static void TokenClick(Token token) {
        if (token == null) {
            selected = null;
            GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
            for (int i = 0; i < tokens.Length; i++) {
                tokens[i].GetComponent<Token>().Deselect();
            }
        }
        else if (token == selected) {
            Deselect();
        }
        else {
            if (selected != null) {
                Deselect();
            }
            Select(token);
        }

    }

    public static void BlockClick(Block block) {
        if (selected != null) {
            selected.PlaceAtBlock(block);
            ReserveController.Adjust();
        }
        else {
            CameraControl.GoToBlock(block);
            // UI.System.Q("TerrainInfo").style.display = DisplayStyle.Flex;
            block.SetTerrainInfo();
            Block.DeselectAll();
            block.Select();
        }
    }

    public static void ReserveSpotClick(ReserveSpot spot) {
        if (UI.ClicksSuspended) {
            return;
        }
        if (selected != null && !selected.InReserve) {
            spot.PlaceAtReserveSpot(selected);
        }
        else if (selected == null && spot == ReserveSpot.LastReserveSpot()) {
            EnableAddToken();
        }
    }

    public static bool IsSelected(Token token)
    {
        return selected == token;
    }

    private static void Select(Token token) {
        token.Select();
        selected = token;
        UI.System.Q("SelectedTokenPanel").AddToClassList("active");
        SetEditPanelValues();
    }

    public static void Deselect() {
        if (selected) {
            selected.Deselect();
            selected = null;
        }
        UI.System.Q("SelectedTokenPanel").RemoveFromClassList("active");
        UI.System.Q<SlideToggle>("TokenEditToggle").value = false;
        DisableFullEdit();
    }

    public static void EnableAddToken() {
        TokenController.CheckCustomTokens();
        GameObject.Find("Engine").GetComponent<MenuController>().EnableModal("AddTokenDialog");
        UI.System.Q<TextField>("TokenNameField").value = "Token Name";
        UI.System.Q<DropdownField>("SizeDropdown").value = "Normal";
        UI.System.Q<DropdownField>("LegendHPDropdown").value = "x1";
        UI.System.Q<Toggle>("EliteToggle").value = false;
        UI.SetHardSuspend(true);
    }

    public static void EnableFullEdit() {
        UI.System.Q("EditTokenPanel").AddToClassList("active");
    }

    private static void SetEditPanelValues() {
        TokenState t = selected.GetComponent<TokenState>();
        UI.System.Q<SliderInt>("e_CurrentHPSlider").highValue = t.MaxHP;
        UI.System.Q<SliderInt>("e_CurrentHPSlider").value = t.CurrentHP;
        UI.System.Q<SliderInt>("e_VigorSlider").highValue = t.MaxHP;
        UI.System.Q<SliderInt>("e_VigorSlider").value = t.Vigor;
        UI.System.Q<NumberNudger>("e_Wounds").value = t.Wounds;
        UI.System.Q<NumberNudger>("e_Aether").value = t.Aether;
        UI.System.Q<NumberNudger>("e_Vigilance").value = t.Vigilance;
        UI.System.Q<Toggle>("e_StackedDie").value = t.StackedDie;
        UI.System.Q<DropdownField>("e_Stance").value = t.Stance;
        UI.System.Q<NumberNudger>("e_Blessings").value = t.Blessings;
    }

    public static void DisableAddToken() {
        GameObject.Find("Engine").GetComponent<MenuController>().Clear();
        UI.SetHardSuspend(false);
    }

    public static void DisableFullEdit() {
        UI.System.Q("EditTokenPanel").RemoveFromClassList("active");
    }
}
