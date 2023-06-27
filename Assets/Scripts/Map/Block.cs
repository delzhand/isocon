using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public enum BlockType
{
  Solid,
  Slope,
  Spacer,
  Hidden
}

public enum BlockMarker
{
    None,
    Dangerous,
    Difficult,
    Interactive,
    Impassable
}

public enum AlterOption
{
    ADD_AT_POSITION,
    DELETE_SELECTED,
    ROTATE_SELECTED,
    CLONE_ROW,
    CLONE_COLUMN,
    SET_SHAPE_SOLID,
    SET_SHAPE_SLOPE,
    SET_SHAPE_EMPTY,
    REMOVE_EFFECTS,
    SET_EFFECT_DANGER,
    SET_EFFECT_DIFFICULT,
    SET_EFFECT_INTERACTIVE,
    SET_EFFECT_IMPASSABLE,
}

public class Block : MonoBehaviour
{
    public float sides;
    public float top;

    public bool Selected = false;
    public BlockType Type = BlockType.Solid;
    public bool Destroyable = true;
    Mesh m;
    MeshFilter mf;
    MeshRenderer mr;
    GameObject blockUI;
    List<BlockMarker> markers;
    
    static Material normal;
    static Material dangerous;
    static Material difficult;
    static Material interactive;
    static Material impassable;
    static Material spacer;
    static Material side1;
    static Material side2;
    static Material top1;
    static Material top2;
    static Material selectedMat;

    void Awake() {
        normal = Resources.Load<Material>("Materials/Block/Marker/Normal");
        dangerous = Resources.Load<Material>("Materials/Block/Marker/Dangerous");
        difficult = Resources.Load<Material>("Materials/Block/Marker/Difficult");
        interactive = Resources.Load<Material>("Materials/Block/Marker/Interactive");
        impassable = Resources.Load<Material>("Materials/Block/Marker/Impassable");
        selectedMat = Resources.Load<Material>("Materials/Block/Marker/Focused");

        side1 = Resources.Load<Material>("Materials/Block/Checker/SideA");
        side2 = Resources.Load<Material>("Materials/Block/Checker/SideB");
        top1 = Resources.Load<Material>("Materials/Block/Checker/TopA");
        top2 = Resources.Load<Material>("Materials/Block/Checker/TopB");

        markers = new List<BlockMarker>();
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
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
    }

    public override string ToString(){
        Column c = transform.parent.GetComponent<Column>();
        string[] bits = new string[]{
            c.X.ToString(),
            c.Y.ToString(),
            transform.localPosition.y.ToString(),
            transform.localEulerAngles.y.ToString(),
            Type.ToString(),
            Destroyable.ToString(),
            string.Join(",", markers.ToArray())
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
        // string[] markersArray = bits[6].Split(",");
        // List<BlockMarker> markers = new List<BlockMarker>();
        // for(int i = 0; i < markersArray.Length; i++) {
        //     BlockMarker bm = (BlockMarker)Enum.Parse(typeof(BlockMarker), markersArray[i], true);
        // }

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
        return block;
    }

    void OnMouseDown()
    {
        Vector3 clickPosition = Input.mousePosition;
        if (UI.IsPointerOverUI(clickPosition)) {
            return;
        }
        switch (ModeController.GetMode()) {
            case Mode.View:
                CameraControl.GoToBlock(this);
                SetTerrainInfo();
                DeselectAll();
                Select();
                break;
            case Mode.Alter:
                Block.DeselectAll();
                Select();
                switch(ModeController.GetAlterOption()) {
                    case AlterOption.ADD_AT_POSITION:
                        TerrainEngine.AddBlocks();
                        break;
                    case AlterOption.DELETE_SELECTED:
                        TerrainEngine.RemoveBlocks();
                        break;
                    case AlterOption.SET_SHAPE_SOLID:
                        TerrainEngine.ChangeType(BlockType.Solid);
                        break;
                    case AlterOption.SET_SHAPE_SLOPE:
                        TerrainEngine.ChangeType(BlockType.Slope);
                        break;
                    case AlterOption.SET_SHAPE_EMPTY:
                        TerrainEngine.ChangeType(BlockType.Spacer);
                        break;
                    case AlterOption.ROTATE_SELECTED:
                        TerrainEngine.RotateBlocks();
                        break;
                    case AlterOption.SET_EFFECT_DANGER:
                        TerrainEngine.ChangeMarker(BlockMarker.Dangerous);
                        break;
                    case AlterOption.SET_EFFECT_DIFFICULT:
                        TerrainEngine.ChangeMarker(BlockMarker.Difficult);
                        break;
                    case AlterOption.SET_EFFECT_IMPASSABLE:
                        TerrainEngine.ChangeMarker(BlockMarker.Impassable);
                        break;
                    case AlterOption.SET_EFFECT_INTERACTIVE:
                        TerrainEngine.ChangeMarker(BlockMarker.Interactive);
                        break;
                    case AlterOption.REMOVE_EFFECTS:
                        TerrainEngine.ChangeMarker(BlockMarker.None);
                        break;
                    case AlterOption.CLONE_ROW:
                        TerrainEngine.CloneRow();
                        Block.ResetMaterials();
                        break;
                    case AlterOption.CLONE_COLUMN:
                        TerrainEngine.CloneColumn();
                        Block.ResetMaterials();
                        break;
                }
                Block.DeselectAll();
                break;
        }
    }

    public int getX() {
        return this.transform.parent.GetComponent<Column>().X;
    }

    public int getY() {
        return this.transform.parent.GetComponent<Column>().Y;
    }

    public void SetTerrainInfo() {
        VisualElement root = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement;
        string height = (transform.localPosition.y + 1).ToString();
        if (Type == BlockType.Slope) {
            height = transform.localPosition.y + "~" + (transform.localPosition.y + 1);
        }
        root.Q<Label>("Height").text = height;
        root.Q<Label>("Coords").text = toAlpha(getY() + 1) + "" + (getX()+1);
        root.Q("Effects").Clear();
        markers.ForEach(marker => {
            Label l = new Label();
            l.text = marker.ToString();
            l.AddToClassList("effect");
            root.Q("Effects").Add(l);
        });
    }

    private string toAlpha(int x) {
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

    public void Activate() {
        Select();
    }

    public void Select() {
        Selected = true;
        SetMaterials();
    }

    public void Deselect() {
        this.Selected = false;
        SetMaterials();
    }

    public static void DeselectAll() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            blocks[i].GetComponent<Block>().Deselect();
        }
    }

    public static bool AreAnySelected() {
        return Block.GetAllSelected().Count > 0;
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
        mf.mesh = m;
        SetMaterials();
    }

    public void MarkerChange(BlockMarker marker) {
        switch (marker) {
            case BlockMarker.None:
                markers.Clear();
                break;
            default:
                if (markers.Contains(marker)) {
                    markers.Remove(marker);
                }
                else {
                    markers.Add(marker);
                }
                break;
        }
        SetMaterials();
    }

    void SetMaterials() {
        List<Material> materials = new List<Material>();
        MeshRenderer mr = GetComponent<MeshRenderer>();

        // Checkerboard
        if (transform.parent != null) {
            // Checkerboard
            float x = transform.parent.GetComponent<Column>().X;
            float y = transform.parent.GetComponent<Column>().Y;
            float z = transform.localPosition.y;
            sides = (x + y + z);
            if ((x + y + z) % 2 == 0) {
                materials.Add(side2);
            }
            else {
                materials.Add(side1);
            }

            top = (x + y);
            if ((x + y) % 2 == 0) {
                materials.Add(top2);
            }
            else {
                materials.Add(top1);
            }
        }
        else {
            materials.Add(side1);
            materials.Add(top1);
        }

        // Modify
        if (Selected) {
            materials.Add(selectedMat);
            materials.Add(selectedMat);
        }
        // else {
        //     materials.Add(side1);
        //     materials.Add(top1);
        // }

        materials.Add(normal);

        // Markers
        if (markers.Contains(BlockMarker.Impassable)) {
            materials.Add(impassable);
        }
        if (markers.Contains(BlockMarker.Dangerous)) {
            materials.Add(dangerous);
        }
        if (markers.Contains(BlockMarker.Difficult)) {
            materials.Add(difficult);
        }
        if (markers.Contains(BlockMarker.Interactive)) {
            materials.Add(interactive);
        }

        // Apply
        mr.SetMaterials(materials);
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

    public static void ResetMaterials() {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < blocks.Length; i++) {
            blocks[i].GetComponent<Block>().SetMaterials();
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
        mesh.subMeshCount = 6;
        mesh.SetTriangles(getCubeTriangles(), 0);
        mesh.SetTriangles(getCubeTopTriangles(), 1);
        mesh.SetTriangles(getCubeTopTriangles(), 2);
        mesh.SetTriangles(getCubeTopTriangles(), 3);
        mesh.SetTriangles(getCubeTopTriangles(), 4);
        mesh.SetTriangles(getCubeTopTriangles(), 5);
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
        mesh.subMeshCount = 6;
        mesh.SetTriangles(getSlopeTriangles(), 0);
        mesh.SetTriangles(getSlopeTopTriangles(), 1);
        mesh.SetTriangles(getSlopeTopTriangles(), 2);
        mesh.SetTriangles(getSlopeTopTriangles(), 3);
        mesh.SetTriangles(getSlopeTopTriangles(), 4);
        mesh.SetTriangles(getSlopeTopTriangles(), 5);
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
