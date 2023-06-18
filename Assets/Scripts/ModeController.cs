using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum Mode {
    View,
    Add,
    Delete,
    Mark
}

public class ModeController : MonoBehaviour
{
    public Mode CurrentMode;

    // Start is called before the first frame update
    void Start()
    {
        CurrentMode = Mode.View;
        RegisterCallbacks();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void RegisterCallbacks() {
        UIDocument ui = GameObject.Find("CameraUI").GetComponent<UIDocument>();

        Button rotateLeftButton = ui.rootVisualElement.Q("RotateLeftButton") as Button;
        rotateLeftButton.RegisterCallback<ClickEvent>(rotateLeft);

        Button rotateRightButton = ui.rootVisualElement.Q("RotateRightButton") as Button;
        rotateRightButton.RegisterCallback<ClickEvent>(rotateRight);

        Slider zoomSlider = ui.rootVisualElement.Q("ZoomSlider") as Slider;
        zoomSlider.RegisterValueChangedCallback(zoom);

        Toggle overheadToggle = ui.rootVisualElement.Q("OverheadToggle") as Toggle;
        overheadToggle.RegisterValueChangedCallback(overhead);
    }

    private void rotateLeft(ClickEvent evt) {
        CameraControl.RotateLeft();
    }

    private void rotateRight(ClickEvent evt) {
        CameraControl.RotateRight();
    }

    private void zoom(ChangeEvent<float> evt) {
        CameraControl.Zoom(evt.newValue);
    }

    private void overhead(ChangeEvent<bool> evt) {
        CameraControl.ToggleOverhead();
    }

    public static Mode GetMode() {
        return GameObject.Find("Mode").GetComponent<ModeController>().CurrentMode;
    }

}
