using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum TokenState
{
    Neutral,
    Focused,
    Pending,
    Dragging,
    MenuOpen,
    Inspecting,
}

public class Token : MonoBehaviour
{
    public int Size = 1;
    public Texture2D Image;
    public TokenData Data;
    public float ShareOffsetX;
    public float ShareOffsetY;
    public Token LastFocused;
    public TokenState State = TokenState.Neutral;

    private Vector3 _dragOrigin;

    void Update()
    {
        AlignToCamera();

        float CutoutSize = Preferences.Current.TokenScale;
        transform.Find("Offset/Avatar/Cutout").localScale = new Vector3(CutoutSize, CutoutSize, CutoutSize);

        // switch (State)
        // {
        //     case TokenState.Neutral:
        //         SetVisualNone();
        //         break;
        //     case TokenState.Focused:
        //         SetVisualSquareBlue();
        //         break;
        //     default:
        //         SetVisualSquareYellow();
        //         break;
        // }

        // string tokenOutline = Preferences.Current.TokenOutline;
        // switch (tokenOutline)
        // {
        //     case "Black":
        //         transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetColor("_BorderColor", Color.black);
        //         transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetFloat("_BorderSize", 1);
        //         break;
        //     case "White":
        //         transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetColor("_BorderColor", Color.white);
        //         transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetFloat("_BorderSize", 1);
        //         break;
        //     case "None":
        //         transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetColor("_BorderColor", Color.black);
        //         transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetFloat("_BorderSize", 0);
        //         break;
        //     default:
        //         transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetColor("_BorderColor", Color.white);
        //         transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetFloat("_BorderSize", 1);
        //         break;
        // }

        Offset();

        if (State == TokenState.Focused && this != LastFocused)
        {
            Unfocus();
        }

        // if (State == TokenState.Pending)
        // {
        //     if (Input.mousePosition != _dragOrigin)
        //     {
        //         StartDragging();
        //     }
        // }
    }

    void LateUpdate()
    {
        // if (!Input.GetMouseButtonUp(0))
        // {
        //     return;
        // }
        // switch (State)
        // {
        //     case TokenState.Pending:
        //         StartInspecting();
        //         break;
        //     case TokenState.Dragging:
        //         StopDragging();
        //         break;
        // }
    }

    private void AlignToCamera()
    {
        Transform t = transform.Find("Offset/Avatar/Cutout").transform;
        t.rotation = Camera.main.transform.rotation;
    }

    private void Offset()
    {
        float x = ShareOffsetX;
        float y = ShareOffsetY;
        if (Size == 2)
        {
            x = 0;
            y = -.73f;
        }
        else if (Size == 3)
        {
            x = 0;
            y = 0;
        }
        transform.Find("Offset").transform.localPosition = new Vector3(x, 0, y);
        transform.Find("Base").transform.localPosition = new Vector3(x, 0, y);
    }

    public void SetImage(Texture2D image)
    {
        Image = image;
        float aspectRatio = Image.width / (float)Image.height;
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", Image);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").transform.localScale = new Vector3(aspectRatio, 1f, 1f);
    }

    public void LeftClickDown()
    {
        switch (State)
        {
            case TokenState.Neutral:
            case TokenState.Focused:
            case TokenState.MenuOpen:
                StartPending();
                break;
            case TokenState.Inspecting:
                Deselect();
                break;
        }
    }

    public void RightClickDown()
    {
        switch (State)
        {
            case TokenState.Neutral:
            case TokenState.Inspecting:
            case TokenState.Focused:
                StartMenu();
                break;
            case TokenState.MenuOpen:
                Deselect();
                break;
        }
    }

    private void StartPending()
    {
        DeselectAll();
        State = TokenState.Pending;
        _dragOrigin = Input.mousePosition;
        // Cursor.Mode = CursorMode.Dragging;
    }

    private void StartMenu()
    {
        DeselectAll();
        State = TokenState.MenuOpen;
        TokenMenu.ShowMenu();
        // Cursor.Mode = CursorMode.Default;
        Block.DeselectAll();
        Block.UnfocusAll();
        BlockMesh.ToggleAllBorders(false);
        Player.Self().ClearOp();
        UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true); // selected indicator in unit bar
        Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UISelectYellow;
    }

    private void StartDragging()
    {
        State = TokenState.Dragging;
        // Cursor.Mode = CursorMode.Dragging;
        StateManager.IsDraggingToken = true;
        Block.DeselectAll();
        Block.UnfocusAll();
        BlockMesh.ToggleAllBorders(true);
        string op = Data.Placed ? "Moving" : "Placing";
        Player.Self().SetOp($"{op} {Data.Name}");
        Player.Self().GetComponent<DirectionalLine>().Init(Data.Id, op);
        UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true); // selected indicator in unit bar
        Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UISelectYellow;
    }

    private void StopDragging()
    {
        StateManager.IsDraggingToken = false;
        Block[] focused = Block.GetFocused();
        if (focused.Length == 0)
        {
            // Release outside map
            Deselect();
            return;
        }
        Block b = focused[0];
        if (b != GetBlock())
        {
            // Release on new block
            Move(b);
        }
        else
        {
            // Release on same block
            Deselect();
        }
    }

    private void StartInspecting()
    {
        State = TokenState.Inspecting;
        // Cursor.Mode = CursorMode.Default;
        BlockMesh.ToggleAllBorders(false);
        Player.Self().SetOp($"Inspecting {Data.Name}");
        Player.Self().GetComponent<DirectionalLine>().Deinit();
    }

    public void Move(Block block)
    {
        Deselect();
        Vector3 v = block.GetMidpoint();
        if (Data.Placed)
        {
            Player.Self().CmdMoveToken(Data.Id, v, false);
        }
        else
        {
            Player.Self().CmdRequestPlaceToken(Data.Id, v);
        }
    }

    public void Remove()
    {
        Player.Self().CmdRequestRemoveToken(Data.Id);
    }

    public void Deselect()
    {
        State = TokenState.Neutral;
        Block.DehighlightAll();
        UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), false);
        Player.Self().ClearOp();
        SelectionMenu.Hide();
        // Cursor.Mode = CursorMode.Default;
        BlockMesh.ToggleAllBorders(false);
        Player.Self().GetComponent<DirectionalLine>().Deinit();
    }

    public static void DeselectAll()
    {
        Token t = GetSelected();
        if (t)
        {
            t.Deselect();
        }
    }

    public static Token GetSelected()
    {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++)
        {
            TokenState ts = tokens[i].GetComponent<Token>().State;
            if (ts == TokenState.Dragging || ts == TokenState.Pending || ts == TokenState.MenuOpen || ts == TokenState.Inspecting)
            {
                return tokens[i].GetComponent<Token>();
            }
        }
        return null;
    }

    public void Focus()
    {
        if (Token.GetSelected() == this)
        {
            return;
        }
        UnfocusAll();
        Data.Focus();
        UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true); // selected indicator in unit bar
        Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UIFocusBlue;
        LastFocused = this;
        State = TokenState.Focused;
        // Focused = true;
    }

    public void Unfocus()
    {
        if (State != TokenState.Focused)
        {
            return;
        }

        State = TokenState.Neutral;
        UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), false);

        // State = TokenState.Neutral;
        // // Focused = false;
        // if (State != TokenState.Inspecting) {
        //     UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), false);
        // }
    }

    public static Token GetFocused()
    {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].GetComponent<Token>().State == TokenState.Focused)
            {

                return tokens[i].GetComponent<Token>();
            }
        }
        return null;
    }

    public static void UnfocusAll()
    {
        Token t = GetFocused();
        if (t)
        {
            t.Unfocus();
        }
    }

    public Block GetBlock()
    {
        return Block.GetClosest(transform.position);
    }

    public static Token GetAtBlock(Block b)
    {
        List<Token> nearby = TileShare.GetNearbyTokens(b.transform.position, .5f);
        if (nearby.Count > 0)
        {
            return nearby[0];
        }
        return null;
    }

    private void SetVisualSquareYellow()
    {
        SetVisual(true, false, false, false);
    }

    private void SetVisualSquareBlue()
    {
        SetVisual(false, false, true, false);
    }

    private void SetVisualNone()
    {
        SetVisual(false, false, false, false);
    }

    private void SetVisual(bool yellowSquare, bool yellowArrows, bool blueSquare, bool blueArrows)
    {
        transform.Find("Offset/Select").GetComponent<MeshRenderer>().material.SetInt("_Selected", yellowSquare ? 1 : 0);
        transform.Find("Offset/Select").GetComponent<MeshRenderer>().material.SetInt("_Moving", yellowArrows ? 1 : 0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", blueSquare ? 1 : 0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", blueArrows ? 1 : 0);
    }

    public void SetDefeated(bool defeated)
    {
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetInt("_Dead", defeated ? 1 : 0);
    }
}
