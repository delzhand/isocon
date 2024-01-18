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

public class Cursor : MonoBehaviour
{  
    public static CursorMode Mode = CursorMode.Default;
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
                b.Focus();
            }
            else {
                BlockHit(b);
                Token.UnfocusAll();
            }
        }
        else if (isHit && hit.collider.tag == "TokenCollider") {
            Token t = hit.collider.GetComponent<Cutout>().GetToken();
            t.GetBlock().Focus();
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
            b.Focus();
        }
        
        BlockClicks(b);
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
