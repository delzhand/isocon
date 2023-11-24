using System;
using System.Collections.Generic;
using System.Linq;
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
    public static Block LastFocused;
    private static Dictionary<string, Material> materials = new Dictionary<string, Material>();

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

    void Awake() {
        if (materials.Count == 0) {
            MaterialSetup();
        }

        effects = new List<string>();
        TypeChange(Type);
    }

    // Start is called before the first frame update
    void Start()
    {
        SetMaterials();
    }

    // Update is called once per frame
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
            PaintColorSideHex
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
        return block;
    }

    public void LeftClick() {
        switch (Cursor.Mode) {
            case ClickMode.Editing:
                Block.DeselectAll();
                Select();
                TerrainController.Edit(this);
                Select();
                Block.DeselectAll();
                break;
            case ClickMode.Default:
                Select();
                break;
            case ClickMode.Placing:
                TokenController.GetSelected().Place(this);
                Cursor.Mode = ClickMode.Default;
                break;
            case ClickMode.Moving:
                TokenController.GetSelected().Move(this);
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

    public void Select() {
        if (Selected) {
            Selected = false;
        }
        else {
            Selected = true;
        }
        SetMaterials();
        TerrainController.SetInfo();
    }

    public void Deselect() {
        Selected = false;
        SetMaterials();
    }

    public void Focus() {
        LastFocused = this;
        Focused = true;
        SetMaterials();
        TerrainController.SetInfo();
    }

    public void Unfocus() {
        Focused = false;
        SetMaterials();
    }

    public void Highlight() {
        Highlighted = true;
        SetMaterials();
    }

    public void Dehighlight() {
        Highlighted = false;
        SetMaterials();
    }

    public static void DehighlightAll() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            blocks[i].GetComponent<Block>().Dehighlight();
        }
    }

    public static void DeselectAll() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            blocks[i].GetComponent<Block>().Deselect();
        }
    }

    public static void UnfocusAll() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            blocks[i].GetComponent<Block>().Unfocus();
        }        
    }

    public static List<GameObject> GetAllSelected() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        List<GameObject> selected = new List<GameObject>();
        for (int i = 0; i < blocks.Length; i++) {
            if (blocks[i].GetComponent<Block>().Selected) {
                selected.Add(blocks[i]);
            }
        }
        return selected;
    }

    public void TypeChange(BlockType blocktype) {
        Type = blocktype;
        Mesh m = null;
        switch (Type) {
            case BlockType.Solid:
                m = generateCubeMesh(1f);
                break;
            case BlockType.Slope:
                m = generateSlopeMesh();
                break;
            case BlockType.Spacer:
                m = generateCubeMesh(.3f);
                break;
            case BlockType.Hidden:
                m = generateCubeMesh(0f);
                break;
        }
        GetComponent<MeshFilter>().mesh = m;
        SetMaterials();
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
        SetMaterials();
    }

    public void Paint(Color top, Color sides) {
        PaintColorSide = sides;
        PaintColorTop = top;
        Painted = true;
        SetMaterials();
    }

    public void Depaint() {
        Painted = false;
        SetMaterials();
    }

    public Color[] SamplePaint() {
        return new Color[]{PaintColorTop, PaintColorSide};
    }

    void SetMaterials() {
        List<Material> blockMaterials = new List<Material>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (Painted) {
            if (PaintMaterialSide == null) {
                PaintMaterialSide = Instantiate(Resources.Load<Material>("Materials/Block/Checker/SideA"));
            }
            if (PaintMaterialTop == null) {
                PaintMaterialTop = Instantiate(Resources.Load<Material>("Materials/Block/Checker/TopA"));
            }
            PaintMaterialSide.color = PaintColorSide;
            PaintMaterialTop.color = PaintColorTop;
            blockMaterials.Add(PaintMaterialSide);
            blockMaterials.Add(PaintMaterialTop);
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
            blockMaterials.Add(materials["side" + (altSides ? "1" : "2")]);
            blockMaterials.Add(materials["top" + (altSides ? "1" : "2")]);
        }

        // Overwrite checkerboard/paint if highlighted
        if (Highlighted) {
            blockMaterials.RemoveAt(blockMaterials.Count-1);
            blockMaterials.Add(materials["highlighted"]);
        }

        // Markers
        Material markerMaterial = Instantiate(Resources.Load<Material>("Materials/Block/Marker"));
        if (GameSystem.Current().HasEffect("Blocked", effects)) {
            markerMaterial.SetInt("_Impassable", 1);
        }
        if (GameSystem.Current().HasEffect("Spiky", effects)) {
            markerMaterial.SetInt("_Dangerous", 1);
        }
        if (GameSystem.Current().HasEffect("Wavy", effects)) {
            markerMaterial.SetInt("_Difficult", 1);
        }
        if (GameSystem.Current().HasEffect("Hand", effects)) {
            markerMaterial.SetInt("_Interactive", 1);
        }
        if (GameSystem.Current().HasEffect("Hole", effects)) {
            markerMaterial.SetInt("_Pit", 1);
        }
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

    // Cube
    static Vector3[] getCubeVertices(float size) {
        Vector3[] vertices = new Vector3[24];
        float halfSize = size/2f;

        vertices[0] = new Vector3(-halfSize, halfSize, -halfSize);
        vertices[1] = new Vector3(-halfSize, halfSize, halfSize);
        vertices[2] = new Vector3(halfSize, halfSize, halfSize);
        vertices[3] = new Vector3(halfSize, halfSize, -halfSize);
        vertices[4] = new Vector3(-halfSize, -halfSize, -halfSize);
        vertices[5] = new Vector3(-halfSize, -halfSize, halfSize);
        vertices[6] = new Vector3(halfSize, -halfSize, halfSize);
        vertices[7] = new Vector3(halfSize, -halfSize, -halfSize);
        vertices[8] = new Vector3(-halfSize, halfSize, halfSize);
        vertices[9] = new Vector3(-halfSize, -halfSize, halfSize);
        vertices[10] = new Vector3(halfSize, -halfSize, halfSize);
        vertices[11] = new Vector3(halfSize, halfSize, halfSize);
        vertices[12] = new Vector3(halfSize, halfSize, -halfSize);
        vertices[13] = new Vector3(halfSize, -halfSize, -halfSize);
        vertices[14] = new Vector3(-halfSize, -halfSize, -halfSize);
        vertices[15] = new Vector3(-halfSize, halfSize, -halfSize);
        vertices[16] = new Vector3(-halfSize, halfSize, halfSize);
        vertices[17] = new Vector3(-halfSize, halfSize, -halfSize);
        vertices[18] = new Vector3(-halfSize, -halfSize, -halfSize);
        vertices[19] = new Vector3(-halfSize, -halfSize, halfSize);
        vertices[20] = new Vector3(halfSize, -halfSize, halfSize);
        vertices[21] = new Vector3(halfSize, -halfSize, -halfSize);
        vertices[22] = new Vector3(halfSize, halfSize, -halfSize);
        vertices[23] = new Vector3(halfSize, halfSize, halfSize);

        return vertices;
    }

    static int[] getCubeTriangles() {
        int[] triangles = new int[36];
        // triangles[0] = 0;
        // triangles[1] = 1;
        // triangles[2] = 2;
        // triangles[3] = 0;
        // triangles[4] = 2;
        // triangles[5] = 3;
        
        // triangles[6] = 4;
        // triangles[7] = 5;
        // triangles[8] = 6;
        // triangles[9] = 4;
        // triangles[10] = 6;
        // triangles[11] = 7;
        triangles[12] = 8;
        triangles[13] = 9;
        triangles[14] = 10;
        triangles[15] = 8;
        triangles[16] = 10;
        triangles[17] = 11;
        triangles[18] = 12;
        triangles[19] = 13;
        triangles[20] = 14;
        triangles[21] = 12;
        triangles[22] = 14;
        triangles[23] = 15;
        triangles[24] = 16;
        triangles[25] = 17;
        triangles[26] = 18;
        triangles[27] = 16;
        triangles[28] = 18;
        triangles[29] = 19;
        triangles[30] = 20;
        triangles[31] = 21;
        triangles[32] = 22;
        triangles[33] = 20;
        triangles[34] = 22;
        triangles[35] = 23;
        return triangles;
    }


    static int[] getCubeTopTriangles() {
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;
        return triangles;
    }

    static Vector2[] getCubeUVs() {
        Vector2[] uv = new Vector2[24];
        // Bottom face
        uv[0] = new Vector2(0f, 0f);
        uv[1] = new Vector2(1f, 0f);
        uv[2] = new Vector2(1f, 1f);
        uv[3] = new Vector2(0f, 1f);

        // Top face
        uv[4] = new Vector2(0f, 1f);
        uv[5] = new Vector2(1f, 1f);
        uv[6] = new Vector2(1f, 0f);
        uv[7] = new Vector2(0f, 0f);

        // Front face
        uv[8] = new Vector2(0f, 0f);
        uv[9] = new Vector2(1f, 0f);
        uv[10] = new Vector2(1f, 1f);
        uv[11] = new Vector2(0f, 1f);

        // Back face
        uv[12] = new Vector2(0f, 0f);
        uv[13] = new Vector2(1f, 0f);
        uv[14] = new Vector2(1f, 1f);
        uv[15] = new Vector2(0f, 1f);

        // Left face
        uv[16] = new Vector2(0f, 0f);
        uv[17] = new Vector2(1f, 0f);
        uv[18] = new Vector2(1f, 1f);
        uv[19] = new Vector2(0f, 1f);

        // Right face
        uv[20] = new Vector2(0f, 0f);
        uv[21] = new Vector2(1f, 0f);
        uv[22] = new Vector2(1f, 1f);
        uv[23] = new Vector2(0f, 1f);

        return uv;
    }

    static Mesh generateCubeMesh(float size)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = getCubeVertices(size);
        mesh.subMeshCount = 4;
        mesh.SetTriangles(getCubeTriangles(), 0);
        mesh.SetTriangles(getCubeTopTriangles(), 1);
        mesh.SetTriangles(getCubeTopTriangles(), 2);
        mesh.SetTriangles(getCubeTopTriangles(), 3);
        mesh.uv = getCubeUVs();
        mesh.normals = getNormals(mesh.vertices, mesh.triangles);
        return mesh;
    }

    // Slope
    static Vector3[] getSlopeVertices() {
        Vector3[] vertices = new Vector3[14];
        float halfSize = 0.5f;

        // Slope side
        vertices[0] = new Vector3(-halfSize, halfSize, -halfSize);
        vertices[1] = new Vector3(halfSize, halfSize, -halfSize);
        vertices[2] = new Vector3(-halfSize, -halfSize, halfSize);
        vertices[3] = new Vector3(halfSize, -halfSize, halfSize);

        // Back
        vertices[4] = new Vector3(-halfSize, halfSize, -halfSize);
        vertices[5] = new Vector3(halfSize, halfSize, -halfSize);
        vertices[6] = new Vector3(-halfSize, -halfSize, -halfSize);
        vertices[7] = new Vector3(halfSize, -halfSize, -halfSize);

        // Side 1
        vertices[8] = new Vector3(-halfSize, halfSize, -halfSize);
        vertices[9] = new Vector3(-halfSize, -halfSize, -halfSize);
        vertices[10] = new Vector3(-halfSize, -halfSize, halfSize);

        // Side 2
        vertices[11] = new Vector3(halfSize, halfSize, -halfSize);
        vertices[12] = new Vector3(halfSize, -halfSize, -halfSize);
        vertices[13] = new Vector3(halfSize, -halfSize, halfSize);

        return vertices;
    }

    static int[] getSlopeTriangles() {
        return new int[] {
            // Back
            4,5,7,
            4,7,6,
            // Sides
            8,9,10,
            11,13,12
        };
    }

    static int[] getSlopeTopTriangles() {
        return new int[] {
            1,0,2,
            1,2,3,
        };
    }

    static Vector2[] getSlopeUVs() {
        Vector2[] uv = new Vector2[14];
        uv[0] = new Vector2(0f, 0f);
        uv[1] = new Vector2(0f, 1f);
        uv[2] = new Vector2(1f, 0f);
        uv[3] = new Vector2(1f, 1f);
        return uv;
    }

    static Mesh generateSlopeMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = getSlopeVertices();
        mesh.subMeshCount = 4;
        mesh.SetTriangles(getSlopeTriangles(), 0);
        mesh.SetTriangles(getSlopeTopTriangles(), 1);
        mesh.SetTriangles(getSlopeTopTriangles(), 2);
        mesh.SetTriangles(getSlopeTopTriangles(), 3);
        mesh.uv = getSlopeUVs();
        mesh.normals = getNormals(mesh.vertices, mesh.triangles);
        return mesh;
    }

    // Shared
    static Vector3[] getNormals(Vector3[] vertices, int[] triangles) {
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 normal = Vector3.zero;

            // Find adjacent triangles and accumulate their face normals
            for (int j = 0; j < triangles.Length; j += 3)
            {
                if (triangles[j] == i || triangles[j + 1] == i || triangles[j + 2] == i)
                {
                    Vector3 vertex1 = vertices[triangles[j]];
                    Vector3 vertex2 = vertices[triangles[j + 1]];
                    Vector3 vertex3 = vertices[triangles[j + 2]];
                    normal += Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1).normalized;
                }
            }

            normals[i] = normal.normalized;
        }       
        return normals; 
    }

}
