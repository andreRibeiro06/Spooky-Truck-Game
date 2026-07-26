using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainMeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int xSize = 50;
    public int zSize = 50;

    [Header("Noise Settings")]
    public float scale = 1f;
    public float heightMultiplier = 10f;

    [Header("World Offsets")]
    public float offsetX = 0f;
    public float offsetZ = 0f;


    public void Initialize(int chunkX, int chunkZ, int chunkSize)
    {
        xSize = chunkSize;
        zSize = chunkSize;

        offsetX = chunkX * chunkSize;
        offsetZ = chunkZ * chunkSize;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for(int i = 0, z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                float sampleX = (x + offsetX) * scale;
                float sampleZ = (z + offsetZ) * scale;

                float y = Mathf.PerlinNoise(sampleX, sampleZ) * heightMultiplier;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        int vert = 0;
        int tris = 0;
        triangles = new int[xSize * zSize * 6];

        for(int z = 0; z < zSize; z++)
        {
            for(int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
                
                vert++;
                tris+=6;
            }
        vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.normals = CalculateContinuousNormals();
    }

    Vector3[] CalculateContinuousNormals()
    {
        Vector3[] normals = new Vector3[vertices.Length];
        
        float delta = 0.05f;

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float worldX = x + offsetX;
                float worldZ = z + offsetZ;

                float hL = Mathf.PerlinNoise((worldX - delta) * scale, worldZ * scale) * heightMultiplier;
                float hR = Mathf.PerlinNoise((worldX + delta) * scale, worldZ * scale) * heightMultiplier;
                float hD = Mathf.PerlinNoise(worldX * scale, (worldZ - delta) * scale) * heightMultiplier;
                float hU = Mathf.PerlinNoise(worldX * scale, (worldZ + delta) * scale) * heightMultiplier;

                Vector3 normal = new Vector3(hL - hR, 2f * delta, hD - hU).normalized;
                normals[i] = normal;
                i++;
            }
        }

        return normals;
    }
}
