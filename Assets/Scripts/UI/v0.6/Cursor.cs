using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public enum CursorMode {
    Editing,
    Default,
    Dragging,
    TerrainEffecting
}

public enum FocusMode {
    Single,
    Row,
    Column,
}

public class Cursor : MonoBehaviour
{  
    public static CursorMode Mode = CursorMode.Default;
    public static FocusMode FocusMode { get; set; } = FocusMode.Single;
    private static Ray ray;
    public static bool OverUnitBarElement = false;


    void Update()
    {
        if (Modal.IsOpen()) {
            return;
        }
        if (UI.ClicksSuspended) {
            return;
        }

        if (!Player.IsOnline()) {
            return;
        }

        if (CameraControl.Drag) {
            return;
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        bool isHit = Physics.Raycast(ray, out hit, 100f);
        if (isHit && hit.collider.tag == "Block") {
            Block b = hit.collider.GetComponent<Block>();
            Token t = Token.GetAtBlock(b);
            if (t != null) {
                TokenHit(t);
                Focus(b);
            }
            else {
                BlockHit(b);
                Token.UnfocusAll();
            }
        }
        else if (isHit && hit.collider.tag == "TokenCollider") {
            Token t = hit.collider.GetComponent<Cutout>().GetToken();
            Focus(t.GetBlock());
            TokenHit(t);
        }
        else if (!isHit) {
            Token.UnfocusAll();
            Block.UnfocusAll();
            Block.DehighlightAll();
        }

        TerrainController.SetInfo();
    }

    private void BlockHit(Block b) {
        switch (Mode) {
            case CursorMode.Dragging:
                Block.DehighlightAll();
                HighlightSizeArea(b);
                break;
        }

        if (!b.Focused) {
            Focus(b);
        }
        
        BlockClicks(b);
    }

    private void Focus(Block b)
    {
        switch (FocusMode)
        {
            case FocusMode.Single:
                b.Focus();
                break;
            case FocusMode.Row:
                Block.UnfocusAll();
                foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetRow(new Vector2Int(b.getX(), b.getY()))))
                {
                    block.Focused = true;
                }
                break;
            case FocusMode.Column:
                Block.UnfocusAll();
                foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetColumn(new Vector2Int(b.getX(), b.getY()))))
                {
                    block.Focused = true;
                }
                break;
            default:
                Debug.LogError($"Unsupported Focus Mode {FocusMode}");
                break;
        }
    }

    private void TokenHit(Token t) {
        switch (Mode) {
            case CursorMode.Default:
            case CursorMode.Dragging:
                if (t.State != TokenState.Focused) {
                    t.Focus();
                }
                TokenClicks(t);
                Block.DehighlightAll();
                break;
        }
    }

    private void HighlightSizeArea(Block block) {
        block.Highlight();
        int size = Token.GetSelected().Size;
        Block[] neighbors = TerrainController.FindNeighbors(block, size);
        for(int i = 0; i < neighbors.Length; i++) {
            neighbors[i].Highlight();
        }
    }

    private void BlockClicks(Block block) {
        if (block == null) {
            return;
        }
        if (IsLeftClick()) {
            block.LeftClick();
        }
        if (IsRightClick()) {
            block.RightClick();
        }
    }

    private void TokenClicks(Token token) {
        if (token == null) {
            return;
        }
        if (IsLeftClick()) {
            token.LeftClickDown();
        }
        if (IsRightClick()) {
            token.RightClickDown();
        }
    }

    public static bool IsRightClick() {
        return Input.GetMouseButtonDown(1);
    }

    public static bool IsLeftClick() {
        return Input.GetMouseButtonDown(0);
    }
}
