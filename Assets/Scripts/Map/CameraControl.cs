using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraControl : MonoBehaviour
{
    public static bool PanMode = true;
    public static bool Drag = false;
    public Vector3 Origin;
    public Vector3 Difference;
    public float OriginRY = 315;
    public float OriginRZ = 0;
    public Quaternion OriginR;
    public Vector3 MouseOrigin;
    public Vector3 MouseDifference;

    void Start()
    {
        registerCallbacks();
    }

    private static void registerCallbacks() {
        UI.TopBar.Q("DragMode").RegisterCallback<ClickEvent>((evt) => {
            if (PanMode) {
                disablePanMode();
            }
            else {
                enablePanMode();
            }
        });
    }

    void LateUpdate() {
        if (Modal.IsOpen()) {
            return;
        }
        if (UI.ClicksSuspended) {
            return;
        }

        if (!Player.IsOnline()) {
            return;
        }

        if (Input.GetMouseButton(1)) {
            Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
            MouseDifference = MouseOrigin - Input.mousePosition;
            if (Drag == false) {
                Drag = true;
                Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                MouseOrigin = Input.mousePosition;
                OriginRY = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.y;
                OriginRZ = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.z;
                OriginR = GameObject.Find("CameraOrigin").transform.rotation;
            }
        }
        else {
            Drag = false;
        }

        if (Drag) {
            if (PanMode) {
                Camera.main.transform.position = Origin - Difference;
            }
            else {
                Quaternion q = Quaternion.identity;
                float targetY = OriginRY - MouseDifference.x/2;
                Quaternion qy = Quaternion.Euler(0f, targetY, 0f);
                q *= qy;

                float targetZ = OriginRZ + MouseDifference.y/2;
                while (targetZ < -180) {
                    targetZ += 360;
                }
                while (targetZ > 180) {
                    targetZ -= 360;
                }
                targetZ = Mathf.Clamp(targetZ, -20, 20);
                Quaternion qz = Quaternion.Euler(0f, 0f, targetZ);
                q *= qz;

                GameObject.Find("CameraOrigin").transform.rotation = q;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!UI.ClicksSuspended && Player.IsOnline() && !Modal.IsOpen()) {
            Vector3 view = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            bool isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (!isOutside && scroll != 0) {
                float z = Camera.main.GetComponent<Camera>().orthographicSize;
                z += scroll;
                z = Mathf.Clamp(z, 1.5f, 8f);
                Camera.main.GetComponent<Camera>().orthographicSize = z;
            }
        }
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
