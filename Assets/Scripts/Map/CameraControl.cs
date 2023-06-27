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

        UIDocument cameraUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();

        (cameraUI.rootVisualElement.Q("RotateLeftButton") as Button).RegisterCallback<ClickEvent>(rotateLeft);
        UI.AttachHelp(cameraUI, "RotateLeftButton", "Rotate the battlefield counter-clockwise.");

        (cameraUI.rootVisualElement.Q("RotateRightButton") as Button).RegisterCallback<ClickEvent>(rotateRight);
        UI.AttachHelp(cameraUI, "RotateRightButton", "Rotate the battlefield clockwise.");

        (cameraUI.rootVisualElement.Q("ZoomSlider") as Slider).RegisterValueChangedCallback(zoom);
        UI.AttachHelp(cameraUI, "ZoomSlider", "Zoom in or out.");

        (cameraUI.rootVisualElement.Q("OverheadToggle") as Toggle).RegisterValueChangedCallback(toggleOverhead);
        UI.AttachHelp(cameraUI, "OverheadToggle", "Toggle an overhead fixed perspective.");
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
        CameraControl.CameraTransform.GetComponent<CameraControl>().Translate(block.transform.position);
    }

    public void Translate(Vector3 value) {
        if (!isLocked && !overhead) {
            initializeTransition(.25f);
            targetPosition = value;
        }
    }

    private void zoom(ChangeEvent<float> evt) {
        Camera.main.GetComponent<Camera>().orthographicSize = evt.newValue;
    }

    private void toggleOverhead(ChangeEvent<bool> evt) {
        if (!isLocked) {
            if (!overhead) {
                overhead = true;
                reserveRotation = GameObject.Find("CameraOrigin").transform.rotation;
                reservePosition = GameObject.Find("CameraOrigin").transform.position;
                initializeTransition(.25f);
                targetRotation = Quaternion.Euler(0, 0, 30);
                targetPosition = TerrainEngine.Center();
            }
            else {
                overhead = false;
                initializeTransition(.25f);
                targetRotation = reserveRotation;
                targetPosition = reservePosition;
            }
        }
    }

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
