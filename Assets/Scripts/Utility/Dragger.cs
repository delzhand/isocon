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
    public static bool IsLeftDown { get => _left != ClickState.Up; }
    public static bool IsLeftDragging { get => _left == ClickState.Dragging; }
    private static Vector3 _leftMouseOrigin;
    public static DraggerCallback LeftClickStart;
    public static DraggerCallback LeftDragStart;
    public static DraggerCallback LeftDragUpdate;
    public static DraggerCallback LeftClickRelease;
    public static DraggerCallback LeftDragRelease;

    private static ClickState _right = ClickState.Up;
    public static bool IsRightDown { get => _right != ClickState.Up; }
    public static bool IsRightDragging { get => _right == ClickState.Dragging; }
    private static Vector3 _rightMouseOrigin;
    public static DraggerCallback RightClickStart;
    public static DraggerCallback RightDragStart;
    public static DraggerCallback RightDragUpdate;
    public static DraggerCallback RightClickRelease;
    public static DraggerCallback RightDragRelease;

    private static ClickState _middle = ClickState.Up;
    public static bool IsMiddleDown { get => _middle != ClickState.Up; }
    public static bool IsMiddleDragging { get => _middle == ClickState.Dragging; }
    private static Vector3 _middleMouseOrigin;
    public static DraggerCallback MiddleClickStart;
    public static DraggerCallback MiddleDragStart;
    public static DraggerCallback MiddleDragUpdate;
    public static DraggerCallback MiddleClickRelease;
    public static DraggerCallback MiddleDragRelease;

    void Update()
    {
        if (Input.GetMouseButton(0) && !Modal.IsOpen())
        {
            if (_left == ClickState.Up)
            {
                LeftClickStart?.Invoke();
                _left = ClickState.Checking;
                _leftMouseOrigin = Input.mousePosition;
            }
            if (_left == ClickState.Checking)
            {
                if (Input.mousePosition != _leftMouseOrigin)
                {
                    LeftDragStart?.Invoke();
                    _left = ClickState.Dragging;
                }
            }
            if (_left == ClickState.Dragging)
            {
                LeftDragUpdate?.Invoke();
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
            _left = ClickState.Up;
        }

        if (Input.GetMouseButton(1) && !Modal.IsOpen())
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
                    RightDragStart?.Invoke();
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
            _right = ClickState.Up;
        }

        if (Input.GetMouseButton(2) && !Modal.IsOpen())
        {
            if (_middle == ClickState.Up)
            {
                MiddleClickStart?.Invoke();
                _middle = ClickState.Checking;
                _middleMouseOrigin = Input.mousePosition;
            }
            if (_middle == ClickState.Checking)
            {
                if (Input.mousePosition != _middleMouseOrigin)
                {
                    MiddleDragStart?.Invoke();
                    _middle = ClickState.Dragging;
                }
            }
            if (_middle == ClickState.Dragging)
            {
                MiddleDragUpdate?.Invoke();
            }
        }
        else
        {
            if (_middle == ClickState.Checking)
            {
                MiddleClickRelease?.Invoke();
            }
            else if (_middle == ClickState.Dragging)
            {
                MiddleDragRelease?.Invoke();
            }
            _middle = ClickState.Up;
        }
    }

}
