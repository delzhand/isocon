using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum ActorState
{
    Neutral,
    Focused,
    Dragging,
    Selected,
}

public class Actor : MonoBehaviour
{
    public int Size = 1;
    public Texture2D Image;
    public ActorData Data;
    public float ShareOffsetX;
    public float ShareOffsetY;
    public Actor LastFocused;
    public ActorState State = ActorState.Neutral;

    public static bool RebuildPanels = false;
    private static Actor _focused;
    private static Actor _selected;
    private static Actor _dragging;

    void Update()
    {
        AlignToCamera();
        // OffsetForSizeAndSharing();
        GlobalTokenScale();

        State = ActorState.Neutral;
        if (this == _focused)
        {
            State = ActorState.Focused;
        }
        if (this == _selected)
        {
            State = ActorState.Selected;
        }
        if (this == _dragging)
        {
            State = ActorState.Dragging;
        }

        switch (State)
        {
            case ActorState.Dragging:
            case ActorState.Selected:
                SetVisualSquareYellow();
                UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true);
                Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UISelectYellow;
                break;
            case ActorState.Focused:
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

    // private void OffsetForSizeAndSharing()
    // {
    //     float x = ShareOffsetX;
    //     float y = ShareOffsetY;
    //     if (Size == 2)
    //     {
    //         x = 0;
    //         y = -.73f;
    //     }
    //     else if (Size == 3)
    //     {
    //         x = 0;
    //         y = 0;
    //     }
    //     transform.Find("Offset").transform.localPosition = new Vector3(x, 0, y);
    //     transform.Find("Base").transform.localPosition = new Vector3(x, 0, y);
    // }

    private void GlobalTokenScale()
    {
        transform.Find("Offset/Avatar/Cutout").localScale = Vector3.one * Preferences.Current.TokenScale;
    }

    public void SetImage(Texture2D image)
    {
        Image = image;
        float aspectRatio = Image.width / Data.Token.Frames / (float)Image.height;
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", Image);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetInt("_XFrames", Data.Token.Frames);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetInt("_FPS", Data.Token.FPS);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").transform.localScale = new Vector3(aspectRatio, 1f, 1f);
        SetAllTokenOutlines();
    }

    private void StateChange(ActorState state)
    {
        State = state;
        RebuildPanels = true;
        switch (state)
        {
            case ActorState.Selected:
                _selected = this;
                if (_focused == this)
                {
                    _focused = null;
                }
                SelectionMenu.Hide();
                break;
            case ActorState.Dragging:
                _dragging = this;
                SelectionMenu.Hide();
                break;
            case ActorState.Focused:
                _focused = this;
                break;
            case ActorState.Neutral:
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
        StateChange(ActorState.Selected);
        StateChange(ActorState.Dragging);
        BlockRendering.ToggleAllBorders(true);
        string op = Data.Placed ? "Moving" : "Placing";
        Player.Self().SetOp($"{op} {Data.Name}");
        Player.Self().GetComponent<DirectionalLine>().Init(Data.Id, op);
    }

    public static void StopDragging(Block b, Vector3 v)
    {
        if (_dragging)
        {
            if (b != null)
            {
                _dragging.StateChange(ActorState.Neutral);
                if (_dragging.Data.CornerTargeting())
                {
                    _dragging.Move(v);
                }
                else
                {
                    _dragging.Move(b);
                }
            }
            else
            {
                _dragging.StateChange(ActorState.Neutral);
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
            StateChange(ActorState.Focused);
            SelectionMenu.Hide();
        }
        else
        {
            StateChange(ActorState.Selected);
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
            StateChange(ActorState.Selected);
            ActorMenu.ShowMenu();
        }
        else if (this == _selected)
        {
            ActorMenu.ShowMenu();
        }
    }

    public void Move(Block block)
    {
        Vector3 v = block.GetMidpoint();
        Block optimal = Block.FindOptimalFromPosition(v);
        v = optimal.GetMidpoint();
        Move(v);
    }

    public void Move(Vector3 v)
    {
        Deselect();

        if (Data.Placed)
        {
            Player.Self().CmdMoveActor(Data.Id, v, false);
        }
        else
        {
            Player.Self().CmdRequestPlaceActor(Data.Id, v);
        }
    }

    public void Remove()
    {
        Player.Self().CmdRequestRemoveActor(Data.Id);
    }

    public static void Deselect()
    {
        _selected = null;
        Player.Self().GetComponent<DirectionalLine>().Deinit();
        SelectionMenu.Hide();
    }

    public static Actor GetSelected()
    {
        return _selected;
    }

    public static Actor GetDragging()
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
            StateChange(ActorState.Focused);
        }
    }

    public void Unfocus()
    {
        if (this == _selected)
        {
            return;
        }
        StateChange(ActorState.Neutral);
    }

    public static Actor GetFocused()
    {
        return _focused;
    }

    public static void UnfocusAll()
    {
        _focused?.StateChange(ActorState.Neutral);
        _focused = null;
    }

    public Block GetBlock()
    {
        return Block.GetClosest(transform.position);
    }

    public static Actor GetAtBlock(Block b)
    {
        List<Actor> nearby = TileShare.GetNearbyActors(b.transform.position, .5f);
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
        GameObject[] actors = GameObject.FindGameObjectsWithTag("Actor");
        for (int i = 0; i < actors.Length; i++)
        {
            actors[i].GetComponent<Actor>().SetTokenOutline();
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

    public static void MoveAllActorsToOptimalBlock()
    {
        foreach (var gameObject in GameObject.FindGameObjectsWithTag("Actor"))
        {
            gameObject.GetComponent<Actor>().MoveToOptimalBlock();
        }

    }

    private void MoveToOptimalBlock()
    {
        Block optimal = Block.FindOptimalFromPosition(transform.position);
        Vector3 v = optimal.GetMidpoint();
        if (Data.CornerTargeting())
        {
            v = optimal.GetNearestCorner(transform.position + new Vector3(0, -20, 0));
        }
        Player.Self().CmdMoveActor(Data.Id, v, false);
    }
}
