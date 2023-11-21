using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraControl : MonoBehaviour
{
    public static Transform CameraTransform;

    private static float LerpTimer;
    private static float LerpDuration;

    private static Quaternion OriginalRotation;
    private static Quaternion TargetRotation;
    private static Quaternion ReserveRotation;

    private static Vector3 OriginPosition;
    private static Vector3 TargetPosition;
    private static Vector3 ReservePosition;

    private static bool IsLocked;
    private static bool Overhead;

    void Start()
    {
        IsLocked = false;
        Overhead = false;
        CameraTransform = GameObject.Find("CameraOrigin").transform;
        registerCallbacks();
    }

    private static void registerCallbacks() {
        UI.System.Q("RotateCCW").RegisterCallback<ClickEvent>(rotateLeft);
        UI.System.Q("RotateCW").RegisterCallback<ClickEvent>(rotateRight);
        UI.System.Q<Slider>("ZoomScale").RegisterValueChangedCallback(zoom);
        // UI.System.Q<Slider>("TiltAngle").RegisterValueChangedCallback(tilt);
        UI.System.Q("Tilt").Q("TiltUp").RegisterCallback<ClickEvent>(tiltUp);
        UI.System.Q("Tilt").Q("TiltDown").RegisterCallback<ClickEvent>(tiltDown);

        // UI.System.Q<Button>("OverheadButton").RegisterCallback<ClickEvent>((evt) => {
        //     if (Overhead) {
        //         disableOverhead();
        //     }
        //     else {
        //         enableOverhead();
        //     }
        // });

        // UI.System.Q<Button>("IndicatorsButton").RegisterCallback<ClickEvent>((evt) => {
        //     bool val = !TerrainController.Indicators;
        //     TerrainController.Indicators = val;
        //     if (val) {
        //         UI.System.Q("IndicatorsButton").AddToClassList("active");
        //     }
        //     else {
        //         UI.System.Q("IndicatorsButton").RemoveFromClassList("active");
        //     }
        // });

        // UI.System.Q<Button>("EditButton").RegisterCallback<ClickEvent>((evt) => {
        //     bool val = !TerrainController.Editing;
        //     TerrainController.Editing = val;
        //     Block.ToggleSpacers(val);
        //     if (val) {
        //         UI.System.Q("EditButton").AddToClassList("active");
        //     }
        //     else {
        //         UI.System.Q("EditButton").RemoveFromClassList("active");
        //         State.SetCurrentJson();
        //         Player.Self().CmdMapSync();
        //     }
        // });
    }

    // Update is called once per frame
    void Update()
    {
        if (IsLocked) {
            if (LerpTimer <= LerpDuration) {
                LerpTimer += Time.deltaTime;
                float percent = Mathf.Clamp(LerpTimer / LerpDuration, 0, 1);
                CameraTransform.rotation = Quaternion.Lerp(OriginalRotation, TargetRotation, percent);
                CameraTransform.position = Vector3.Lerp(OriginPosition, TargetPosition, percent);
            }
            if (LerpTimer > LerpDuration) {
                IsLocked = false;
            }
        }
    }

    private static void rotateLeft(ClickEvent evt) {
        rotate(90);
    }

    private static void rotateRight(ClickEvent evt) {
        rotate(-90);
    }

    private static void rotate(float value) {
        if (!IsLocked && !Overhead) {
            initializeTransition(.25f);
            TargetRotation = OriginalRotation * Quaternion.Euler(0, value, 0);
        }
    }

    public static void GoToBlock(Block block) {
        if (block != null) {
            CameraControl.CameraTransform.GetComponent<CameraControl>().Translate(block.transform.position);
        }
    }

    public void Translate(Vector3 value) {
        if (!IsLocked /*&& !overhead */) {
            initializeTransition(.25f);
            TargetPosition = value;
        }
    }

    private static void zoom(ChangeEvent<float> evt) {
        Camera.main.GetComponent<Camera>().orthographicSize = evt.newValue;
    }

    private static void tiltUp(ClickEvent evt) {
        tilt(-5);
    }

    private static void tiltDown(ClickEvent evt) {
        tilt(5);
    }

    private static void tilt(float value) {
        if (!IsLocked && !Overhead) {
            initializeTransition(0f);
            TargetRotation = OriginalRotation * Quaternion.Euler(0, 0, value);
        }
    }

    private static void enableOverhead() {
        Overhead = true;
        ReserveRotation = CameraTransform.rotation;
        initializeTransition(.25f);
        TargetRotation = Quaternion.Euler(0, 0, 30);
        UI.System.Q("OverheadButton").AddToClassList("active");
    }

    private static void disableOverhead() {
        Overhead = false;
        initializeTransition(.25f);
        if (ReserveRotation != null) {
            TargetRotation = ReserveRotation;
        }
        UI.System.Q("OverheadButton").RemoveFromClassList("active");
    }

    private static void initializeTransition(float duration) {
        TargetPosition = CameraTransform.position;
        OriginPosition = TargetPosition;
        TargetRotation = CameraTransform.rotation;
        OriginalRotation = TargetRotation;
        LerpTimer = 0;
        LerpDuration = duration;
        IsLocked = true;
    }

}
