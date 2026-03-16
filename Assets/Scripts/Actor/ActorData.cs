using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ActorData : NetworkBehaviour
{
    [SyncVar]
    public string Type;
    [SyncVar]
    public string Id;
    [SyncVar]
    public string Name = "";
    [SyncVar]
    public Token Token;
    [SyncVar]
    public string Shape;
    [SyncVar]
    public Color Color;
    [SyncVar]
    public string TypeData;

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
        CreateWorldActor();
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

                var texture = TokenSync.LoadHashedFileAsTexture(Token.Hash);
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
                UI.FollowTransform(WorldObject.GetComponent<Actor>().transform.Find("Offset/Avatar/Cutout/Cutout Quad/LabelAnchor").transform, OverheadElement, UI.World, Camera.main, Vector2.zero);
            }
            IActorType st = ActorTypeRegistry.DoInterfaceCallback(Type, TypeData);
            st.UpdateOverhead(this);
        }

        // if (GameSystem.Current() != null)
        // {
        //     GameSystem.Current().UpdateData(this);
        // }
        gameObject.name = $"ActorData:{Name}";
    }

    private void LoadGraphic()
    {
        FileLogger.Write($"Load graphic for {Token.TruncateHash(Token.Hash)}");
        Texture2D graphic = TokenSync.LoadHashedFileAsTexture(Token.Hash);
        if (graphic)
        {
            FileLogger.Write($"Hashed image {Token.TruncateHash(Token.Hash)} found locally");
            SetGraphic(graphic);
        }
        else
        {
            FileLogger.Write($"Hashed image {Token.TruncateHash(Token.Hash)} not found locally, starting sync");
            TokenSync.Add(Token, this);
        }
    }

    private void CreateWorldActor()
    {
        WorldObject = Instantiate(Resources.Load<GameObject>("Prefabs/Actor"));
        WorldObject.transform.parent = GameObject.Find("Actors").transform;
        if (!Placed)
        {
            Place(false);
        }
        else
        {
            Place();
            WorldObject.transform.position = LastKnownPosition;
        }
        WorldObject.GetComponent<Actor>().Data = this;
        SetShape();
    }

    public void CreateOverheadElement()
    {
        IActorType st = ActorTypeRegistry.DoInterfaceCallback(Type, TypeData);
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

    public bool CornerTargeting()
    {
        string[] cornerShapes = StringUtility.CreateArray("Square 2x2", "Square 4x4", "Hex 2", "Hex 4");
        return cornerShapes.Contains(Shape);
    }

    public void SetShape()
    {
        switch (Shape)
        {
            case "Square 1/2":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(.75f, .75f, .75f);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Square", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(.6f, .6f, 4);
                break;
            case "Square 1x1":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(1, 1, 1);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Square", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(.7f, .7f, 4);
                break;
            case "Square 2x2":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Square", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(1.7f, 1.7f, 4);
                break;
            case "Square 3x3":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(2f, 2f, 2f);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Square", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2.7f, 2.7f, 4);
                break;
            case "Square 4x4":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Square", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(3.7f, 3.7f, 4);
                break;
            case "Hex 1/2":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(.75f, .75f, .75f);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Hex1", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(.6f, .6f, 4);
                break;
            case "Hex 1":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(1, 1, 1);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Hex1", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(1.12f, 1.12f, 4);
                break;
            case "Hex 2":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Hex2", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2.2f, 2.2f, 4);
                break;
            case "Hex 3":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Hex3", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2.7f, 2.7f, 4);
                break;
            case "Hex 4":
                WorldObject.transform.Find("Offset").transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().material = Shadow.GetMaterial("Hex4", ColorUtility.GetHex(Color));
                WorldObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(5f, 5f, 4);
                break;
        }
    }

    public Actor GetActor()
    {
        return WorldObject.GetComponent<Actor>();
    }

    private void CreateUnitBarElement()
    {
        // Create the element in the UI
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UI/UnitTemplate");
        UnitBarElement = template.Instantiate();
        UnitBarElement.style.display = DisplayStyle.Flex;

        Actor t = WorldObject.GetComponent<Actor>();
        UnitBarElement.RegisterCallback<MouseEnterEvent>((evt) =>
        {
            t.Focus();
            Pointer.UnitBarMouseoverActor = t;
        });
        UnitBarElement.RegisterCallback<MouseLeaveEvent>((evt) =>
        {
            t.Unfocus();
            Pointer.UnitBarMouseoverActor = null;
        });

        // Add it to the UI
        UI.System.Q("UnitBar").Add(UnitBarElement);
    }

    public void SetGraphic(Texture2D graphic)
    {
        Graphic = graphic;
        SetGraphicSingle();

        // Set the world object graphic
        Actor actor = WorldObject.GetComponent<Actor>();
        actor.SetImage(Graphic);

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
        Color[] pixels = Graphic.GetPixels(0, 0, Graphic.width / Token.Frames, Graphic.height);
        GraphicSingle = new Texture2D(Graphic.width / Token.Frames, Graphic.height);
        GraphicSingle.SetPixels(pixels);
        GraphicSingle.Apply();
        GraphicSingle.wrapMode = TextureWrapMode.Clamp;
        GraphicSingle.filterMode = FilterMode.Point;
    }

    public static ActorData Find(string id)
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("ActorData"))
        {
            ActorData data = g.GetComponent<ActorData>();
            if (data.Id == id)
            {
                return data;
            }
        }
        return null;
    }

    public static void Command(string actorId, string command)
    {
        ActorData data = ActorData.Find(actorId);
        data.HandleCommand(command);
    }

    private void HandleCommand(string command)
    {
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }

        IActorType st = ActorTypeRegistry.DoInterfaceCallback(Type, TypeData);
        st.Command(command, this);
        TypeData = st.Serialize();
        // NeedsRedraw = true;
    }

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

    public void UpdateActorPanel(string elementName)
    {
        VisualElement panel = UI.System.Q(elementName);
        if (Graphic != null)
        {
            panel.Q("Portrait").style.backgroundImage = GraphicSingle;
        }
        IActorType st = ActorTypeRegistry.DoInterfaceCallback(Type, TypeData);
        st.UpdatePanel(this, elementName);
        Name = st.Label();
        panel.Q<Label>("Name").text = Name;
        panel.Q("ClassBackground").style.borderTopColor = Color;
        panel.Q("ClassBackground").style.borderRightColor = Color;
        panel.Q("ClassBackground").style.borderBottomColor = Color;
        panel.Q("ClassBackground").style.borderLeftColor = Color;
        UI.ToggleDisplay(panel.Q<Label>("Name"), Name.Length > 0);

    }

    public IActorType GetActorType()
    {
        return ActorTypeRegistry.DoInterfaceCallback(Type, TypeData);
    }

    public void Delete()
    {
        UI.System.Q("UnitBar").Remove(UnitBarElement);
        UI.World.Q("Worldspace").Remove(OverheadElement);
        Destroy(WorldObject);
        Actor.Deselect();
    }

    public ActorPersistence Persist()
    {
        ActorPersistence ap = new();
        ap.ActorType = TypeData;
        ap.Name = Name;
        ap.Token = Token;
        ap.Shape = Shape;
        ap.Color = Color;
        ap.ActorTypeId = Type;
        ap.Placed = Placed;
        ap.Position = GetActor().transform.position;
        return ap;
    }
}

[Serializable]
public class ActorPersistence
{
    public string Name;
    public Token Token;
    public string ActorTypeId;
    public string ActorType;
    public string Shape;
    public Vector3 Position;
    public Color Color;
    public bool Placed;
}