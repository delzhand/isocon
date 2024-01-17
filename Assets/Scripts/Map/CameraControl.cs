using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraControl : MonoBehaviour
{
    public static bool PanMode = true;

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
