using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrentOp : NetworkBehaviour
{
    [SyncVar]
    public string Operation;

    [SyncVar]
    public bool Visible;

    private VisualElement element;

    // Start is called before the first frame update
    void Start()
    {
        element = new VisualElement();
        element.style.flexGrow = 0;
        element.style.backgroundColor = ColorUtility.UIBlue;
        element.style.borderTopWidth = 2;
        element.style.borderBottomWidth = 2;
        element.style.borderLeftWidth = 2;
        element.style.borderRightWidth = 2;
        element.style.borderTopLeftRadius = 18;
        element.style.borderTopRightRadius = 18;
        element.style.borderBottomLeftRadius = 18;
        element.style.borderBottomRightRadius = 18;
        element.style.borderTopColor = Color.white;
        element.style.borderBottomColor = Color.white;
        element.style.borderLeftColor = Color.white;
        element.style.borderRightColor = Color.white;
        element.style.paddingBottom = 0;
        element.style.paddingTop = 0;
        element.style.paddingLeft = 10;
        element.style.paddingRight = 10;
        element.style.marginLeft = 4;
        element.style.marginRight = 4;
        
        Label l = new();
        l.name = "Operation";
        element.Add(l);

        UI.System.Q("CurrentOps").Add(element);
    }

    // Update is called once per frame
    void Update()
    {

        element.Q<Label>("Operation").text = $"{ GetComponent<Player>().Name }: {Operation}";
        UI.ToggleDisplay(element, Visible);
    }

    public void Show(string op) {
        Operation = op;
        Visible = true;
    }

    public void Hide() {
        Visible = false;
    }
}
