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
        UI.SetBlocking(UI.System, new string[]{"CameraControls"});

        UI.System.Q<Button>("RotateCCW").RegisterCallback<ClickEvent>(rotateLeft);
        UI.System.Q<Button>("RotateCW").RegisterCallback<ClickEvent>(rotateRight);
        UI.System.Q<Slider>("ZoomScale").RegisterValueChangedCallback(zoom);

        UI.System.Q<Button>("OverheadButton").RegisterCallback<ClickEvent>((evt) => {
            if (overhead) {
                disableOverhead();
            }
            else {
                enableOverhead();
            }
        });

        UI.System.Q<Button>("IndicatorsButton").RegisterCallback<ClickEvent>((evt) => {
            bool val = !TerrainController.Indicators;
            TerrainController.Indicators = val;
            if (val) {
                UI.System.Q("IndicatorsButton").AddToClassList("active");
            }
            else {
                UI.System.Q("IndicatorsButton").RemoveFromClassList("active");
            }
        });

        UI.System.Q<Button>("EditButton").RegisterCallback<ClickEvent>((evt) => {
            bool val = !TerrainController.Editing;
            TerrainController.Editing = val;
            if (val) {
                UI.System.Q("EditButton").AddToClassList("active");
            }
            else {
                UI.System.Q("EditButton").RemoveFromClassList("active");
                State.SetCurrentJson();
                Player.Self().CmdRequestMapSync();
            }
        });
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
        UI.System.Q("OverheadButton").AddToClassList("active");
    }

    private void disableOverhead() {
        overhead = false;
        initializeTransition(.25f);
        if (reserveRotation != null) {
            targetRotation = reserveRotation;
        }
        UI.System.Q("OverheadButton").RemoveFromClassList("active");
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
