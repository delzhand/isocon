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

    private static float yRotation = 315;
    private static float zRotation = 0; // -20 to 20

    private static Vector3 OriginPosition;
    private static Vector3 TargetPosition;
    private static Vector3 ReservePosition;

    private static bool IsLocked;
    public static bool Overhead;
    public static bool PanMode = true;

    void Start()
    {
        IsLocked = false;
        Overhead = false;
        CameraTransform = GameObject.Find("CameraOrigin").transform;
        registerCallbacks();
    }

    private static void registerCallbacks() {
        // UI.TopBar.Q("RotateCCW").RegisterCallback<ClickEvent>(rotateLeft);
        // UI.TopBar.Q("RotateCW").RegisterCallback<ClickEvent>(rotateRight);
        // UI.TopBar.Q<Slider>("ZoomSlider").RegisterValueChangedCallback(zoom);
        // UI.TopBar.Q<Slider>("TiltSlider").RegisterValueChangedCallback(tilt);

        UI.TopBar.Q("FixedView").RegisterCallback<ClickEvent>((evt) => {
            if (Overhead) {
                disableOverhead();
            }
            else {
                enableOverhead();
            }
        });

        UI.TopBar.Q("DragMode").RegisterCallback<ClickEvent>((evt) => {
            if (PanMode) {
                disablePanMode();
            }
            else {
                enablePanMode();
            }
        });
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

        if (!UI.ClicksSuspended && Player.IsOnline() && !Modal.IsOpen()) {
            Vector3 view = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            bool isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (!isOutside && scroll != 0) {
                float z = UI.System.Q<Slider>("ZoomSlider").value;
                z += scroll;
                Camera.main.GetComponent<Camera>().orthographicSize = z;
                UI.System.Q<Slider>("ZoomSlider").value = z;
            }
        }
    }

    private static void rotateLeft(ClickEvent evt) {
        rotate(15);
    }

    private static void rotateRight(ClickEvent evt) {
        rotate(-15);
    }

    private static void rotate(float value) {
        if (!IsLocked && !Overhead) {
            initializeTransition(.25f);
            yRotation += value;
            // TargetRotation = OriginalRotation * Quaternion.Euler(0, value, 0);

            Quaternion qy = Quaternion.Euler(0f, yRotation, 0f);
            Quaternion qz = Quaternion.Euler(0f, 0f, zRotation);
            
            Quaternion q = Quaternion.identity;
            q *= qy;
            q *= qz;

            TargetRotation = q;
        }
    }

    public static void GoToBlock(Block block) {
        if (block != null) {
            CameraControl.CameraTransform.GetComponent<CameraControl>().Translate(block.transform.position);
        }
    }

    public static void GoToToken(Token token) {
        if (token != null) {
            CameraControl.CameraTransform.GetComponent<CameraControl>().Translate(token.transform.position);
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

    private static void tilt(ChangeEvent<float> evt) {
        zRotation = evt.newValue;

        Quaternion qy = Quaternion.Euler(0f, yRotation, 0f);
        Quaternion qz = Quaternion.Euler(0f, 0f, zRotation);

        Quaternion q = Quaternion.identity;
        q *= qy;
        q *= qz;
        CameraTransform.rotation = q;
    }

    // private static void tiltUp(ClickEvent evt) {
    //     tilt(-5);
    // }

    // private static void tiltDown(ClickEvent evt) {
    //     tilt(5);
    // }

    // private static void tilt(float value) {
    //     if (!IsLocked && !Overhead) {
    //         initializeTransition(.25f);
    //         zRotation += value;
    //         zRotation = Mathf.Clamp(zRotation, -20, 20);

    //         Quaternion qy = Quaternion.Euler(0f, yRotation, 0f);
    //         Quaternion qz = Quaternion.Euler(0f, 0f, zRotation);
            
    //         Quaternion q = Quaternion.identity;
    //         q *= qy;
    //         q *= qz;

    //         TargetRotation = q;
    //     }
    // }

    private static void enableOverhead() {
        Overhead = true;
        ReserveRotation = CameraTransform.rotation;
        initializeTransition(.25f);
        TargetRotation = Quaternion.Euler(0, 0, 30);
        UI.TopBar.Q("FixedView").AddToClassList("active");
    }

    private static void disableOverhead() {
        Overhead = false;
        initializeTransition(.25f);
        if (ReserveRotation != null) {
            TargetRotation = ReserveRotation;
        }
        UI.TopBar.Q("FixedView").RemoveFromClassList("active");
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

    private static void disablePanMode() {
        PanMode = false;
        UI.TopBar.Q("DragMode").Q<Label>("Label").text = "Rotate Camera";
    }

    private static void enablePanMode() {
        PanMode = true;
        UI.TopBar.Q("DragMode").Q<Label>("Label").text = "Pan Camera";
    }
}
