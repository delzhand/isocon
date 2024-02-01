using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Viewport
{

    private static bool _dragMode = true;

    private static Vector3 _panOrigin;
    private static Vector3 _mouseOrigin;
    private static float _originRY = 315;
    private static float _originRZ = 0;

    public static bool IsDragging = false;

    public static void HandleInput()
    {
        HandleScrolling();
    }

    public static void InitializeDrag()
    {
        Debug.Log("init drag");
        _mouseOrigin = Input.mousePosition;
        _panOrigin = Camera.main.ScreenToWorldPoint(_mouseOrigin);
        _originRY = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.y;
        _originRZ = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.z;
    }

    public static void UpdateDrag()
    {

        if (_dragMode)
        {
            Vector3 panDifference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
            Camera.main.transform.position = _panOrigin - panDifference;
        }
        else
        {
            Vector3 mouseDifference = _mouseOrigin - Input.mousePosition;
            Quaternion q = Quaternion.identity;
            float targetY = _originRY - mouseDifference.x / 2;
            Quaternion qy = Quaternion.Euler(0f, targetY, 0f);
            q *= qy;

            float targetZ = _originRZ + mouseDifference.y / 2;
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

    private static void HandleDragging()
    {
        // if (Input.GetMouseButton(1))
        // {
        //     if (!Dragger.IsRightDown)
        //     {
        //         Debug.Log("viewport drag init");
        //         _panOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //         _originRY = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.y;
        //         _originRZ = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.z;
        //         _originR = GameObject.Find("CameraOrigin").transform.rotation;
        //     }
        //     _panDifference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
        //     _mouseDifference = _mouseOrigin - Input.mousePosition;

        //     // if (Dragger.IsRightDragging)
        //     // {
        //     //     if (_dragMode)
        //     //     {
        //     //         Camera.main.transform.position = _panOrigin - _panDifference;
        //     //     }
        //     // }
        // }

        // if (_isDragging)
        // {
        //     if (_dragMode)
        //     {
        //         Camera.main.transform.position = _panOrigin - _panDifference;
        //     }
        //     else
        //     {
        //         Quaternion q = Quaternion.identity;
        //         float targetY = _originRY - _mouseDifference.x / 2;
        //         Quaternion qy = Quaternion.Euler(0f, targetY, 0f);
        //         q *= qy;

        //         float targetZ = _originRZ + _mouseDifference.y / 2;
        //         while (targetZ < -180)
        //         {
        //             targetZ += 360;
        //         }
        //         while (targetZ > 180)
        //         {
        //             targetZ -= 360;
        //         }
        //         targetZ = Mathf.Clamp(targetZ, -20, 20);
        //         Quaternion qz = Quaternion.Euler(0f, 0f, targetZ);
        //         q *= qz;

        //         GameObject.Find("CameraOrigin").transform.rotation = q;
        //     }
        // }
    }

    private static void HandleScrolling()
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

    public static void TogglePanMode()
    {
        if (_dragMode)
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
        _dragMode = false;
        UI.TopBar.Q("DragMode").Q<Label>("Label").text = "Rotate <u>C</u>amera";
    }

    private static void EnablePanMode()
    {
        _dragMode = true;
        UI.TopBar.Q("DragMode").Q<Label>("Label").text = "Pan <u>C</u>amera";
    }
}
