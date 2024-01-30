using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer
{
    private static Ray _ray;

    public static void Point()
    {
        PointWithMask(LayerMask.GetMask("Block", "Token"), BlockFocusMode.Single);
    }

    public static void PointAtBlocks(BlockFocusMode mode)
    {
        PointWithMask(LayerMask.GetMask("Block"), mode);
    }

    private static void PointWithMask(LayerMask mask, BlockFocusMode focusMode)
    {
        if (Viewport.IsDragging)
        {
            return;
        }

        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(_ray, out RaycastHit hit, 9999f, mask);
        if (isHit && hit.collider.tag == "Block")
        {
            Block b = hit.collider.GetComponent<Block>();
            Token t = Token.GetAtBlock(b);
            if (t != null && Cursor.Mode != CursorMode.Editing)
            {
                TokenHit(t);
                FocusBlocks(b, focusMode);
            }
            else
            {
                BlockHit(b, focusMode);
                Token.UnfocusAll();
            }
        }
        else if (isHit && hit.collider.tag == "TokenCollider")
        {
            Token t = hit.collider.GetComponent<Cutout>().GetToken();
            FocusBlocks(t.GetBlock(), focusMode);
            TokenHit(t);
        }
        else if (!isHit)
        {
            Token.UnfocusAll();
            Block.UnfocusAll();
            Block.DehighlightAll();
        }
    }

    private static void TokenHit(Token t)
    {
        // switch (Mode)
        // {
        //     case CursorMode.Default:
        //     case CursorMode.Dragging:
        //         if (t.State != TokenState.Focused)
        //         {
        //             t.Focus();
        //         }
        //         TokenClicks(t);
        //         Block.DehighlightAll();
        //         break;
        // }
    }

    private static void TokenClicks(Token token)
    {
        if (token == null)
        {
            return;
        }
        if (IsLeftClick())
        {
            token.LeftClickDown();
        }
        if (IsRightClick())
        {
            token.RightClickDown();
        }
    }

    private static void FocusBlocks(Block b, BlockFocusMode mode)
    {
        switch (mode)
        {
            case BlockFocusMode.Single:
                b.Focus();
                break;
            case BlockFocusMode.Row:
                Block.UnfocusAll();
                foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetRow(new Vector2Int(b.GetX(), b.GetY()))))
                {
                    block.Focused = true;
                }
                break;
            case BlockFocusMode.Column:
                Block.UnfocusAll();
                foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetColumn(new Vector2Int(b.GetX(), b.GetY()))))
                {
                    block.Focused = true;
                }
                break;
            default:
                Debug.LogError($"Unsupported BlockFocusMode {mode}");
                break;
        }
    }

    private static void BlockHit(Block b, BlockFocusMode mode)
    {
        // switch (Mode)
        // {
        //     case CursorMode.Dragging:
        //         Block.DehighlightAll();
        //         HighlightSizeArea(b);
        //         break;
        // }

        if (!b.Focused)
        {
            FocusBlocks(b, mode);
        }

        BlockClicks(b);
    }


    private static void BlockClicks(Block block)
    {

        if (block == null)
        {
            return;
        }
        if (IsLeftClick())
        {
            block.LeftClickDown();
        }
        else if (IsLeftHeld())
        {
            // block.LeftClickHeld();
        }
        if (IsRightClick())
        {
            block.RightClickDown();
        }
    }

    #region Helpers
    public static bool IsRightClick() => Input.GetMouseButtonDown(1);
    public static bool IsLeftClick() => Input.GetMouseButtonDown(0);
    public static bool IsLeftHeld() => Input.GetMouseButton(0);
    #endregion 
}
