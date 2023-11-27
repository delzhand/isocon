using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorMode {
    Editing,
    Default,
    Placing,
    Moving
}

public class Cursor : MonoBehaviour
{  


    public static CursorMode Mode = CursorMode.Default;
    private static Ray ray;
    private static RaycastHit hit;

    void Update()
    {
        if (UI.ClicksSuspended) {
            return;
        }

        if (!Player.IsOnline()) {
            return;
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100f)) {
            BlockHitCheck(hit);
            TokenHitCheck(hit);
        }
        else {
            Block.UnfocusAll();
            Token.UnfocusAll();
        }
    }

    private void BlockHitCheck(RaycastHit hit) {
        if (!hit.collider.gameObject) {
            return;
        }

        Block b = hit.collider.GetComponent<Block>();
        if (!b) {
            return;
        }

        switch (Mode) {
            case CursorMode.Default:
            case CursorMode.Editing:
                if (!b.Focused) {
                    b.Focus();
                }
                BlockClicks(b);
                break;
            case CursorMode.Moving:
            case CursorMode.Placing:
                Block.DehighlightAll();
                HighlightSizeArea(b);
                BlockClicks(b);
                break;
        }
    }

    private void TokenHitCheck(RaycastHit hit) {
        if (!hit.collider.gameObject) {
            return;
        }

        Cutout c = hit.collider.GetComponent<Cutout>();
        if (!c) {
            return;
        }

        Token t = c.GetToken();
        switch (Mode) {
            case CursorMode.Default:
                if (!t.Focused) {
                    t.Focus();
                }
                TokenClicks(t);
                break;
            case CursorMode.Moving:
            case CursorMode.Placing:
                TokenClicks(t);
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
        if (Input.GetMouseButtonDown(0)) {
            block.LeftClick();
        }
        if (Input.GetMouseButtonDown(1)) {
            block.RightClick();
        }
    }

    private void TokenClicks(Token token) {
        if (token == null) {
            return;
        }
        if (Input.GetMouseButtonDown(0)) {
            token.LeftClick();
        }
        if (Input.GetMouseButtonDown(1)) {
            token.RightClick();
        }
    }
}
