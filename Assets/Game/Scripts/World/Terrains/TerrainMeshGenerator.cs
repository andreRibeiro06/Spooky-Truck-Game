using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainMeshGenerator : MonoBehaviour
{
    private Mesh mesh;
    private MeshCollider meshCollider;

    private Vector3[] vertices;
    private int[] triangles;

    [Header("Grid Size")]
    public int xSize = 50;
    public int zSize = 50;

    [Header("Noise Settings")]
    public float scale = 50f;
    public float heightMultiplier = 30f;
    [Range(1, 8)] public int octaves = 4;
    [Range(0f, 1f)] public float persistence = 0.5f;
    public float lacunarity = 2.0f;

    [Header("Terrain Shaping")]
    [Tooltip("Exaggerates peaks and flattens valleys. Try 2.0 to 3.0")]
    [Range(1f, 5f)] public float redistributionPower = 2.5f;

    [Header("Road Carving Settings")]
    public bool enableRoadCarving = true;
    public float roadWidth = 8f;
    public float blendDistance = 10f; // Distance to smoothly transition from flat road bed to raw mountain

    [Header("World Offsets")]
    public float offsetX = 0f;
    public float offsetZ = 0f;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Initialize(int chunkX, int chunkZ, int chunkSize)
    {
        xSize = chunkSize;
        zSize = chunkSize;

        offsetX = chunkX * chunkSize;
        offsetZ = chunkZ * chunkSize;

        CreateShape();
        UpdateMesh();
    }

    /// <summary>
    /// Calculates untouched raw mountain noise without any road carving.
    /// Public so the RoadMeshGenerator can sample it.
    /// </summary>
    public float GetRawTerrainHeight(float worldX, float worldZ)
    {
        if (scale <= 0) scale = 0.0001f;

        float totalHeight = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxPossibleHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (worldX / scale) * frequency;
            float sampleZ = (worldZ / scale) * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);
            totalHeight += perlinValue * amplitude;

            maxPossibleHeight += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        // Normalize (0..1)
        float normalized = totalHeight / maxPossibleHeight;

        // Apply redistribution power curve to flatten lows and exaggerate peaks
        float shapedHeight = Mathf.Pow(normalized, redistributionPower);

        return shapedHeight * heightMultiplier;
    }

    /// <summary>
    /// Evaluates final vertex height, applying road flattening if enabled.
    /// </summary>
    public float EvaluateTerrainHeight(float worldX, float worldZ)
{
    float rawHeight = GetRawTerrainHeight(worldX, worldZ);

    if (!enableRoadCarving) return rawHeight;

    float roadX = RoadPath.GetRoadX(worldZ);
    float distToRoad = Mathf.Abs(worldX - roadX);
    float halfRoadWidth = roadWidth * 0.5f;

    // Get exact same smooth elevation used by the road mesh
    float targetRoadBedHeight = RoadPath.GetSmoothRoadElevation(worldZ, scale, heightMultiplier);

    // 1. Inside the road lane: Perfectly flat bed
    if (distToRoad <= halfRoadWidth)
    {
        return targetRoadBedHeight;
    }
    // 2. Transition zone: Smooth slope from road bed to mountain side
    else if (distToRoad < halfRoadWidth + blendDistance)
    {
        float t = (distToRoad - halfRoadWidth) / blendDistance;
        t = Mathf.SmoothStep(0f, 1f, t);

        return Mathf.Lerp(targetRoadBedHeight, rawHeight, t);
    }

    return rawHeight;
}

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float worldX = x + offsetX;
                float worldZ = z + offsetZ;

                float y = EvaluateTerrainHeight(worldX, worldZ);
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        int vert = 0;
        int tris = 0;
        triangles = new int[xSize * zSize * 6];

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
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

        mesh.normals = CalculateContinuousNormals();

        // Refresh MeshCollider so physics works properly
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    /// <summary>
    /// Calculates continuous cross-chunk normals using terrain math.
    /// </summary>
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

                float hL = EvaluateTerrainHeight(worldX - delta, worldZ);
                float hR = EvaluateTerrainHeight(worldX + delta, worldZ);
                float hD = EvaluateTerrainHeight(worldX, worldZ - delta);
                float hU = EvaluateTerrainHeight(worldX, worldZ + delta);

                Vector3 normal = new Vector3(hL - hR, 2f * delta, hD - hU).normalized;
                normals[i] = normal;
                i++;
            }
        }

        return normals;
    }
}