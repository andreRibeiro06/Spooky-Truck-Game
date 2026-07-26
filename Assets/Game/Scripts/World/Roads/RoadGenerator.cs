using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class RoadGenerator : MonoBehaviour
{
    [Header("TargetPoints")]
    public Transform pointA;
    public Transform pointB;

    [Header("Road Properties")]
    public float roadWidth = 2f;
    public int segments = 30;
    public float heightOffset = 0.05f;


    [Header("Terrain Reference Parameter")]
    public float scale = 0.1f;
    public float heightMultiplier = 5f;
    public float xOffset = 0f;
    public float zOffset = 0f;

    private Mesh roadMesh;

    private void OnValidate()
    {
        BuildRoad();
    }

    public void BuildRoad()
    {
        if(pointA == null || pointB == null) return;

        if(roadMesh == null)
        {
            roadMesh = new Mesh {name = "Road Mesh"};
            GetComponent<MeshFilter>().mesh = roadMesh;
        }

        Vector3[] verts = new Vector3[(segments + 1) * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int [segments * 6];

        float totalDistance = 0f;

        for(int i = 0; i<= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 center = Vector3.Lerp(pointA.position, pointB.position, t);

            Vector3 forward = (pointB.position - pointA.position).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 leftWorld = center - (right * (roadWidth * 0.5f));
            Vector3 rightWorld = center + (right * (roadWidth * 0.5f));

            leftWorld.y = TerrainMath.GetHeight(leftWorld.x, leftWorld.z, scale, heightMultiplier, xOffset, zOffset) + heightOffset;    
            rightWorld.y = TerrainMath.GetHeight(rightWorld.x, rightWorld.z, scale, heightMultiplier, xOffset, zOffset) + heightOffset;

            int vIdx = i * 2;
            verts[vIdx] = transform.InverseTransformPoint(leftWorld);
            verts[vIdx + 1] = transform.InverseTransformPoint(rightWorld);

            if(i < segments)
            {
                int tIdx = i * 6;
                tris[tIdx] = vIdx;
                tris[tIdx + 1] = vIdx + 2;
                tris[tIdx + 2] = vIdx + 1;

                tris[tIdx + 3] = vIdx + 1;
                tris[tIdx + 4] = vIdx + 2;
                tris[tIdx + 5] = vIdx + 3;


            }
        }

        roadMesh.Clear();
        roadMesh.vertices = verts;
        roadMesh.triangles = tris;
        roadMesh.uv = uvs;
        roadMesh.RecalculateNormals();
    }
}