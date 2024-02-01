using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragger : MonoBehaviour
{
    private enum ClickState
    {
        Up,
        Checking,
        Dragging
    }

    public delegate void DraggerCallback();

    private static ClickState _left = ClickState.Up;
    private static Vector3 _leftMouseOrigin;
    public static DraggerCallback LeftClickRelease;
    public static DraggerCallback LeftDragRelease;

    private static ClickState _right = ClickState.Up;
    public static bool IsRightDown { get => _right != ClickState.Up; }
    public static bool IsRightDragging { get => _right == ClickState.Dragging; }
    private static Vector3 _rightMouseOrigin;
    public static DraggerCallback RightClickStart;
    public static DraggerCallback RightDragUpdate;
    public static DraggerCallback RightClickRelease;
    public static DraggerCallback RightDragRelease;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (_left == ClickState.Up)
            {
                _left = ClickState.Checking;
                _leftMouseOrigin = Input.mousePosition;
            }
            if (_left == ClickState.Checking)
            {
                if (Input.mousePosition != _leftMouseOrigin)
                {
                    _left = ClickState.Dragging;
                }
            }
        }
        else
        {
            if (_left == ClickState.Checking)
            {
                LeftClickRelease?.Invoke();
            }
            else if (_left == ClickState.Dragging)
            {
                LeftDragRelease?.Invoke();
            }
            LeftClickRelease = null;
            LeftDragRelease = null;
            _left = ClickState.Up;
        }

        if (Input.GetMouseButton(1))
        {
            if (_right == ClickState.Up)
            {
                RightClickStart?.Invoke();
                _right = ClickState.Checking;
                _rightMouseOrigin = Input.mousePosition;
            }
            if (_right == ClickState.Checking)
            {
                if (Input.mousePosition != _rightMouseOrigin)
                {
                    _right = ClickState.Dragging;
                }
            }
            if (_right == ClickState.Dragging)
            {
                RightDragUpdate?.Invoke();
            }
        }
        else
        {
            if (_right == ClickState.Checking)
            {
                RightClickRelease?.Invoke();
            }
            else if (_right == ClickState.Dragging)
            {
                RightDragRelease?.Invoke();
            }
            RightClickRelease = null;
            RightDragRelease = null;
            _right = ClickState.Up;
        }
    }

}
