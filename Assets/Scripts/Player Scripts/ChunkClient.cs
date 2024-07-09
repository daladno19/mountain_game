using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkClient : MonoBehaviour
{
    public ChunkManager chunkManager; // Reference to the ChunkManager
    public Transform player; // Reference to the player
    public int viewDistance = 2; // Distance (in chunks) to keep loaded around the player
    private Vector2Int lastPlayerChunkCoord;

    void Start()
    {
        StartCoroutine(UpdateChunks());
    }

    void Update()
    {
        Vector2Int currentChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkManager.chunkSize),
            Mathf.FloorToInt(player.position.z / chunkManager.chunkSize)
        );

        if (currentChunkCoord != lastPlayerChunkCoord)
        {
            StartCoroutine(UpdateChunks());
            lastPlayerChunkCoord = currentChunkCoord;
        }
    }

    private IEnumerator UpdateChunks()
    {
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkManager.chunkSize),
            Mathf.FloorToInt(player.position.z / chunkManager.chunkSize)
        );

        List<Vector2Int> chunksToLoad = new List<Vector2Int>();
        List<Vector2Int> chunksToUnload = new List<Vector2Int>(chunkManager.getChunks().Keys);

        for (int y = -viewDistance; y <= viewDistance; y++)
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunkCoord.x + x, playerChunkCoord.y + y);
                float distance = Vector2.Distance(chunkCoord, playerChunkCoord);
                if (distance <= viewDistance)
                {
                    if (!chunkManager.IsChunkLoaded(chunkCoord))
                    {
                        chunksToLoad.Add(chunkCoord);
                    }
                    chunksToUnload.Remove(chunkCoord);
                }
            }
        }

        chunkManager.UnloadChunks(chunksToUnload);

        foreach (var chunkCoord in chunksToLoad)
        {
            yield return chunkManager.LoadChunk(chunkCoord);
        }
    }
}
