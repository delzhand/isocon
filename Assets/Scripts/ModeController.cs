using System;
using System.Collections;
using System.Collections.Generic;
using IsoconUILibrary;
using UnityEngine;
using UnityEngine.UIElements;

public enum ClickMode {
    Play,
    Edit,
}

public class ModeController : MonoBehaviour
{
    public static ClickMode Mode = ClickMode.Play;

    public bool ClickSuppress;

    private List<VisualElement> elements = new List<VisualElement>();

    // Start is called before the first frame update
    void Start()
    {        
        registerCallbacks();
        playMode();
        // Mode = ClickMode.Play;
        // setup();
        // UI.SetBlocking(UI.System, new string[]{"ModeControls", "HelpBar", "EditMapFlyout", "RotateCCW", "RotateCW", "CamControls", "AddTokenModal"});
        // enterPlayMode(null);
    }

    void Update() {
        // ClickSuppress = UI.ClicksSuspended;
    }

    private void registerCallbacks() {
        UI.SetBlocking(UI.System, new string[]{"ModeSwitch"});

        UI.System.Q("ModeSwitch").RegisterCallback<ClickEvent>((evt) => {
            if (Mode == ClickMode.Play) {
                editMode();
            }   
            else {
                playMode();
            }
        });
    }

    private void editMode() {
        TerrainController.Reorg();
        Mode = ClickMode.Edit;
        UI.System.Q("ModeSwitch").RemoveFromClassList("active");
        UI.System.Q("SelectedTokenPanel").RemoveFromClassList("active");
        UI.System.Q("EditTokenPanel").RemoveFromClassList("active");
        UI.System.Q("EditTools").AddToClassList("active");
        GameObject.Find("ReserveCamera").GetComponent<Camera>().enabled = false;
        UI.System.Q("TerrainInfo").style.display = DisplayStyle.None;
        Block.ToggleSpacers(true);
    }

    private void playMode() {
        Mode = ClickMode.Play;
        UI.System.Q("ModeSwitch").AddToClassList("active");
        UI.System.Q("EditTools").RemoveFromClassList("active");
        GameObject.Find("ReserveCamera").GetComponent<Camera>().enabled = true;
        UI.System.Q<SlideToggle>("TokenEditToggle").value = false;
        UI.System.Q("TerrainInfo").style.display = DisplayStyle.Flex;
        Block.ToggleSpacers(false);
    }

    private void setup() {
    }

    public void ActivateElement(VisualElement v) {
        elements.Add(v);
        v.AddToClassList("active");
        if (v.ClassListContains("modal")) {
            v.style.display = DisplayStyle.Flex;
        }
    }

    public void ActivateElementByName(string name) {
        VisualElement v = UI.System.Q(name);
        ActivateElement(v);
    }

    public void DeactivateElement(VisualElement v) {
        v.RemoveFromClassList("active");
        if (v.ClassListContains("modal")) {
            v.style.display = DisplayStyle.None;
        }
    }

    public void DeactivateByName(string name) {
        VisualElement v = UI.System.Q(name);
        DeactivateElement(v);
    }
}
