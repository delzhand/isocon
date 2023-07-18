using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraControl : MonoBehaviour
{
    public static Transform CameraTransform;

    private float lerpTimer;
    private float lerpDuration;

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private Quaternion reserveRotation;

    private Vector3 originPosition;
    private Vector3 targetPosition;
    private Vector3 reservePosition;

    private bool isLocked;
    private bool overhead;

    // Start is called before the first frame update
    void Start()
    {
        isLocked = false;
        overhead = false;
        CameraTransform = GameObject.Find("CameraOrigin").transform;
        registerCallbacks();
    }

    private void registerCallbacks() {
        UI.SetBlocking(UI.System, new string[]{"RotateCCW", "RotateCW", "CameraControls"});

        UI.System.Q<Button>("RotateCCW").RegisterCallback<ClickEvent>(rotateLeft);
        // UI.AttachHelp(UI.System, "RotateCCW", "Rotate the battlefield counter-clockwise.");

        UI.System.Q<Button>("RotateCW").RegisterCallback<ClickEvent>(rotateRight);
        // UI.AttachHelp(UI.System, "RotateCW", "Rotate the battlefield clockwise.");

        UI.System.Q<Slider>("ZoomScale").RegisterValueChangedCallback(zoom);
        // UI.AttachHelp(UI.System, "ZoomScale", "Zoom in or out.");

        UI.System.Q("OverheadSwitch").RegisterCallback<ClickEvent>((evt) => {
            if (overhead) {
                disableOverhead();
            }
            else {
                enableOverhead();
            }
        });
        // UI.AttachHelp(UI.System, "OverheadToggle", "Toggle an overhead fixed perspective.");

        // UI.System.Q<Toggle>("IndicatorSwitch").RegisterValueChangedCallback(toggleIndicators);
        // UI.AttachHelp(UI.System, "IndicatorSwitch", "Toggle row and column indicators.");
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocked) {
            if (lerpTimer <= lerpDuration) {
                lerpTimer += Time.deltaTime;
                float percent = Mathf.Clamp(lerpTimer / lerpDuration, 0, 1);
                CameraTransform.rotation = Quaternion.Lerp(originalRotation, targetRotation, percent);
                CameraTransform.position = Vector3.Lerp(originPosition, targetPosition, percent);
            }
            if (lerpTimer > lerpDuration) {
                isLocked = false;
            }
        }
    }

    private void rotateLeft(ClickEvent evt) {
        rotate(90);
    }

    private void rotateRight(ClickEvent evt) {
        rotate(-90);
    }

    private void rotate(float value) {
        if (!isLocked && !overhead) {
            initializeTransition(.25f);
            targetRotation = originalRotation * Quaternion.Euler(0, value, 0);
        }
    }

    public static void GoToBlock(Block block) {
        if (block != null) {
            CameraControl.CameraTransform.GetComponent<CameraControl>().Translate(block.transform.position);
        }
    }

    public void Translate(Vector3 value) {
        if (!isLocked /*&& !overhead */) {
            initializeTransition(.25f);
            targetPosition = value;
        }
    }

    private void zoom(ChangeEvent<float> evt) {
        Camera.main.GetComponent<Camera>().orthographicSize = evt.newValue;
    }

    private void enableOverhead() {
        overhead = true;
        reserveRotation = GameObject.Find("CameraOrigin").transform.rotation;
        initializeTransition(.25f);
        targetRotation = Quaternion.Euler(0, 0, 30);
        UI.System.Q("OverheadSwitch").AddToClassList("active");
    }

    private void disableOverhead() {
        overhead = false;
        initializeTransition(.25f);
        if (reserveRotation != null) {
            targetRotation = reserveRotation;
        }
        UI.System.Q("OverheadSwitch").RemoveFromClassList("active");
    }

    // private void toggleIndicators(ChangeEvent<bool> evt) {
    //     TerrainController.ToggleIndicators(evt.newValue);
    // }

    private void initializeTransition(float duration) {
        targetPosition = GameObject.Find("CameraOrigin").transform.position;
        originPosition = targetPosition;
        targetRotation = GameObject.Find("CameraOrigin").transform.rotation;
        originalRotation = targetRotation;
        lerpTimer = 0;
        lerpDuration = duration;
        isLocked = true;
    }

}
