using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class Token : MonoBehaviour
{
    public static Token TokenHeld = null;
    public static Vector3 Size2Offset = new Vector3(0, 0, -.73f);

    public bool InReserve = true;
    public int Size = 1;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        alignToCamera();
        if (Token.TokenHeld == this) {
            GetComponent<UnitState>().Focused = true;
            GetComponent<HpBar>().Focused = false;
        }
        else {
            GetComponent<UnitState>().Focused = false;
            GetComponent<HpBar>().Focused = true;
        }
    }

    private void alignToCamera() {
        if (InReserve) {
            Transform camera = GameObject.Find("ReserveCamera").transform;
            transform.Find("Offset").transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 180, 0);
        }
        else {
            Transform camera = GameObject.Find("CameraOrigin").transform;
            transform.Find("Offset").transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 90, 0);
        }
    }

    void OnMouseDown()
    {
        if (ModeController.GetMode() == Mode.View) {
            ChangeHeld(TokenHeld == this ? null : this);
        }
    }

    public static void ChangeHeld(Token token) {
        if (token == null) {
            VisualElement element = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("FocusToken");
            element.style.display = DisplayStyle.None;
        }
        else {
            VisualElement element = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement.Q("FocusToken");
            element.style.display = DisplayStyle.Flex;
        }
        GameObject[] allTokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < allTokens.Length; i++) {
            allTokens[i].GetComponentInChildren<MeshRenderer>().material.SetInt("_Selected", 0);
        }
        if (token) {
            token.GetComponentInChildren<MeshRenderer>().material.SetInt("_Selected", 1);
        }
        TokenHeld = token;
    }

    public void PlaceAtBlock(Block block) {
        transform.position = block.transform.position + new Vector3(0, .25f, 0);
        InReserve = false;
        transform.Find("Base").gameObject.SetActive(true);

        ReserveSpot rs = ReserveSpot.GetReserveSpot(this);
        if (rs != null) {
            rs.Token = null;
        }
        Token.ChangeHeld(null);
    }

    public static void InitModal() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        List<string> avatars = new List<string>{
            "Enochian",
            "Shade",
            "Relict",
            "Object"
        };
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/tokens/");
        if (info.Exists) {
            FileInfo[] fileInfo = info.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++) {
                avatars.Add("Custom: " + fileInfo[i].Name);
            }        
        }
        (modeUI.rootVisualElement.Q("AvatarDropdown") as DropdownField).choices = avatars;
        (modeUI.rootVisualElement.Q("AvatarDropdown") as DropdownField).value = "Enochian";

        (modeUI.rootVisualElement.Q("TokenTypeDropdown") as DropdownField).choices = new List<string>{
            "Player",
            "Foe",
        };
        (modeUI.rootVisualElement.Q("TokenTypeDropdown") as DropdownField).value = "Player";

        (modeUI.rootVisualElement.Q("SizeDropdown") as DropdownField).choices = new List<string>{
            "1-Standard",
            "2-Large",
            "3-Enormous",
        };
        (modeUI.rootVisualElement.Q("SizeDropdown") as DropdownField).value = "1-Standard";

        (modeUI.rootVisualElement.Q("JobClassDropdown") as DropdownField).choices = jobOptions();
        (modeUI.rootVisualElement.Q("JobClassDropdown") as DropdownField).value = defaultJobOption();
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

    public static void CreateNew() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        DropdownField avatarField = (modeUI.rootVisualElement.Q("AvatarDropdown") as DropdownField);
        DropdownField tokenTypeField = (modeUI.rootVisualElement.Q("TokenTypeDropdown") as DropdownField);
        DropdownField jobField = (modeUI.rootVisualElement.Q("JobClassDropdown") as DropdownField);
        TextField nameField = (modeUI.rootVisualElement.Q("TokenNameField") as TextField);
        Toggle eliteField = (modeUI.rootVisualElement.Q("EliteCheckbox") as Toggle);
        IntegerField legendScale = (modeUI.rootVisualElement.Q("LegendScale") as IntegerField);
        DropdownField sizeField = (modeUI.rootVisualElement.Q("SizeDropdown") as DropdownField);

        GameObject newToken = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        newToken.name = nameField.value;

        switch(avatarField.value) {
            case "Enochian":
            case "Shade":
            case "Relict":
                Texture2D t = Resources.Load<Texture2D>("Textures/Chibis/" + avatarField.value);
                newToken.transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", t);
                break;
            case "Object":
                newToken.transform.Find("Offset/Avatar").gameObject.SetActive(false);
                newToken.transform.Find("Offset/Object").gameObject.SetActive(true);
                break;
            default:
                string filename = avatarField.value;
                filename = filename.Replace("Custom: ", "");
                newToken.GetComponent<Token>().CustomCutout("file://" + Application.persistentDataPath + "/tokens/" + filename);
                break;
        }

        string jobclass = jobField.value.Split("-")[0];
        string job = "";
        if (jobField.value.Split("-").Length > 1) {
            job = jobField.value.Split("-")[0];
        }

        UnitState unitstate = newToken.AddComponent<UnitState>();
        unitstate.Job = job;

        HpBar hpbar = newToken.AddComponent<HpBar>();
        hpbar.VIG = 0;
        hpbar.Wounds = 0;
        switch(jobclass) {
            case "Wright":
            case "Artillery":
                hpbar.Color = "blue";
                hpbar.MHP = 32;
                unitstate.Damage = 8;
                unitstate.Fray = 3;
                unitstate.Range = 6;
                unitstate.Speed = 4;
                unitstate.Dash = 2;
                unitstate.Defense = 7;
                break;
            case "Vagabond":
            case "Skirmisher":
                hpbar.Color = "yellow";
                hpbar.MHP = 28;
                unitstate.Damage = 10;
                unitstate.Fray = 2;
                unitstate.Range = 4;
                unitstate.Speed = 4;
                unitstate.Dash = 4;
                unitstate.Defense = 10;
                break;
            case "Stalwart":
            case "Heavy":
                hpbar.Color = "red";
                hpbar.MHP = 40;
                unitstate.Damage = 6;
                unitstate.Fray = 4;
                unitstate.Range = 3;
                unitstate.Speed = 4;
                unitstate.Dash = 2;
                unitstate.Defense = 6;
                break;
            case "Leader":
            case "Mendicant":
                hpbar.Color = "green";
                hpbar.MHP = 40;
                unitstate.Damage = 6;
                unitstate.Fray = 3;
                unitstate.Range = 3;
                unitstate.Speed = 4;
                unitstate.Dash = 2;
                unitstate.Defense = 8;
                break;
            case "Legend":
                hpbar.Color = "purple";
                hpbar.MHP = 50 * legendScale.value;
                unitstate.Damage = 8;
                unitstate.Fray = 3;
                unitstate.Range = 3;
                unitstate.Speed = 4;
                unitstate.Dash = 2;
                unitstate.Defense = 8;
                break;
            case "Mob":
                hpbar.Color = "gray";
                hpbar.MHP = 2;
                unitstate.Damage = 6;
                unitstate.Fray = 3;
                unitstate.Range = 1;
                unitstate.Speed = 4;
                unitstate.Dash = 2;
                unitstate.Defense = 8;
                break;
        }
        if (eliteField.value) {
            hpbar.Elite = true;
            hpbar.MHP *= 2;
        }
        hpbar.CHP = hpbar.MHP;
        unitstate.Color = hpbar.Color;
        unitstate.Foe = tokenTypeField.value == "Foe";

        if (sizeField.value == "2-Large") {
            newToken.GetComponent<Token>().Size = 2;
            newToken.transform.Find("Offset").transform.localPosition += Size2Offset;
            newToken.transform.Find("Base").transform.localPosition += Size2Offset;
            newToken.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
            newToken.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2, 2, 4);
        }
        else if (sizeField.value == "3-Enormous") {
            newToken.GetComponent<Token>().Size = 3;
            newToken.transform.Find("Offset").transform.localScale = new Vector3(3, 3, 3);
            newToken.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(3, 3, 4);
        }

        Material m = Instantiate(Resources.Load<Material>("Materials/Token/BorderBase"));
        switch (hpbar.Color) {
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

    private void CustomCutout(string filename) {
        StartCoroutine(LoadLocalFileIntoCutout(filename));
    }

    private IEnumerator LoadLocalFileIntoCutout(string filename) {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(filename))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                transform.Find("Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", texture);
            }
        }
    }

}
