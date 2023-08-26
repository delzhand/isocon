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
        if (markField.value == "Other") {
            UI.ToggleDisplay(otherMark, true);
            MarkerEffect = otherMark.value;
        }
        else {
            UI.ToggleDisplay(otherMark, false);
        }
    }

    public static List<string> GetOps() {
        return editOps;
    }


    private void registerCallbacks() {
        UI.SetBlocking(UI.System, new string[]{"ToolsSidebar"});
        UI.System.Query<Button>(null, "tool-button").ForEach(registerButton);
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
