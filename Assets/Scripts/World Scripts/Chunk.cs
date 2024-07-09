using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;
    private bool isLoaded = false; 
    private float[,] heightmap;
    public float temperatureScale = 0.25f;
    public float erosionScale = 0.25f;
    private float [,] temperatureMap;
    private float [,] erosionMap;
    private static float temperatureOffsetX;
    private static float temperatureOffsetY;
    private static float erosionOffsetX;
    private static float erosionOffsetY;
    public void Init(int chunkSize, Vector3 position)
    {
        this.chunkSize = chunkSize;
        transform.position = position;
        gameObject.layer = LayerMask.NameToLayer("Ground");
        GenerateHeightmap();
        GenerateTempMap();
        GenerateErosionMap();
    }
    public void setParams(float tOffsetX, float tOffsetY, float eOffsetX, float eOffsetY)
    {
        temperatureOffsetX = tOffsetX;
        temperatureOffsetY = tOffsetY;
        erosionOffsetX = eOffsetX;
        erosionOffsetY = eOffsetY;
    } 
    // Load the chunk (e.g., generating terrain data, creating the mesh, etc.)
    public void Load(string chunkPath)
    {
        if (isLoaded)
            return;

        if (File.Exists(chunkPath))
        {
            LoadHeightmap(chunkPath);
        }
        else
        {
            GenerateHeightmap();
            GenerateTempMap();
            GenerateErosionMap();
        }

        GenerateMesh();
        isLoaded = true;
    }

    // Unload the chunk (e.g., destroying the mesh, freeing up resources, etc.)
    public void Unload(string chunkPath)
    {
        if (!isLoaded)
            return;

        SaveHeightmap(chunkPath);

        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());

        isLoaded = false;
    }

    void OnDrawGizmos()
    {
        if (temperatureMap == null)
        {
            GenerateTempMap();
        }
        if (erosionMap == null)
        {
            GenerateErosionMap();
        }
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireCube(transform.position + new Vector3(chunkSize / 2f, 0, chunkSize / 2f), new Vector3(chunkSize, 0, chunkSize));
        for (int y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float temp = temperatureMap[x, y];
                float erosion = erosionMap[x, y];
                Gizmos.color = new Color (temp, erosion, 1-temp);
                Gizmos.DrawCube(transform.position + new Vector3(x, 15, y), new Vector3(1, 1, 1));
            }
        }
    }

    Color GetBiomeColor(){
        float temp = calcAverage(temperatureMap);
        float erosion = calcAverage(erosionMap);
        Debug.Log($"temp: {temp} and erosion {erosion}");
        Color color = Color.black;
        if (temp >= 0.60f)
        {
            if(erosion >= 0.5f)
                color = Color.yellow;
            else
                color = Color.grey;
        }
        if (temp >= 0.4f)
        {
            if(erosion >= 0.6f)
                color = new Color32(102, 235, 61, 255);
            if(erosion >= 0.4f)
                color = new Color32(35, 133, 5, 255);
            else
                color = Color.cyan;
        }
        else
        {
            if(erosion >= 0.5f)
                color = Color.white;
            else
                color = Color.blue;
        }
        return color;
    }
    float calcAverage(float[,] arr)
    {
        float sum = 0;
        float count = 0;
        foreach(float value in arr)
        {
            sum += value;
            count++;
        }
        return sum / count;
    }
    void GenerateHeightmap()
    {
        heightmap = new float[chunkSize + 1, chunkSize + 1];
        for (int y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                heightmap[x, y] = Mathf.PerlinNoise((x + transform.position.x) / chunkSize, (y + transform.position.z) / chunkSize) * 10f;
            }
        }
    }
    void GenerateTempMap()
    {
        temperatureMap = new float[chunkSize + 1, chunkSize + 1];
        for (int y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float xCoord = (x + transform.position.x + temperatureOffsetX) / chunkSize * temperatureScale;
                float yCoord = (y + transform.position.z + temperatureOffsetY) / chunkSize * temperatureScale;
                temperatureMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
                Debug.Log($"Generating erosion with ofsets: {temperatureOffsetX} and {temperatureOffsetY}");
            }
        }
    }

    void GenerateErosionMap()
    {
        erosionMap = new float[chunkSize + 1, chunkSize + 1];
        for (int y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float xCoord = (x + transform.position.x + erosionOffsetX) / chunkSize * erosionScale;
                float yCoord = (y + transform.position.z + erosionOffsetY) / chunkSize * erosionScale;
                erosionMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
                Debug.Log($"Generating erosion with ofsets: {erosionOffsetX} and {erosionOffsetY}");
            }
        }
    }
    void GenerateMesh()
    {
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = GetBiomeColor();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
        int[] triangles = new int[chunkSize * chunkSize * 6];

        int vert = 0;
        int tris = 0;
        for (int y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                vertices[vert] = new Vector3(x, heightmap[x, y], y);
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
    }

    void SaveHeightmap(string path)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(path);
        bf.Serialize(file, heightmap);
        file.Close();
    }

    void LoadHeightmap(string path)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(path, FileMode.Open);
        heightmap = (float[,])bf.Deserialize(file);
        file.Close();
    }
}
