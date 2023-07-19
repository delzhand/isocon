using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using System.Linq;

public class TokenController : MonoBehaviour
{
    private static Token held = null;
    public static Vector3 Size2Offset = new Vector3(0, 0, -.73f);

    // Start is called before the first frame update
    void Start()
    {
        // UI.GameInfo.Q<Button>("CloneButton").RegisterCallback<ClickEvent>((evt) => {
        //     GameObject newToken = GameObject.Instantiate(TokenController.held.gameObject);
        //     newToken.name = TokenController.held.name;
        //     ReserveSpot openReserve = ReserveSpot.LastReserveSpot();
        //     openReserve.PlaceAtReserveSpot(newToken.GetComponent<Token>());
        // });

        // UI.GameInfo.Q<Button>("DeleteButton").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<HpBar>().DestroyFloater();
        //     GameObject.Destroy(TokenController.held.gameObject);
        //     TokenController.DropHeld();
        //     ReserveController.Adjust();
        // });

        // UI.GameInfo.Q<Button>("CloseButton").RegisterCallback<ClickEvent>((evt) => {
        //     DisableFullEdit();
        // });

        // InitAddModal();


        // UI.GameInfo.Q<SliderInt>("HpSlider").RegisterValueChangedCallback<int>((evt) => {
        //     held.GetComponent<HpBar>().CHP = evt.newValue;
        // });

        // UI.SetBlocking(UI.GameInfo, new string[]{"QuickHP", "ModifyPane", "FocusToken"});

        // InitEditPaneCallbacks();

        registerCallbacks();   
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void registerCallbacks() {
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
    }

    private void setJobOptions(string class_) {
        List<string> jobs = getJobs(class_);
        UI.System.Q<DropdownField>("ClassDropdown").choices = getJobs(class_);
        UI.System.Q<DropdownField>("ClassDropdown").value = jobs[0];
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

    private List<string> getClasses() {
        string version = PlayerPrefs.GetString("IconVersion", "1.5");
        switch (version) {
            case "1.5":
                return new string[]{"Stalwart", "Vagabond", "Mendicant", "Wright", "Heavy", "Skirmisher","Leader","Artillery","Legend","Mob"}.ToList();
        }
        throw new Exception("Invalid data version");
    }

    private string getDefaultClass() {
        string version = PlayerPrefs.GetString("IconVersion", "1.5");
        switch (version) {
            case "1.5":
                return "Stalwart";
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
                }
                throw new Exception("Invalid class");
        }
        throw new Exception("Invalid data version");
    }

    public void InitAddModal() {
        List<string> avatars = new List<string>{};
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/tokens/");
        if (info.Exists) {
            FileInfo[] fileInfo = info.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++) {
                avatars.Add(fileInfo[i].Name);
                if (i == 0) {
                    UI.System.Q<DropdownField>("AvatarDropdown").value = fileInfo[i].Name;
                }
            }        
        }
        UI.System.Q<DropdownField>("AvatarDropdown").choices = avatars;

        UI.System.Q<DropdownField>("ClassDropdown").choices = getClasses();
        UI.System.Q<DropdownField>("ClassDropdown").value = getDefaultClass();

        // UI.System.Q<DropdownField>("TokenTypeDropdown").choices = new List<string>{
        //     "Player",
        //     "Foe",
        // };
        // UI.System.Q<DropdownField>("TokenTypeDropdown").value = "Player";

        // UI.System.Q<DropdownField>("SizeDropdown").choices = new List<string>{
        //     "1-Standard",
        //     "2-Large",
        //     "3-Enormous",
        // };
        // UI.System.Q<DropdownField>("SizeDropdown").value = "1-Standard";

        // UI.System.Q<DropdownField>("JobClassDropdown").choices = jobOptions();
        // UI.System.Q<DropdownField>("JobClassDropdown").value = defaultJobOption();
    }

    private static List<string> jobOptions() {
        string v = PlayerPrefs.GetString("IconVersion", "1.5");
        switch(v) {
            case "1.5":
                return new List<string>{
                    "Stalwart-Bastion",
                    "Stalwart-Demon Slayer",
                    "Stalwart-Colossus",
                    "Stalwart-Knave",
                    "Vagabond-Fool",
                    "Vagabond-Freelancer",
                    "Vagabond-Shade",
                    "Vagabond-Warden",
                    "Mendicant-Chanter",
                    "Mendicant-Harvester",
                    "Mendicant-Sealer",
                    "Mendicant-Seer",
                    "Wright-Enochian",
                    "Wright-Geomancer",
                    "Wright-Spellblade",
                    "Wright-Stormbender",
                    "Heavy",
                    "Skirmisher",
                    "Leader",
                    "Artillery",
                    "Mob",
                };
            default:
                return new List<string>{};
        }
    }

    private static string defaultJobOption() {
        string v = PlayerPrefs.GetString("IconVersion", "1.5");
        switch(v) {
            case "1.5":
                return "Stalwart-Bastion";
            default:
                return null;
        }
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
        
        // Toggle eliteField = UI.System.Q<Toggle>("EliteCheckbox");
        // IntegerField legendScale = UI.System.Q<IntegerField>("LegendScale");
        // DropdownField sizeField = UI.System.Q<DropdownField>("SizeDropdown");

        GameObject newToken = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        newToken.name = nameField.value;
        newToken.GetComponent<Token>().CustomCutout("file://" + Application.persistentDataPath + "/tokens/" + graphicField.value);

        TokenState tokenState = newToken.AddComponent<TokenState>();
        tokenState.Job = jobField.value;
        tokenState.Class_ = classField.value;
        SetStats(tokenState);

        // if (sizeField.value == "2-Large") {
        //     newToken.GetComponent<Token>().Size = 2;
        //     newToken.transform.Find("Offset").transform.localPosition += Size2Offset;
        //     newToken.transform.Find("Base").transform.localPosition += Size2Offset;
        //     newToken.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
        //     newToken.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2, 2, 4);
        // }
        // else if (sizeField.value == "3-Enormous") {
        //     newToken.GetComponent<Token>().Size = 3;
        //     newToken.transform.Find("Offset").transform.localScale = new Vector3(3, 3, 3);
        //     newToken.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(3, 3, 4);
        // }

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

    public static void InitEditPaneCallbacks() {
        // VisualElement edit = UI.GameInfo.Q("ModifyPane");

        // edit.Q<TextField>("NameEdit").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.name = evt.newValue;
        // });

        // edit.Q<Button>("CHP-up").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<HpBar>().CHP++;
        // });
        // edit.Q<Button>("CHP-down").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<HpBar>().CHP--;
        // });

        // edit.Q<Button>("VIG-up").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<HpBar>().VIG++;
        // });
        // edit.Q<Button>("VIG-down").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<HpBar>().VIG--;
        // });

        // edit.Q<Button>("RES-up").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Resolve++;
        // });
        // edit.Q<Button>("RES-down").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Resolve--;
        // });

        // edit.Q<Button>("GRES-up").RegisterCallback<ClickEvent>((evt) => {
        //     UnitState.GResolve++;
        // });
        // edit.Q<Button>("GRES-down").RegisterCallback<ClickEvent>((evt) => {
        //     UnitState.GResolve--;
        // });

        // edit.Q<Button>("ATH-up").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Aether++;
        // });
        // edit.Q<Button>("ATH-down").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Aether--;
        // });

        // edit.Q<Button>("VGL-up").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Vigilance++;
        // });
        // edit.Q<Button>("VGL-down").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Vigilance--;
        // });

        // edit.Q<Button>("BLS-up").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Blessings++;
        // });
        // edit.Q<Button>("BLS-down").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Blessings--;
        // });

        // edit.Q<Button>("WND-up").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<HpBar>().Wounds++;
        // });
        // edit.Q<Button>("WND-down").RegisterCallback<ClickEvent>((evt) => {
        //     TokenController.held.GetComponent<HpBar>().Wounds--;
        // });

        // edit.Q<TextField>("HatredEdit").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Hatred = evt.newValue;
        // });

        // edit.Q<TextField>("MarkEdit").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Marked = evt.newValue;
        // });

        // edit.Q<Toggle>("SlashedToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Slashed = evt.newValue;
        // });
        // edit.Q<Toggle>("BlindToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Blind = evt.newValue;
        // });
        // edit.Q<Toggle>("DazedToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Dazed = evt.newValue;
        // });
        // edit.Q<Toggle>("PacifiedToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Pacified = evt.newValue;
        // });
        // edit.Q<Toggle>("SealedToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Sealed = evt.newValue;
        // });
        // edit.Q<Toggle>("ShatteredToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Shattered = evt.newValue;
        // });
        // edit.Q<Toggle>("StunnedToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Stunned = evt.newValue;
        // });
        // edit.Q<Toggle>("WeakenedToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Weakened = evt.newValue;
        // });
        // edit.Q<Toggle>("VulnerableToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Vulnerable = evt.newValue;
        // });
        
        // edit.Q<Toggle>("CounterToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Counter = evt.newValue;
        // });
        // edit.Q<Toggle>("DefianceToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Defiance = evt.newValue;
        // });
        // edit.Q<Toggle>("DodgeToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Dodge = evt.newValue;
        // });
        // edit.Q<Toggle>("EvasionToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Evasion = evt.newValue;
        // });
        // edit.Q<Toggle>("FlyingToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Flying = evt.newValue;
        // });
        // edit.Q<Toggle>("PhasingToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Phasing = evt.newValue;
        // });
        // edit.Q<Toggle>("StealthToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Stealth = evt.newValue;
        // });
        // edit.Q<Toggle>("SturdyToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Sturdy = evt.newValue;
        // });
        // edit.Q<Toggle>("UnstoppableToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Unstoppable = evt.newValue;
        // });
        // edit.Q<Toggle>("RegenerationToggle").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Regeneration = evt.newValue;
        // });

        // edit.Q<TextField>("MarkEdit").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Marked = evt.newValue;
        // });

        // edit.Q<TextField>("HatredEdit").RegisterValueChangedCallback((evt) => {
        //     TokenController.held.GetComponent<UnitState>().Hatred = evt.newValue;
        // });

    }

    public static void GrabValues() {
        // VisualElement edit = UI.GameInfo.Q("ModifyPane");
        // edit.Q<TextField>("NameEdit").value = TokenController.held.name;

        // edit.Q<Toggle>("SlashedToggle").value = TokenController.held.GetComponent<UnitState>().Slashed;
        // edit.Q<Toggle>("BlindToggle").value = TokenController.held.GetComponent<UnitState>().Blind;
        // edit.Q<Toggle>("DazedToggle").value = TokenController.held.GetComponent<UnitState>().Dazed;
        // edit.Q<Toggle>("PacifiedToggle").value = TokenController.held.GetComponent<UnitState>().Pacified;
        // edit.Q<Toggle>("SealedToggle").value = TokenController.held.GetComponent<UnitState>().Sealed;
        // edit.Q<Toggle>("ShatteredToggle").value = TokenController.held.GetComponent<UnitState>().Shattered;
        // edit.Q<Toggle>("StunnedToggle").value = TokenController.held.GetComponent<UnitState>().Stunned;
        // edit.Q<Toggle>("WeakenedToggle").value = TokenController.held.GetComponent<UnitState>().Weakened;
        // edit.Q<Toggle>("VulnerableToggle").value = TokenController.held.GetComponent<UnitState>().Vulnerable;
        
        // edit.Q<Toggle>("CounterToggle").value = TokenController.held.GetComponent<UnitState>().Counter;
        // edit.Q<Toggle>("DefianceToggle").value = TokenController.held.GetComponent<UnitState>().Defiance;
        // edit.Q<Toggle>("DodgeToggle").value = TokenController.held.GetComponent<UnitState>().Dodge;
        // edit.Q<Toggle>("EvasionToggle").value = TokenController.held.GetComponent<UnitState>().Evasion;
        // edit.Q<Toggle>("FlyingToggle").value = TokenController.held.GetComponent<UnitState>().Flying;
        // edit.Q<Toggle>("PhasingToggle").value = TokenController.held.GetComponent<UnitState>().Phasing;
        // edit.Q<Toggle>("StealthToggle").value = TokenController.held.GetComponent<UnitState>().Stealth;
        // edit.Q<Toggle>("SturdyToggle").value = TokenController.held.GetComponent<UnitState>().Sturdy;
        // edit.Q<Toggle>("UnstoppableToggle").value = TokenController.held.GetComponent<UnitState>().Unstoppable;
        // edit.Q<Toggle>("RegenerationToggle").value = TokenController.held.GetComponent<UnitState>().Regeneration;

        // edit.Q<TextField>("HatredEdit").value = TokenController.held.GetComponent<UnitState>().Hatred;
        // edit.Q<TextField>("MarkEdit").value = TokenController.held.GetComponent<UnitState>().Marked;
    }

    public static void TokenClick(Token token) {
        if (token == null) {
            held = null;
            GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
            for (int i = 0; i < tokens.Length; i++) {
                tokens[i].GetComponent<Token>().ClearState();
            }
        }
        else if (token == held) {
            token.AdvanceState();
        }
        else {
            if (held != null) {
                held.ClearState();
            }
            token.SetState(HoldState.Held);
        }

    }

    public static void BlockClick(Block block) {
        if (held != null) {
            held.PlaceAtBlock(block);
            ReserveController.Adjust();
            // DropHeld();  
        }
        else {
            CameraControl.GoToBlock(block);
            block.SetTerrainInfo();
            Block.DeselectAll();
            block.Select();
        }
    }

    public static void ReserveSpotClick(ReserveSpot spot) {
        if (UI.ClicksSuspended) {
            return;
        }
        if (held != null && !held.InReserve) {
            spot.PlaceAtReserveSpot(held);
        }
        else if (held == null && spot == ReserveSpot.LastReserveSpot()) {
            EnableAddToken();
        }
    }

    public static void SetHeld(Token token) {
        held = token;
    }

    public static bool IsHeld(Token token)
    {
        return held == token;
    }

    public static void DropHeld() {
        TokenClick(null);
    }

    public static void EnableQuickEdit() {
        // UI.GameInfo.Q("QuickHP").style.display = DisplayStyle.Flex;
        // HpBar hp = held.GetComponent<HpBar>();
        // UI.GameInfo.Q<Label>("QuickHPName").text = hp.name;
        // UI.GameInfo.Q<SliderInt>("HpSlider").highValue = hp.MaxHP;
        // UI.GameInfo.Q<SliderInt>("HpSlider").value = hp.CHP;
        // UI.GameInfo.Q<Label>("QuickHPNum").text = hp.CHP.ToString();
    }

    public static void EnableAddToken() {
        TokenController.CheckCustomTokens();
        GameObject.Find("Engine").GetComponent<MenuController>().EnableModal("AddTokenDialog");
        UI.System.Q<TextField>("TokenNameField").value = "Token Name";
        UI.SetHardSuspend(true);
    }

    public static void EnableFocusPane() {
        // UI.GameInfo.Q("FocusToken").style.display = DisplayStyle.Flex;
    }

    public static void EnableFullEdit() {
        // UI.GameInfo.Q("ModifyPane").style.display = DisplayStyle.Flex;
        // HpBar.SuppressFloaters = true;
    }

    public static void DisableAddToken() {
        GameObject.Find("Engine").GetComponent<MenuController>().Clear();
        UI.SetHardSuspend(false);
    }

    public static void DisableQuickEdit() {
        // UI.GameInfo.Q("QuickHP").style.display = DisplayStyle.None;
    }

    public static void DisableFocusPane() {
        // UI.GameInfo.Q("FocusToken").style.display = DisplayStyle.None;
    }

    public static void DisableFullEdit() {
        // UI.GameInfo.Q("ModifyPane").style.display = DisplayStyle.None;
        // HpBar.SuppressFloaters = false;
    }
}
