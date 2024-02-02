using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer
{
    private static Ray _ray;
    private static Token _unitBarMouseoverToken;
    public static Token UnitBarMouseoverToken
    {
        get => _unitBarMouseoverToken;
        set => _unitBarMouseoverToken = value;
    }

    public static Block PickBlock()
    {
        if (UI.ClicksSuspended || Modal.IsOpen())
        {
            return null;
        }
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(_ray, out RaycastHit hit, 9999f, LayerMask.GetMask("Block"));
        if (isHit && hit.collider.CompareTag("Block"))
        {
            return hit.collider.GetComponent<Block>();
        }
        return null;
    }

    public static Token PickToken()
    {
        if (_unitBarMouseoverToken)
        {
            return _unitBarMouseoverToken;
        }

        if (UI.ClicksSuspended || Modal.IsOpen())
        {
            return null;
        }
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(_ray, out RaycastHit hit, 9999f, LayerMask.GetMask("Token"));
        if (isHit && hit.collider.CompareTag("TokenCollider"))
        {
            return hit.collider.GetComponent<Cutout>().GetToken();
        }

        return null;
    }

    public static void PointAtBlocks()
    {
        var focusMode = BlockFocusMode.Single;
        if (MapEdit.EditOp == "ResizeMap" && MapEdit.ResizeOp.EndsWith("Col"))
        {
            focusMode = BlockFocusMode.Column;
        }
        else if (MapEdit.EditOp == "ResizeMap" && MapEdit.ResizeOp.EndsWith("Row"))
        {
            focusMode = BlockFocusMode.Row;
        }
        PointWithMask(LayerMask.GetMask("Block"), focusMode);
    }

    public static void Point()
    {
        PointWithMask(LayerMask.GetMask("Token", "Block"), BlockFocusMode.Single);
    }

    private static bool MaskContainsLayer(LayerMask layermask, string layer)
    {
        return layermask == (layermask | (1 << LayerMask.NameToLayer(layer)));
    }

    private static void PointWithMask(LayerMask mask, BlockFocusMode focusMode)
    {
        Block.DehighlightAll();

        if (Viewport.IsDragging || UI.ClicksSuspended || Modal.IsOpen())
        {
            return;
        }

        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(_ray, out RaycastHit hit, 9999f, mask);
        if (isHit && hit.collider.CompareTag("Block"))
        {
            Block b = hit.collider.GetComponent<Block>();
            Token t = Token.GetAtBlock(b);
            if (t != null && MaskContainsLayer(mask, "Token"))
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
        else if (isHit && hit.collider.CompareTag("TokenCollider"))
        {
            Token t = hit.collider.GetComponent<Cutout>().GetToken();
            FocusBlocks(t.GetBlock(), focusMode);
            TokenHit(t);
        }
        else if (!isHit)
        {
            Token.UnfocusAll();
            Block.UnfocusAll();
        }
    }

    private static void TokenHit(Token t)
    {
        if (t.State != TokenState.Focused)
        {
            t.Focus();
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
        if (Token.GetDragging() != null)
        {
            HighlightSizeArea(b);
        }

        if (!b.Focused)
        {
            FocusBlocks(b, mode);
        }
    }

    private static void HighlightSizeArea(Block block)
    {
        block.Highlight();
        int size = Token.GetSelected().Size;
        Block[] neighbors = TerrainController.FindNeighbors(block, size);
        for (int i = 0; i < neighbors.Length; i++)
        {
            neighbors[i]?.Highlight();
        }
    }
}
