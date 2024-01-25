using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BlockShape
{
    Solid,
    Slope,
    SlopeInt,
    SlopeExt,
    Steps,
    Corner,
    FlatCorner,
    Upslope,
    Spacer,
    Hidden
}

public class Block : MonoBehaviour
{
    // Focus State
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

    public bool Highlighted = false;
    public BlockShape Shape = BlockShape.Solid;
    public bool Destroyable = true;

    private List<string> _effects = new();
    private Material _markerMaterial;

    private bool _painted = false;
    private Color _paintColorTop;
    private Color _paintColorSide;
    private Material _paintMaterialTop;
    private Material _paintMaterialSide;

    private string _textureTop = "";
    private string _textureSide = "";

    private bool _materialReset = true;

    private Vector3 _dragOrigin;
    private bool _dragging;

    void Awake()
    {
        if (!BlockMesh.IsSetup)
        {
            BlockMesh.Setup();
        }
        _paintMaterialSide = Instantiate(Resources.Load<Material>("Materials/Block/Checker/SideC"));
        _paintMaterialTop = Instantiate(Resources.Load<Material>("Materials/Block/Checker/TopC"));
        _markerMaterial = Instantiate(Resources.Load<Material>("Materials/Block/Marker"));
        ShapeChange(Shape);
    }

    void LateUpdate()
    {
        if (!Input.GetMouseButtonUp(1))
        {
            return;
        }
        if (_dragging && Input.mousePosition == _dragOrigin)
        {
            // Mouse up where clicked
            TileMenu.ShowMenu(this);
        }
    }

    void Update()
    {
        GameObject indicator = transform.Find("Indicator").gameObject;
        if (Shape == BlockShape.Solid || Shape == BlockShape.Slope)
        {
            indicator.transform.eulerAngles = new Vector3(90, -90, 0);
            indicator.SetActive(TerrainController.Indicators);
        }
        else
        {
            indicator.SetActive(false);
        }


        if (_materialReset)
        {
            _materialReset = false;
            SetMaterials();
        }
    }

    public string WriteOut()
    {
        Column c = transform.parent.GetComponent<Column>();
        string PaintColorTopHex = "";
        string PaintColorSideHex = "";
        if (_painted)
        {
            PaintColorTopHex = ColorUtility.GetHex(_paintColorTop);
            PaintColorSideHex = ColorUtility.GetHex(_paintColorSide);
        }
        string[] bits = new string[]{
            c.X.ToString(),
            c.Y.ToString(),
            transform.localPosition.y.ToString(),
            transform.localEulerAngles.y.ToString(),
            Shape.ToString(),
            Destroyable.ToString(),
            string.Join(",", _effects.ToArray()),
            _painted.ToString(),
            PaintColorTopHex,
            PaintColorSideHex,
            _textureTop,
            _textureSide,
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
        if (painted)
        {
            Color top = ColorUtility.GetColor(data[8]);
            Color sides = ColorUtility.GetColor(data[9]);
            block.GetComponent<Block>().ApplyPaint(top, sides);
        }
        if (data.Length > 10)
        {
            (string, string) TextureStrings = BlockMesh.TextureMap(data[10]);
            block.GetComponent<Block>()._textureTop = TextureStrings.Item2;
            block.GetComponent<Block>()._textureSide = TextureStrings.Item1;
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
        if (painted)
        {
            Color top = ColorUtility.GetColor(data[8]);
            Color sides = ColorUtility.GetColor(data[9]);
            block.GetComponent<Block>().ApplyPaint(top, sides);
        }
        if (data.Length > 10)
        {
            block.GetComponent<Block>()._textureTop = data[10];
            block.GetComponent<Block>()._textureSide = data[11];
        }

        return block;
    }

    public void LeftClickDown()
    {
        switch (Cursor.Mode)
        {
            case CursorMode.TerrainEffecting:
                Select();
                break;
            case CursorMode.Editing:
                Block.DeselectAll();
                Select();
                TerrainController.Edit(this);
                Select();
                Block.DeselectAll();
                break;
            case CursorMode.Default:
                Token.DeselectAll();
                break;
            case CursorMode.Dragging:
                Token.GetSelected().Move(this);
                break;
        }
    }

    public void RightClickDown()
    {
        switch (Cursor.Mode)
        {
            case CursorMode.TerrainEffecting:
                if (SelectionMenu.Visible)
                {
                    SelectionMenu.Hide();
                }
                else
                {
                    _dragging = true;
                    _dragOrigin = Input.mousePosition;
                }
                break;
        }
    }

    public static void SetColor(string id, Color color)
    {
        if (!BlockMesh.IsSetup)
        {
            BlockMesh.Setup();
        }
        BlockMesh.GetSharedMaterial(id).SetColor("_Color", color);
    }

    public int GetX()
    {
        return this.transform.parent.GetComponent<Column>().X;
    }

    public int GetY()
    {
        return this.transform.parent.GetComponent<Column>().Y;
    }

    public int GetZ()
    {
        return (int)(this.transform.position.y / .5f) + 2;
    }

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
                    m = BlockMesh.Shapes[BlockShape.Solid];
                    transform.localScale = new Vector3(.3f, .3f, .3f);
                    break;
                case BlockShape.Hidden:
                    m = BlockMesh.Shapes[BlockShape.Solid];
                    transform.localScale = Vector3.zero;
                    break;
                default:
                    try
                    {
                        m = BlockMesh.Shapes[Shape];
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
            m = BlockMesh.Hex;
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

    public void ApplyPaint(Color top, Color sides)
    {
        _paintColorSide = sides;
        _paintColorTop = top;
        _painted = true;
        _textureTop = "";
        _textureSide = "";
        _materialReset = true;
    }

    public void RemovePaint()
    {
        _painted = false;
        _materialReset = true;
    }

    /// <summary>
    /// Copies texture and colour from another block.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="copyShape">Also copy the shape</param>
    public void CopyStyle(Block other, bool copyShape = false)
    {
        _painted = other._painted;
        _textureTop = other._textureTop;
        _textureSide = other._textureSide;
        _paintColorTop = other._paintColorTop;
        _paintColorSide = other._paintColorSide;
        if (copyShape)
        {
            ShapeChange(other.Shape);
        }
        _materialReset = true;
    }

    public Color[] SamplePaint()
    {
        if (_painted)
        {
            return new Color[] { _paintColorTop, _paintColorSide };
        }
        return null;
    }

    public void ApplyTexture(string top, string side)
    {
        _textureTop = top;
        _textureSide = side;
        _painted = false;
        _materialReset = true;
    }

    public void RemoveTexture()
    {
        _textureTop = "";
        _textureSide = "";
        _materialReset = true;
    }

    public (string, string) SampleTexture()
    {

        return (BlockMesh.ReverseTextureMap(_textureTop), BlockMesh.ReverseTextureMap(_textureSide));
    }

    void SetMaterials()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();

        Material[] mats = mr.materials;

        if (_painted)
        {
            _paintMaterialSide.color = _paintColorSide;
            _paintMaterialTop.color = _paintColorTop;
            mats[BlockMesh.MaterialSideIndex(Shape)] = _paintMaterialSide;
            mats[BlockMesh.MaterialTopIndex(Shape)] = _paintMaterialTop;
        }
        else if (_textureTop.Length > 0)
        {
            mats[BlockMesh.MaterialSideIndex(Shape)] = BlockMesh.GetSharedMaterial(_textureSide);
            mats[BlockMesh.MaterialTopIndex(Shape)] = BlockMesh.GetSharedMaterial(_textureTop);
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
            mats[BlockMesh.MaterialSideIndex(Shape)] = BlockMesh.GetSharedMaterial("side" + (altSides ? "1" : "2"));
            mats[BlockMesh.MaterialTopIndex(Shape)] = BlockMesh.GetSharedMaterial("top" + (altSides ? "1" : "2"));
        }

        // Overwrite checkerboard/paint if highlighted
        if (Highlighted)
        {
            mats[BlockMesh.MaterialTopIndex(Shape)] = BlockMesh.GetSharedMaterial("highlighted");
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

        mats[BlockMesh.MaterialMarkerIndex(Shape)] = _markerMaterial;

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
        mats[BlockMesh.MaterialFocusIndex(Shape)] = BlockMesh.GetSharedMaterial(focusState);

        // Apply
        mr.SetMaterials(mats.ToList());
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

    public void Select(bool append = false)
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
    }

    public void Highlight()
    {
        Highlighted = true;
        _materialReset = true;
        TerrainController.SetInfo();
    }

    public void Dehighlight()
    {
        Highlighted = false;
        _materialReset = true;
    }

    public static Block[] GetHighlighted()
    {
        List<Block> highlighted = new();
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < gos.Length; i++)
        {
            Block block = gos[i].GetComponent<Block>();
            if (block.Highlighted)
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

    public static void ToggleBorders(bool show)
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++)
        {
            Block b = blocks[i].GetComponent<Block>();
            b._paintMaterialSide.SetInt("_ShowOutline", show ? 1 : 0);
            b._paintMaterialTop.SetInt("_ShowOutline", show ? 1 : 0);
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
            Vector2Int blockCoords = new Vector2Int(block.GetX(), block.GetY());
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
            if (block.GetX() == coordinate.x && block.GetY() == coordinate.y)
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
}
