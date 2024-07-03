using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Chunk chunkPrefab; // Reference to the TerrainChunk prefab
    public Transform player; // Reference to the player
    public int chunkSize = 16; // Size of each chunk
    public int viewDistance = 2; // Distance (in chunks) to keep loaded around the player
    public string savePath = "Chunks"; // Path to save the chunk data

    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

    void Start()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        UpdateChunks();
    }

    void Update()
    {
        UpdateChunks();
    }

    void UpdateChunks()
    {
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.z / chunkSize)
        );

        List<Vector2Int> chunksToUnload = new List<Vector2Int>(chunks.Keys);

        for (int y = -viewDistance; y <= viewDistance; y++)
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunkCoord.x + x, playerChunkCoord.y + y);
                if (!chunks.ContainsKey(chunkCoord))
                {
                    CreateChunk(chunkCoord);
                }
                chunksToUnload.Remove(chunkCoord);
            }
        }

        foreach (Vector2Int chunkCoord in chunksToUnload)
        {
            string chunkPath = Path.Combine(savePath, $"{chunkCoord.x}_{chunkCoord.y}.chunk");
            chunks[chunkCoord].Unload(chunkPath);
            Destroy(chunks[chunkCoord].gameObject);
            chunks.Remove(chunkCoord);
        }
    }

    void CreateChunk(Vector2Int coord)
    {
        Vector3 position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        Chunk newChunk = Instantiate(chunkPrefab, position, Quaternion.identity);
        newChunk.transform.parent = transform;
        newChunk.Init(chunkSize, position);
        string chunkPath = Path.Combine(savePath, $"{coord.x}_{coord.y}.chunk");
        newChunk.Load(chunkPath);
        chunks.Add(coord, newChunk);
    }
}
