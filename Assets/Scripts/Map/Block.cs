using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

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
    public static Block LastFocused;

    // Focus State
    public bool Selected = false;
    public bool Focused = false;
    public bool Highlighted = false;

    public BlockShape Shape = BlockShape.Solid;
    public bool Destroyable = true;

    private List<string> effects = new();
    private Material markerMaterial;

    private bool Painted = false;
    private Color PaintColorTop;
    private Color PaintColorSide;
    private Material PaintMaterialTop;
    private Material PaintMaterialSide;

    private string Texture = "";

    private bool MaterialReset = true;

    void Awake() {
        if (!BlockMesh.IsSetup) {
            BlockMesh.Setup();
        }
        PaintMaterialSide = Instantiate(Resources.Load<Material>("Materials/Block/Checker/SideC"));
        PaintMaterialTop = Instantiate(Resources.Load<Material>("Materials/Block/Checker/TopC"));
        markerMaterial = Instantiate(Resources.Load<Material>("Materials/Block/Marker"));
        ShapeChange(Shape);
    }

    void Update()
    {
        GameObject indicator = transform.Find("Indicator").gameObject;
        if (Shape == BlockShape.Solid || Shape == BlockShape.Slope) {
            indicator.transform.eulerAngles = new Vector3(90, -90, 0);
            indicator.SetActive(TerrainController.Indicators);
        }
        else {
            indicator.SetActive(false);
        }

        if (Focused && this != LastFocused) {
            Unfocus();
        }

        if (MaterialReset) {
            MaterialReset = false;
            SetMaterials();
        }
    }

    public string WriteOut(){
        Column c = transform.parent.GetComponent<Column>();
        string PaintColorTopHex = "";
        string PaintColorSideHex = "";
        if (Painted) {
            PaintColorTopHex = ColorUtility.ColorToHex(PaintColorTop);
            PaintColorSideHex = ColorUtility.ColorToHex(PaintColorSide);
        }
        string[] bits = new string[]{
            c.X.ToString(),
            c.Y.ToString(),
            transform.localPosition.y.ToString(),
            transform.localEulerAngles.y.ToString(),
            Shape.ToString(),
            Destroyable.ToString(),
            string.Join(",", effects.ToArray()),
            Painted.ToString(),
            PaintColorTopHex,
            PaintColorSideHex,
            Texture
        };
        return string.Join("|", bits);
    }  

    public static GameObject ReadIn(string version, string block) {
        string[] data = block.Split("|");
        switch (version) {
            case "v1":
                return parseV1(data);
        }
        return null;
    }

    private static GameObject parseV1(string[] data) {
        int x = int.Parse(data[0]);
        int y = int.Parse(data[1]);
        float z = float.Parse(data[2]);
        float r = float.Parse(data[3]);
        BlockShape type = (BlockShape)Enum.Parse(typeof(BlockShape), data[4], true);
        bool destroyable = bool.Parse(data[5]);
        string[] markersArray = data[6].Split(",");
        List<string> markers = new List<string>();
        for(int i = 0; i < markersArray.Length; i++) {
            if (markersArray[i].Length > 0) {
                markers.Add(markersArray[i]);
            }
        }
        // Default to false here to not break older saves
        bool painted = false;
        if (data.Length > 7) {
            painted = bool.Parse(data[7]);
        }

        GameObject map = GameObject.Find("Terrain");
        GameObject column = GameObject.Find(x+","+y);
        if (column == null) {
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
        for (int i = 0; i < markers.Count; i++) {
            block.GetComponent<Block>().EffectChange(markers[i]);
        }
        if (painted) {
            Color top = ColorUtility.ColorFromHex(data[8]);
            Color sides = ColorUtility.ColorFromHex(data[9]);
            block.GetComponent<Block>().ApplyPaint(top, sides);
        }
        if (data.Length > 10) {
            block.GetComponent<Block>().Texture = data[10];
        }

        return block;
    }

    public void LeftClick() {
        switch (Cursor.Mode) {
            case CursorMode.Editing:
                Block.DeselectAll();
                Select();
                TerrainController.Edit(this);
                Select();
                Block.DeselectAll();
                break;
            case CursorMode.Default:
                Select();
                break;
            case CursorMode.Placing:
                TokenMenu.DoPlace(this);
                break;
            case CursorMode.Moving:
                TokenMenu.DoMove(this);
                break;
        }
    }

    public void RightClick() {
        CameraControl.GoToBlock(this);
    }

    public static void SetColor(string id, Color color) {
        if (BlockMesh.SharedMaterials.Count == 0) {
            BlockMesh.Setup();
        }
        BlockMesh.SharedMaterials[id].SetColor("_Color", color);
    }

    public int getX() {
        return this.transform.parent.GetComponent<Column>().X;
    }

    public int getY() {
        return this.transform.parent.GetComponent<Column>().Y;
    }

    public int getZ() {
        return (int)(this.transform.position.y/.5f)+2;
    }

    public List<string> GetEffects() {
        return effects;
    }
    
    public void ShapeChange(BlockShape blocktype) {
        Shape = blocktype;
        Mesh m = null;
        if (TerrainController.GridType == "Square") {
            switch (Shape) {
                case BlockShape.Spacer:
                    m = BlockMesh.Shapes[BlockShape.Solid];
                    transform.localScale = new Vector3(.3f, .3f, .3f);
                    break;
                case BlockShape.Hidden:
                    m = BlockMesh.Shapes[BlockShape.Solid];
                    transform.localScale = Vector3.zero;
                    break;
                default:
                    try {
                        m = BlockMesh.Shapes[Shape];
                        transform.localScale = Vector3.one;
                        break;
                    }
                    catch (Exception e) {
                        Debug.LogError(e.Message);
                        throw new Exception($"No such shape {blocktype.ToString()}");
                    }

            }
        }
        else if (TerrainController.GridType == "Hex") {
            m = BlockMesh.Hex;
            transform.localScale = Vector3.one;
        }
        GetComponent<MeshFilter>().mesh = m;
        MaterialReset = true;
    }

    public void EffectChange(string effect) {
        switch (effect) {
            case "None":
                effects.Clear();
                break;
            default:
                if (effects.Contains(effect)) {
                    effects.Remove(effect);
                }
                else {
                    effects.Add(effect);
                }
                break;
        }
        MaterialReset = true;
    }

    public void ApplyPaint(Color top, Color sides) {
        PaintColorSide = sides;
        PaintColorTop = top;
        Painted = true;
        Texture = "";
        MaterialReset = true;
    }

    public void RemovePaint() {
        Painted = false;
        MaterialReset = true;
    }

    public Color[] SamplePaint() {
        if (Painted) {
            return new Color[]{PaintColorTop, PaintColorSide};
        }
        return null;
    }

    public void ApplyTexture(string name) {
        Texture = name;
        Painted = false;
        MaterialReset = true;
    }

    public void RemoveTexture() {
        Texture = "";
        MaterialReset = true;
    }

    public string SampleTexture() {
        return Texture;
    }

    void SetMaterials() {
        MeshRenderer mr = GetComponent<MeshRenderer>();

        Material[] mats = mr.materials;

        if (Painted) {
            PaintMaterialSide.color = PaintColorSide;
            PaintMaterialTop.color = PaintColorTop;
            mats[BlockMesh.MaterialSideIndex(Shape)] = PaintMaterialSide;
            mats[BlockMesh.MaterialTopIndex(Shape)] = PaintMaterialTop;
        }
        else if (Texture.Length > 0) {
            (string,string) styleMats = BlockMesh.StyleMaterials(Texture);
            mats[BlockMesh.MaterialSideIndex(Shape)] = BlockMesh.SharedMaterials[styleMats.Item1];
            mats[BlockMesh.MaterialTopIndex(Shape)] = BlockMesh.SharedMaterials[styleMats.Item2];
        }
        else {
            // Checkerboard
            bool altSides = false;
            // bool altTop = false;
            if (transform.parent != null) {
                // Checkerboard
                float x = transform.parent.GetComponent<Column>().X;
                float y = transform.parent.GetComponent<Column>().Y;
                float z = transform.localPosition.y;
                altSides = ((x + y + z) % 2 == 0);
                // altTop = ((x + y) % 2 == 0);
            }
            mats[BlockMesh.MaterialSideIndex(Shape)] = BlockMesh.SharedMaterials["side" + (altSides ? "1" : "2")];
            mats[BlockMesh.MaterialTopIndex(Shape)] = BlockMesh.SharedMaterials["top" + (altSides ? "1" : "2")];
        }

        // Overwrite checkerboard/paint if highlighted
        if (Highlighted) {
            mats[BlockMesh.MaterialTopIndex(Shape)] = BlockMesh.SharedMaterials["highlighted"];
        }

        // Markers
        markerMaterial.SetInt("_Impassable", 0);
        markerMaterial.SetInt("_Dangerous", 0);
        markerMaterial.SetInt("_Difficult", 0);
        markerMaterial.SetInt("_Interactive", 0);
        markerMaterial.SetInt("_Pit", 0);
        markerMaterial.SetInt("_Other", 0);
        markerMaterial.SetInt("_Skull", 0);
        
        foreach (string effect in effects) {
            switch (effect) {
                case "Blocked":
                    markerMaterial.SetInt("_Impassable", 1);
                    break;
                case "Spiky":
                    markerMaterial.SetInt("_Dangerous", 1);
                    break;
                case "Wavy":
                    markerMaterial.SetInt("_Difficult", 1);
                    break;
                case "Hand":
                    markerMaterial.SetInt("_Interactive", 1);
                    break;
                case "Hole":
                    markerMaterial.SetInt("_Pit", 1);
                    break;
                case "Corners":
                    markerMaterial.SetInt("_Other", 1);
                    break;
                case "Skull":
                    markerMaterial.SetInt("_Skull", 1);
                    break;
            }
        }

        mats[BlockMesh.MaterialMarkerIndex(Shape)] = markerMaterial;

        // Selected/Focused
        string focusState = "unfocused";
        if (Selected && !Focused) {
            focusState = "selected";
        }
        else if (!Selected && Focused) {
            focusState = "focused";
        }
        else if (Selected && Focused) {
            focusState = "selectfocused";
        }
        mats[BlockMesh.MaterialFocusIndex(Shape)] = BlockMesh.SharedMaterials[focusState];

        // Apply
        mr.SetMaterials(mats.ToList());
    }

    public static void ToggleSpacers(bool show) {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for(int i = 0; i < blocks.Length; i++) {
            if (!show && blocks[i].GetComponent<Block>().Shape == BlockShape.Spacer) {
                blocks[i].GetComponent<Block>().ShapeChange(BlockShape.Hidden);
            }
            if (show && blocks[i].GetComponent<Block>().Shape == BlockShape.Hidden) {
                blocks[i].GetComponent<Block>().ShapeChange(BlockShape.Spacer);
            }
        }
    }

    #region Select
    public void Select() {
        if (Selected) {
            Selected = false;
        }
        else {
            Selected = true;
        }
        MaterialReset = true;
        TerrainController.SetInfo();
    }

    public void Deselect() {
        Selected = false;
        MaterialReset = true;
    }

    public static Block[] GetSelected() {
        List<Block> selected = new();
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < gos.Length; i++) {
            Block block = gos[i].GetComponent<Block>();
            if (block.Selected) {
                selected.Add(block);
            }
        }
        return selected.ToArray();
    }

    public static void DeselectAll() {
        foreach (Block b in GetSelected()) {
            b.Deselect();
        }
    }
    #endregion

    #region Focus
    public void Focus() {
        LastFocused = this;
        Focused = true;
        MaterialReset = true;
        TerrainController.SetInfo();
    }

    public void Unfocus() {
        Focused = false;
        LastFocused = null;
        MaterialReset = true;
    }

    public static Block[] GetFocused() {
        List<Block> focused = new();
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < gos.Length; i++) {
            Block block = gos[i].GetComponent<Block>();
            if (block.Focused) {
                focused.Add(block);
            }
        }
        return focused.ToArray();
    }


    public static void UnfocusAll() {
        foreach (Block b in GetFocused()) {
            b.Unfocus();
        }
    }
    #endregion
    
    #region Highlight
    public void Highlight() {
        Highlighted = true;
        MaterialReset = true;
        TerrainController.SetInfo();
    }

    public void Dehighlight() {
        Highlighted = false;
        MaterialReset = true;
    }

    public static Block[] GetHighlighted() {
        List<Block> highlighted = new();
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < gos.Length; i++) {
            Block block = gos[i].GetComponent<Block>();
            if (block.Highlighted) {
                highlighted.Add(block);
            }
        }
        return highlighted.ToArray();
    }

    public static void DehighlightAll() {
        foreach (Block b in GetHighlighted()) {
            b.Dehighlight();
        }
    }
    #endregion

    public static void ToggleBorders(bool show) {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            Block b = blocks[i].GetComponent<Block>();
            b.PaintMaterialSide.SetInt("_ShowOutline", show ? 1 : 0);            
            b.PaintMaterialTop.SetInt("_ShowOutline", show ? 1 : 0);            
        }
    }
}
