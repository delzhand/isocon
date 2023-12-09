using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMesh
{
    public static Dictionary<string, Mesh> Shapes;

    public static void Setup() {
        Shapes = new Dictionary<string, Mesh>();
        Shapes.Add("Block", Resources.Load<Mesh>("Models/Block"));
        Shapes.Add("Slope", Resources.Load<Mesh>("Models/Slope"));
        Shapes.Add("Hex", Resources.Load<Mesh>("Models/Hex"));
    }


        // Cube
    public static Vector3[] GetCubeVertices(float size) {
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

    public static int[] GetCubeTriangles() {
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


    public static int[] GetCubeTopTriangles() {
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;
        return triangles;
    }

    public static Vector2[] GetCubeUVs() {
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

    public static Mesh GenerateBlockMesh(float size) {
        switch (TerrainController.GridType) {
            case "Square":
                return GenerateCubeMesh(size);
            // case "Hex":
            //     return GenerateHexMesh();
        }
        return null;
    }

    public static Mesh GenerateCubeMesh(float size)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = GetCubeVertices(size);
        mesh.subMeshCount = 4;
        mesh.SetTriangles(GetCubeTriangles(), 0);
        mesh.SetTriangles(GetCubeTopTriangles(), 1);
        mesh.SetTriangles(GetCubeTopTriangles(), 2);
        mesh.SetTriangles(GetCubeTopTriangles(), 3);
        mesh.uv = GetCubeUVs();
        mesh.normals = GetNormals(mesh.vertices, mesh.triangles);
        return mesh;
    }

    // Slope
    public static Vector3[] GetSlopeVertices() {
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

    public static int[] GetSlopeTriangles() {
        return new int[] {
            // Back
            4,5,7,
            4,7,6,
            // Sides
            8,9,10,
            11,13,12
        };
    }

    public static int[] GetSlopeTopTriangles() {
        return new int[] {
            1,0,2,
            1,2,3,
        };
    }

    public static Vector2[] GetSlopeUVs() {
        Vector2[] uv = new Vector2[14];
        uv[0] = new Vector2(0f, 0f);
        uv[1] = new Vector2(1f, 0f);
        uv[2] = new Vector2(0f, 1f);
        uv[3] = new Vector2(1f, 1f);
        return uv;
    }

    public static Mesh GenerateSlopeMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = GetSlopeVertices();
        mesh.subMeshCount = 4;
        mesh.SetTriangles(GetSlopeTriangles(), 0);
        mesh.SetTriangles(GetSlopeTopTriangles(), 1);
        mesh.SetTriangles(GetSlopeTopTriangles(), 2);
        mesh.SetTriangles(GetSlopeTopTriangles(), 3);
        mesh.uv = GetSlopeUVs();
        mesh.normals = GetNormals(mesh.vertices, mesh.triangles);
        return mesh;
    }

    // Hex
    private static Vector3[] rawHexVertices() {
        Vector3[] vertices = new Vector3[12];
        float height = 1.0f;
        float radius = 1.0f;

        // Top hexagon
        for (int i = 0; i < 6; i++) {
            float angle = i * Mathf.PI / 3;
            float x = radius * Mathf.Cos(angle);
            float y = height / 2.0f;
            float z = radius * Mathf.Sin(angle);
            vertices[i] = new Vector3(x, y, z);
        }

        // Bottom hexagon
        for (int i = 0; i < 6; i++) {
            float angle = i * Mathf.PI / 3;
            float x = radius * Mathf.Cos(angle);
            float y = -height / 2.0f;
            float z = radius * Mathf.Sin(angle);
            vertices[i + 6] = new Vector3(x, y, z);
        }

        return vertices;
    }        

    public static Vector3[] GetHexVertices() {
        Vector3[] rawVertices = rawHexVertices();
        Vector3[] vertices = new Vector3[28];

        // Top
        vertices[0] = rawVertices[0];
        vertices[1] = rawVertices[1];
        vertices[2] = rawVertices[2];
        vertices[3] = rawVertices[3];
        vertices[4] = rawVertices[4];
        vertices[5] = rawVertices[5];


        vertices[6] = rawVertices[0];
        vertices[7] = rawVertices[1];
        vertices[8] = rawVertices[7];
        vertices[9] = rawVertices[6];

        return vertices;
    }

    public static int[] GetHexTriangles() {
        int[] triangles = new int[42];
        return triangles;
    }

    public static int[] GetHexTopTriangles() {
        int[] triangles = new int[6];
        return triangles;
    }

    public static Vector2[] GetHexUVs() {
        Vector2[] uv = new Vector2[28];
        return uv;
    }

    public static Mesh GenerateHexMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = GetHexVertices();
        mesh.subMeshCount = 4;
        mesh.SetTriangles(GetHexTriangles(), 0);
        mesh.SetTriangles(GetHexTopTriangles(), 1);
        mesh.SetTriangles(GetHexTopTriangles(), 2);
        mesh.SetTriangles(GetHexTopTriangles(), 3);
        mesh.uv = GetHexUVs();
        mesh.normals = GetNormals(mesh.vertices, mesh.triangles);
        return mesh;
    }

    // Shared
    public static Vector3[] GetNormals(Vector3[] vertices, int[] triangles) {
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
