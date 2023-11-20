using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cylinder : MonoBehaviour
{
    void Awake() {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        Mesh cylinderMesh = GenerateCylinderMesh(6, 1, 1);

        meshFilter.mesh = cylinderMesh;

        // Set default material
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Move the cylinder to local position zero
        transform.localPosition = Vector3.zero;    
    }

    public Mesh GenerateCylinderMesh(int segments, int radius, int height)
    {
 MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int numVertices = (segments + 1) * 2 * 2; // Each segment has two quads (top and bottom), each with 4 vertices
        int numTriangles = segments * 6 * 2; // Each segment has two quads, each with two triangles

        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uv = new Vector2[numVertices];
        int[] triangles = new int[numTriangles];

        float angleIncrement = 360f / segments;

        // Generate vertices and UVs
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Top vertices
            vertices[i * 4] = new Vector3(x, height * 0.5f, z);
            uv[i * 4] = new Vector2((float)i / segments, 1f);

            vertices[i * 4 + 1] = new Vector3(x, height * 0.5f, z);
            uv[i * 4 + 1] = new Vector2((float)(i + 1) / segments, 1f);

            // Bottom vertices
            vertices[i * 4 + 2] = new Vector3(x, -height * 0.5f, z);
            uv[i * 4 + 2] = new Vector2((float)i / segments, 0f);

            vertices[i * 4 + 3] = new Vector3(x, -height * 0.5f, z);
            uv[i * 4 + 3] = new Vector2((float)(i + 1) / segments, 0f);
        }

        // Generate triangles
        for (int i = 0, ti = 0; i < segments; i++, ti += 6)
        {
            int currentVertex = i * 4;

            // Top face
            triangles[ti] = currentVertex;
            triangles[ti + 1] = currentVertex + 2;
            triangles[ti + 2] = currentVertex + 1;

            // Bottom face
            triangles[ti + 3] = currentVertex + 1;
            triangles[ti + 4] = currentVertex + 2;
            triangles[ti + 5] = currentVertex + 3;
        }

        // Assign the generated data to the mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        // Recalculate normals to ensure correct lighting
        mesh.RecalculateNormals();
        
        return mesh;
    }
}
