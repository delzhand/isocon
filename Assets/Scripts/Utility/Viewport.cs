using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum DragMode
{
    Rotate,
    Pan
}

public class Viewport
{
    private static DragMode _mode;
    private static Vector3 _panOrigin;
    private static Vector3 _mouseOrigin;
    private static float _originRY = 315;
    private static float _originRZ = 0;

    private static bool _isRightDragging = false;
    private static bool _isMiddleDragging = false;
    public static bool IsDragging { get => _isRightDragging || _isMiddleDragging; }

    public static void HandleInput()
    {
        HandleScrolling();
    }

    public static void InitializeRightDrag()
    {
        if (_isMiddleDragging)
        {
            return;
        }
        _isRightDragging = true;
        switch (_mode)
        {
            case DragMode.Pan:
                InitializePanDrag();
                break;
            case DragMode.Rotate:
                InitializeRotateDrag();
                break;
        }
    }

    public static void InitializeMiddleDrag()
    {
        if (_isRightDragging)
        {
            return;
        }
        _isMiddleDragging = true;
        switch (_mode)
        {
            case DragMode.Pan:
                InitializeRotateDrag();
                break;
            case DragMode.Rotate:
                InitializePanDrag();
                break;
        }
    }

    private static void InitializeRotateDrag()
    {
        _mouseOrigin = Input.mousePosition;
        _originRY = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.y;
        _originRZ = GameObject.Find("CameraOrigin").transform.rotation.eulerAngles.z;
    }

    private static void InitializePanDrag()
    {
        _mouseOrigin = Input.mousePosition;
        _panOrigin = Camera.main.ScreenToWorldPoint(_mouseOrigin);
    }

    public static void UpdateRightDrag()
    {
        if (!_isRightDragging)
        {
            return;
        }
        switch (_mode)
        {
            case DragMode.Pan:
                UpdatePanDrag();
                break;
            case DragMode.Rotate:
                UpdateRotateDrag();
                break;
        }
    }

    public static void UpdateMiddleDrag()
    {
        if (!_isMiddleDragging)
        {
            return;
        }
        switch (_mode)
        {
            case DragMode.Pan:
                UpdateRotateDrag();
                break;
            case DragMode.Rotate:
                UpdatePanDrag();
                break;
        }
    }

    private static void UpdatePanDrag()
    {
        Vector3 panDifference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
        Camera.main.transform.position = _panOrigin - panDifference;
    }

    private static void UpdateRotateDrag()
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

    public static void EndRightDrag()
    {
        _isRightDragging = false;
    }

    public static void EndMiddleDrag()
    {
        _isMiddleDragging = false;
    }

    private static void HandleScrolling()
    {
        if (UI.ClicksSuspended || Modal.IsOpen())
        {
            return;
        }
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
        switch (_mode)
        {
            case DragMode.Pan:
                EnableRotateMode();
                break;
            case DragMode.Rotate:
                EnablePanMode();
                break;
        }
    }

    private static void EnableRotateMode()
    {
        _mode = DragMode.Rotate;
        UI.TopBar.Q("DragMode").Q<Label>("Label").text = "Rotate <u>C</u>amera";
    }

    private static void EnablePanMode()
    {
        _mode = DragMode.Pan;
        UI.TopBar.Q("DragMode").Q<Label>("Label").text = "Pan <u>C</u>amera";
    }
}
