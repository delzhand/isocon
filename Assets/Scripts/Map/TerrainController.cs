using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using TMPro;

public class TerrainController
{
    public static string GridType = "Square";
    public static float LightAngle = 330f;
    public static float LightHeight = 50f;
    public static float LightIntensity = 2;

    public static bool ReorgNeeded = false;

    public static bool MapDirty = false;

    public static void Edit(Block block)
    {
        MapDirty = true;
        switch (MapEdit.EditOp)
        {
            case "AddBlock":
                AddHeight(block);
                break;
            case "RemoveBlock":
                RemoveBlock(block);
                break;
            case "RotateBlock":
                RotateBlock(block);
                break;
            case "ResizeMap":
                ResizeMap(block);
                break;
            case "ChangeShape":
                ChangeShape(block);
                break;
            case "StyleBlock":
                ApplyStyle(block);
                break;
        }
    }

    public static void DestroyAllBlocks()
    {
        GameObject terrain = GameObject.Find("Terrain");
        terrain.name = "_Terrain";
        GameObject.DestroyImmediate(terrain);
        new GameObject("Terrain").transform.localScale = new Vector3(1, .5f, 1);
    }

    public static void InitializeTerrain(int length, int width, int height)
    {
        DestroyAllBlocks();
        GameObject map = GameObject.Find("Terrain");
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                try
                {
                    string columnName = x + "," + y;
                    GameObject column = GameObject.Find(columnName);
                    if (column == null)
                    {
                        column = new GameObject();
                        column.tag = "Column";
                        column.name = columnName;
                        column.transform.parent = map.transform;
                        column.transform.localPosition = new Vector3(x, 0, y);
                        column.transform.localScale = Vector3.one;
                        column.AddComponent<Column>().Set(x, y);
                    }
                    for (int z = 0; z < height; z++)
                    {
                        GameObject block = GameObject.Instantiate(Resources.Load("Prefabs/Block") as GameObject);
                        block.transform.parent = column.transform;
                        block.transform.localPosition = new Vector3(0, z, 0);
                        block.transform.localScale = Vector3.one;
                        if (z == 0)
                        {
                            block.GetComponent<Block>().Destroyable = false;
                        }
                        Block b = block.GetComponent<Block>();
                        block.name = "block " + b.Coordinate.x + "," + b.Coordinate.y + "," + b.Coordinate.z;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        ReorgNeeded = true;
        Environment.SetBackgroundColors(Environment.BgBottomColor, Environment.BgTopColor);
    }

    public static void ResetTerrain(int width, int length, int height)
    {
        DestroyAllBlocks();
        InitializeTerrain(width, length, height);
        MapDirty = false;
    }

    public static Vector2Int GetDimensions()
    {
        Vector2Int size = Vector2Int.zero;
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++)
        {
            size.x = Mathf.Max(size.x, blocks[i].GetComponent<Block>().Coordinate.x + 1);
            size.y = Mathf.Max(size.y, blocks[i].GetComponent<Block>().Coordinate.y + 1);
        }
        return size;
    }

    public static void AddHeight(Block block)
    {
        Column column = block.transform.parent.GetComponent<Column>();
        if (MapEditingState.MarkedColumns.Contains(column))
        {
            return;
        }
        MapEditingState.MarkedColumns.Add(column);
        GameObject currentTop = TopBlock(column.gameObject);
        GameObject newblock = GameObject.Instantiate(Resources.Load("Prefabs/Block") as GameObject);
        newblock.transform.parent = block.transform.parent;
        newblock.transform.localPosition = new Vector3(0, currentTop.transform.localPosition.y + 1, 0);
        newblock.transform.localScale = block.transform.localScale;
        newblock.GetComponent<Block>().CopyStyle(currentTop.GetComponent<Block>(), true);
        ReorgNeeded = true;
    }

    public static void RemoveBlock(Block block)
    {
        Column column = block.transform.parent.GetComponent<Column>();
        if (MapEditingState.MarkedColumns.Contains(column))
        {
            return;
        }
        MapEditingState.MarkedColumns.Add(column);
        if (block.Destroyable)
        {
            GravityDrop(block.transform.parent.gameObject, block.transform.localPosition.y);
            GameObject.DestroyImmediate(block.gameObject);
        }
        else
        {
            if (!Dragger.IsLeftDragging)
            {
                Toast.AddError("Foundation blocks cannot be deleted (but can be hidden).");
            }
        }
        ReorgNeeded = true;
    }

    public static void RotateBlock(Block block)
    {
        if (GridType == "Square")
        {
            block.transform.Rotate(0, 90f, 0);
        }
        else if (GridType == "Hex")
        {
            block.transform.Rotate(0, 60f, 0);
        }
    }

    public static void ResizeMap(Block block)
    {
        switch (MapEdit.ResizeOp)
        {
            case "ResizeCloneRow":
                CloneRow(block);
                break;
            case "ResizeDeleteRow":
                DeleteRow(block);
                break;
            case "ResizeCloneCol":
                CloneColumn(block);
                break;
            case "ResizeDeleteCol":
                DeleteColumn(block);
                break;
            case "ResizeAddLayer":
                AddLayer();
                break;
        }
    }

    public static void StyleBlock(Block block, string styleTop, string styleSide, Color colorTop, Color colorSide)
    {
        string top = $"{BlockRendering.MaterialName(styleTop, true)}::{ColorUtility.GetHex(colorTop)}";
        string side = $"{BlockRendering.MaterialName(styleSide, false)}::{ColorUtility.GetHex(colorSide)}";
        block.ApplyStyle(top, side);
    }

    public static void DestyleBlock(Block block)
    {
        block.RemoveStyle();
    }

    public static void SampleStyle(Block block)
    {
        (string, string) styles = block.SampleStyles();
        if (styles.Item1.Length == 0)
        {
            Toast.AddSimple("Block is using default material, nothing sampled.");
            return;
        }

        UI.System.Q("ToolOptions").Q<DropdownField>("TopTexture").value = BlockRendering.ReverseTextureMap(styles.Item1.Split("::")[0]);
        Environment.CurrentPaintTop = ColorUtility.GetColor(styles.Item1.Split("::")[1]);
        UI.System.Q("ToolOptions").Q("TopBlockPaint").style.backgroundColor = Environment.CurrentPaintTop;

        UI.System.Q("ToolOptions").Q<DropdownField>("SideTexture").value = BlockRendering.ReverseTextureMap(styles.Item2.Split("::")[0]);
        Environment.CurrentPaintSide = ColorUtility.GetColor(styles.Item2.Split("::")[1]);
        UI.System.Q("ToolOptions").Q("SideBlockPaint").style.backgroundColor = Environment.CurrentPaintSide;

        MapEdit.ActivateStylePaint();
    }

    public static void CloneRow(Block block)
    {
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        for (int i = 0; i < columns.Length; i++)
        {
            GameObject column = columns[i];
            int x = column.GetComponent<Column>().X;
            int y = column.GetComponent<Column>().Y;
            if (x == block.transform.parent.GetComponent<Column>().X)
            {
                GameObject clone = GameObject.Instantiate(column);
                clone.transform.parent = column.transform.parent;
                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition += new Vector3(1, 0, 0);
                clone.name = (x + 1) + "," + y;
                clone.GetComponent<Column>().Set(x + 1, y);
                for (int b = 0; b < column.transform.childCount; b++)
                {
                    var new_block = clone.transform.GetChild(b).GetComponent<Block>();
                    var old_block = column.transform.GetChild(b).GetComponent<Block>();
                    new_block.CopyStyle(old_block);
                }
            }
            if (x > block.transform.parent.GetComponent<Column>().X)
            {
                column.transform.localPosition += new Vector3(1, 0, 0);
                column.name = (x + 1) + "," + y;
                column.GetComponent<Column>().Set((x + 1), y);
            }
        }
        ReorgNeeded = true;
    }

    public static void DeleteRow(Block block)
    {
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        int selectedX = block.transform.parent.GetComponent<Column>().X;
        for (int i = 0; i < columns.Length; i++)
        {
            GameObject column = columns[i];
            int x = column.GetComponent<Column>().X;
            int y = column.GetComponent<Column>().Y;
            if (x == selectedX)
            {
                GameObject.Destroy(columns[i]);
            }
            if (x > selectedX)
            {
                column.transform.localPosition -= new Vector3(1, 0, 0);
                column.name = (x - 1) + "," + y;
                column.GetComponent<Column>().Set((x - 1), y);
            }
        }
        ReorgNeeded = true;
    }

    public static void CloneColumn(Block block)
    {
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        for (int i = 0; i < columns.Length; i++)
        {
            GameObject column = columns[i];
            int x = column.GetComponent<Column>().X;
            int y = column.GetComponent<Column>().Y;
            if (y == block.transform.parent.GetComponent<Column>().Y)
            {
                GameObject clone = GameObject.Instantiate(column);
                clone.transform.parent = column.transform.parent;
                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition += new Vector3(0, 0, 1);
                clone.name = x + "," + (y + 1);
                clone.GetComponent<Column>().Set(x, y + 1);
                for (int b = 0; b < column.transform.childCount; b++)
                {
                    var new_block = clone.transform.GetChild(b).GetComponent<Block>();
                    var old_block = column.transform.GetChild(b).GetComponent<Block>();
                    new_block.CopyStyle(old_block);
                }
            }
            if (y > block.transform.parent.GetComponent<Column>().Y)
            {
                column.transform.localPosition += new Vector3(0, 0, 1);
                column.name = x + "," + (y + 1);
                column.GetComponent<Column>().Set(x, (y + 1));
            }
        }
        ReorgNeeded = true;
    }

    public static void DeleteColumn(Block block)
    {
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        int selectedY = block.transform.parent.GetComponent<Column>().Y;
        for (int i = 0; i < columns.Length; i++)
        {
            GameObject column = columns[i];
            int x = column.GetComponent<Column>().X;
            int y = column.GetComponent<Column>().Y;
            if (y == selectedY)
            {
                GameObject.Destroy(columns[i]);
            }
            if (y > selectedY)
            {
                column.transform.localPosition -= new Vector3(0, 0, 1);
                column.name = x + "," + (y - 1);
                column.GetComponent<Column>().Set(x, (y - 1));
            }
        }
        ReorgNeeded = true;
    }

    public static void AddLayer()
    {
        GameObject[] columns = GameObject.FindGameObjectsWithTag("Column");
        foreach (GameObject column in columns)
        {
            GameObject currentTop = TopBlock(column.gameObject);
            if (currentTop.GetComponent<Block>().Shape != BlockShape.Spacer)
            {
                GameObject newblock = GameObject.Instantiate(Resources.Load("Prefabs/Block") as GameObject);
                newblock.transform.parent = currentTop.transform.parent;
                newblock.transform.localPosition = new Vector3(0, currentTop.transform.localPosition.y + 1, 0);
                newblock.transform.localScale = currentTop.transform.localScale;
            }
        }
        ReorgNeeded = true;
    }

    public static void ChangeShape(Block block)
    {
        BlockShape shape = BlockShape.Solid;
        switch (MapEdit.ShapeOp)
        {
            case "ShapeSlope":
                shape = BlockShape.Slope;
                break;
            case "ShapeHidden":
                shape = BlockShape.Spacer;
                break;
            case "ShapeSteps":
                shape = BlockShape.Steps;
                break;
            case "ShapeCorner":
                shape = BlockShape.Corner;
                break;
            case "ShapeFlatCorner":
                shape = BlockShape.FlatCorner;
                break;
            case "ShapeUpslope":
                shape = BlockShape.Upslope;
                break;
            case "ShapeSlopeInt":
                shape = BlockShape.SlopeInt;
                break;
            case "ShapeSlopeExt":
                shape = BlockShape.SlopeExt;
                break;
        }
        block.ShapeChange(shape);
        RotateBlock(block);
        ReorgNeeded = true;
    }

    public static void ApplyStyle(Block block)
    {
        string op = MapEdit.StyleOp;
        if (MapEditingState.AltMode)
        {
            op = "StyleSample";
        }
        switch (op)
        {
            case "StylePaint":
                string textureTop = UI.System.Q<DropdownField>("TopTexture").value;
                string textureSide = UI.System.Q<DropdownField>("SideTexture").value;
                Color colorTop = Environment.CurrentPaintTop;
                Color colorSide = Environment.CurrentPaintSide;
                StyleBlock(block, textureTop, textureSide, colorTop, colorSide);
                break;
            case "StyleEraser":
                DestyleBlock(block);
                break;
            case "StyleSample":
                SampleStyle(block);
                break;
        }
    }

    private static GameObject TopBlock(GameObject column)
    {
        if (column == null)
        {
            return null;
        }
        GameObject top = null;
        Block[] blocks = column.GetComponentsInChildren<Block>();
        float highest = float.MinValue;

        for (int i = 0; i < blocks.Length; i++)
        {
            float height = blocks[i].transform.localPosition.y;
            if (height > highest)
            {
                top = blocks[i].gameObject;
                highest = height; // Update the highest value
            }
        }

        return top;
    }

    private static void GravityDrop(GameObject column, float threshold)
    {
        Block[] blocks = column.GetComponentsInChildren<Block>();
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].transform.localPosition.y > threshold)
            {
                blocks[i].transform.localPosition -= new Vector3(0, 1, 0);
            }
        }
    }

    public static void SetInfo()
    {
        VisualElement root = UI.System.Q("TerrainInfo");
        UI.ToggleDisplay(root.Q("Elev"), false);
        UI.ToggleDisplay(root.Q("Pos"), false);
        UI.ToggleDisplay(root.Q("Elev").Q("SelectedMarker"), false);
        UI.ToggleDisplay(root.Q("Pos").Q("SelectedMarker"), false);
        UI.ToggleDisplay(root, false);

        Block[] selected = Block.GetSelected();
        Block focused = Block.AllFocusedBlocks.FirstOrDefault();
        Block block = null;

        if (selected.Length == 0 && focused == null)
        {
            return;
        }
        else
        {
            UI.ToggleDisplay(root.Q("Elev"), true);
            UI.ToggleDisplay(root.Q("Pos"), true);
        }

        Color color = Color.white;
        if (selected.Length > 0)
        {
            color = ColorUtility.UISelectYellow;
            UI.ToggleDisplay(root.Q("Elev").Q("SelectedMarker"), true);
            root.Q("Elev").Q("SelectedMarker").style.backgroundColor = color;
            UI.ToggleDisplay(root.Q("Pos").Q("SelectedMarker"), true);
            root.Q("Pos").Q("SelectedMarker").style.backgroundColor = color;
        }

        UI.ToggleDisplay(root, true);

        string height;
        string coords;

        if (selected.Length > 1)
        {
            (float, float) lowHigh = Block.GetElevationRange();
            height = $"{lowHigh.Item1}";
            if (lowHigh.Item1 != lowHigh.Item2)
            {
                height = $"{lowHigh.Item1}~{lowHigh.Item2}";
            }
            coords = $"({selected.Length})";
        }
        else
        {
            if (selected.Length == 1)
            {
                block = selected[0];
            }
            else if (focused)
            {
                block = focused;
            }
            height = $"{block.GetHeight()}";
            coords = StringUtility.ConvertIntToAlpha(block.Coordinate.y + 1) + "" + (block.Coordinate.x + 1);
        }

        root.Q<Label>("Height").text = $"{height}";
        root.Q<Label>("Height").style.color = color;
        root.Q<Label>("Coords").text = coords;
        root.Q<Label>("Coords").style.color = color;

        root.Q("CurrentEffects").Clear();
        if (block)
        {
            block.Marks.ForEach(effect =>
            {
                VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/TerrainEffect");
                VisualElement instance = template.Instantiate();
                instance.Q<Label>("Label").text = effect.ToUpper().Split("::")[0];
                root.Q("CurrentEffects").Add(instance);
            });
        }
    }

    public static Block[] FindNeighbors(Block origin, int radius)
    {
        int[,] offsets = {
            {-1, -1}, {-1, 0}, {0, -1},
            {-1, 1},           {0, 1},
            {1, -1},  {1, 0},  {1, 1}
        };
        List<Block> neighbors = new();
        int j = 0;
        if (radius == 2)
        {
            j = 3;
        }
        else if (radius == 3)
        {
            j = 8;
        }
        for (int i = 0; i < j; i++)
        {
            int x = origin.Coordinate.x + offsets[i, 0];
            int y = origin.Coordinate.y + offsets[i, 1];
            GameObject col = GameObject.Find($"{x},{y}");
            GameObject topBlockObj = TopBlock(col);
            if (topBlockObj)
            {
                neighbors.Add(topBlockObj.GetComponent<Block>());
            }
        }
        return neighbors.ToArray();
    }

    public static void Organize()
    {
        if (ReorgNeeded)
        {
            ReorgNeeded = false;
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            for (int i = 0; i < blocks.Length; i++)
            {
                Block b = blocks[i].GetComponent<Block>();
                // Rename
                blocks[i].name = "block " + b.Coordinate.x + "," + b.Coordinate.y + "," + b.Coordinate.z;
                // Redraw materials to fix checkerboard effect for clones
                b.MarkForRedraw();
            }
            HideObscuredBlocks();
        }
    }

    private static void HideObscuredBlocks()
    {
        int count = 0;
        int hiding = 0;
        Vector2 size = GetDimensions();
        bool[,,] solids = new bool[(int)size.x, (int)size.y, MaxElevation() + 1];
        GameObject[,,] blks = new GameObject[(int)size.x, (int)size.y, MaxElevation() + 1];
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++)
        {
            Block b = blocks[i].GetComponent<Block>();
            if (b.Shape == BlockShape.Solid)
            {
                count++;
                solids[b.Coordinate.x, b.Coordinate.y, b.Coordinate.z] = true;
                blks[b.Coordinate.x, b.Coordinate.y, b.Coordinate.z] = b.gameObject;
            }
        }
        bool isEdgePiece(int x, int y, int z) => (x >= 0 && x < size.x && y >= 0 && y < size.y && z < 30);
        bool isObscured(int x, int y, int z)
        {
            return solids[x, y, z + 1] && // obscured above
                solids[x + 1, y, z] && // obscured west
                solids[x - 1, y, z] && // obscured east
                solids[x, y - 1, z] && // obscured south
                solids[x, y + 1, z];    //obscured north
        }
        for (int i = 0; i < blocks.Length; i++)
        {
            Block b = blocks[i].GetComponent<Block>();
            int x = b.Coordinate.x;
            int y = b.Coordinate.y;
            int z = b.Coordinate.z;
            if (isEdgePiece(x, y, z))
            {
                ShowBlock(b);
            }
            else if (isObscured(x, y, z))
            {
                hiding++;
                HideBlock(b);
            }
            else
            {
                ShowBlock(b);
            }
        }
    }

    private static void HideBlock(Block b)
    {
        b.GetComponent<MeshRenderer>().enabled = false;
        b.transform.Find("Indicator").GetComponent<TextMeshPro>().enabled = false;
    }

    private static void ShowBlock(Block b)
    {
        b.GetComponent<MeshRenderer>().enabled = true;
        b.transform.Find("Indicator").GetComponent<TextMeshPro>().enabled = true;
    }

    public static void UpdateLight()
    {
        Light l = GameObject.Find("Light").GetComponent<Light>();
        l.intensity = LightIntensity;
        l.transform.eulerAngles = new Vector3(LightHeight, LightAngle, 0);
    }

    public static void ToggleTerrainEffectMode(ClickEvent evt)
    {
        // // Disable map edit mode if necessary
        // if (Cursor.Mode == CursorMode.Editing)
        // {
        //     MapEdit.ToggleEditMode(evt);
        // }

        // if (Cursor.Mode != CursorMode.Marking)
        // {
        //     Tutorial.Init("terrain effect mode");
        //     Cursor.Mode = CursorMode.Marking;
        //     Token.DeselectAll();
        //     Token.UnfocusAll();
        //     BlockMesh.ToggleAllBorders(true);
        //     UI.ToggleActiveClass("MarkerMode", true);
        //     UI.ToggleDisplay("BottomBar", false);
        //     UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), false);
        //     Player.Self().SetOp("Editing Terrain Effects");
        // }
        // else
        // {
        //     Cursor.Mode = CursorMode.Default;
        //     BlockMesh.ToggleAllBorders(false);
        //     UI.ToggleActiveClass("MarkerMode", false);
        //     Block.DeselectAll();
        //     UI.ToggleDisplay("BottomBar", true);
        //     UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), true);
        //     SelectionMenu.Hide();
        //     Player.Self().ClearOp();
        // }
    }

    private static int MaxElevation()
    {
        int max = 0;
        foreach (var gameObject in GameObject.FindGameObjectsWithTag("Block"))
        {
            max = Math.Max(max, gameObject.GetComponent<Block>().Coordinate.z);
        }
        return max;
    }
}
