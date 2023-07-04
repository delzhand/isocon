using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class Token : MonoBehaviour
{
    public static Token TokenHeld = null;

    public bool InReserve = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        alignToCamera();
    }

    private void alignToCamera() {
        if (InReserve) {
            Transform camera = GameObject.Find("ReserveCamera").transform;
            transform.Find("Avatar").transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 180, 0);
        }
        else {
            Transform camera = GameObject.Find("CameraOrigin").transform;
            transform.Find("Avatar").transform.rotation = Quaternion.Euler(0, camera.eulerAngles.y + 90, 0);
        }
    }

    void OnMouseDown()
    {
        if (ModeController.GetMode() == Mode.View) {
            ChangeHeld(TokenHeld == this ? null : this);
        }
    }

    public static void ChangeHeld(Token token) {
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
        FileInfo[] fileInfo = info.GetFiles();
        for (int i = 0; i < fileInfo.Length; i++) {
            avatars.Add("Custom: " + fileInfo[i].Name);
        }        
        (modeUI.rootVisualElement.Q("AvatarDropdown") as DropdownField).choices = avatars;


        (modeUI.rootVisualElement.Q("TokenTypeDropdown") as DropdownField).choices = new List<string>{
            "Player",
            "Foe",
            "Object",
            "NPC"
        };

        (modeUI.rootVisualElement.Q("JobClassDropdown") as DropdownField).choices = jobOptions();
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
                    "Mendicant-Seer",
                    "Wright-Enochian",
                    "Wright-Geomancer",
                    "Wright-Spellblade",
                    "Wright-Stormbender",
                };
            default:
                return new List<string>{};
        }
    }

    public static void CreateNew() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        DropdownField typeField = (modeUI.rootVisualElement.Q("AvatarDropdown") as DropdownField);
        DropdownField jobField = (modeUI.rootVisualElement.Q("JobClassDropdown") as DropdownField);
        TextField nameField = (modeUI.rootVisualElement.Q("TokenNameField") as TextField);
        GameObject newToken = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        ReserveSpot openReserve = ReserveSpot.LastReserveSpot();
        newToken.name = nameField.value;
        newToken.transform.parent = GameObject.Find("Reserve").transform;
        newToken.transform.localPosition = openReserve.transform.localPosition;
        newToken.GetComponent<Token>().InReserve = true;
        openReserve.Token = newToken.GetComponent<Token>();
        Token.ChangeHeld(null);
        Reserve.Adjust();

        switch(typeField.value) {
            case "Enochian":
            case "Shade":
            case "Relict":
                Texture2D t = Resources.Load<Texture2D>("Textures/Chibis/" + typeField.value);
                newToken.transform.Find("Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", t);
                break;
            case "Object":
                newToken.transform.Find("Avatar").gameObject.SetActive(false);
                newToken.transform.Find("Object").gameObject.SetActive(true);
                break;
            default:
                string filename = typeField.value;
                filename = filename.Replace("Custom: ", "");
                newToken.GetComponent<Token>().CustomCutout("file://" + Application.persistentDataPath + "/tokens/" + filename);
                break;
        }

        string jobclass = jobField.value.Split("-")[0];
        string job = jobField.value.Split("-")[1];
        HpBar hpbar = newToken.AddComponent<HpBar>();
        hpbar.VIG = 0;
        hpbar.Wounds = 0;
        switch(jobclass) {
            case "Wright":
                hpbar.MHP = 32;
                break;
            case "Vagabond":
                hpbar.MHP = 28;
                break;
            case "Stalwart":
                hpbar.MHP = 40;
                break;
            case "Mendicant":
                hpbar.MHP = 40;
                break;
        }
        hpbar.CHP = hpbar.MHP;
        
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
