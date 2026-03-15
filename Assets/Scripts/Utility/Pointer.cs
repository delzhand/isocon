using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer
{
    private static Ray _ray;
    private static Actor _unitBarMouseoverToken;
    public static Actor UnitBarMouseoverActor
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

    public static Vector3 PickPoint()
    {
        if (UI.ClicksSuspended || Modal.IsOpen())
        {
            return Vector3.zero;
        }
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(_ray, out RaycastHit hit, 9999f, LayerMask.GetMask("Block"));
        if (isHit && hit.collider.CompareTag("Block"))
        {
            return hit.collider.GetComponent<Block>().GetNearestCorner(hit.point);
        }
        return Vector3.zero;
    }

    public static Actor PickToken(bool worldOnly = false)
    {
        if (_unitBarMouseoverToken && !worldOnly)
        {
            return _unitBarMouseoverToken;
        }

        if (UI.ClicksSuspended || Modal.IsOpen())
        {
            return null;
        }
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool isHit = Physics.Raycast(_ray, out RaycastHit hit, 9999f, LayerMask.GetMask("Actor"));
        if (isHit && hit.collider.CompareTag("TokenCollider"))
        {
            return hit.collider.GetComponent<Cutout>().GetActor();
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
        if (Actor.GetSelected()?.Data.Shape == "Square 2x2")
            PointWithMask(LayerMask.GetMask("Block"), focusMode);
    }

    public static void Point()
    {
        PointWithMask(LayerMask.GetMask("Actor", "Block"), BlockFocusMode.Single);
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
            Actor t = Actor.GetAtBlock(b);
            if (t != null && MaskContainsLayer(mask, "Actor"))
            {
                TokenHit(t);
                FocusBlocks(b, focusMode);
            }
            else
            {
                BlockHit(b, focusMode);
                Actor.UnfocusAll();
            }
        }
        else if (isHit && hit.collider.CompareTag("TokenCollider"))
        {
            Actor t = hit.collider.GetComponent<Cutout>().GetActor();
            FocusBlocks(t.GetBlock(), focusMode);
            TokenHit(t);
        }
        else if (!isHit)
        {
            Actor.UnfocusAll();
            Block.UnfocusAll();
        }
    }

    private static void TokenHit(Actor t)
    {
        if (t.State != ActorState.Focused)
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
                foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetRow(new Vector2Int(b.Coordinate.x, b.Coordinate.y))))
                {
                    block.Focused = true;
                }
                break;
            case BlockFocusMode.Column:
                Block.UnfocusAll();
                foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetColumn(new Vector2Int(b.Coordinate.x, b.Coordinate.y))))
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
        if (Actor.GetDragging() != null)
        {
            HighlightSizeArea(b);
        }

        if (!b.Focused)
        {
            FocusBlocks(b, mode);
        }

        if (Actor.GetSelected() != null && Actor.GetSelected().Data.CornerTargeting())
        {
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(_ray, out RaycastHit hit, 9999f, LayerMask.GetMask("Block"));
            Player.Self().GetComponent<DirectionalLine>().SetTarget(b.GetNearestCorner(hit.point));
        }
        else
        {
            Player.Self().GetComponent<DirectionalLine>().SetTarget(b.GetMidpoint());
        }
    }

    private static void HighlightSizeArea(Block block)
    {
        block.Highlight();
        int size = Actor.GetDragging().Size;
        Block[] neighbors = TerrainController.FindNeighbors(block, size);
        for (int i = 0; i < neighbors.Length; i++)
        {
            neighbors[i]?.Highlight();
        }
    }
}
