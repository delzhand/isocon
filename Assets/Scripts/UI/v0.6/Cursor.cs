using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public enum CursorMode
{
    Editing,
    Default,
    Dragging,
    Marking,
    Targeting,
}

public enum FocusMode
{
    Single,
    Row,
    Column,
}

public class Cursor : MonoBehaviour
{
    // public static CursorMode Mode { get; set; } = CursorMode.Default;
    // public static FocusMode FocusMode { get; set; } = FocusMode.Single;
    // private static Ray ray;
    // public static bool OverUnitBarElement = false;

    // void Update()
    // {
    //     if (Modal.IsOpen())
    //     {
    //         return;
    //     }
    //     if (UI.ClicksSuspended && Mode != CursorMode.Dragging)
    //     {
    //         return;
    //     }

    //     if (!Player.IsOnline())
    //     {
    //         return;
    //     }

    //     if (CameraControl.Drag)
    //     {
    //         return;
    //     }

    //     SetFocusMode();
    //     ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //     bool isHit = Physics.Raycast(ray, out RaycastHit hit, 9999f, GetMaskForCursorMode(Mode));
    //     if (isHit && hit.collider.tag == "Block")
    //     {
    //         Block b = hit.collider.GetComponent<Block>();
    //         Token t = Token.GetAtBlock(b);
    //         if (t != null && Cursor.Mode != CursorMode.Editing)
    //         {
    //             TokenHit(t);
    //             Focus(b);
    //         }
    //         else
    //         {
    //             BlockHit(b);
    //             Token.UnfocusAll();
    //         }
    //     }
    //     else if (isHit && hit.collider.tag == "TokenCollider")
    //     {
    //         Token t = hit.collider.GetComponent<Cutout>().GetToken();
    //         Focus(t.GetBlock());
    //         TokenHit(t);
    //     }
    //     else if (!isHit)
    //     {
    //         Token.UnfocusAll();
    //         Block.UnfocusAll();
    //         Block.DehighlightAll();
    //     }

    //     TerrainController.SetInfo();
    // }

    // private void BlockHit(Block b)
    // {
    //     switch (Mode)
    //     {
    //         case CursorMode.Dragging:
    //             Block.DehighlightAll();
    //             HighlightSizeArea(b);
    //             break;
    //     }

    //     if (!b.Focused)
    //     {
    //         Focus(b);
    //     }

    //     BlockClicks(b);
    // }

    // private void Focus(Block b)
    // {
    //     switch (FocusMode)
    //     {
    //         case FocusMode.Single:
    //             b.Focus();
    //             break;
    //         case FocusMode.Row:
    //             Block.UnfocusAll();
    //             foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetRow(new Vector2Int(b.GetX(), b.GetY()))))
    //             {
    //                 block.Focused = true;
    //             }
    //             break;
    //         case FocusMode.Column:
    //             Block.UnfocusAll();
    //             foreach (var block in Block.GetTopBlocks(CoordinateUtility.GetColumn(new Vector2Int(b.GetX(), b.GetY()))))
    //             {
    //                 block.Focused = true;
    //             }
    //             break;
    //         default:
    //             Debug.LogError($"Unsupported Focus Mode {FocusMode}");
    //             break;
    //     }
    // }

    // private void TokenHit(Token t)
    // {
    //     switch (Mode)
    //     {
    //         case CursorMode.Default:
    //         case CursorMode.Dragging:
    //             if (t.State != TokenState.Focused)
    //             {
    //                 t.Focus();
    //             }
    //             TokenClicks(t);
    //             Block.DehighlightAll();
    //             break;
    //     }
    // }

    // private void HighlightSizeArea(Block block)
    // {
    //     block.Highlight();
    //     int size = Token.GetSelected().Size;
    //     Block[] neighbors = TerrainController.FindNeighbors(block, size);
    //     for (int i = 0; i < neighbors.Length; i++)
    //     {
    //         neighbors[i].Highlight();
    //     }
    // }

    // private void BlockClicks(Block block)
    // {

    //     if (block == null)
    //     {
    //         return;
    //     }
    //     if (IsLeftClick())
    //     {
    //         block.LeftClickDown();
    //     }
    //     else if (IsLeftHeld())
    //     {
    //         block.LeftClickHeld();
    //     }
    //     if (IsRightClick())
    //     {
    //         block.RightClickDown();
    //     }
    // }

    // private void TokenClicks(Token token)
    // {
    //     if (token == null)
    //     {
    //         return;
    //     }
    //     if (IsLeftClick())
    //     {
    //         token.LeftClickDown();
    //     }
    //     if (IsRightClick())
    //     {
    //         token.RightClickDown();
    //     }
    // }

    // public static bool IsRightClick()
    // {
    //     return Input.GetMouseButtonDown(1);
    // }

    // public static bool IsLeftClick()
    // {
    //     return Input.GetMouseButtonDown(0);
    // }

    // public static bool IsLeftHeld() => Input.GetMouseButton(0);

    // private void SetFocusMode()
    // {
    //     if (Mode == CursorMode.Editing)
    //     {
    //         switch (MapEdit.EditOp)
    //         {
    //             case "ResizeMap":
    //                 switch (MapEdit.ResizeOp)
    //                 {
    //                     case "ResizeCloneRow":
    //                     case "ResizeDeleteRow":
    //                         FocusMode = FocusMode.Row;
    //                         break;
    //                     case "ResizeCloneCol":
    //                     case "ResizeDeleteCol":
    //                         FocusMode = FocusMode.Column;
    //                         break;
    //                     default:
    //                         FocusMode = FocusMode.Single;
    //                         break;
    //                 }
    //                 break;
    //             default:
    //                 FocusMode = FocusMode.Single;
    //                 break;
    //         }
    //     }
    // }

    // public static bool IgnoreTokens()
    // {
    //     return Mode == CursorMode.Editing || Mode == CursorMode.Marking;
    // }

    // private static LayerMask GetMaskForCursorMode(CursorMode mode)
    // {
    //     // Set the raycast filter to suit the cursor mode we are using
    //     switch (mode)
    //     {
    //         case CursorMode.Default:
    //         case CursorMode.Targeting:
    //             return LayerMask.GetMask("Block", "Token");
    //         case CursorMode.Dragging:
    //         case CursorMode.Marking:
    //         case CursorMode.Editing:
    //             return LayerMask.GetMask("Block");
    //         default:
    //             Debug.LogError($"{mode} does not have an explicitly defined raycast filter yet");
    //             return LayerMask.GetMask("Block", "Token");
    //     }
    // }
}
