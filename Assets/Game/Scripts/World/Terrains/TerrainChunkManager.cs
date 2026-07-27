using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainChunkManager : MonoBehaviour
{
    public Transform target;
    public GameObject terrainPrefab;
    public GameObject roadPrefab;

    public int chunkSize = 50;
    public int renderDistance = 2;

    private Dictionary<Vector2Int, GameObject> terrainChunks = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<int, GameObject> roadChunks = new Dictionary<int, GameObject>();

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
        Vector2Int newChunk = new Vector2Int(
            Mathf.FloorToInt(target.position.x / chunkSize),
            Mathf.FloorToInt(target.position.z / chunkSize)
        );

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

        // --- 1. SPRAWL TERRAIN CHUNKS ---
        List<Vector2Int> removeTerrain = new List<Vector2Int>();

        for (int z = -renderDistance; z <= renderDistance; z++)
        {
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                Vector2Int coord = new Vector2Int(currentChunkX + x, currentChunkZ + z);

                if (!terrainChunks.ContainsKey(coord))
                {
                    CreateTerrainChunk(coord);
                }
            }
        }

        // Cleanup out-of-range terrain
        foreach (var chunk in terrainChunks)
        {
            int dx = Mathf.Abs(chunk.Key.x - currentChunkX);
            int dz = Mathf.Abs(chunk.Key.y - currentChunkZ);

            if (dx > renderDistance || dz > renderDistance)
            {
                Destroy(chunk.Value);
                removeTerrain.Add(chunk.Key);
            }
        }

        foreach (var key in removeTerrain)
        {
            terrainChunks.Remove(key);
        }

        // --- 2. SPRAWL ROAD CHUNKS (along Z) ---
        List<int> removeRoads = new List<int>();

        for (int z = -renderDistance; z <= renderDistance; z++)
        {
            int roadChunkZ = currentChunkZ + z;

            if (!roadChunks.ContainsKey(roadChunkZ))
            {
                CreateRoadChunk(roadChunkZ);
            }
        }

        // Cleanup out-of-range roads
        foreach (var road in roadChunks)
        {
            int dz = Mathf.Abs(road.Key - currentChunkZ);
            if (dz > renderDistance)
            {
                Destroy(road.Value);
                removeRoads.Add(road.Key);
            }
        }

        foreach (var key in removeRoads)
        {
            roadChunks.Remove(key);
        }
    }

    void CreateTerrainChunk(Vector2Int coord)
    {
        GameObject chunk = Instantiate(terrainPrefab, new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize), Quaternion.identity);
        chunk.name = $"Terrain Chunk {coord.x}, {coord.y}";

        TerrainMeshGenerator generator = chunk.GetComponent<TerrainMeshGenerator>();
        generator.Initialize(coord.x, coord.y, chunkSize);

        terrainChunks.Add(coord, chunk);
    }

    void CreateRoadChunk(int chunkZ)
    {
        if (roadPrefab == null) return;

        // FIXED: Retrieve terrain generator from existing chunks, or fall back to prefab
        TerrainMeshGenerator terrainRef = null;
        if (terrainChunks.Count > 0)
        {
            terrainRef = terrainChunks.Values.First().GetComponent<TerrainMeshGenerator>();
        }
        else
        {
            terrainRef = terrainPrefab.GetComponent<TerrainMeshGenerator>();
        }

        GameObject roadChunk = Instantiate(roadPrefab, new Vector3(0, 0, chunkZ * chunkSize), Quaternion.identity);
        roadChunk.name = $"Road Chunk {chunkZ}";

        RoadMeshGenerator roadGen = roadChunk.GetComponent<RoadMeshGenerator>();
        roadGen.InitializeRoadChunk(chunkZ, chunkSize, terrainRef);

        roadChunks.Add(chunkZ, roadChunk);
    }
}