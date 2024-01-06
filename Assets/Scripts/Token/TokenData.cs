using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TokenData : NetworkBehaviour
{
    [SyncVar]
    public string System;
    [SyncVar]
    public string Id;
    [SyncVar]
    public string Name;
    [SyncVar]
    public string GraphicHash;
    [SyncVar]
    public int Size;
    [SyncVar]
    public Color Color;
    [SyncVar]
    public string SystemData;

    [SyncVar]
    public bool Destroyed;
    [SyncVar]
    public bool Placed;
    [SyncVar]
    public Vector3 LastKnownPosition;
    
    public bool NeedsRedraw;
    public Texture2D Graphic;
    public GameObject WorldObject;
    public VisualElement UnitBarElement;
    public VisualElement OverheadElement;

    private float GraphicSyncInterval = 0;

    void Start() {
        if (Destroyed) {
            return;
        }
        CreateWorldToken();
        CreateUnitBarElement();
        CreateOverheadElement();
    }

    void Update() {
        if (Destroyed) {
            return;
        }

        if (Graphic == null) {
            GraphicSync();
        }

        if (OverheadElement != null) {
            OverheadElement.style.display = Placed ? DisplayStyle.Flex : DisplayStyle.None;
            if (WorldObject != null) {
                UI.FollowToken(WorldObject.GetComponent<Token>(), OverheadElement, Camera.main, Vector2.zero, true);
            }
        }

        if (GameSystem.Current() != null) {
            GameSystem.Current().UpdateData(this);   
            gameObject.name = $"TokenData:{Name}";
        }
    }

    private void GraphicSync() {
        if (GraphicSyncInterval > 0) {
            GraphicSyncInterval -= Time.deltaTime;
        }
        else {
            Graphic = TextureSender.LoadImageFromFile(GraphicHash, true);
            if (Graphic) {
                Graphic.wrapMode = TextureWrapMode.Clamp;
                UpdateGraphic();
            }
            else {
                GraphicSyncInterval = 2.5f;
            }
        }
    }

    private void CreateWorldToken() {
        WorldObject = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        WorldObject.transform.parent = GameObject.Find("Tokens").transform;
        if (!Placed) {
            Place(false);
        }
        else {
            Place();
            WorldObject.transform.position = LastKnownPosition;
        }
        WorldObject.GetComponent<Token>().Data = this;
        SetSize();
    }
    
    private void CreateOverheadElement() {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>(GameSystem.Current().GetOverheadAsset());
        VisualElement instance = template.Instantiate();
        OverheadElement = instance.Q("Overhead");
        UI.System.Q("Worldspace").Add(OverheadElement);
    }

    public void Place(bool place = true) {
        if (place) {
            Placed = true;
            WorldObject.transform.localScale = Vector3.one;
        }
        else {
            Placed = false;
            WorldObject.transform.localScale = Vector3.zero;
            LastKnownPosition = new Vector3(0, -10, 0);
        }
    }

    private void SetSize() {
        if (Size == 1) {
            WorldObject.GetComponent<Token>().Size = 1;
            WorldObject.transform.Find("Offset").transform.localScale = new Vector3(1, 1, 1);
            WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(.7f, .7f, 4);
        }
        if (Size == 2) {
            WorldObject.GetComponent<Token>().Size = 2;
            WorldObject.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
            WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(1.7f, 1.7f, 4);
        }
        else if (Size == 3) {
            WorldObject.GetComponent<Token>().Size = 3;
            WorldObject.transform.Find("Offset").transform.localScale = new Vector3(3, 3, 3);
            WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2.7f, 2.7f, 4);
        }      
    }

    private void CreateUnitBarElement() {
        // Create the element in the UI
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/UnitTemplate");
        UnitBarElement = template.Instantiate();
        UnitBarElement.style.display = DisplayStyle.Flex;

        Token t = WorldObject.GetComponent<Token>();
        UnitBarElement.RegisterCallback<MouseDownEvent>((evt) => {
            if (Cursor.IsLeftClick()) {
                t.LeftClick();
                t.Focus();
            }
            else if (Cursor.IsRightClick()) {
                t.RightClick();
                t.Focus();
            }
        });
        UnitBarElement.RegisterCallback<MouseEnterEvent>((evt) => {
            Cursor.OverUnitBarElement = true;
            t.Focus();
        });
        UnitBarElement.RegisterCallback<MouseLeaveEvent>((evt) => {
            Cursor.OverUnitBarElement = false;
            t.Unfocus();
        });

        // Add it to the UI
        UI.System.Q("UnitBar").Add(UnitBarElement);
    }

    private void UpdateGraphic() {
        // Set the world object graphic
        Token token = WorldObject.GetComponent<Token>();
        token.SetImage(Graphic);

        // Set the UI portrait
        float height = 60;
        float width = 60;
        if (Graphic.width > Graphic.height) {
            height *= Graphic.height/(float)Graphic.width;
        }
        else {
            width *= Graphic.width/(float)Graphic.height;
        }
        UnitBarElement.Q("Portrait").style.width = width;
        UnitBarElement.Q("Portrait").style.height = height;
        UnitBarElement.Q("Portrait").style.backgroundImage = Graphic;
        UI.Redraw();
        UnitBarElement.Q("ClassBackground").style.borderTopColor = Color;
        UnitBarElement.Q("ClassBackground").style.borderRightColor = Color;
        UnitBarElement.Q("ClassBackground").style.borderBottomColor = Color;
        UnitBarElement.Q("ClassBackground").style.borderLeftColor = Color;
    }

    public static TokenData Find(string id) {
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
            TokenData data = g.GetComponent<TokenData>();
            if (data.Id == id) {
                return data;
            }
        }
        return null;
    }

    public void Select() {
        NeedsRedraw = true;
    }

    public void Focus() {
        NeedsRedraw = true;
    }

    public void Disconnect() {
        UI.System.Q("UnitBar").Remove(UnitBarElement);
        UI.System.Q("Worldspace").Remove(OverheadElement);
        Destroy(WorldObject);
    }

    public void UpdateTokenPanel(string elementName) {
        VisualElement panel = UI.System.Q(elementName);
        if (Graphic != null) {
            panel.Q("Portrait").style.backgroundImage = Graphic;
        }
        panel.Q<Label>("Name").text = Name.Trim();
        UI.ToggleDisplay(panel.Q<Label>("Name"), Name.Trim().Length > 0);
    }  

    public void Delete() {
        UI.System.Q("UnitBar").Remove(UnitBarElement);
        UI.System.Q("Worldspace").Remove(OverheadElement);
        Destroy(WorldObject);
        Token.DeselectAll();
    }
}
