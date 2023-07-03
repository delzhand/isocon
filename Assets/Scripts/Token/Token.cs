using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
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
        DropdownField typeField = (modeUI.rootVisualElement.Q("TokenTypeDropdown") as DropdownField);
        List<string> choices = new List<string>();
        choices.Add("Enochian");
        choices.Add("Shade");
        choices.Add("Relict");
        choices.Add("Object");
        typeField.choices = choices;
    }

    public static void CreateNew() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        DropdownField typeField = (modeUI.rootVisualElement.Q("TokenTypeDropdown") as DropdownField);
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
                break;
        }
    }

}
