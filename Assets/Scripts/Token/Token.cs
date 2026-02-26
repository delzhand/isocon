using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum TokenState
{
    Neutral,
    Focused,
    Dragging,
    Selected,
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

    public static bool RebuildPanels = false;
    private static Token _focused;
    private static Token _selected;
    private static Token _dragging;

    void Update()
    {
        AlignToCamera();
        OffsetForSizeAndSharing();
        GlobalTokenScale();

        State = TokenState.Neutral;
        if (this == _focused)
        {
            State = TokenState.Focused;
        }
        if (this == _selected)
        {
            State = TokenState.Selected;
        }
        if (this == _dragging)
        {
            State = TokenState.Dragging;
        }

        switch (State)
        {
            case TokenState.Dragging:
            case TokenState.Selected:
                SetVisualSquareYellow();
                UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true);
                Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UISelectYellow;
                break;
            case TokenState.Focused:
                SetVisualSquareBlue();
                UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true);
                Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UIFocusBlue;
                break;
            default:
                SetVisualNone();
                UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), false);
                break;
        }
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
        RebuildPanels = true;
        switch (state)
        {
            case TokenState.Selected:
                _selected = this;
                if (_focused == this)
                {
                    _focused = null;
                }
                SelectionMenu.Hide();
                break;
            case TokenState.Dragging:
                _dragging = this;
                SelectionMenu.Hide();
                break;
            case TokenState.Focused:
                _focused = this;
                break;
            case TokenState.Neutral:
                if (_focused == this)
                {
                    _focused = null;
                }
                if (_selected == this)
                {
                    _selected = null;
                }
                SelectionMenu.Hide();
                break;
        }
    }

    public void StartDragging()
    {
        StateChange(TokenState.Selected);
        StateChange(TokenState.Dragging);
        BlockRendering.ToggleAllBorders(true);
        string op = Data.Placed ? "Moving" : "Placing";
        Player.Self().SetOp($"{op} {Data.Name}");
        Player.Self().GetComponent<DirectionalLine>().Init(Data.Id, op);
    }

    public static void StopDragging(Block b)
    {
        if (_dragging)
        {
            if (b != null)
            {
                _dragging.StateChange(TokenState.Neutral);
                _dragging.Move(b);
            }
            else
            {
                _dragging.StateChange(TokenState.Neutral);
            }
        }
        _dragging = null;
        Player.Self().ClearOp();
        Player.Self().GetComponent<DirectionalLine>().Deinit();
        BlockRendering.ToggleAllBorders(false);
    }

    public void ToggleSelect()
    {
        if (this == _selected)
        {
            _selected = null;
            StateChange(TokenState.Focused);
            SelectionMenu.Hide();
        }
        else
        {
            StateChange(TokenState.Selected);
        }
    }

    public void ToggleMenu()
    {
        if (this == _selected && SelectionMenu.Visible)
        {
            SelectionMenu.Hide();
        }
        else if (this != _selected)
        {
            StateChange(TokenState.Selected);
            TokenMenu.ShowMenu();
        }
        else if (this == _selected)
        {
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

    public static void Deselect()
    {
        _selected = null;
        Player.Self().GetComponent<DirectionalLine>().Deinit();
        SelectionMenu.Hide();
    }

    public static Token GetSelected()
    {
        return _selected;
    }

    public static Token GetDragging()
    {
        return _dragging;
    }

    public void Focus()
    {
        if (this == _selected)
        {
            return;
        }
        else
        {
            _focused = this;
            StateChange(TokenState.Focused);
        }
    }

    public void Unfocus()
    {
        if (this == _selected)
        {
            return;
        }
        StateChange(TokenState.Neutral);
    }

    public static Token GetFocused()
    {
        return _focused;
    }

    public static void UnfocusAll()
    {
        _focused?.StateChange(TokenState.Neutral);
        _focused = null;
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
