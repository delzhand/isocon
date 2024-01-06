using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

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
    private bool firstBlockHit = false;
    private bool firstTokenHit = false;
    private bool firstHit = false;

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

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray, 100f); // use an array so we can hit the block behind a token
        System.Array.Sort(hits, (x,y) => x.distance.CompareTo(y.distance));
        firstHit = false;
        firstBlockHit = false;
        firstTokenHit = false;
        if (hits.Length > 0) {
            foreach (RaycastHit hit in hits) {    
                TokenHitCheck(hit);
                BlockHitCheck(hit);
            }
        }

        if (!firstHit) {
            Block.UnfocusAll();
            Block.DehighlightAll();
            if (Block.GetSelected().Length == 0) {
                TerrainController.SetInfo();
            }
        }

        if (!firstTokenHit && !OverUnitBarElement) {
            Token.UnfocusAll();
        }

        if (Block.GetSelected().Length == 0) {
            TerrainController.SetInfo();
        }

        if (!firstHit && Block.GetSelected().Length == 0) {
            // Debug.Log("foo");
            Block.UnfocusAll();
            TerrainController.SetInfo();
        }

        // else {
        //     Block.UnfocusAll();
        //     Block.DehighlightAll();
        //     Token.UnfocusAll();
        //     if (Block.GetSelected().Length == 0) {
        //         TerrainController.SetInfo();
        //     }
        // }
    }

    private void BlockHitCheck(RaycastHit hit) {
        if (firstBlockHit) {
            // Ignore blocks past the first one on the ray
            return;
        }

        if (!hit.collider || !hit.collider.gameObject) {
            // Block.UnfocusAll();
            // Block.DehighlightAll();
            return;
        }

        Block b = hit.collider.GetComponent<Block>();
        if (!b) {
            // Block.UnfocusAll();
            // Block.DehighlightAll();
            return;
        }


        switch (Mode) {
            case CursorMode.Moving:
            case CursorMode.Placing:
                Block.DehighlightAll();
                HighlightSizeArea(b);
                break;
        }

        if (!b.Focused) {
            b.Focus();
        }
        
        if (!firstTokenHit) {
            BlockClicks(b);
        }

        firstHit = true;
        firstBlockHit = true;
    }

    private void TokenHitCheck(RaycastHit hit) {
        if (firstHit) {
            return;
        }

        if (!hit.collider || !hit.collider.gameObject) {
            // Token.UnfocusAll();
            return;
        }


        Cutout c = hit.collider.GetComponent<Cutout>();
        if (!c) {
            // if the hit was a block, don't do this
            // Token.UnfocusAll();
            return;
        }

        Token t = c.GetToken();
        switch (Mode) {
            case CursorMode.Default:
            case CursorMode.Moving:
            case CursorMode.Placing:
                if (!t.Focused) {
                    t.Focus();
                }
                TokenClicks(t);
                break;
        }

        firstHit = true;
        firstTokenHit = true;
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
            token.LeftClick();
        }
        if (IsRightClick()) {
            token.RightClick();
        }
    }

    public static bool IsRightClick() {
        return Input.GetMouseButtonDown(1);
    }

    public static bool IsLeftClick() {
        return Input.GetMouseButtonDown(0);
    }
}
