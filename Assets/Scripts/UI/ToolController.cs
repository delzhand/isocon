using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolController : MonoBehaviour
{
    private List<string> editOps = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        registerCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void registerCallbacks() {
        UI.SetBlocking(UI.System, new string[]{"EditTools"});
        UI.System.Query<Button>(null, "tool-button").ForEach(registerButton);
    }

    private void registerButton(Button button) {
        button.clickable.clickedWithEventInfo += buttonClick;
    }

    public List<string> GetOps() {
        return editOps;
    }

    private void buttonClick(EventBase obj) {
        Button button = (Button)obj.target;
        switch (button.name) {
            case "CenterView":
            case "Paint":
            case "Empty":
            case "AddBlock":
            case "Impassable":
            case "RemoveBlock":
            case "CloneRow":
            case "RemoveRow":
            case "CloneCol":
            case "RemoveCol":
            case "Solid":
            case "Slope":
            case "Hidden":
            case "ClearMarks":
                clearAll();
                break;
            case "Difficult":
            case "Pit":
            case "Dangerous":
            case "Interactive":
                clearExclusive();
                break;
        }
        UI.System.Query<Button>(null, "tool-button").ForEach(disableButton);
        editOps.Add(button.name);
        foreach(string editOp in editOps) {
            UI.System.Q(editOp).AddToClassList("active");
        }
    }

    private void clearAll() {
        editOps.Clear();
        UI.System.Query<Button>(null, "tool-button").ForEach(disableButton);
    }

    private void disableButton(Button button) {
        button.RemoveFromClassList("active");
    }

    private void clearExclusive() {
        editOps.Remove("CenterView");
        editOps.Remove("Paint");
        editOps.Remove("Empty");
        editOps.Remove("AddBlock");
        editOps.Remove("Impassable");
        editOps.Remove("RemoveBlock");
        editOps.Remove("CloneRow");
        editOps.Remove("RemoveRow");
        editOps.Remove("CloneCol");
        editOps.Remove("RemoveCol");
        editOps.Remove("Solid");
        editOps.Remove("Slope");
        editOps.Remove("Hidden");
        editOps.Remove("ClearMarks");
    }
}
