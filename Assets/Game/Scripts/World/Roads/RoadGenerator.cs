using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class RoadMeshGenerator : MonoBehaviour
{
    public float roadWidth = 8f;
    public float segmentLength = 2f;
    public float yOffset = 0.2f; // Keeps road mesh slightly above terrain to avoid clipping

    private Mesh mesh;

    public void InitializeRoadChunk(int chunkZ, int chunkSize, TerrainMeshGenerator terrainGenerator)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        float startZ = chunkZ * chunkSize;
        int segments = Mathf.CeilToInt(chunkSize / segmentLength);
        int vertCount = (segments + 1) * 2;

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[segments * 6];

        int vertIndex = 0;
        int triIndex = 0;

        // Pull scale and height parameters from terrain generator
        float scale = (terrainGenerator != null) ? terrainGenerator.scale : 50f;
        float heightMultiplier = (terrainGenerator != null) ? terrainGenerator.heightMultiplier : 30f;

        for (int i = 0; i <= segments; i++)
        {
            float currentZ = startZ + (i * segmentLength);
            float centerX = RoadPath.GetRoadX(currentZ);

            // 1. Get ultra-smooth road elevation
            float smoothY = RoadPath.GetSmoothRoadElevation(currentZ, scale, heightMultiplier) + yOffset;

            // 2. Local space coordinates
            Vector3 centerPosLocal = new Vector3(centerX, smoothY, currentZ - startZ);

            Vector3 forward = RoadPath.GetRoadTangent(currentZ);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // 3. Extrude left and right vertices horizontally (forced same Y level)
            Vector3 leftVert = centerPosLocal - (right * (roadWidth * 0.5f));
            Vector3 rightVert = centerPosLocal + (right * (roadWidth * 0.5f));

            leftVert.y = smoothY;  // Lock Y to prevent sideways tilt
            rightVert.y = smoothY; // Lock Y to prevent sideways tilt

            vertices[vertIndex] = leftVert;
            vertices[vertIndex + 1] = rightVert;

            uvs[vertIndex] = new Vector2(0, currentZ / roadWidth);
            uvs[vertIndex + 1] = new Vector2(1, currentZ / roadWidth);

            if (i < segments)
            {
                triangles[triIndex + 0] = vertIndex;
                triangles[triIndex + 1] = vertIndex + 2;
                triangles[triIndex + 2] = vertIndex + 1;

                triangles[triIndex + 3] = vertIndex + 1;
                triangles[triIndex + 4] = vertIndex + 2;
                triangles[triIndex + 5] = vertIndex + 3;

                triIndex += 6;
            }

            vertIndex += 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        // Update physics collider
        MeshCollider col = GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = mesh;
        }
    }
}