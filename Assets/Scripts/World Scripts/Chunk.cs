using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;
    private bool isLoaded = false; 

    // Initialize the chunk (e.g., setting up the mesh, noise parameters, etc.)
    public void Init(int chunkSize, Vector3 position)
    {
        this.chunkSize = chunkSize;
        transform.position = position;
        gameObject.layer = LayerMask.NameToLayer("Ground");
        // TODO migrate terrain generation here 
        Debug.Log($"Chunk initialized at {position}");
    }

    // Load the chunk (e.g., generating terrain data, creating the mesh, etc.)
    public void Load()
    {
        if (isLoaded)
            return;

        // TODO read pre loaded terrain from save file
        // For now, create a simple plane as a placeholder

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        meshRenderer.material.color = GetRandomGrayscaleColor();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
        int[] triangles = new int[chunkSize * chunkSize * 6];

        int vert = 0;
        int tris = 0;
        for (int y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                vertices[vert] = new Vector3(x, 0, y);
                if (x < chunkSize && y < chunkSize)
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + chunkSize + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + chunkSize + 1;
                    triangles[tris + 5] = vert + chunkSize + 2;
                    tris += 6;
                }
                vert++;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        isLoaded = true;
        Debug.Log("Chunk loaded");
    }

    // Unload the chunk (e.g., destroying the mesh, freeing up resources, etc.)
    public void Unload()
    {
        if (!isLoaded)
            return;

        // TODO save terrain in a file, to read it next time, instead of generating
        // For now, destroy the mesh components

        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());

        isLoaded = false;
        Debug.Log("Chunk unloaded");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3(chunkSize / 2f, 0, chunkSize / 2f), new Vector3(chunkSize, 0, chunkSize));
    }

    Color GetRandomGrayscaleColor(){
        float grayValue = Random.Range(0f, 1f);
        return new Color(grayValue, grayValue, grayValue);
    }
}
