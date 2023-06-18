using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainEngine : MonoBehaviour
{
    static GameObject map;

    void Start() {
        InitializeTerrain(8, 8);
    }

    private static void findMap() {
        if (map == null) {
            map = GameObject.Find("Terrain");
        }
    }

    public static void InitializeTerrain(int length, int width) {
        findMap();
        for (int y = 0; y < width; y++) {
            for (int x = 0; x < length; x++) {
                try {
                    GameObject column = new GameObject();
                    column.name = x + "," + y;
                    column.transform.parent = map.transform;
                    column.transform.localPosition = new Vector3(x, 0, y);
                    column.transform.localScale = Vector3.one;
                    column.AddComponent<Column>().Set(x, y);

                    GameObject block = Instantiate(Resources.Load("Prefabs/Block") as GameObject);
                    block.name = "block";
                    block.transform.parent = column.transform;
                    block.transform.localPosition = Vector3.zero;
                    block.transform.localScale = Vector3.one;
                    block.GetComponent<Block>().Destroyable = false;
                }
                catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }
        }
    }

    public static void AddBlocks() {        
        List<GameObject> selected = Block.GetAllSelected();
        List<Column> markedCols = new List<Column>();
        selected.ForEach(block => {
            Column column = block.transform.parent.GetComponent<Column>();
            if (!markedCols.Contains(column)) {
                markedCols.Add(column); 
                GameObject currentTop = TopBlock(column.gameObject);
                GameObject newblock = Instantiate(Resources.Load("Prefabs/Block") as GameObject);                 
                newblock.transform.parent = block.transform.parent;
                newblock.transform.localPosition = new Vector3(0, currentTop.transform.localPosition.y + 1, 0);
                newblock.transform.localScale = block.transform.localScale;
                // Change type as appropriate
            }
        });
    }

    public static void RemoveBlocks() {
        List<GameObject> selected = Block.GetAllSelected();
        selected.ForEach(block =>  {
            if (block.GetComponent<Block>().Destroyable) {
                GravityDrop(block.transform.parent.gameObject, block.transform.localPosition.y);
                GameObject.DestroyImmediate(block);
            }
        });
        if (!Block.AreAnySelected()) {
            GameObject.Find("Canvas/BlockControls").GetComponent<UIDocument>().rootVisualElement.style.opacity = 0;
        }
    }

    public static void RotateBlocks() {
        List<GameObject> selected = Block.GetAllSelected();
        selected.ForEach(block => {
            block.transform.Rotate(0, 90f, 0);
        });
    }

    public static void ChangeType(BlockType type) {
        List<GameObject> selected = Block.GetAllSelected();
        selected.ForEach(block => {
            block.GetComponent<Block>().TypeChange(type);
            block.transform.Rotate(0, 90f, 0);
        });

    }

    public static void ChangeMarker(BlockMarker marker) {
        List<GameObject> selected = Block.GetAllSelected();
        selected.ForEach(block => {
            block.GetComponent<Block>().MarkerChange(marker);
        });
    }

    private static GameObject TopBlock(GameObject column) {
        Block[] blocks = column.GetComponentsInChildren<Block>();
        GameObject top = null;
        float highest = -1;
        for (int i = 0; i < blocks.Length; i++)
        {
            float height = blocks[i].transform.localPosition.y;
            if (height > highest) {
                top = blocks[i].gameObject;
            }
        }
        return top;
    }

    private static void GravityDrop(GameObject column, float threshold) {
        Block[] blocks = column.GetComponentsInChildren<Block>();
        for (int i = 0; i < blocks.Length; i++) {
            if (blocks[i].transform.localPosition.y > threshold) {
                blocks[i].transform.localPosition -= new Vector3(0, 1, 0);
            }
        }
    }
}
