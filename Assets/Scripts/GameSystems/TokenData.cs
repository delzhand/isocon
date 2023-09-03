using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class TokenData : NetworkBehaviour
{
    [SyncVar]
    public string Name;

    [SyncVar]
    public string GraphicHash;

    [SyncVar]
    public string Json;

    [SyncVar]
    public bool OnField;

    public GameObject TokenObject;
    public VisualElement Element;
    public VisualElement overhead;
    public Texture2D Graphic;

    private bool initialized = false;
    private float awaitingGraphicSync = 0;

    void Update() {
        BaseUpdate();
    }

    public void Disconnect() {
        UI.System.Q("UnitBar").Remove(Element);
        UI.System.Q("Worldspace").Remove(overhead);
        Destroy(TokenObject);
        initialized = false;
    }

    public virtual void BaseUpdate() {
        if (!initialized && NeedsSetup() && Json.Length > 0) {
            DoTokenDataSetup();
        }

        // Every 5 seconds until the graphic is provided, recheck
        if (Graphic == null) {
            if (awaitingGraphicSync > 0) {
                awaitingGraphicSync -= Time.deltaTime;
            }
            else {
                initialized = false;
            }

        }


        if (!initialized && GraphicHash.Length > 0) {
            initialized = true;
            Graphic = TextureSender.LoadImageFromFile(GraphicHash, true);
            if (Graphic) {
                CreateWorldToken();
                CreateUnitBarItem();
                CreateOverhead();
            }
            else {
                awaitingGraphicSync = 2.5f;
            }
        }


        if (TokenObject) {
            TokenObject.transform.position = transform.position;
            TokenObject.transform.localScale = OnField ? Vector3.one : Vector3.zero;
        }
        if (overhead != null) {
            UpdateOverheadScreenPosition();
            UpdateUIData();
            overhead.style.display = (OnField ? DisplayStyle.Flex : DisplayStyle.None);
        }

    }

    public virtual bool NeedsSetup() {
        return false;
    }

    public virtual void UpdateUIData() {
    }

    public virtual void TokenDataSetup(string json) {
        Json = json;
    }

    public virtual void DoTokenDataSetup() {        
    }

    public virtual void CreateWorldToken() {
        TokenObject = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        TokenObject.transform.parent = GameObject.Find("Tokens").transform;
        OnField = false;

        Token token = TokenObject.GetComponent<Token>();
        token.SetImage(Graphic);
        token.onlineDataObject = gameObject;

        int size = GetSize();
        if (size == 2) {
            TokenObject.GetComponent<Token>().Size = 2;
            TokenObject.transform.Find("Offset").transform.localPosition += new Vector3(0, 0, -.73f);
            TokenObject.transform.Find("Base").transform.localPosition += new Vector3(0, 0, -.73f);
            TokenObject.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
            TokenObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2, 2, 4);
        }
        else if (size == 3) {
            TokenObject.GetComponent<Token>().Size = 3;
            TokenObject.transform.Find("Offset").transform.localScale = new Vector3(3, 3, 3);
            TokenObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(3, 3, 4);
        }        
    }

    public virtual void CreateUnitBarItem() {
        // Create the element in the UI
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/UnitTemplate");
        Element = template.Instantiate();
        Element.style.display = DisplayStyle.Flex;

        // Set the UI portrait
        float height = 60;
        float width = 60;
        if (Graphic.width > Graphic.height) {
            height *= (Graphic.height/(float)Graphic.width);
        }
        else {
            width *= (Graphic.width/(float)Graphic.height);
        }
        Element.Q("Portrait").style.backgroundImage = Graphic;
        Element.Q("Portrait").style.width = width;
        Element.Q("Portrait").style.height = height;

        Element.RegisterCallback<ClickEvent>((evt) => {
            Token t = TokenObject.GetComponent<Token>();
            TokenController.TokenClick(t);
        });

        // Add it to the UI
        UI.System.Q("UnitBar").Add(Element);
    }

    public virtual void CreateOverhead() {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/Overhead");
        VisualElement instance = template.Instantiate();
        overhead = instance.Q("Overhead");
        overhead.Q<VisualElement>("Color").style.display = DisplayStyle.None;
        overhead.Q<VisualElement>("Elite").style.display = DisplayStyle.None;
        UI.System.Q("Worldspace").Add(overhead);
    }

    public virtual int GetSize() {
        return 1;
    }

    private void DestroyOverhead() {
        if (overhead != null) {
            UI.System.Remove(overhead);
        }
    }

    private void UpdateOverheadScreenPosition() {
        overhead.style.display = DisplayStyle.Flex;
        UI.FollowToken(TokenObject.GetComponent<Token>(), overhead, Camera.main, Vector2.zero, true);
    }
}
