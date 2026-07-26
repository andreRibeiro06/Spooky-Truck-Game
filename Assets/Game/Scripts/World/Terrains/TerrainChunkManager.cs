using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkManager : MonoBehaviour
{
    public Transform target;
    public GameObject terrainPrefab;
    public int chunkSize = 50;
    public int renderDistance = 2;


    private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();

    private Vector2Int currentChunk;
    void Start()
    {
        currentChunk = new Vector2Int(
        Mathf.FloorToInt(target.position.x / chunkSize),
        Mathf.FloorToInt(target.position.z / chunkSize)
    );

    UpdateChunks();
    }

    void Update()
    {
        Vector2Int newChunk = new Vector2Int(Mathf.FloorToInt(target.position.x / chunkSize), Mathf.FloorToInt(target.position.z / chunkSize));
        if (newChunk != currentChunk)
        {
            currentChunk = newChunk;
            UpdateChunks();
        }
    }

    public void UpdateChunks()
    {
        int currentChunkX = Mathf.FloorToInt(target.position.x / chunkSize);
        int currentChunkZ = Mathf.FloorToInt(target.position.z / chunkSize);

        List<Vector2Int> remove = new List<Vector2Int>();

        for(int z = -renderDistance; z <= renderDistance; z++)
        {
            for(int x = -renderDistance; x <= renderDistance; x++)
            {
                Vector2Int coord = new Vector2Int(currentChunkX + x, currentChunkZ + z);

                if (!chunks.ContainsKey(coord))
                {
                    CreateChunk(coord);
                }
            }
        }

        foreach (var chunk in chunks)
        {
            int dx = Mathf.Abs(chunk.Key.x - currentChunkX);
            int dz = Mathf.Abs(chunk.Key.y - currentChunkZ);

            if(dx > renderDistance || dz > renderDistance)
            {
                Destroy(chunk.Value);
                remove.Add(chunk.Key);
            }
        }

        foreach(var key in remove)
        {
            chunks.Remove(key);
        }
    }

    public void CreateChunk(Vector2Int coord)
    {
        GameObject chunk = Instantiate(terrainPrefab, new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize), Quaternion.identity);    

        chunk.name = $"Chunk {coord.x}, {coord.y}";

        TerrainMeshGenerator generator = chunk.GetComponent<TerrainMeshGenerator>();

        generator.Initialize(coord.x, coord.y, chunkSize);

        chunks.Add(coord, chunk);
    }
}