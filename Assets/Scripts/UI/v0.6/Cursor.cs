using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClickMode {
    Editing,
    Default,
    Placing,
    Moving
}

public class Cursor : MonoBehaviour
{  


    public static ClickMode Mode = ClickMode.Default;

    void Update()
    {
        if (UI.ClicksSuspended) {
            return;
        }

        if (!Player.IsOnline()) {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f)) {
            if (hit.collider.gameObject) {
                Block FocusedBlock = hit.collider.GetComponent<Block>();
                switch (Mode) {
                    case ClickMode.Default:
                    case ClickMode.Editing:
                        if (FocusedBlock) {
                            if (!FocusedBlock.Focused) {
                                FocusedBlock.Focus();
                            }
                            BlockClicks(FocusedBlock);
                        }
                        break;
                    case ClickMode.Moving:
                    case ClickMode.Placing:
                        Block.DehighlightAll();
                        HighlightSizeArea(FocusedBlock);
                        BlockClicks(FocusedBlock);
                        break;
                }
            }
        }
    }

    private void HighlightSizeArea(Block block) {
        if (block == null) {
            return;
        }
        if (TokenController.GetSelected() == null) {
            return;
        }
        block.Highlight();
        int size = TokenController.GetSelected().Size;
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
}
