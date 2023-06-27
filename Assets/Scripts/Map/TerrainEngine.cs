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
                    column.tag = "Column";
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

    public static Vector3 Center() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        float lowX = float.MaxValue;
        float highX = float.MinValue;
        float lowZ = float.MaxValue;
        float highZ = float.MinValue;
        for (int i = 0; i < blocks.Length; i++) {
            float x = blocks[i].transform.position.x;
            float z = blocks[i].transform.position.z;
            if (x < lowX) {
                lowX = x;
            }
            if (x > highX) {
                highX = x;
            }
            if (z < lowZ) {
                lowZ = z;
            }
            if (z > highZ) {
                highZ = z;
            }
        }
        float centerX = (highX - lowX) / 2f;
        float centerZ = (highZ - lowZ) / 2f;
        return new Vector3(lowX + centerX, 0, lowZ + centerZ);

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
            else {
                UI.SetHelpText("Foundation blocks cannot be deleted (but can be changed to SHAPE_EMPTY)", HelpType.Error);
            }
        });
    }

    public static void RotateBlocks() {
        List<GameObject> selected = Block.GetAllSelected();
        selected.ForEach(block => {
            block.transform.Rotate(0, 90f, 0);
        });
    }

    public static void CloneRow() {
        List<GameObject> selected = Block.GetAllSelected();
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        selected.ForEach(selectedBlock => {
            for (int i = 0; i < columns.Length; i++) {
                GameObject column = columns[i];
                int x = column.GetComponent<Column>().X;
                int y = column.GetComponent<Column>().Y;
                if (x == selectedBlock.transform.parent.GetComponent<Column>().X) {
                    GameObject clone = GameObject.Instantiate(column);
                    clone.transform.parent = column.transform.parent;
                    clone.transform.localScale = Vector3.one;
                    clone.transform.localPosition += new Vector3(1, 0, 0);
                    clone.name = (x+1) + "," + y;
                    clone.GetComponent<Column>().Set(x + 1, y);
                }
                if (x > selectedBlock.transform.parent.GetComponent<Column>().X) {
                    column.transform.localPosition += new Vector3(1, 0, 0);
                    column.name = (x+1) + "," + y;
                    column.GetComponent<Column>().Set((x+1), y);
                }
            }
        });
    }

    public static void CloneColumn() {
        List<GameObject> selected = Block.GetAllSelected();
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        selected.ForEach(selectedBlock => {
            for (int i = 0; i < columns.Length; i++) {
                GameObject column = columns[i];
                int x = column.GetComponent<Column>().X;
                int y = column.GetComponent<Column>().Y;
                if (y == selectedBlock.transform.parent.GetComponent<Column>().Y) {
                    GameObject clone = GameObject.Instantiate(column);
                    clone.transform.parent = column.transform.parent;
                    clone.transform.localScale = Vector3.one;
                    clone.transform.localPosition += new Vector3(0, 0, 1);
                    clone.name = x + "," + (y+1);
                    clone.GetComponent<Column>().Set(x, y + 1);
                }
                if (y > selectedBlock.transform.parent.GetComponent<Column>().Y) {
                    column.transform.localPosition += new Vector3(0, 0, 1);
                    column.name = x + "," + (y+1);
                    column.GetComponent<Column>().Set(x, (y+1));
                }
            }
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
