using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenData : NetworkBehaviour
{
    [SyncVar]
    public string Name;

    [SyncVar]
    public string GraphicHash;

    public GameObject TokenObject;
    public VisualElement Element;
    public VisualElement overhead;

    void Update() {
        BaseUpdate();
    }

    public virtual void BaseUpdate() {
        if (TokenObject) {
            TokenObject.transform.position = transform.position;
        }
        if (overhead != null) {
            UpdateOverheadScreenPosition();
            UpdateUIData();
        }
    }

    public virtual void UpdateUIData() {
    }

    public virtual void Initialize(string json) {
        TokenObject = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        TokenObject.transform.parent = GameObject.Find("Tokens").transform;
        Texture2D graphic = TextureSender.LoadImageFromFile(GraphicHash, true);
        Token token = TokenObject.GetComponent<Token>();
        token.SetImage(graphic);
        token.onlineDataObject = gameObject;

        // Create UI elements
        // CreateUnitBarItem(graphic);
        // CreateOverhead();
    }

    private void CreateUnitBarItem(Texture2D graphic) {
        // Create the element in the UI
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/UnitTemplate");
        Element = template.Instantiate();
        Element.style.display = DisplayStyle.Flex;

        // Set the UI portrait
        float height = 60;
        float width = 60;
        if (graphic.width > graphic.height) {
            height *= (graphic.height/(float)graphic.width);
        }
        else {
            width *= (graphic.width/(float)graphic.height);
        }
        Element.Q("Portrait").style.backgroundImage = graphic;
        Element.Q("Portrait").style.width = width;
        Element.Q("Portrait").style.height = height;

        Element.RegisterCallback<ClickEvent>((evt) => {
            TokenObject.GetComponent<Token>().Select(true);
        });

        // Add it to the UI
        UI.System.Q("UnitBar").Add(Element);
    }

    private void CreateOverhead() {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/Overhead");
        VisualElement instance = template.Instantiate();
        overhead = instance.Q("Overhead");
        overhead.Q<VisualElement>("Color").style.display = DisplayStyle.None;
        overhead.Q<VisualElement>("Elite").style.display = DisplayStyle.None;
        UI.System.Q("Worldspace").Add(overhead);
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
