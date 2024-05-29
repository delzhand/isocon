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

    void Update()
    {
        AlignToCamera();
        OffsetForSizeAndSharing();
        GlobalTokenScale();
        CheckStillFocused();
    }

    private void AlignToCamera()
    {
        Transform t = transform.Find("Offset/Avatar/Cutout").transform;
        t.rotation = Camera.main.transform.rotation;
    }

    private void OffsetForSizeAndSharing()
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

    private void GlobalTokenScale()
    {
        transform.Find("Offset/Avatar/Cutout").localScale = Vector3.one * Preferences.Current.TokenScale;
    }

    private void CheckStillFocused()
    {
        if (State == TokenState.Focused && this != LastFocused)
        {
            Unfocus();
        }
    }

    public void SetImage(Texture2D image)
    {
        Image = image;
        float aspectRatio = Image.width / Data.TokenMeta.Frames / (float)Image.height;
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", Image);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetInt("_XFrames", Data.TokenMeta.Frames);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetInt("_FPS", Data.TokenMeta.FPS);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").transform.localScale = new Vector3(aspectRatio, 1f, 1f);
        SetAllTokenOutlines();
    }

    private void StateChange(TokenState state)
    {
        State = state;
        switch (state)
        {
            case TokenState.Inspecting:
            case TokenState.Dragging:
            case TokenState.MenuOpen:
                SetVisualSquareYellow();
                UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true);
                Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UISelectYellow;
                Data.NeedsRedraw = true;
                break;
            case TokenState.Focused:
                SetVisualSquareBlue();
                UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true);
                Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UIFocusBlue;
                break;
            case TokenState.Neutral:
                SetVisualNone();
                UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), false);
                break;
        }
        SelectionMenu.Hide();
    }

    public void StartDragging()
    {
        StateChange(TokenState.Dragging);
        BlockRendering.ToggleAllBorders(true);
        string op = Data.Placed ? "Moving" : "Placing";
        Player.Self().SetOp($"{op} {Data.Name}");
        Player.Self().GetComponent<DirectionalLine>().Init(Data.Id, op);
    }

    public void StopDragging(Block b)
    {
        if (b != null)
        {
            StateChange(TokenState.Neutral);
            Move(b);
        }
        else
        {
            StateChange(TokenState.Neutral);
        }
        BlockRendering.ToggleAllBorders(false);
    }

    public void ToggleInspect()
    {
        if (State == TokenState.Inspecting)
        {
            StateChange(TokenState.Neutral);
            Player.Self().ClearOp();
        }
        else
        {
            StateChange(TokenState.Inspecting);
            Player.Self().SetOp($"Inspecting {Data.Name}");
        }
    }

    public void ToggleMenu()
    {
        if (SelectionMenu.Visible)
        {
            StateChange(TokenState.Neutral);
        }
        else
        {
            StateChange(TokenState.MenuOpen);
            TokenMenu.ShowMenu();
        }
    }

    public void Move(Block block)
    {
        Deselect();
        Vector3 v = block.GetMidpoint();
        Block optimal = Block.FindOptimalFromPosition(v);
        v = optimal.GetMidpoint();
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
        StateChange(TokenState.Neutral);
        Player.Self().ClearOp();
        Player.Self().GetComponent<DirectionalLine>().Deinit();
        SelectionMenu.Hide();
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
            if (ts == TokenState.Dragging || ts == TokenState.MenuOpen || ts == TokenState.Inspecting)
            {
                return tokens[i].GetComponent<Token>();
            }
        }
        return null;
    }

    public static Token GetDragging()
    {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++)
        {
            TokenState ts = tokens[i].GetComponent<Token>().State;
            if (ts == TokenState.Dragging)
            {
                return tokens[i].GetComponent<Token>();
            }
        }
        return null;
    }

    public void Focus()
    {
        if (GetSelected() == this)
        {
            return;
        }
        else
        {
            StateChange(TokenState.Focused);
            LastFocused = this;
        }
    }

    public void Unfocus()
    {
        if (GetSelected() == this)
        {
            return;
        }
        else
        {
            StateChange(TokenState.Neutral);
        }
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

    public static void SetAllTokenOutlines()
    {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i].GetComponent<Token>().SetTokenOutline();
        }
    }

    private void SetTokenOutline()
    {
        string tokenOutline = Preferences.Current.TokenOutline;
        switch (tokenOutline)
        {
            case "Black":
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetColor("_BorderColor", Color.black);
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetFloat("_BorderSize", 1);
                break;
            case "White":
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetColor("_BorderColor", Color.white);
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetFloat("_BorderSize", 1);
                break;
            case "None":
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetColor("_BorderColor", Color.black);
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetFloat("_BorderSize", 0);
                break;
            default:
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetColor("_BorderColor", Color.white);
                transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetFloat("_BorderSize", 1);
                break;
        }
    }

    public static void MoveAllTokensToOptimalBlock()
    {
        foreach (var gameObject in GameObject.FindGameObjectsWithTag("Token"))
        {
            gameObject.GetComponent<Token>().MoveToOptimalBlock();
        }

    }

    private void MoveToOptimalBlock()
    {
        Block optimal = Block.FindOptimalFromPosition(transform.position);
        Player.Self().CmdMoveToken(Data.Id, optimal.GetMidpoint(), false);
    }
}
