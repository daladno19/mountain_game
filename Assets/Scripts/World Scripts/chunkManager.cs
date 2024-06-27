using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Chunk chunkPrefab; // Reference to the TerrainChunk prefab
    public Transform player; // Reference to the player
    public int chunkSize = 16; // Size of each chunk
    public int viewDistance = 2; // Distance (in chunks) to keep loaded around the player

    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

    void Start()
    {
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
            chunks[chunkCoord].Unload();
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
        newChunk.Load();
        chunks.Add(coord, newChunk);
    }
}
