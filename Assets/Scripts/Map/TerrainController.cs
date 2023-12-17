using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using TMPro;

public class TerrainController : MonoBehaviour
{
    public static string GridType = "Square";
    public static float LightAngle = 330f;
    public static float LightHeight = 50f;
    public static float LightIntensity = 2;
    
    private List<Label> identifierLabels = new List<Label>();

    public static bool Indicators = false;
    public static bool Editing = false;

    public static bool ReorgNeeded = false;

    public static bool MapDirty = false;

    void LateUpdate() {
        if (ReorgNeeded) {
            Reorg();
            ReorgNeeded = false;
        }
    }

    public static void Edit(Block block) {
        MapDirty = true;
        switch (MapEdit.EditOp) {
            case "AddBlock":
                AddBlocks();
                break;
            case "RemoveBlock":
                RemoveBlocks();
                break;
            case "RotateBlock":
                RotateBlocks();
                break;
            case "ResizeMap":
                ResizeMap();
                break;
            case "ChangeShape":
                ChangeShape();
                break;
            case "StyleBlock":
                ApplyStyle();
                break;
            case "TerrainEffect":
                ChangeEffect();
                break;
        }
    }

    public static void DestroyAllBlocks() {
        GameObject terrain = GameObject.Find("Terrain");
        terrain.name = "_Terrain";
        GameObject.DestroyImmediate(terrain);
        new GameObject("Terrain").transform.localScale = new Vector3(1, .5f, 1);
    }

    public static void InitializeTerrain(int length, int width, int height) {
        DestroyAllBlocks();
        GameObject map = GameObject.Find("Terrain");
        for (int y = 0; y < width; y++) {
            for (int x = 0; x < length; x++) {
                try {
                    string columnName = x + "," + y;
                    GameObject column = GameObject.Find(columnName);
                    if (column == null) {
                        column = new GameObject();
                        column.tag = "Column";
                        column.name = columnName;
                        column.transform.parent = map.transform;
                        column.transform.localPosition = new Vector3(x, 0, y);
                        column.transform.localScale = Vector3.one;
                        column.AddComponent<Column>().Set(x, y);
                    }
                    for (int z = 0; z < height; z++) {
                        GameObject block = Instantiate(Resources.Load("Prefabs/Block") as GameObject);
                        block.transform.parent = column.transform;
                        block.transform.localPosition = new Vector3(0, z, 0);
                        block.transform.localScale = Vector3.one;
                        if (z == 0) {
                            block.GetComponent<Block>().Destroyable = false;
                        }
                        Block b = block.GetComponent<Block>();
                        block.name = "block " + b.getX() + "," + b.getY() + "," + b.getZ();
                    }
                }
                catch (Exception e) {
                    Debug.LogError(e.Message);
                }
            }
        }
        ReorgNeeded = true;
    }

    public static void ResetTerrain() {
        DestroyAllBlocks();
        InitializeTerrain(8, 8, 1);
        MapDirty = false;
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

    public static Vector2Int Size() {
        Vector2Int size = Vector2Int.zero;
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            size.x = Mathf.Max(size.x, blocks[i].GetComponent<Block>().getX()+1);
            size.y = Mathf.Max(size.y, blocks[i].GetComponent<Block>().getY()+1);
        }
        return size;
    }

    public static void AddBlocks() {        
        List<Block> selected = Block.GetSelected().ToList();
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
        ReorgNeeded = true;
    }

    public static void RemoveBlocks() {
        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block =>  {
            if (block.Destroyable) {
                GravityDrop(block.transform.parent.gameObject, block.transform.localPosition.y);
                GameObject.DestroyImmediate(block.gameObject);
            }
            else {
                Toast.Add("Foundation blocks cannot be deleted (but can be hidden).", ToastType.Error);
            }
        });
        ReorgNeeded = true;
    }

    public static void MultiBlock() {
        switch (MapEdit.ResizeOp) {
            case "CloneRow":
                CloneRow();
                break;
            case "CloneCol":
                CloneColumn();
                break;
            case "RemoveRow":
                DeleteRow();
                break;
            case "RemoveCol":
                DeleteColumn();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static void RotateBlocks() {
        // List<Block> selected = Block.GetSelected().ToList();
        // selected.ForEach(block => {
        //     if (GridType == "Square") {
        //         block.transform.Rotate(0, 90f, 0);
        //     }
        //     else if (GridType == "Hex") {
        //         block.transform.Rotate(0, 60f, 0);
        //     }
        // });

        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block => {
            if (GridType == "Square") {
                block.transform.Rotate(0, 90f, 0);
                if (block.Type == BlockShape.Slope) {
                    // counter-rotate indicator
                    block.transform.Find("Indicator").transform.eulerAngles = new Vector3(90, -90, 0);
                }
            }
            else if (GridType == "Hex") {
                block.transform.Rotate(0, 60f, 0);
            }
        });

    }

    public static void ResizeMap() {
        switch (MapEdit.ResizeOp) {
            case "ResizeCloneRow": 
                CloneRow();
                break;
            case "ResizeDeleteRow":
                DeleteRow();
                break;
            case "ResizeCloneCol":
                CloneColumn();
                break;
            case "ResizeDeleteCol":
                DeleteColumn();
                break;
            case "ResizeAddLayer":
                AddLayer();
                break;
        }
    }

    public static void PaintBlocks() {
        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block => {
            block.ApplyPaint(Environment.CurrentPaintTop, Environment.CurrentPaintSide);
        });
    }

    public static void DepaintBlocks() {
        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block => {
            block.RemovePaint();
        });
    }

    public static void StyleBlocks(string style) {
        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block => {
            block.ApplyTexture(style);
        });
    }

    public static void DestyleBlocks() {
        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block => {
            block.RemoveTexture();
            block.RemovePaint();
        });
    }

    public static void SampleStyle() {
        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block => {
            Color[] colors = block.SamplePaint();
            if (colors != null) {
                Environment.CurrentPaintTop = colors[0];
                Environment.CurrentPaintSide = colors[1];
                UI.System.Q("ToolOptions").Q("TopBlockPaint").style.backgroundColor = colors[0];
                UI.System.Q("ToolOptions").Q("SideBlockPaint").style.backgroundColor = colors[1];
                UI.ToggleDisplay(UI.System.Q("ToolOptions"), true);
                UI.ToggleDisplay(UI.System.Q("ToolOptions").Q("StylePaintOptions"), true);
                UI.ToggleDisplay(UI.System.Q("ToolOptions").Q("StyleTextureOptions"), false);
                return;
            }
            String texture = block.SampleTexture();
            if (texture.Length > 0) {
                UI.System.Q("ToolOptions").Q<DropdownField>("BlockTexture").value = texture;
                UI.ToggleDisplay(UI.System.Q("ToolOptions"), true);
                UI.ToggleDisplay(UI.System.Q("ToolOptions").Q("StylePaintOptions"), false);
                UI.ToggleDisplay(UI.System.Q("ToolOptions").Q("StyleTextureOptions"), true);
                return;
            }
            Toast.Add("Nothing sampled.");
        });
    }

    public static void CloneRow() {
        List<Block> selected = Block.GetSelected().ToList();
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
        ReorgNeeded = true;
    }

    public static void DeleteRow() {
        List<Block> selected = Block.GetSelected().ToList();
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        selected.ForEach(selectedBlock => {
            int selectedX = selectedBlock.transform.parent.GetComponent<Column>().X;
            for (int i = 0; i < columns.Length; i++) {
                GameObject column = columns[i];
                int x = column.GetComponent<Column>().X;
                int y = column.GetComponent<Column>().Y;
                if (x == selectedX) {
                    GameObject.Destroy(columns[i]);
                }
                if (x > selectedX) {
                    column.transform.localPosition -= new Vector3(1, 0, 0);
                    column.name = (x-1) + "," + y;
                    column.GetComponent<Column>().Set((x-1), y);
                }
            }
        });
        ReorgNeeded = true;
    }

    public static void CloneColumn() {
        List<Block> selected = Block.GetSelected().ToList();
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
        ReorgNeeded = true;
    }

    public static void DeleteColumn() {
        List<Block> selected = Block.GetSelected().ToList();
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        selected.ForEach(selectedBlock => {
            int selectedY = selectedBlock.transform.parent.GetComponent<Column>().Y;
            for (int i = 0; i < columns.Length; i++) {
                GameObject column = columns[i];
                int x = column.GetComponent<Column>().X;
                int y = column.GetComponent<Column>().Y;
                if (y == selectedY) {
                    GameObject.Destroy(columns[i]);
                }
                if (y > selectedY) {
                    column.transform.localPosition -= new Vector3(0, 0, 1);
                    column.name = x + "," + (y-1);
                    column.GetComponent<Column>().Set(x, (y-1));
                }
            }
        });
        ReorgNeeded = true;
    }

    public static void AddLayer() {
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        foreach(GameObject column in columns) {
            GameObject currentTop = TopBlock(column.gameObject);
            if (currentTop.GetComponent<Block>().Type != BlockShape.Spacer) {
                GameObject newblock = Instantiate(Resources.Load("Prefabs/Block") as GameObject);                 
                newblock.transform.parent = currentTop.transform.parent;
                newblock.transform.localPosition = new Vector3(0, currentTop.transform.localPosition.y + 1, 0);
                newblock.transform.localScale = currentTop.transform.localScale;
            }
        }
        ReorgNeeded = true;
    }

    public static void ChangeShape() {
        BlockShape shape = BlockShape.Solid;
        switch(MapEdit.ShapeOp) {
            case "ShapeSlope":
                shape = BlockShape.Slope;
                break;
            case "ShapeHidden":
                shape = BlockShape.Spacer;
                break;            
        }
        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block => {
            block.ShapeChange(shape);
            RotateBlocks();
        });
        ReorgNeeded = true;
    }

    public static void ChangeEffect() {
        string effect = UI.System.Q("ToolOptions").Q<DropdownField>("BlockEffect").value;
        List<Block> selected = Block.GetSelected().ToList();
        selected.ForEach(block => {
            block.EffectChange(effect);
        });
    }

    public static void ApplyStyle() {
        switch (MapEdit.StyleOp) {
            case "StylePaint":
                PaintBlocks();
                break;
            case "StyleTexture":
                string style = UI.System.Q<DropdownField>("BlockTexture").value;
                StyleBlocks(style);
                break;
            case "StyleEraser":
                DepaintBlocks();
                DestyleBlocks();
                break;
            case "StyleSample":
                SampleStyle();
                break;
        }
    }

    private static GameObject TopBlock(GameObject column) {
        try {
            GameObject top = null;
            Block[] blocks = column.GetComponentsInChildren<Block>();
            float highest = float.MinValue;
            for (int i = 0; i < blocks.Length; i++)
            {
                float height = blocks[i].transform.localPosition.y;
                if (height > highest) {
                    top = blocks[i].gameObject;
                }
            }
            return top;
        }
        #pragma warning disable
        catch(Exception e) {
            // Exceptions are fine, it means there's no block because it's out of bounds
            return null;
        }
        #pragma warning restore
    }

    private static void GravityDrop(GameObject column, float threshold) {
        Block[] blocks = column.GetComponentsInChildren<Block>();
        for (int i = 0; i < blocks.Length; i++) {
            if (blocks[i].transform.localPosition.y > threshold) {
                blocks[i].transform.localPosition -= new Vector3(0, 1, 0);
            }
        }
    }

    public static void SetInfo() {
        VisualElement root = UI.System.Q("TerrainInfo");
        UI.ToggleDisplay(root.Q("Elev"), false);
        UI.ToggleDisplay(root.Q("Pos"), false);
        UI.ToggleDisplay(root.Q("Elev").Q("SelectedMarker"), false);
        UI.ToggleDisplay(root.Q("Pos").Q("SelectedMarker"), false);
        UI.ToggleDisplay(root.Q("AddEffect"), false);
        UI.ToggleDisplay(root.Q("ClearSelected"), false);
        UI.ToggleDisplay(root, false);

        Block[] selected = Block.GetSelected();
        Block focused = Block.LastFocused;
        Block block = null;

        if (selected.Length == 0 && focused == null) {
            return;
        }
        else {
            UI.ToggleDisplay(root.Q("Elev"), true);
            UI.ToggleDisplay(root.Q("Pos"), true);
        }

        Color color = Color.white;
        if (selected.Length > 0 && Cursor.Mode != CursorMode.Editing) {
            color = ColorUtility.ColorFromHex("#9C7A19");
            UI.ToggleDisplay(root.Q("Elev").Q("SelectedMarker"), true);
            UI.ToggleDisplay(root.Q("Pos").Q("SelectedMarker"), true);
            UI.ToggleDisplay(root.Q("ClearSelected"), true);
        }
 
        UI.ToggleDisplay(root, true);

        string height;
        string coords;
        bool singleSelected = false;

        if (selected.Length > 1) {
            height = "*";
            coords = "*";
        }
        else {
            if (selected.Length == 1) {
                block = selected[0];
                singleSelected = true;
                // UI.ToggleDisplay(root.Q("AddObject"), true);
            }
            else if (focused) {
                block = focused;
            }
            height = (block.transform.localPosition.y + 1).ToString();
            if (block.Type == BlockShape.Slope) {
                height = height + ".5";
            }
            coords = Block.GetAlpha(block.getY() + 1) + "" + (block.getX()+1);
        }

        root.Q<Label>("Height").text = $"{height}";
        root.Q<Label>("Height").style.color = color;
        root.Q<Label>("Coords").text = coords;
        root.Q<Label>("Coords").style.color = color;

        root.Q("CurrentEffects").Clear();
        if (block) {
            block.GetEffects().ForEach(marker => {
                VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/TerrainEffect");
                VisualElement instance = template.Instantiate();
                instance.Q<Label>("Label").text = marker.ToUpper();
                VisualElement remove = instance.Q("Remove");
                if (!singleSelected) {
                    UI.ToggleDisplay(remove, false);
                }
                else {
                    remove.RegisterCallback<ClickEvent>((evt) => {
                        Player.Self().CmdRequestMapSetValue(new string[]{block.name}, "Effect", marker);
                    });
                }
                root.Q("CurrentEffects").Add(instance);
            });

        }
        if (selected.Length > 0) {
            UI.ToggleDisplay(root.Q("AddEffect"), true);
        }

    }
    
    public static Block[] FindNeighbors(Block origin, int radius) {
        int[,] offsets = {
            {-1, -1}, {-1, 0}, {0, -1},
            {-1, 1},           {0, 1},
            {1, -1},  {1, 0},  {1, 1}
        };
        List<Block> neighbors = new();
        int j = 0;
        if (radius == 2) {
            j = 3;
        }
        else if (radius == 3) {
            j = 8;
        }
        for (int i = 0; i < j; i++) {
            int x = origin.getX() + offsets[i, 0];
            int y = origin.getY() + offsets[i, 1];
            GameObject col = GameObject.Find($"{x},{y}");
            GameObject topBlockObj = TopBlock(col);
            if (topBlockObj) {
                neighbors.Add(topBlockObj.GetComponent<Block>());
            }
        }
        return neighbors.ToArray();
    }

    public static void Reorg() {
        UpdateIndicators();
        HideObscuredBlocks();
    }

    private static void UpdateIndicators() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            Block b = blocks[i].GetComponent<Block>();
            TextMeshPro tm = blocks[i].transform.Find("Indicator").GetComponent<TextMeshPro>();
            tm.text = Block.GetAlpha(b.getY() + 1) + (b.getX() + 1);
            blocks[i].name = "block " + b.getX() + "," + b.getY() + "," + b.getZ();

        }         
    }

    private static void HideObscuredBlocks() {
        int count = 0;
        int hiding = 0;
        Vector2 size = Size();
        bool[,,] solids = new bool[(int)size.x,(int)size.y,30];
        GameObject[,,] blks = new GameObject[(int)size.x,(int)size.y,30];
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            Block b = blocks[i].GetComponent<Block>();
            if (b.Type == BlockShape.Solid) {
                count++;
                solids[b.getX(), b.getY(), b.getZ()] = true;
                blks[b.getX(), b.getY(), b.getZ()] = b.gameObject;
            }
        }
        for (int i = 0; i < blocks.Length; i++) {
            Block b = blocks[i].GetComponent<Block>();
            int x = b.getX();
            int y = b.getY();
            int z = b.getZ();
            try {
                if (
                    solids[x,y,z+1] && // obscured above
                    solids[x+1,y,z] && // obscured west
                    solids[x-1,y,z] && // obscured east
                    solids[x,y-1,z] && // obscured south
                    solids[x,y+1,z]    //obscured north
                ) {
                    hiding++;
                    hideBlock(b);
                }
                else {
                    showBlock(b);
                }
            }
            #pragma warning disable
            catch(Exception e) {
                // Exceptions are fine, it means there's no block because it's out of bounds
                showBlock(b);
            }
            #pragma warning restore

        }
    }

    private static void hideBlock(Block b) {
        b.GetComponent<MeshRenderer>().enabled = false;
        b.transform.Find("Indicator").GetComponent<TextMeshPro>().enabled = false;
    }

    private static void showBlock(Block b) {
        b.GetComponent<MeshRenderer>().enabled = true;
        b.transform.Find("Indicator").GetComponent<TextMeshPro>().enabled = true;
    }

    public static void UpdateLight() {
        Light l = GameObject.Find("Light").GetComponent<Light>();
        l.intensity = LightIntensity;
        l.transform.eulerAngles = new Vector3(LightHeight, LightAngle, 0);
    }
}
