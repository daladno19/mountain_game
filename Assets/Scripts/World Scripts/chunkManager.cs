using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Chunk chunkPrefab; // Reference to the Chunk prefab
    public int chunkSize = 16; // Size of each chunk
    public string savePath = "Chunks"; // Path to save the chunk data

    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

    public Dictionary<Vector2Int, Chunk> getChunks(){
        return chunks;
    }
    void Start()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    public bool IsChunkLoaded(Vector2Int chunkCoord)
    {
        return chunks.ContainsKey(chunkCoord);
    }

    public void UnloadChunks(List<Vector2Int> chunksToUnload)
    {
        foreach (Vector2Int chunkCoord in chunksToUnload)
        {
            if (chunks.ContainsKey(chunkCoord))
            {
                string chunkPath = Path.Combine(savePath, $"{chunkCoord.x}_{chunkCoord.y}.chunk");
                chunks[chunkCoord].Unload(chunkPath);
                Destroy(chunks[chunkCoord].gameObject);
                chunks.Remove(chunkCoord);
            }
        }
    }

    public IEnumerator<Chunk> LoadChunk(Vector2Int coord)
    {
        if (chunks.ContainsKey(coord))
            yield break;

        Vector3 position = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
        Chunk newChunk = Instantiate(chunkPrefab, position, Quaternion.identity);
        newChunk.transform.parent = transform;
        newChunk.Init(chunkSize, position);
        string chunkPath = Path.Combine(savePath, $"{coord.x}_{coord.y}.chunk");
        newChunk.Load(chunkPath);
        chunks.Add(coord, newChunk);

        yield return null; // Simulate asynchronous loading
    }
}
