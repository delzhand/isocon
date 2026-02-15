using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block : MonoBehaviour
{
    public bool Selected { get; private set; } = false;
    private bool _focused = false;
    public bool Focused
    {
        get => _focused;
        set
        {
            if (value == _focused)
                return;
            _focused = value;
            _allFocused.Add(this);
            MarkForRedraw();
        }
    }
    static private HashSet<Block> _allFocused = new();
    static public IEnumerable<Block> AllFocusedBlocks => _allFocused;
    private bool _highlighted = false;
    public BlockShape Shape = BlockShape.Solid;
    public bool Destroyable = true;

    private List<string> _marks = new();
    public List<string> Marks { get => _marks; }

    private Material _markerMaterial;

    private string _customMaterialKeyTop = "";
    private string _customMaterialKeySide = "";

    private bool _materialReset = true;

    public Vector3Int Coordinate => transform.parent.GetComponent<Column>().Coordinate3 + new Vector3Int(0, 0, (int)(transform.position.y / .5f) + 2);
    public Vector2Int Coordinate2 => transform.parent.GetComponent<Column>().Coordinate;

    void Awake()
    {
        _markerMaterial = Instantiate(Resources.Load<Material>("Materials/Block/Marker"));
        ShapeChange(Shape);
    }

    void Update()
    {
        if (_materialReset)
        {
            _materialReset = false;
            SetMaterials();
        }
    }

    /// <summary>
    /// Shows the block menu for this block, unless the menu is visible already, in which case it hides it.
    /// </summary>
    public void ToggleMenu()
    {
        if (SelectionMenu.Visible)
        {
            SelectionMenu.Hide();
        }
        else
        {
            TileMenu.ShowMenu(this);
        }
    }

    /// <summary>
    /// Get the "game system height" of this block.
    /// </summary>
    /// <returns>A float representing the number of blocks high this block is, with slopes/steps counting as half.</returns>
    public float GetHeight()
    {
        float height = transform.localPosition.y + 1;
        if (Shape == BlockShape.Slope || Shape == BlockShape.Steps || Shape == BlockShape.SlopeInt || Shape == BlockShape.SlopeExt)
        {
            height -= .5f;
        }
        return height;
    }

    /// <summary>
    /// Get the midpoint of this block, which is the visual center of the top surface plus a small offset.
    /// </summary>
    /// <returns>A vector3 representing the point at which tokens should be placed.</returns>
    public Vector3 GetMidpoint()
    {
        Vector3 v = transform.position + new Vector3(0, .25f, 0);
        if (Shape == BlockShape.Slope || Shape == BlockShape.SlopeExt || Shape == BlockShape.SlopeInt || Shape == BlockShape.Steps)
        {
            v -= new Vector3(0, .25f, 0);
        }
        return v;
    }

    /// <summary>
    /// Changes the shape of this block
    /// </summary>
    /// <param name="blocktype">The block type to change to</param>
    /// <exception cref="Exception">If there is no mesh corresponding to the block type</exception>
    public void ShapeChange(BlockShape blocktype)
    {
        Shape = blocktype;
        Mesh m = null;
        if (TerrainController.GridType == "Square")
        {
            switch (Shape)
            {
                case BlockShape.Spacer:
                    m = BlockRendering.Meshes[BlockShape.Solid];
                    transform.localScale = new Vector3(.3f, .3f, .3f);
                    break;
                case BlockShape.Hidden:
                    m = BlockRendering.Meshes[BlockShape.Solid];
                    transform.localScale = Vector3.zero;
                    break;
                default:
                    try
                    {
                        m = BlockRendering.Meshes[Shape];
                        transform.localScale = Vector3.one;
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        throw new Exception($"No such shape {blocktype.ToString()}");
                    }

            }
        }
        else if (TerrainController.GridType == "Hex")
        {
            m = BlockRendering.Hex;
            transform.localScale = Vector3.one;
        }
        GetComponent<MeshFilter>().mesh = m;
        MarkForRedraw();
    }

    public void AddMark(string mark)
    {
        switch (mark)
        {
            case "None":
                _marks.Clear();
                break;
            default:
                if (!_marks.Contains(mark))
                {
                    _marks.Add(mark);
                }
                break;
        }
        MarkForRedraw();
    }

    public void RemoveMark(string mark)
    {
        if (_marks.Contains(mark))
        {
            _marks.Remove(mark);
        }
        MarkForRedraw();
    }

    /// <summary>
    /// Copies style from another block.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="copyShape">Also copy the shape</param>
    public void CopyStyle(Block other, bool copyShape = false)
    {
        _customMaterialKeyTop = other._customMaterialKeyTop;
        _customMaterialKeySide = other._customMaterialKeySide;
        if (copyShape)
        {
            ShapeChange(other.Shape);
        }
        MarkForRedraw();
    }

    /// <summary>
    /// Applies the specified style to the block
    /// </summary>
    /// <param name="top"></param>
    /// <param name="side"></param>
    public void ApplyStyle(string top, string side)
    {
        _customMaterialKeyTop = top;
        _customMaterialKeySide = side;
        MarkForRedraw();
    }

    /// <summary>
    /// Changes the block's appearance to the default
    /// </summary>
    public void RemoveStyle()
    {
        _customMaterialKeyTop = "";
        _customMaterialKeySide = "";
        MarkForRedraw();
    }

    /// <summary>
    /// Get a string pair representing the current style top and side keys
    /// </summary>
    /// <returns></returns>
    public (string, string) SampleStyles()
    {
        return (_customMaterialKeyTop, _customMaterialKeySide);
    }

    /// <summary>
    /// Rebuild the materials for this block. Should never be called directly - instead set _materialReset to true and let Update call this
    /// </summary>
    private void SetMaterials()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();

        Material[] mats = mr.materials;

        if (_customMaterialKeyTop.Length > 0)
        {
            mats[BlockRendering.MaterialTopIndex(Shape)] = BlockRendering.GetCustomMaterial(_customMaterialKeyTop, true);
            mats[BlockRendering.MaterialSideIndex(Shape)] = BlockRendering.GetCustomMaterial(_customMaterialKeySide, false);
        }
        else
        {
            // Checkerboard
            bool altSides = false;
            // bool altTop = false;
            if (transform.parent != null)
            {
                // Checkerboard
                float x = transform.parent.GetComponent<Column>().X;
                float y = transform.parent.GetComponent<Column>().Y;
                float z = transform.localPosition.y;
                altSides = ((x + y + z) % 2 == 0);
                // altTop = ((x + y) % 2 == 0);
            }
            mats[BlockRendering.MaterialSideIndex(Shape)] = BlockRendering.GetSharedMaterial("side" + (altSides ? "1" : "2"));
            mats[BlockRendering.MaterialTopIndex(Shape)] = BlockRendering.GetSharedMaterial("top" + (altSides ? "1" : "2"));
        }

        // Overwrite checkerboard/paint if highlighted
        if (_highlighted)
        {
            mats[BlockRendering.MaterialTopIndex(Shape)] = BlockRendering.GetSharedMaterial("highlighted");
        }

        // Markers
        _markerMaterial.SetInt("_Impassable", 0);
        _markerMaterial.SetInt("_Dangerous", 0);
        _markerMaterial.SetInt("_Difficult", 0);
        _markerMaterial.SetInt("_Interactive", 0);
        _markerMaterial.SetInt("_Pit", 0);
        _markerMaterial.SetInt("_Other", 0);
        _markerMaterial.SetInt("_Skull", 0);
        _markerMaterial.SetInt("_Border", 0);
        _markerMaterial.SetColor("_Color", Color.black);

        foreach (string mark in _marks)
        {
            string[] split = mark.Split("::");
            if (split.Length > 1)
            {
                string markEffect = split[1];
                switch (markEffect)
                {
                    case "Blocked":
                        _markerMaterial.SetInt("_Impassable", 1);
                        break;
                    case "Spiky":
                        _markerMaterial.SetInt("_Dangerous", 1);
                        break;
                    case "Wavy":
                        _markerMaterial.SetInt("_Difficult", 1);
                        break;
                    case "Hand":
                        _markerMaterial.SetInt("_Interactive", 1);
                        break;
                    case "Hole":
                        _markerMaterial.SetInt("_Pit", 1);
                        break;
                    case "Corners":
                        _markerMaterial.SetInt("_Other", 1);
                        break;
                    case "Skull":
                        _markerMaterial.SetInt("_Skull", 1);
                        break;
                    case "Border":
                        _markerMaterial.SetInt("_Border", 1);
                        break;
                }
            }
            if (split.Length > 2)
            {
                string markColor = split[2];
                switch (markColor)
                {
                    case "Red":
                        _markerMaterial.SetColor("_Color", new Color(1.5f, 0, 0));
                        break;
                    case "Green":
                        _markerMaterial.SetColor("_Color", new Color(0, 1.5f, .2f));
                        break;
                    case "Blue":
                        _markerMaterial.SetColor("_Color", new Color(0, .2f, 1.5f));
                        break;
                    case "Yellow":
                        _markerMaterial.SetColor("_Color", new Color(1.5f, 1.2f, 0f));
                        break;
                    case "White":
                        _markerMaterial.SetColor("_Color", new Color(1.5f, 1.5f, 1.5f));
                        break;
                }
            }
        }

        mats[BlockRendering.MaterialMarkerIndex(Shape)] = _markerMaterial;

        // Selected/Focused
        string focusState = "unfocused";
        if (Selected && !Focused)
        {
            focusState = "selected";
        }
        else if (!Selected && Focused)
        {
            focusState = "focused";
        }
        else if (Selected && Focused)
        {
            focusState = "selectfocused";
        }
        mats[BlockRendering.MaterialFocusIndex(Shape)] = BlockRendering.GetSharedMaterial(focusState);

        // Apply
        mr.SetMaterials(mats.ToList());

        mr.enabled = true;
    }

    /// <summary>
    /// Select this block as a target for marking
    /// </summary>
    public void Select()
    {
        SelectAppend(false);
    }

    /// <summary>
    /// Add this block to the list of selected blocks for marking
    /// </summary>
    /// <param name="append">If append is false and the block is already selected, instead deselect it</param>
    public void SelectAppend(bool append = false)
    {
        SelectionMenu.Hide();
        if (Selected && !append)
        {
            Deselect();
        }
        else
        {
            Selected = true;
        }
        MarkForRedraw();
        TerrainController.SetInfo();
    }

    /// <summary>
    /// Remove this block from the list of blocks for marking
    /// </summary>
    private void Deselect()
    {
        Selected = false;
        MarkForRedraw();
        SelectionMenu.Hide();
    }

    /// <summary>
    /// Get all of the currently selected blocks
    /// </summary>
    /// <returns></returns>
    public static Block[] GetSelected()
    {
        List<Block> selected = new();
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < gos.Length; i++)
        {
            Block block = gos[i].GetComponent<Block>();
            if (block.Selected)
            {
                selected.Add(block);
            }
        }
        return selected.ToArray();
    }

    /// <summary>
    /// Clear the list of currently selected blocks
    /// </summary>
    public static void DeselectAll()
    {
        foreach (Block b in GetSelected())
        {
            b.Deselect();
        }
        TerrainController.SetInfo();
    }

    /// <summary>
    /// Indicate that this block would be affected by the current tool or pointer
    /// </summary>
    public void Focus()
    {
        UnfocusAll();
        Focused = true;
        TerrainController.SetInfo();
        Player.Self().GetComponent<DirectionalLine>().SetTarget(GetMidpoint());
    }

    /// <summary>
    /// Clear the focus state of all focused blocks
    /// </summary>
    public static void UnfocusAll()
    {
        if (Player.Self() != null)
        {
            Player.Self().GetComponent<DirectionalLine>().UnsetTarget();
        }
        foreach (Block b in _allFocused.ToArray())
        {
            b.Focused = false;
            Block._allFocused.Remove(b);
        }
        _allFocused.Clear();
        TerrainController.SetInfo();
    }

    /// <summary>
    /// Indicate that this block is part of a targeted area
    /// </summary>
    public void Highlight()
    {
        _highlighted = true;
        MarkForRedraw();
        TerrainController.SetInfo();
    }

    /// <summary>
    /// Get an array of all highlighted blocks
    /// </summary>
    /// <returns></returns>
    private static Block[] GetHighlighted()
    {
        List<Block> highlighted = new();
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < gos.Length; i++)
        {
            Block block = gos[i].GetComponent<Block>();
            if (block._highlighted)
            {
                highlighted.Add(block);
            }
        }
        return highlighted.ToArray();
    }

    /// <summary>
    /// Clear the highlighted state of all highlighted blocks
    /// </summary>
    public static void DehighlightAll()
    {
        foreach (Block b in GetHighlighted())
        {
            b._highlighted = false;
            b.MarkForRedraw();
        }
    }

    /// <summary>
    /// Get the block whose transform position is nearest the specified vector
    /// </summary>
    /// <param name="targetPosition">The point to check against for proximity</param>
    /// <returns>The block component with the shortest distance</returns>
    public static Block GetClosest(Vector3 targetPosition)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block");
        GameObject closestObject = null;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            float distance = Vector3.Distance(gameObjects[i].transform.position, targetPosition);
            if (distance < closestDistance)
            {
                closestObject = gameObjects[i];
                closestDistance = distance;
            }
        }
        return closestObject.GetComponent<Block>();
    }

    /// <summary>
    /// Get the top blocks at the given coordinates
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    public static Block[] GetTopBlocks(Vector2Int[] coordinates)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block");
        List<Block> blocks = new List<Block>();
        List<Vector2Int> v2is = new List<Vector2Int>(coordinates);
        foreach (var gameObject in gameObjects)
        {
            Block block = gameObject.GetComponent<Block>();
            Vector2Int blockCoords = block.Coordinate2;
            if (v2is.Contains(blockCoords))
            {
                Block topBlock = block.transform.parent.GetComponent<Column>().GetTopBlock();
                blocks.Add(topBlock);
            }
            v2is.Remove(blockCoords);
            if (v2is.Count() == 0)
                return blocks.ToArray();
        }
        return blocks.ToArray();
    }

    /// <summary>
    /// Gets the top block at a given coordinate, returns null if it would be out of bounds.
    /// </summary>
    /// <param name="coordinate"></param>
    /// <returns></returns>
    public static Block GetTopBlock(Vector2Int coordinate)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block");
        foreach (var gameObject in gameObjects)
        {
            Block block = gameObject.GetComponent<Block>();
            if (block.Coordinate2 == coordinate)
            {
                Block topBlock = block.transform.parent.GetComponent<Column>().GetTopBlock();
                return topBlock;
            }
        }
        return null;
    }

    /// <summary>
    /// Get a float pair indicating the low and high points of selected blocks
    /// </summary>
    /// <returns></returns>
    public static (float, float) GetElevationRange()
    {
        (float, float) lowHigh = (float.MaxValue, float.MinValue);
        foreach (Block b in GetSelected())
        {
            lowHigh.Item1 = Mathf.Min(lowHigh.Item1, b.GetHeight());
            lowHigh.Item2 = Mathf.Max(lowHigh.Item2, b.GetHeight());
        }
        return lowHigh;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Block FindOptimalFromPosition(Vector3 v)
    {
        Block closest = GetClosest(v);
        Block top = GetTopBlock(new Vector2Int(closest.Coordinate.x, closest.Coordinate.y));
        return top;
    }

    /// <summary>
    /// Sets block to rebuild materials on the next update call
    /// </summary>
    public void MarkForRedraw()
    {
        _materialReset = true;
    }

    #region Parsing
    public string WriteOut()
    {
        Column c = transform.parent.GetComponent<Column>();
        string[] bits = new string[]{
            c.X.ToString(),
            c.Y.ToString(),
            transform.localPosition.y.ToString(),
            transform.localEulerAngles.y.ToString(),
            Shape.ToString(),
            Destroyable.ToString(),
            string.Join(",", _marks.ToArray()),
            _customMaterialKeyTop,
            _customMaterialKeySide
        };
        return string.Join("|", bits);
    }

    public static GameObject ReadIn(string version, string block)
    {
        string[] data = block.Split("|");
        switch (version)
        {
            case "v1":
                return ParseV1(data);
            case "v2":
                return ParseV2(data);
            case "v3":
                return ParseV3(data);
        }
        return null;
    }

    private static GameObject ParseV1(string[] data)
    {
        int x = int.Parse(data[0]);
        int y = int.Parse(data[1]);
        float z = float.Parse(data[2]);
        float r = float.Parse(data[3]);
        BlockShape type = (BlockShape)Enum.Parse(typeof(BlockShape), data[4], true);
        bool destroyable = bool.Parse(data[5]);
        string[] markersArray = data[6].Split(",");
        List<string> markers = new List<string>();
        for (int i = 0; i < markersArray.Length; i++)
        {
            if (markersArray[i].Length > 0)
            {
                markers.Add(markersArray[i]);
            }
        }
        // Default to false here to not break older saves
        bool painted = false;
        if (data.Length > 7)
        {
            painted = bool.Parse(data[7]);
        }

        GameObject map = GameObject.Find("Terrain");
        GameObject column = GameObject.Find(x + "," + y);
        if (column == null)
        {
            column = new GameObject();
            column.name = x + "," + y;
            column.tag = "Column";
            column.transform.parent = map.transform;
            column.transform.localPosition = new Vector3(x, 0, y);
            column.transform.localScale = Vector3.one;
            column.AddComponent<Column>().Set(x, y);
        }

        GameObject block = Instantiate(Resources.Load("Prefabs/Block") as GameObject);
        block.name = "block-" + x + "," + z + "," + y;
        block.transform.parent = column.transform;
        block.transform.localScale = Vector3.one;
        block.transform.localPosition = new Vector3(0, z, 0);
        block.transform.localRotation = Quaternion.Euler(0, r, 0);
        block.GetComponent<Block>().Destroyable = destroyable;
        block.GetComponent<Block>().ShapeChange(type);
        for (int i = 0; i < markers.Count; i++)
        {
            block.GetComponent<Block>().AddMark(markers[i]);
        }

        return block;
    }

    private static GameObject ParseV2(string[] data)
    {
        int x = int.Parse(data[0]);
        int y = int.Parse(data[1]);
        float z = float.Parse(data[2]);
        float r = float.Parse(data[3]);
        BlockShape type = (BlockShape)Enum.Parse(typeof(BlockShape), data[4], true);
        bool destroyable = bool.Parse(data[5]);
        string[] markersArray = data[6].Split(",");
        List<string> markers = new List<string>();
        for (int i = 0; i < markersArray.Length; i++)
        {
            if (markersArray[i].Length > 0)
            {
                markers.Add(markersArray[i]);
            }
        }
        // Default to false here to not break older saves
        bool painted = false;
        if (data.Length > 7)
        {
            painted = bool.Parse(data[7]);
        }

        GameObject map = GameObject.Find("Terrain");
        GameObject column = GameObject.Find(x + "," + y);
        if (column == null)
        {
            column = new GameObject();
            column.name = x + "," + y;
            column.tag = "Column";
            column.transform.parent = map.transform;
            column.transform.localPosition = new Vector3(x, 0, y);
            column.transform.localScale = Vector3.one;
            column.AddComponent<Column>().Set(x, y);
        }

        GameObject block = Instantiate(Resources.Load("Prefabs/Block") as GameObject);
        block.name = "block-" + x + "," + z + "," + y;
        block.transform.parent = column.transform;
        block.transform.localScale = Vector3.one;
        block.transform.localPosition = new Vector3(0, z, 0);
        block.transform.localRotation = Quaternion.Euler(0, r, 0);
        block.GetComponent<Block>().Destroyable = destroyable;
        block.GetComponent<Block>().ShapeChange(type);
        for (int i = 0; i < markers.Count; i++)
        {
            block.GetComponent<Block>().AddMark(markers[i]);
        }

        return block;
    }

    private static GameObject ParseV3(string[] data)
    {
        int x = int.Parse(data[0]);
        int y = int.Parse(data[1]);
        float z = float.Parse(data[2]);
        float r = float.Parse(data[3]);
        BlockShape type = (BlockShape)Enum.Parse(typeof(BlockShape), data[4], true);
        bool destroyable = bool.Parse(data[5]);
        string[] markersArray = data[6].Split(",");
        List<string> markers = new List<string>();
        for (int i = 0; i < markersArray.Length; i++)
        {
            if (markersArray[i].Length > 0)
            {
                markers.Add(markersArray[i]);
            }
        }
        string topStyle = data[7];
        string sideStyle = data[8];

        GameObject map = GameObject.Find("Terrain");
        GameObject column = GameObject.Find(x + "," + y);
        if (column == null)
        {
            column = new GameObject();
            column.name = x + "," + y;
            column.tag = "Column";
            column.transform.parent = map.transform;
            column.transform.localPosition = new Vector3(x, 0, y);
            column.transform.localScale = Vector3.one;
            column.AddComponent<Column>().Set(x, y);
        }

        GameObject block = Instantiate(Resources.Load("Prefabs/Block") as GameObject);
        block.name = "block-" + x + "," + z + "," + y;
        block.transform.parent = column.transform;
        block.transform.localScale = Vector3.one;
        block.transform.localPosition = new Vector3(0, z, 0);
        block.transform.localRotation = Quaternion.Euler(0, r, 0);
        block.GetComponent<Block>().Destroyable = destroyable;
        block.GetComponent<Block>().ShapeChange(type);
        for (int i = 0; i < markers.Count; i++)
        {
            block.GetComponent<Block>().AddMark(markers[i]);
        }
        block.GetComponent<Block>().ApplyStyle(topStyle, sideStyle);
        return block;
    }
    #endregion    
}
