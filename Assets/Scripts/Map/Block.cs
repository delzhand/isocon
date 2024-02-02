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
            _materialReset = true;
        }
    }
    static private HashSet<Block> _allFocused = new();
    static public IEnumerable<Block> AllFocusedBlocks => _allFocused;
    private bool _highlighted = false;
    public BlockShape Shape = BlockShape.Solid;
    public bool Destroyable = true;

    private List<string> _effects = new();
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

    public static void SetColor(string id, Color color)
    {
        if (!BlockRendering.IsSetup)
        {
            BlockRendering.Setup();
        }
        BlockRendering.GetSharedMaterial(id).SetColor("_Color", color);
    }

    /// <summary>
    /// (deprecated) Use Coordinate.x instead.
    /// </summary>
    public int GetX() => Coordinate.x;
    /// <summary>
    /// (deprecated) Use Coordinate.y instead.
    /// </summary>
    public int GetY() => Coordinate.y;
    /// <summary>
    /// (deprecated) Use Coordinate.z instead.
    /// </summary>
    public int GetZ() => Coordinate.z;

    public float GetHeight()
    {
        float height = transform.localPosition.y + 1;
        if (Shape == BlockShape.Slope || Shape == BlockShape.Steps || Shape == BlockShape.SlopeInt || Shape == BlockShape.SlopeExt)
        {
            height += .5f;
        }
        return height;
    }

    public Vector3 GetMidpoint()
    {
        Vector3 v = transform.position + new Vector3(0, .25f, 0);
        if (Shape == BlockShape.Slope || Shape == BlockShape.SlopeExt || Shape == BlockShape.SlopeInt || Shape == BlockShape.Steps)
        {
            v -= new Vector3(0, .25f, 0);
        }
        return v;
    }

    public List<string> GetEffects()
    {
        return _effects;
    }

    public void ShapeChange(BlockShape blocktype)
    {
        Shape = blocktype;
        Mesh m = null;
        if (TerrainController.GridType == "Square")
        {
            switch (Shape)
            {
                case BlockShape.Spacer:
                    m = BlockRendering.Shapes[BlockShape.Solid];
                    transform.localScale = new Vector3(.3f, .3f, .3f);
                    break;
                case BlockShape.Hidden:
                    m = BlockRendering.Shapes[BlockShape.Solid];
                    transform.localScale = Vector3.zero;
                    break;
                default:
                    try
                    {
                        m = BlockRendering.Shapes[Shape];
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
        _materialReset = true;
    }

    public void EffectChange(string effect)
    {
        switch (effect)
        {
            case "None":
                _effects.Clear();
                break;
            default:
                if (_effects.Contains(effect))
                {
                    _effects.Remove(effect);
                }
                else
                {
                    _effects.Add(effect);
                }
                break;
        }
        _materialReset = true;
    }

    public void EffectRemove(string effect)
    {
        if (_effects.Contains(effect))
        {
            _effects.Remove(effect);
        }
        _materialReset = true;
    }

    /// <summary>
    /// Copies texture and colour from another block.
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
        _materialReset = true;
    }

    public void ApplyStyle(string top, string side)
    {
        _customMaterialKeyTop = top;
        _customMaterialKeySide = side;
        _materialReset = true;
    }

    public void RemoveStyle()
    {
        _customMaterialKeyTop = "";
        _customMaterialKeySide = "";
        _materialReset = true;
    }

    public (string, string) SampleStyles()
    {
        return (_customMaterialKeyTop, _customMaterialKeySide);
    }

    void SetMaterials()
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

        foreach (string fullEffect in _effects)
        {
            string[] split = fullEffect.Split("::");
            if (split.Length > 1)
            {
                string marker = split[1];
                switch (marker)
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
                string color = split[2];
                switch (color)
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

    public static void ToggleSpacers(bool show)
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++)
        {
            if (!show && blocks[i].GetComponent<Block>().Shape == BlockShape.Spacer)
            {
                blocks[i].GetComponent<Block>().ShapeChange(BlockShape.Hidden);
            }
            if (show && blocks[i].GetComponent<Block>().Shape == BlockShape.Hidden)
            {
                blocks[i].GetComponent<Block>().ShapeChange(BlockShape.Spacer);
            }
        }
    }

    public void Select()
    {
        SelectAppend(false);
    }

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
        _materialReset = true;
        TerrainController.SetInfo();
    }

    public void Deselect()
    {
        Selected = false;
        _materialReset = true;
        SelectionMenu.Hide();
    }

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

    public static void DeselectAll()
    {
        foreach (Block b in GetSelected())
        {
            b.Deselect();
        }
        TerrainController.SetInfo();
    }

    public void Focus()
    {
        UnfocusAll();
        Focused = true;
        TerrainController.SetInfo();
        Player.Self().GetComponent<DirectionalLine>().SetTarget(GetMidpoint());
    }

    public void Unfocus()
    {
        Focused = false;
        _allFocused.Remove(this);
    }

    public static Block[] GetFocused()
    {
        return _allFocused.ToArray();

    }

    public static void UnfocusAll()
    {
        Player.Self().GetComponent<DirectionalLine>().UnsetTarget();
        foreach (Block b in GetFocused())
        {
            b.Unfocus();
        }
        _allFocused.Clear();
        TerrainController.SetInfo();
    }

    public void Highlight()
    {
        _highlighted = true;
        _materialReset = true;
        TerrainController.SetInfo();
    }

    public void Dehighlight()
    {
        _highlighted = false;
        _materialReset = true;
    }

    public static Block[] GetHighlighted()
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

    public static void DehighlightAll()
    {
        foreach (Block b in GetHighlighted())
        {
            b.Dehighlight();
        }
    }

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

    public Token GetToken()
    {
        return Token.GetAtBlock(this);
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
            string.Join(",", _effects.ToArray()),
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
            block.GetComponent<Block>().EffectChange(markers[i]);
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
            block.GetComponent<Block>().EffectChange(markers[i]);
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
            block.GetComponent<Block>().EffectChange(markers[i]);
        }
        block.GetComponent<Block>().ApplyStyle(topStyle, sideStyle);
        return block;
    }
    #endregion    
}
