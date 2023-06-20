using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum Mode {
    View,
    Add,
    Delete,
    Alter
}

public class ModeController : MonoBehaviour
{
    public Mode CurrentMode;
    public AlterOption CurrentAlter = AlterOption.Shape_Solid;

    // Start is called before the first frame update
    void Start()
    {
        RegisterCallbacks();
        viewMode(null);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void RegisterCallbacks() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();

        Button viewModeButton = modeUI.rootVisualElement.Q("ViewMode") as Button;
        viewModeButton.RegisterCallback<ClickEvent>(viewMode);

        Button addModeButton = modeUI.rootVisualElement.Q("AddMode") as Button;
        addModeButton.RegisterCallback<ClickEvent>(addMode);

        Button deleteModeButton = modeUI.rootVisualElement.Q("DeleteMode") as Button;
        deleteModeButton.RegisterCallback<ClickEvent>(deleteMode);

        Button alterModeButton = modeUI.rootVisualElement.Q("AlterMode") as Button;
        alterModeButton.RegisterCallback<ClickEvent>(alterMode);

        EnumField alterOptionField = modeUI.rootVisualElement.Q("AlterOptionField") as EnumField;
        alterOptionField.RegisterValueChangedCallback(alterOption);
    }

    public static Mode GetMode() {
        return GameObject.Find("Mode").GetComponent<ModeController>().CurrentMode;
    }

    public static AlterOption GetAlterOption() {
        return GameObject.Find("Mode").GetComponent<ModeController>().CurrentAlter;
    }

    private void viewMode(ClickEvent evt) {
        CurrentMode = Mode.View;
        activateButton("ViewMode");
        Block.ToggleSpacers(false);
    }

    private void addMode(ClickEvent evt) {
        CurrentMode = Mode.Add;
        activateButton("AddMode");
        Block.ToggleSpacers(false);
    }

    private void deleteMode(ClickEvent evt) {
        CurrentMode = Mode.Delete;
        activateButton("DeleteMode");
        Block.ToggleSpacers(false);
    }

    private void alterMode(ClickEvent evt) {
        CurrentMode = Mode.Alter;
        activateButton("AlterMode");
        Block.ToggleSpacers(false);
    }

    private void activateButton(string name) {
        UIDocument doc = GameObject.Find("ModeUI").GetComponent<UIDocument>();

        (doc.rootVisualElement.Q("AddMode") as Button).RemoveFromClassList("active");
        (doc.rootVisualElement.Q("DeleteMode") as Button).RemoveFromClassList("active");
        (doc.rootVisualElement.Q("ViewMode") as Button).RemoveFromClassList("active");
        (doc.rootVisualElement.Q("AlterMode") as Button).RemoveFromClassList("active");

        (doc.rootVisualElement.Q(name) as Button).AddToClassList("active");
    }

    private void alterOption(ChangeEvent<Enum> evt) {
        CurrentAlter = (AlterOption)evt.newValue;
    }


}
