using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolsSidebar : MonoBehaviour
{
    private static List<string> editOps = new List<string>();
    public static string MarkerEffect;

    // Start is called before the first frame update
    void Start()
    {
        registerCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
        UI.ToggleDisplay("EditButton", Player.IsGM());
        UI.ToggleDisplay("ToolsSidebar", TerrainController.Editing && Player.IsGM());
        UI.ToggleActiveClass("EditButton", TerrainController.Editing);
        HandleMarkFields();
        if (!TerrainController.Editing) {
            UI.ToggleDisplay("ColorSidebar", false);
        }
    }

    public static List<string> GetOps() {
        return editOps;
    }

    private void HandleMarkFields() {
        DropdownField markField = UI.System.Q<DropdownField>("MarkField");
        TextField otherMark = UI.System.Q<TextField>("OtherMark");
        if (editOps.Contains("AddMark")) {
            UI.ToggleDisplay(markField, true);
        }
        else {
            UI.ToggleDisplay(markField, false);
            UI.ToggleDisplay(otherMark, false);
        }    
        MarkerEffect = markField.value;
        if (markField.value == "Custom") {
            UI.ToggleDisplay(otherMark, true);
            MarkerEffect = otherMark.value;
        }
        else {
            UI.ToggleDisplay(otherMark, false);
        }
    }

    private void registerCallbacks() {
        UI.SetBlocking(UI.System, new string[]{"ToolsSidebar"});
        UI.System.Query<Button>(null, "tool-button").ForEach(registerButton);

        RegisterColorChangeCallback("Color1");
        RegisterColorChangeCallback("Color2");
        RegisterColorChangeCallback("Color3");
        RegisterColorChangeCallback("Color4");
        RegisterColorChangeCallback("Color5");
        RegisterColorChangeCallback("Color6");
    }

    private void RegisterColorChangeCallback(string elementName) {
        UI.System.Q(elementName).RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("ColorSidebar", true);
            ColorSidebar colorSidebar = FindObjectOfType<ColorSidebar>();
            colorSidebar.ClearColorChangeListeners();
            ColorSidebar.SetColor(UI.System.Q(elementName).resolvedStyle.backgroundColor);
            colorSidebar.onColorChange += (c) => HandleColorChange(elementName, c);
        });
    }
    private void HandleColorChange(string elementName, Color c) {
        UI.System.Q(elementName).style.backgroundColor = c;
        switch(elementName) {
            case "Color1":
                Environment.Color1 = c;
                Block.SetColor("top1", c);
                Block.SetColor("top2", ColorSidebar.DarkenColor(c, .2f));
                break;
            case "Color2":
                Environment.Color2 = c;
                Block.SetColor("side1", c);
                Block.SetColor("side2", ColorSidebar.DarkenColor(c, .2f));
                break;
            case "Color3":
                Environment.Color3 = c;
                MeshRenderer mra = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
                mra.material.SetColor("_Color1", c);
                break;
            case "Color4":
                Environment.Color4 = c;
                MeshRenderer mrb = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
                mrb.material.SetColor("_Color2", c);
                break;
            case "Color5":
                Environment.Color5 = c;
                break;
            case "Color6":
                Environment. Color6 = c;
                break;
        }
    }

    private void registerButton(Button button) {
        button.clickable.clickedWithEventInfo += buttonClick;
    }

    private void buttonClick(EventBase obj) {
        UI.System.Query<Button>(null, "tool-button").ForEach(disableButton);
        Button button = (Button)obj.target;
        button.AddToClassList("active");
        editOps.Clear();
        editOps.Add(button.name);
    }

    private void disableButton(Button button) {
        button.RemoveFromClassList("active");
    }
}
