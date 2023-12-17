using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Frame : MonoBehaviour
{
    void Start()
    {
        // UI.ToggleDisplay("Frame", false);
        UI.SetBlocking(UI.System, new string[]{"BottomBar"});

        UI.System.Q("FloatingControls").Q("EditButton").RegisterCallback<ClickEvent>((evt) => {
            TerrainController.Editing = !TerrainController.Editing;
        });
    }

    void Update()
    {
        VisualElement editButton = UI.System.Q("FloatingControls").Q("EditButton");
        UI.ToggleDisplay(editButton, Player.IsGM());
        UI.ToggleActiveClass(editButton, TerrainController.Editing);

        UI.ToggleDisplay("ToolsPanel", TerrainController.Editing && Player.IsGM());
    }
}
