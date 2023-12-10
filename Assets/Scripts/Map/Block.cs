using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public enum BlockType
{
  Solid,
  Slope,
  Spacer,
  Hidden
}

public class Block : MonoBehaviour
{
    private static Dictionary<string, Material> materials = new Dictionary<string, Material>();
    private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

    public static Block LastFocused;

    public bool Selected = false;
    public bool Focused = false;
    public bool Highlighted = false;

    public BlockType Type = BlockType.Solid;
    public bool Destroyable = true;
    private List<string> effects;

    private bool Painted = false;
    private Color PaintColorTop;
    private Color PaintColorSide;
    private Material PaintMaterialTop;
    private Material PaintMaterialSide;

    private string Style = "";

    private Material markerMaterial;
    private bool MaterialReset = true;

    void Awake() {
        if (materials.Count == 0) {
            MaterialSetup();
            BlockMesh.Setup();
        }

        effects = new List<string>();
        TypeChange(Type);
    }

    void Start()
    {
    }

    void Update()
    {
        GameObject indicator = transform.Find("Indicator").gameObject;
        if (Type == BlockType.Solid || Type == BlockType.Slope) {
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

    public static void MaterialSetup() {
        materials.Add("side1", Instantiate(Resources.Load<Material>("Materials/Block/Checker/SideA")));
        materials.Add("side2", Instantiate(Resources.Load<Material>("Materials/Block/Checker/SideB")));
        materials.Add("top1", Instantiate(Resources.Load<Material>("Materials/Block/Checker/TopA")));
        materials.Add("top2", Instantiate(Resources.Load<Material>("Materials/Block/Checker/TopB")));

        materials.Add("highlighted", Instantiate(Resources.Load<Material>("Materials/Block/Highlighted")));

        materials.Add("unfocused", Instantiate(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        materials.Add("focused", Instantiate(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        materials["focused"].SetInt("_Focused", 1);

        materials.Add("selectfocused", Instantiate(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        materials["selectfocused"].SetInt("_Selected", 1);
        materials["selectfocused"].SetInt("_Focused", 1);

        materials.Add("selected", Instantiate(Resources.Load<Material>("Materials/Block/Marker/Focused")));
        materials["selected"].SetInt("_Selected", 1);

        foreach(string s in StringUtility.Arr("AcidSide","AcidTopFlow","AcidTopStill","Brick2Side","Brick2Top","BrickSide","BrickTop","DryGrassTop","GoldSide","GoldTop","GrassTop","LavaSide","LavaTopFlow","LavaTopStill","MetalSide","MetalTop","PoisonSide","PoisonTopFlow","PoisonTopStill","SandSide","SandTop","SnowSide","SnowTop","SoilSide","SoilTop","StoneSide","StoneTop","WaterSide","WaterTopFlow","WaterTopStill","Wood2Side","Wood2Top","WoodSide","WoodTop", "GrayBrickSide", "GrayBrickTop", "GrayMetalSide", "GrayMetalTop")) {
            materials.Add($"{s}", Instantiate(Resources.Load<Material>($"Materials/Block/Artistic/{s}")));
        }
    }

    public override string ToString(){
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
            Type.ToString(),
            Destroyable.ToString(),
            string.Join(",", effects.ToArray()),
            Painted.ToString(),
            PaintColorTopHex,
            PaintColorSideHex,
            Style
        };
        return string.Join("|", bits);
    }  

    public static GameObject FromString(string version, string block) {
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
        BlockType type = (BlockType)Enum.Parse(typeof(BlockType), data[4], true);
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
        block.GetComponent<Block>().TypeChange(type);
        for (int i = 0; i < markers.Count; i++) {
            block.GetComponent<Block>().EffectChange(markers[i]);
        }
        if (painted) {
            Color top = ColorUtility.ColorFromHex(data[8]);
            Color sides = ColorUtility.ColorFromHex(data[9]);
            block.GetComponent<Block>().Paint(top, sides);
        }
        if (data.Length > 10) {
            block.GetComponent<Block>().Style = data[10];
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
        if (materials.Count == 0) {
            MaterialSetup();
        }
        materials[id].SetColor("_BaseColor", color);
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

    public static string GetAlpha(int x) {
        const int Base = 26;
        const int Offset = 64; // ASCII offset for uppercase letters

        string column = "";
        
        while (x > 0)
        {
            int remainder = x % Base;
            char letter = (char)(remainder + Offset);
            
            column = letter + column;
            x = (x - 1) / Base; // Adjust number for next iteration
        }
        
        return column;    
    }

    public void TypeChange(BlockType blocktype) {
        Type = blocktype;
        Mesh m = null;
        if (TerrainController.GridType == "Square") {
            switch (Type) {
                case BlockType.Solid:
                    m = BlockMesh.Shapes["Block"];
                    transform.localScale = Vector3.one;
                    break;
                case BlockType.Slope:
                    m = BlockMesh.Shapes["Slope"];
                    transform.localScale = Vector3.one;
                    break;
                case BlockType.Spacer:
                    m = BlockMesh.Shapes["Block"];
                    transform.localScale = new Vector3(.3f, .3f, .3f);
                    break;
                case BlockType.Hidden:
                    m = BlockMesh.Shapes["Block"];
                    m = BlockMesh.GenerateCubeMesh(0f);
                    transform.localScale = Vector3.zero;
                    break;
            }
        }
        else if (TerrainController.GridType == "Hex") {
            m = BlockMesh.Shapes["Hex"];
            transform.localScale = Vector3.one;
        }
        GetComponent<MeshFilter>().mesh = m;
        MaterialReset = true;
    }

    public void EffectChange(string effect) {
        switch (effect) {
            case "Clear":
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

    public void Paint(Color top, Color sides) {
        PaintColorSide = sides;
        PaintColorTop = top;
        Painted = true;
        MaterialReset = true;
    }

    public void Depaint() {
        Painted = false;
        MaterialReset = true;
    }

    public void ApplyStyle(string name) {
        Style = name;
        MaterialReset = true;
    }

    public Color[] SamplePaint() {
        return new Color[]{PaintColorTop, PaintColorSide};
    }

    void SetMaterials() {
        List<Material> blockMaterials = new List<Material>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (Painted) {
            if (PaintMaterialSide == null) {
                PaintMaterialSide = Instantiate(Resources.Load<Material>("Materials/Block/Checker/SideC"));
            }
            if (PaintMaterialTop == null) {
                PaintMaterialTop = Instantiate(Resources.Load<Material>("Materials/Block/Checker/TopC"));
            }
            PaintMaterialSide.color = PaintColorSide;
            PaintMaterialTop.color = PaintColorTop;
            blockMaterials.Add(PaintMaterialTop);
            blockMaterials.Add(PaintMaterialSide);
        }
        else if (Style.Length > 0) {
            string side = "";
            string top = "";
            switch (Style) {
                case "Acid Flow":
                    side = "AcidSide";
                    top = "AcidTopStill";
                    break;
                case "Acid":
                    side = "AcidSide";
                    top = "AcidTopFlow";
                    break;
                case "Old Brick":
                    side = "Brick2Side";
                    top = "Brick2Top";
                    break;
                case "Brick":
                    side = "BrickSide";
                    top = "BrickTop";
                    break;
                case "Dry Grass":
                    side = "SoilSide";
                    top = "DryGrassTop";
                    break;
                case "Grass":
                    side = "SoilSide";
                    top = "GrassTop";
                    break;
                case "Gold":
                    side = "GoldSide";
                    top = "GoldTop";
                    break;
                case "Lava Flow":
                    side = "LavaSide";
                    top = "LavaTopFlow";
                    break;
                case "Lava":
                    side = "LavaSide";
                    top = "LavaTopStill";
                    break;
                case "Metal":
                    side = "MetalSide";
                    top = "MetalTop";
                    break;
                case "Poison Flow":
                    side = "PoisonSide";
                    top = "PoisonTopFlow";
                    break;
                case "Poison":
                    side = "PoisonSide";
                    top = "PoisonTopStill";
                    break;
                case "Sand":
                    side = "SandSide";
                    top = "SandTop";
                    break;
                case "Snow":
                    side = "SnowSide";
                    top = "SnowTop";
                    break;
                case "Soil":
                    side = "SoilSide";
                    top = "SoilTop";
                    break;
                case "Stone":
                    side = "StoneSide";
                    top = "StoneTop";
                    break;
                case "Water Flow":
                    side = "WaterSide";
                    top = "WaterTopFlow";
                    break;
                case "Water":
                    side = "WaterSide";
                    top = "WaterTopStill";
                    break;
                case "Wood":
                    side = "WoodSide";
                    top = "WoodTop";
                    break;
                case "Old Wood":
                    side = "Wood2Side";
                    top = "Wood2Top";
                    break;
                case "Gray Metal":
                    side = "GrayMetalSide";
                    top = "GrayMetalTop";
                    break;
                case "Gray Brick":
                    side = "GrayBrickSide";
                    top = "GrayBrickTop";
                    break;
            }
            blockMaterials.Add(materials[top]);
            blockMaterials.Add(materials[side]);
        }
        else {
            // Checkerboard
            bool altSides = false;
            bool altTop = false;
            if (transform.parent != null) {
                // Checkerboard
                float x = transform.parent.GetComponent<Column>().X;
                float y = transform.parent.GetComponent<Column>().Y;
                float z = transform.localPosition.y;
                altSides = ((x + y + z) % 2 == 0);
                altTop = ((x + y) % 2 == 0);
            }
            blockMaterials.Add(materials["top" + (altSides ? "1" : "2")]);
            blockMaterials.Add(materials["side" + (altSides ? "1" : "2")]);
        }

        // Overwrite checkerboard/paint if highlighted
        if (Highlighted) {
            blockMaterials.RemoveAt(blockMaterials.Count-1);
            blockMaterials.Add(materials["highlighted"]);
        }

        // Markers
        if (markerMaterial == null) {
            markerMaterial = Instantiate(Resources.Load<Material>("Materials/Block/Marker"));
        }
        markerMaterial.SetInt("_Impassable", 0);
        if (GameSystem.Current().HasEffect("Blocked", effects)) {
            markerMaterial.SetInt("_Impassable", 1);
        }
        markerMaterial.SetInt("_Dangerous", 0);
        if (GameSystem.Current().HasEffect("Spiky", effects)) {
            markerMaterial.SetInt("_Dangerous", 1);
        }
        markerMaterial.SetInt("_Difficult", 0);
        if (GameSystem.Current().HasEffect("Wavy", effects)) {
            markerMaterial.SetInt("_Difficult", 1);
        }
        markerMaterial.SetInt("_Interactive", 0);
        if (GameSystem.Current().HasEffect("Hand", effects)) {
            markerMaterial.SetInt("_Interactive", 1);
        }
        markerMaterial.SetInt("_Pit", 0);
        if (GameSystem.Current().HasEffect("Hole", effects)) {
            markerMaterial.SetInt("_Pit", 1);
        }
        markerMaterial.SetInt("_Other", 0);
        if (GameSystem.Current().HasCustomEffect(effects)) {
            markerMaterial.SetInt("_Other", 1);
        }
        blockMaterials.Add(markerMaterial);
        
        // Selected/Focused
        string fmat = "unfocused";
        if (Selected && !Focused) {
            fmat = "selected";
        }
        else if (!Selected && Focused) {
            fmat = "focused";
        }
        else if (Selected && Focused) {
            fmat = "selectfocused";
        }
        blockMaterials.Add(materials[fmat]);

        // Apply
        mr.SetMaterials(blockMaterials);
    }

    public static void ToggleSpacers(bool show) {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for(int i = 0; i < blocks.Length; i++) {
            if (!show && blocks[i].GetComponent<Block>().Type == BlockType.Spacer) {
                blocks[i].GetComponent<Block>().TypeChange(BlockType.Hidden);
            }
            if (show && blocks[i].GetComponent<Block>().Type == BlockType.Hidden) {
                blocks[i].GetComponent<Block>().TypeChange(BlockType.Spacer);
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
}
