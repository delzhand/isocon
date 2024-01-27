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

    void LateUpdate()
    {
        if (Modal.IsOpen() || UI.ClicksSuspended || !Player.IsOnline())
        {
            return;
        }

        if (Input.GetMouseButton(1))
        {
            if (Drag == false)
            {
                Drag = true;
                Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                MouseOrigin = Input.mousePosition;
                OriginRY = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.y;
                OriginRZ = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.z;
                OriginR = GameObject.Find("CameraOrigin").transform.rotation;
            }
            Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
            MouseDifference = MouseOrigin - Input.mousePosition;
        }
        else
        {
            Drag = false;
        }

        if (Drag)
        {
            if (PanMode)
            {
                Camera.main.transform.position = Origin - Difference;
            }
            else
            {
                Quaternion q = Quaternion.identity;
                float targetY = OriginRY - MouseDifference.x / 2;
                Quaternion qy = Quaternion.Euler(0f, targetY, 0f);
                q *= qy;

                float targetZ = OriginRZ + MouseDifference.y / 2;
                while (targetZ < -180)
                {
                    targetZ += 360;
                }
                while (targetZ > 180)
                {
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
        if (!UI.ClicksSuspended && Player.IsOnline() && !Modal.IsOpen())
        {
            Vector3 view = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            bool isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (!isOutside && scroll != 0)
            {
                float z = Camera.main.GetComponent<Camera>().orthographicSize;
                z -= scroll;
                z = Mathf.Clamp(z, 1.5f, 8f);
                Camera.main.GetComponent<Camera>().orthographicSize = z;
            }
        }
    }

    public static void CreateBindings()
    {
        UI.TopBar.Q("DragMode").RegisterCallback<MouseEnterEvent>((evt) =>
        {
            Tutorial.Init("camera modes");
        });
        UI.TopBar.Q("DragMode").RegisterCallback<ClickEvent>((evt) =>
        {
            TogglePanMode();
        });
    }

    public static void TogglePanMode()
    {
        if (PanMode)
        {
            DisablePanMode();
        }
        else
        {
            EnablePanMode();
        }
    }

    private static void DisablePanMode()
    {
        PanMode = false;
        UI.TopBar.Q("DragMode").Q<Label>("Label").text = "Rotate <u>C</u>amera";
    }

    private static void EnablePanMode()
    {
        PanMode = true;
        UI.TopBar.Q("DragMode").Q<Label>("Label").text = "Pan <u>C</u>amera";
    }
}
