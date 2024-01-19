using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mirror;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public enum TokenState {
    Neutral, // neither
    Focused, // right
    Pending, // neither
    Dragging, // left partial
    MenuOpen, // left partial
    Inspecting, // left full
}

public class Token : MonoBehaviour
{
    public int Size = 1;
    public Texture2D Image;

    public TokenData Data;

    public float ShareOffsetX;
    public float ShareOffsetY;

    public Token LastFocused;

    // public bool Selected = false;
    // public bool SoftSelect = true;
    // public bool Focused = false;

    private Vector3 DragOrigin;

    public TokenState State = TokenState.Neutral;

    void LateUpdate() {
        if (!Input.GetMouseButtonUp(0)) {
            return;
        }
        switch (State) {
            case TokenState.Pending:
                StartInspecting();
                break;
            case TokenState.Dragging:
                StopDragging();
                break;
        }
    }

    void Update()
    {
        alignToCamera();

        float CutoutSize = PlayerPrefs.GetFloat("TokenScale", 1f);
        transform.Find("Offset/Avatar/Cutout").localScale = new Vector3(CutoutSize, CutoutSize, CutoutSize);

        switch(State) {
            case TokenState.Neutral:
                SetVisualNone();
                break;
            case TokenState.Focused:
                SetVisualSquareBlue();
                break;
            default:
                SetVisualSquareYellow();
                break;
        }

        string tokenOutline = PlayerPrefs.GetString("TokenOutline", "White");
        switch(tokenOutline) {
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

        Offset();

        if (State == TokenState.Focused && this != LastFocused) {
            Unfocus();
        }

        if (State == TokenState.Pending) {
            if (Input.mousePosition != DragOrigin) {
                StartDragging();
            }
        }
    }

    private void alignToCamera() {
        Transform t = transform.Find("Offset/Avatar/Cutout").transform;
        t.rotation = Camera.main.transform.rotation;
    }

    private void UpdateScale() {
        float CutoutSize = PlayerPrefs.GetFloat("TokenScale", 1f);
        transform.Find("Offset/Avatar/Cutout").localScale = new Vector3(CutoutSize, CutoutSize, CutoutSize);
    }

    private void Offset() {
        float x = ShareOffsetX;
        float y = ShareOffsetY;
        if (Size == 2) {
            x = 0;
            y = -.73f;
        }
        else if (Size == 3) {
            x = 0;
            y = 0;
        }
        transform.Find("Offset").transform.localPosition = new Vector3(x, 0, y);
        transform.Find("Base").transform.localPosition = new Vector3(x, 0, y);
    }

    public void SetImage(Texture2D image) {
        Image = image;
        float aspectRatio = Image.width/(float)Image.height;
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetTexture("_Image", Image);
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").transform.localScale = new Vector3(aspectRatio, 1f, 1f);
    }

    public void LeftClickDown() {
        switch (State) {
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

    public void RightClickDown() {
        switch (State) {
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

    private void StartPending() {
        DeselectAll();
        State = TokenState.Pending;
        DragOrigin = Input.mousePosition;
        Cursor.Mode = CursorMode.Dragging;
    }

    private void StartMenu() {
        DeselectAll();
        State = TokenState.MenuOpen;
        TokenMenu.ShowMenu();
        Cursor.Mode = CursorMode.Default;
        Block.DeselectAll();
        Block.UnfocusAll();
        BlockMesh.ToggleBorders(false);
        UI.ToggleDisplay("CurrentOp", false);
        UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true); // selected indicator in unit bar
        Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UISelectYellow;
    }

    private void StartDragging() {
        State = TokenState.Dragging;
        Cursor.Mode = CursorMode.Dragging;
        Block.DeselectAll();
        Block.UnfocusAll();
        BlockMesh.ToggleBorders(true);
        UI.ToggleDisplay("CurrentOp", true);
        string op = Data.Placed ? "Moving" : "Placing";
        UI.System.Q("CurrentOp").Q<Label>("Op").text = $"{op} {Data.Name}"; 
        Player.Self().GetComponent<DestinationRenderer>().Init(Data.Id, op);
        UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true); // selected indicator in unit bar
        Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UISelectYellow;
    }

    private void StopDragging() {
        Block[] focused = Block.GetFocused();
        if (focused.Length == 0) {
            // Release outside map
            Deselect();
            return;
        }
        Block b = focused[0];
        if (b != GetBlock()) {
            // Release on new block
            Move(b);
        }
        else {
            // Release on same block
            Deselect();
        }
    }

    private void StartInspecting() {
        State = TokenState.Inspecting;
        Cursor.Mode = CursorMode.Default;
        BlockMesh.ToggleBorders(false);
        UI.ToggleDisplay("CurrentOp", true);
        UI.System.Q("CurrentOp").Q<Label>("Op").text = $"Inspecting {Data.Name}"; 
        Player.Self().GetComponent<DestinationRenderer>().Deinit();
    }

    // private void StartPlacing(){
    //     Block.DeselectAll();
    //     Block.UnfocusAll();
    //     BlockMesh.ToggleBorders(true);
    //     Cursor.Mode = CursorMode.Placing;
    //     UI.ToggleDisplay("CurrentOp", true);
    //     UI.System.Q("CurrentOp").Q<Label>("Op").text = $"Placing {Data.Name}"; 
    //     Player.Self().GetComponent<DestinationRenderer>().Init(Data.Id, "Placing");
    // }

    // public void Place(Block block) {
    //     Vector3 v = block.getMidpoint();
    //     Player.Self().CmdRequestPlaceToken(Data.Id, v);
    //     Deselect();
    // }

    // private void StartMoving() {
    //     SetVisualArrows();
    //     Block.DeselectAll();
    //     Block.UnfocusAll();
    //     BlockMesh.ToggleBorders(true);
    //     Cursor.Mode = CursorMode.Moving;
    //     UI.ToggleDisplay("CurrentOp", true);
    //     UI.System.Q("CurrentOp").Q<Label>("Op").text = $"Moving {Data.Name}"; 
    //     Player.Self().GetComponent<DestinationRenderer>().Init(Data.Id, "Moving");
    // }

    public void Move(Block block) {
        Deselect();
        Vector3 v = block.getMidpoint();
        if (Data.Placed) {
            Player.Self().CmdMoveToken(Data.Id, v, false);
        }
        else {
            Player.Self().CmdRequestPlaceToken(Data.Id, v);
        }
    }

    public void Remove() {
        Player.Self().CmdRequestRemoveToken(Data.Id);
    }

    // public void Select() {
    //     SoftSelect = true;
    //     UnfocusAll();
    //     DeselectAll();
    //     Selected = true;
    //     Data.Select();
    //     UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), true); // selected indicator in unit bar
    //     Data.UnitBarElement.Q("Selected").style.backgroundColor = ColorUtility.UISelectYellow;
    // }

    public void Deselect() {
        State = TokenState.Neutral;
        // Selected = false;
        // SoftSelect = true;
        Block.DehighlightAll();
        UI.ToggleDisplay(Data.UnitBarElement.Q("Selected"), false);
        UI.ToggleDisplay("CurrentOp", false);
        SelectionMenu.Hide();
        Cursor.Mode = CursorMode.Default;
        BlockMesh.ToggleBorders(false);
        Player.Self().GetComponent<DestinationRenderer>().Deinit();
    }

    public static void DeselectAll() {
        Token t = GetSelected();
        if (t) {
            t.Deselect();
        }
    }

    public static Token GetSelected() {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++) {
            TokenState ts = tokens[i].GetComponent<Token>().State;
            if (ts == TokenState.Dragging || ts == TokenState.Pending || ts == TokenState.MenuOpen || ts == TokenState.Inspecting) {
                return tokens[i].GetComponent<Token>();
            }
        }
        return null;
    }

    public void Focus() {
        if (Token.GetSelected() == this) {
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

    public void Unfocus() {
        if (State != TokenState.Focused) {
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

    public static Token GetFocused() {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < tokens.Length; i++) {
            if (tokens[i].GetComponent<Token>().State == TokenState.Focused) {

                return tokens[i].GetComponent<Token>();
            }
        }
        return null;
    }

    public static void UnfocusAll() {
        Token t = GetFocused();
        if (t) {
            t.Unfocus();
        }
    }

    public Block GetBlock() {
        return Block.GetClosest(transform.position);
    }

    public static Token GetAtBlock(Block b) {
        List<Token> nearby = TileShare.TokensNearby(b.transform.position, .5f);
        if (nearby.Count > 0) {
            return nearby[0];
        }
        return null;
    }

    private void SetVisualArrows() {
        // I don't really like this effect now that we have movement parabolas
        // SetVisual(false, true, false, false);

        // Use this one instead
        SetVisual(true, false, false, false);
    }

    private void SetVisualSquareYellow() {
        SetVisual(true, false, false, false);
    }

    private void SetVisualSquareBlue() {
        SetVisual(false, false, true, false);
    }

    private void SetVisualNone() {
        SetVisual(false, false, false, false);
    }

    private void SetVisual(bool yellowSquare, bool yellowArrows, bool blueSquare, bool blueArrows) {
        transform.Find("Offset/Select").GetComponent<MeshRenderer>().material.SetInt("_Selected", yellowSquare ? 1:0);
        transform.Find("Offset/Select").GetComponent<MeshRenderer>().material.SetInt("_Moving", yellowArrows ? 1:0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Selected", blueSquare ? 1:0);
        transform.Find("Offset/Focus").GetComponent<MeshRenderer>().material.SetInt("_Moving", blueArrows ? 1:0);
    }


    public void SetDefeated(bool defeated) {
        transform.Find("Offset/Avatar/Cutout/Cutout Quad").GetComponent<MeshRenderer>().material.SetInt("_Dead", defeated ? 1 : 0);
    }
    
}
