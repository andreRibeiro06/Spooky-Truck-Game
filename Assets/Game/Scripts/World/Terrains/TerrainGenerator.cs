using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int xSize = 50;
    public int zSize = 50;
    public float cellSize = 1f;

    [Header("Perlin Noise Settings")]
    public float scale = 0.1f;
    public float heightMultiplier = 5f;
    public float xOffset = 0f;
    public float zOffset = 0f;

    private Vector3[] vertices;
    private int[] triangles;
    private Mesh mesh;

    private void OnEnable()
    {
        InitMesh();
        GenerateAndApply();
    }

    private void OnValidate()
    {
        InitMesh();
        GenerateAndApply();
    }

    private void InitMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Procedural Terrain";
            GetComponent<MeshFilter>().mesh = mesh;
        }
    }

    private void GenerateAndApply()
    {
        if (xSize <= 0 || zSize <= 0 || cellSize <= 0) return;

        GenerateMesh();
        UpdateMesh();
    }

    public float EvaluateHeightAt(float worldX, float worldZ)
    {
        return TerrainMath.GetHeight(worldX, worldZ, scale, heightMultiplier, xOffset, zOffset);
    }

    void GenerateMesh()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                // Account for world position offset if the transform moves
                float worldX = (x * cellSize) + transform.position.x;
                float worldZ = (z * cellSize) + transform.position.z;

                float y = EvaluateHeightAt(worldX, worldZ);

                vertices[i] = new Vector3(x * cellSize, y, z * cellSize);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}