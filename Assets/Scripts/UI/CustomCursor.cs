using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    // You must set the cursor in the inspector.
    public static Texture2D defaultCursor;
    public static Texture2D sampleCursor;
    public static Texture2D removeHeightCursor;

    void Start()
    {
        defaultCursor = Resources.Load<Texture2D>("Textures/Cursors/cursor_default");
        sampleCursor = Resources.Load<Texture2D>("Textures/Cursors/cursor_sample");
        removeHeightCursor = Resources.Load<Texture2D>("Textures/Cursors/cursor_removeheight");
        SetDefault();
    }

    public static void SetDefault()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    public static void SetSample()
    {
        Cursor.SetCursor(sampleCursor, Vector2.zero, CursorMode.Auto);
    }

    public static void SetRemoveHeight()
    {
        Cursor.SetCursor(removeHeightCursor, Vector2.zero, CursorMode.Auto);
    }
}