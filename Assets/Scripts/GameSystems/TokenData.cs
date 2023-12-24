using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

[System.Serializable]
public class TokenDataRaw {
    public string Name;
    public string GraphicHash;
    public int Size;
}

public class TokenData : NetworkBehaviour
{
    // public string Id;

    // [SyncVar]
    // public string Name;

    // [SyncVar]
    // public string GraphicHash;

    // [SyncVar]
    // public string Json;

    // [SyncVar]
    // public bool OnField;

    // [SyncVar]
    // public string ConditionSyncHelper;

    // public GameObject TokenObject;
    // public VisualElement UnitBarElement;
    // public VisualElement OverheadElement;
    // public VisualElement ConditionsElement;
    // public bool RedrawConditionsElement = false;

    // public Texture2D Graphic;

    // public static bool MouseOverUnitBarElement = false;
    // public Dictionary<string, StatusEffect> Conditions = new();

    // private bool initialized = false;
    // private float awaitingGraphicSync = 0;

    // void Update() {
    //     BaseUpdate();
    // }

    // public void Disconnect() {
    //     UI.System.Q("UnitBar").Remove(UnitBarElement);
    //     UI.System.Q("Worldspace").Remove(OverheadElement);
    //     Destroy(TokenObject);
    //     initialized = false;
    // }

    // public virtual void BaseUpdate() {
    //     if (!initialized && NeedsSetup() && Json.Length > 0) {
    //         DoTokenDataSetup();
    //     }

    //     // Every 5 seconds until the graphic is provided, recheck
    //     if (Graphic == null) {
    //         if (awaitingGraphicSync > 0) {
    //             awaitingGraphicSync -= Time.deltaTime;
    //         }
    //         else {
    //             initialized = false;
    //         }

    //     }


    //     if (!initialized && GraphicHash.Length > 0 && GameSystem.Current() != null) {
    //         initialized = true;
    //         Graphic = TextureSender.LoadImageFromFile(GraphicHash, true);
    //         if (Graphic) {
    //             Graphic.wrapMode = TextureWrapMode.Clamp;
    //             CreateWorldToken();
    //             CreateUI();
    //             CreateOverhead();
    //         }
    //         else {
    //             awaitingGraphicSync = 2.5f;
    //         }
    //     }


    //     if (TokenObject) {
    //         TokenObject.transform.position = transform.position;
    //         TokenObject.transform.localScale = OnField ? Vector3.one : Vector3.zero;
    //     }
    //     if (OverheadElement != null) {
    //         UpdateOverheadScreenPosition();
    //         UpdateOverheadValues();
    //         OverheadElement.style.display = (OnField ? DisplayStyle.Flex : DisplayStyle.None);
    //     }

    // }

    // public virtual bool NeedsSetup() {
    //     // To return true, choose a required value with an invalid value
    //     // MaxHP == 0 will usually work (unless HP-less objects are possible...)
    //     return false;
    // }

    // public virtual void UpdateTokenPanel(string elementName) {
    //     VisualElement panel = UI.System.Q(elementName);
    //     panel.Q("Portrait").style.backgroundImage = Graphic;
    //     panel.Q<Label>("Name").text = Name;
    // }

    // public void Select() {
    //     RedrawConditionsElement = true;
    // }

    // public void Focus() {
    //     RedrawConditionsElement = true;
    // }

    // public virtual void UpdateOverheadValues() {
    // }

    // public virtual void TokenDataSetup(string json, string id) {
    //     Json = json;
    //     Id = id;
    // }

    // public virtual void DoTokenDataSetup() {        
    // }

    // public virtual void CreateWorldToken() {
    //     TokenObject = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
    //     TokenObject.transform.parent = GameObject.Find("Tokens").transform;
    //     OnField = false;

    //     Token token = TokenObject.GetComponent<Token>();
    //     token.SetImage(Graphic);
    //     // token.onlineDataObject = gameObject;

    //     int size = GetSize();
    //     if (size == 1) {
    //         TokenObject.GetComponent<Token>().Size = 1;
    //         TokenObject.transform.Find("Offset").transform.localScale = new Vector3(1, 1, 1);
    //         TokenObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(.7f, .7f, 4);
    //     }
    //     if (size == 2) {
    //         TokenObject.GetComponent<Token>().Size = 2;
    //         TokenObject.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
    //         TokenObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(1.7f, 1.7f, 4);
    //     }
    //     else if (size == 3) {
    //         TokenObject.GetComponent<Token>().Size = 3;
    //         TokenObject.transform.Find("Offset").transform.localScale = new Vector3(3, 3, 3);
    //         TokenObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2.7f, 2.7f, 4);
    //     }        
    // }

    // public virtual void CreateUI() {
    //     // Create the element in the UI
    //     VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/UnitTemplate");
    //     UnitBarElement = template.Instantiate();
    //     UnitBarElement.style.display = DisplayStyle.Flex;

    //     // Set the UI portrait
    //     float height = 60;
    //     float width = 60;
    //     if (Graphic.width > Graphic.height) {
    //         height *= (Graphic.height/(float)Graphic.width);
    //     }
    //     else {
    //         width *= (Graphic.width/(float)Graphic.height);
    //     }
    //     UnitBarElement.Q("Portrait").style.backgroundImage = Graphic;
    //     UnitBarElement.Q("Portrait").style.width = width;
    //     UnitBarElement.Q("Portrait").style.height = height;

    //     Token t = TokenObject.GetComponent<Token>();
    //     UnitBarElement.RegisterCallback<ClickEvent>((evt) => {
    //         t.LeftClick();
    //         t.Focus();
    //     });
    //     UnitBarElement.RegisterCallback<MouseEnterEvent>((evt) => {
    //         MouseOverUnitBarElement = true;
    //         t.Focus();
    //     });
    //     UnitBarElement.RegisterCallback<MouseLeaveEvent>((evt) => {
    //         MouseOverUnitBarElement = false;
    //         t.Unfocus();
    //     });

    //     // Add it to the UI
    //     UI.System.Q("UnitBar").Add(UnitBarElement);
    // }

    // public virtual void CreateOverhead() {
    //     VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/GameSystem/SimpleOverhead");
    //     VisualElement instance = template.Instantiate();
    //     OverheadElement = instance.Q("Overhead");
    //     UI.System.Q("Worldspace").Add(OverheadElement);
    // }

    // public virtual int GetSize() {
    //     return 1;
    // }

    // private void UpdateOverheadScreenPosition() {
    //     OverheadElement.style.display = DisplayStyle.Flex;
    //     UI.FollowToken(TokenObject.GetComponent<Token>(), OverheadElement, Camera.main, Vector2.zero, true);
    // }

    // public virtual bool CheckCondition(string label) {
    //     throw new NotImplementedException();
    // }

    // public static void DeleteById(string id) {
    //     foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
    //         TokenData t = g.GetComponent<TokenData>();
    //         if (t.Id == id) {
    //             UI.System.Q("UnitBar").Remove(t.UnitBarElement);
    //             UI.System.Q("Worldspace").Remove(t.OverheadElement);
    //             Destroy(t.TokenObject);
    //             Destroy(t);
    //             Destroy(g);
    //             // TokenController.Deselect();
    //         }
    //     }

    // }

    // public virtual void Change(string value) {
    // }

    // protected void RebuildConditionSync() {
    //     List<string> effects = new();
    //     foreach (StatusEffect se in Conditions.Values) {
    //         effects.Add(se.Write());
    //     }
    //     ConditionSyncHelper = string.Join("|", effects.ToArray());
    // }

    // protected void RedrawConditions() {
    //     Debug.Log($"Redraw conditions for { Name }");
    //     RedrawConditionsElement = true;
    //     ConditionsElement.Clear();
    //     string[] effects = ConditionSyncHelper.Split("|");
    //     foreach(string s in effects) {
    //         StatusEffect effect = StatusEffect.Parse(s);
    //         VisualElement e = UI.CreateFromTemplate("UITemplates/GameSystem/ConditionTemplate");
    //         e.Q<Label>("Name").text = effect.Name;
    //         Color c = Color.black;
    //         switch (effect.Color) {
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
    //         if (effect.Type == "Number") {
    //             e.Q<Label>("Name").text = $"{effect.Name} {effect}";
    //             e.Q<Button>("Increment").RegisterCallback<ClickEvent>((evt) => {
    //                 Player.Self().CmdRequestTokenDataSetValue(this, $"IncrementStatus|{ effect.Name }");
    //             });
    //             e.Q<Button>("Decrement").RegisterCallback<ClickEvent>((evt) => {
    //                 Player.Self().CmdRequestTokenDataSetValue(this, $"DecrementStatus|{ effect.Name }");
    //             });
    //         }
    //         else {
    //             UI.ToggleDisplay(e.Q("Increment"), false);
    //             UI.ToggleDisplay(e.Q("Decrement"), false);
    //         }
    //         e.Q<Button>("Remove").RegisterCallback<ClickEvent>((evt) => {
    //             Player.Self().CmdRequestTokenDataSetValue(this, $"LoseStatus|{ effect.Name }");
    //         });
    //         if (effect.Locked) {
    //             UI.ToggleDisplay(e.Q("Remove"), false);
    //         }
    //         ConditionsElement.Add(e);
    //     }
    // }    
}
