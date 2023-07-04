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

        (modeUI.rootVisualElement.Q("SizeDropdown") as DropdownField).choices = new List<string>{
            "1-Standard",
            "2-Large",
            "3-Enormous",
        };

        (modeUI.rootVisualElement.Q("JobClassDropdown") as DropdownField).choices = jobOptions();
    }

    private static List<string> jobOptions() {
        string v = PlayerPrefs.GetString("IconVersion", "1.5");
        switch(v) {
            case "1.5":
                return new List<string>{
                    "Stalwart",
                    "Vagabond",
                    "Mendicant",
                    "Wright",
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

    public static void CreateNew() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        DropdownField typeField = (modeUI.rootVisualElement.Q("AvatarDropdown") as DropdownField);
        DropdownField jobField = (modeUI.rootVisualElement.Q("JobClassDropdown") as DropdownField);
        TextField nameField = (modeUI.rootVisualElement.Q("TokenNameField") as TextField);
        Toggle eliteField = (modeUI.rootVisualElement.Q("EliteCheckbox") as Toggle);
        IntegerField legendScale = (modeUI.rootVisualElement.Q("LegendScale") as IntegerField);
        DropdownField sizeField = (modeUI.rootVisualElement.Q("SizeDropdown") as DropdownField);

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
        HpBar hpbar = newToken.AddComponent<HpBar>();
        hpbar.VIG = 0;
        hpbar.Wounds = 0;
        switch(jobclass) {
            case "Wright":
            case "Artillery":
                hpbar.MHP = 32;
                break;
            case "Vagabond":
            case "Skirmisher":
                hpbar.MHP = 28;
                break;
            case "Stalwart":
            case "Heavy":
            case "Leader":
            case "Mendicant":
                hpbar.MHP = 40;
                break;
            case "Legend":
                hpbar.MHP = 50 * legendScale.value;
                break;
        }
        if (eliteField.value) {
            hpbar.MHP *= 2;
        }
        hpbar.CHP = hpbar.MHP;

        if (sizeField.value == "2-Large") {
            newToken.transform.Find("Avatar").transform.localScale = new Vector3(2, 2, 2);
            newToken.transform.Find("Avatar").transform.localPosition -= new Vector3(0, 0, .5f);
            newToken.transform.Find("Avatar/Shadow").GetComponent<DecalProjector>().size = new Vector3(2, 2, 3);
        }
        else if (sizeField.value == "3-Enormous") {
            newToken.transform.Find("Avatar").transform.localScale = new Vector3(3, 3, 3);
            newToken.transform.Find("Avatar").transform.localPosition -= new Vector3(0, 0, 1f);
            newToken.transform.Find("Avatar/Shadow").GetComponent<DecalProjector>().size = new Vector3(3, 3, 4);
        }
        
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
