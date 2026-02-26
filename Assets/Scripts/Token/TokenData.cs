using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
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
    public string Name = "";
    [SyncVar]
    public TokenMeta TokenMeta;
    [SyncVar]
    public int Size;
    [SyncVar]
    public string Shape;
    [SyncVar]
    public Color Color;
    [SyncVar]
    public string SystemData;

    [SyncVar]
    public bool Destroyed;
    [SyncVar]
    public bool Deletable;
    [SyncVar]
    public bool Placed;
    [SyncVar]
    public Vector3 LastKnownPosition;

    // public bool NeedsRedraw;
    public Texture2D Graphic;
    public Texture2D GraphicSingle;
    public GameObject WorldObject;
    public VisualElement UnitBarElement;
    public VisualElement OverheadElement;

    private static float GraphicSyncInterval = 0;

    void Start()
    {
        if (Destroyed)
        {
            return;
        }
        CreateWorldToken();
        CreateUnitBarElement();
        CreateOverheadElement();
        LoadGraphic();
    }

    void Update()
    {
        if (Destroyed)
        {
            Destroy(gameObject);
            return;
        }

        if (Graphic == null)
        {
            if (GraphicSyncInterval > 0)
            {
                GraphicSyncInterval -= Time.deltaTime;
            }
            else
            {
                TokenSync.SyncStep();
                GraphicSyncInterval = .4f;

                var texture = TokenSync.LoadHashedFileAsTexture(TokenMeta.Hash);
                if (texture != null)
                {
                    SetGraphic(texture);
                }

            }
        }

        if (OverheadElement != null)
        {
            OverheadElement.style.display = Placed ? DisplayStyle.Flex : DisplayStyle.None;
            if (WorldObject != null)
            {
                UI.FollowTransform(WorldObject.GetComponent<Token>().transform.Find("Offset/Avatar/Cutout/Cutout Quad/LabelAnchor").transform, OverheadElement, UI.World, Camera.main, Vector2.zero);
            }
            ISystemToken st = SystemTokenRegistry.DoInterfaceCallback(System, SystemData);
            st.UpdateOverhead(this);
        }

        // if (GameSystem.Current() != null)
        // {
        //     GameSystem.Current().UpdateData(this);
        // }
        gameObject.name = $"TokenData:{Name}";
    }

    private void LoadGraphic()
    {
        FileLogger.Write($"Load graphic for {TokenMeta.TruncateHash(TokenMeta.Hash)}");
        Texture2D graphic = TokenSync.LoadHashedFileAsTexture(TokenMeta.Hash);
        if (graphic)
        {
            FileLogger.Write($"Hashed image {TokenMeta.TruncateHash(TokenMeta.Hash)} found locally");
            SetGraphic(graphic);
        }
        else
        {
            FileLogger.Write($"Hashed image {TokenMeta.TruncateHash(TokenMeta.Hash)} not found locally, starting sync");
            TokenSync.Add(TokenMeta, this);
        }
    }

    private void CreateWorldToken()
    {
        WorldObject = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        WorldObject.transform.parent = GameObject.Find("Tokens").transform;
        if (!Placed)
        {
            Place(false);
        }
        else
        {
            Place();
            WorldObject.transform.position = LastKnownPosition;
        }
        WorldObject.GetComponent<Token>().Data = this;
        SetSize();
    }

    public void CreateOverheadElement()
    {
        ISystemToken st = SystemTokenRegistry.DoInterfaceCallback(System, SystemData);
        string asset = st.GetOverheadAsset();
        if (asset != null)
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>(asset);
            VisualElement instance = template.Instantiate();
            OverheadElement = instance.Q("Overhead");
            UI.World.Q("Worldspace").Add(OverheadElement);
        }
    }

    public void Place(bool place = true)
    {
        if (place)
        {
            Placed = true;
            WorldObject.transform.localScale = Vector3.one;
        }
        else
        {
            Placed = false;
            WorldObject.transform.localScale = Vector3.zero;
            LastKnownPosition = new Vector3(0, -10, 0);
        }
    }

    private void SetSize()
    {
        if (Size == 1)
        {
            WorldObject.GetComponent<Token>().Size = 1;
            WorldObject.transform.Find("Offset").transform.localScale = new Vector3(1, 1, 1);
            WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(.7f, .7f, 4);
        }
        if (Size == 2)
        {
            WorldObject.GetComponent<Token>().Size = 2;
            WorldObject.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
            WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(1.7f, 1.7f, 4);
        }
        else if (Size == 3)
        {
            WorldObject.GetComponent<Token>().Size = 3;
            WorldObject.transform.Find("Offset").transform.localScale = new Vector3(3, 3, 3);
            WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2.7f, 2.7f, 4);
        }
    }

    public Token GetToken()
    {
        return WorldObject.GetComponent<Token>();
    }

    public void UpdateSize(int size)
    {
        Size = size;
        SetSize();
    }

    private void CreateUnitBarElement()
    {
        // Create the element in the UI
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/UnitTemplate");
        UnitBarElement = template.Instantiate();
        UnitBarElement.style.display = DisplayStyle.Flex;

        Token t = WorldObject.GetComponent<Token>();
        UnitBarElement.RegisterCallback<MouseEnterEvent>((evt) =>
        {
            t.Focus();
            Pointer.UnitBarMouseoverToken = t;
        });
        UnitBarElement.RegisterCallback<MouseLeaveEvent>((evt) =>
        {
            t.Unfocus();
            Pointer.UnitBarMouseoverToken = null;
        });

        // Add it to the UI
        UI.System.Q("UnitBar").Add(UnitBarElement);
    }

    public void SetGraphic(Texture2D graphic)
    {
        Graphic = graphic;
        SetGraphicSingle();

        // Set the world object graphic
        Token token = WorldObject.GetComponent<Token>();
        token.SetImage(Graphic);

        // Set the UI portrait
        float height = 60;
        float width = 60;
        if (GraphicSingle.width > Graphic.height)
        {
            height *= GraphicSingle.height / (float)GraphicSingle.width;
        }
        else
        {
            width *= GraphicSingle.width / (float)GraphicSingle.height;
        }
        UnitBarElement.Q("Portrait").style.width = width;
        UnitBarElement.Q("Portrait").style.height = height;
        UnitBarElement.Q("Portrait").style.backgroundImage = GraphicSingle;
        UnitBarElement.Q("Portrait").style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left);
        UnitBarElement.Q("Portrait").style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top);
        UnitBarElement.Q("Portrait").style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
        UnitBarElement.Q("Portrait").style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
        UI.Redraw();
        UnitBarElement.Q("ClassBackground").style.borderTopColor = Color;
        UnitBarElement.Q("ClassBackground").style.borderRightColor = Color;
        UnitBarElement.Q("ClassBackground").style.borderBottomColor = Color;
        UnitBarElement.Q("ClassBackground").style.borderLeftColor = Color;
    }

    private void SetGraphicSingle()
    {
        Color[] pixels = Graphic.GetPixels(0, 0, Graphic.width / TokenMeta.Frames, Graphic.height);
        GraphicSingle = new Texture2D(Graphic.width / TokenMeta.Frames, Graphic.height);
        GraphicSingle.SetPixels(pixels);
        GraphicSingle.Apply();
        GraphicSingle.wrapMode = TextureWrapMode.Clamp;
        GraphicSingle.filterMode = FilterMode.Point;
    }

    public static TokenData Find(string id)
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("TokenData"))
        {
            TokenData data = g.GetComponent<TokenData>();
            if (data.Id == id)
            {
                return data;
            }
        }
        return null;
    }

    public static void Command(string tokenId, string command)
    {
        TokenData data = TokenData.Find(tokenId);
        data.HandleCommand(command);
    }

    private void HandleCommand(string command)
    {
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }

        ISystemToken st = SystemTokenRegistry.DoInterfaceCallback(System, SystemData);
        st.HandleCommand(command, this);
        SystemData = st.Serialize();
        // NeedsRedraw = true;
    }

    // public void Select()
    // {
    //     // NeedsRedraw = true;
    // }

    // public void Focus()
    // {
    //     // NeedsRedraw = true;
    // }

    public void Disconnect()
    {
        if (UnitBarElement != null)
        {
            UnitBarElement.RemoveFromHierarchy();
        }
        if (OverheadElement != null)
        {
            OverheadElement.RemoveFromHierarchy();
        }
        Destroy(WorldObject);
    }

    public void UpdateTokenPanel(string elementName)
    {
        VisualElement panel = UI.System.Q(elementName);
        if (Graphic != null)
        {
            panel.Q("Portrait").style.backgroundImage = GraphicSingle;
        }
        ISystemToken st = SystemTokenRegistry.DoInterfaceCallback(System, SystemData);
        st.UpdateTokenPanel(this, elementName);
        Name = st.Label();
        panel.Q<Label>("Name").text = Name;
        panel.Q("ClassBackground").style.borderTopColor = Color;
        panel.Q("ClassBackground").style.borderRightColor = Color;
        panel.Q("ClassBackground").style.borderBottomColor = Color;
        panel.Q("ClassBackground").style.borderLeftColor = Color;
        UI.ToggleDisplay(panel.Q<Label>("Name"), Name.Length > 0);

    }

    // public void InitTokenPanels()
    // {
    //     Token selected = Token.GetSelected();
    //     Token focused = Token.GetFocused();

    //     // Debug.Log($"{selected?.Data.Name} | {focused?.Data.Name} set as panel tokens");

    //     if (focused && selected)
    //     {
    //         selected.Data.InitTokenPanel("LeftTokenPanel");
    //         focused.Data.InitTokenPanel("RightTokenPanel");
    //     }
    //     else if (focused && !selected)
    //     {
    //         UI.ToggleActiveClass("LeftTokenPanel", true);
    //         UI.ToggleActiveClass("RightTokenPanel", false);
    //         focused.Data.UpdateTokenPanel("LeftTokenPanel");
    //     }
    //     else if (selected && !focused)
    //     {
    //         UI.ToggleActiveClass("LeftTokenPanel", true);
    //         UI.ToggleActiveClass("RightTokenPanel", false);
    //         selected.Data.UpdateTokenPanel("LeftTokenPanel");
    //     }
    // }

    public ISystemToken GetSystemToken()
    {
        return SystemTokenRegistry.DoInterfaceCallback(System, SystemData);
    }

    public void Delete()
    {
        UI.System.Q("UnitBar").Remove(UnitBarElement);
        UI.World.Q("Worldspace").Remove(OverheadElement);
        Destroy(WorldObject);
        Token.Deselect();
    }
}
