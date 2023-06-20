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

        UIDocument cameraUI = GameObject.Find("CameraUI").GetComponent<UIDocument>();

        Button rotateLeftButton = cameraUI.rootVisualElement.Q("RotateLeftButton") as Button;
        rotateLeftButton.RegisterCallback<ClickEvent>(rotateLeft);

        Button rotateRightButton = cameraUI.rootVisualElement.Q("RotateRightButton") as Button;
        rotateRightButton.RegisterCallback<ClickEvent>(rotateRight);

        Slider zoomSlider = cameraUI.rootVisualElement.Q("ZoomSlider") as Slider;
        zoomSlider.RegisterValueChangedCallback(zoom);

        Toggle overheadToggle = cameraUI.rootVisualElement.Q("OverheadToggle") as Toggle;
        overheadToggle.RegisterValueChangedCallback(toggleOverhead);

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

    public static void ToggleUI(bool visible) {
        
    }
    
    private void rotateLeft(ClickEvent evt) {
        rotate(90);
    }

    private void rotateRight(ClickEvent evt) {
        rotate(-90);
    }

    private void rotate(float value) {
        if (!isLocked && !overhead) {
            initializeTransition(.35f);
            targetRotation = originalRotation * Quaternion.Euler(0, value, 0);
        }
    }

    public static void GoToBlock(Block block) {
        CameraControl.CameraTransform.GetComponent<CameraControl>().Translate(block.transform.position);
    }

    public void Translate(Vector3 value) {
        if (!isLocked && !overhead) {
            initializeTransition(.35f);
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
                initializeTransition(.35f);
                targetRotation = Quaternion.Euler(0, 0, 30);
                targetPosition = TerrainEngine.Center();
            }
            else {
                overhead = false;
                initializeTransition(.35f);
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
