using System.IO;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class Chunk : MonoBehaviour
{
    public int chunkSize = 16;
    private bool isLoaded = false; 
    private float[,] heightmap;
    public float temperatureScale = 4.0f;
    public float erosionScale = 4.0f;
    private float [,] temperatureMap;
    private float [,] erosionMap;
    private static float temperatureOffsetX;
    private static float temperatureOffsetY;
    private static float erosionOffsetX;
    private static float erosionOffsetY;
    private static float heightOffsetX;
    private static float heightOffsetY;

    public void Init(int chunkSize, UnityEngine.Vector3 position)
    {
        this.chunkSize = chunkSize;
        transform.position = position;
        gameObject.layer = LayerMask.NameToLayer("Ground");
        GenerateHeightmap();
        GenerateTempMap();
        GenerateErosionMap();
    }

    public void setParams(float tOffsetX, float tOffsetY, float eOffsetX, float eOffsetY, float hOffsetX, float hOffsetY)
    {
        temperatureOffsetX = tOffsetX;
        temperatureOffsetY = tOffsetY;
        erosionOffsetX = eOffsetX;
        erosionOffsetY = eOffsetY;
        heightOffsetX = hOffsetX;
        heightOffsetY = hOffsetY;
    } 
    
    // Load the chunk (e.g., generating terrain data, creating the mesh, etc.)
    public void Load(string chunkPath)
    {
        GenerateTempMap();
        GenerateErosionMap();
        GenerateHeightmap();
        GenerateMesh();
        isLoaded = true;
    }

    // Unload the chunk (e.g., destroying the mesh, freeing up resources, etc.)
    public void Unload(string chunkPath)
    {
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

        float temp = calcAverage(temperatureMap);
        float erosion = calcAverage(erosionMap);
        Gizmos.color = new Color (temp, 0, 1-temp);
        Gizmos.DrawCube(transform.position + new UnityEngine.Vector3(0,chunkSize/2,0), new UnityEngine.Vector3(chunkSize, chunkSize, chunkSize));
        Gizmos.color = new Color(0, erosion, 1-erosion);
        Gizmos.DrawWireCube(transform.position + new UnityEngine.Vector3(0,chunkSize,0), new UnityEngine.Vector3(chunkSize/8, chunkSize*2, chunkSize/8));
    }

    Color GetBiomeColor(){
        float temp = calcAverage(temperatureMap);
        float erosion = calcAverage(erosionMap);
        Color color = Color.black;
        if (temp >= 0.60f)
        {
            if(erosion >= 0.5f)
                color = Color.yellow; // desert
            else
                color = Color.grey; // volcano
        }
        else if (temp >= 0.4f)
        {
            if(erosion >= 0.6f)
                color = new Color32(102, 235, 61, 255); // meadows
            else if(erosion >= 0.4f)
                color = new Color32(35, 133, 5, 255); // forest
            else
                color = Color.cyan; // extreme hills
        }
        else
        {
            if(erosion >= 0.5f)
                color = Color.white; // snowy peaks
            else
                color = Color.blue; // glacier
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
                float firstOctave = 0;
                float secondOctave = 0;
                float thirdOctave = 0;
                float fourthOctave = 0;

                float finiteStep = 0.01f;
                float firstOctaveGradient = 0;
                float secondOctaveGradient = 0;
                float thirdOctaveGradient = 0;
                float fourthOctaveGradient = 0;

                float xCoord = 0;
                float yCoord = 0;
                
                xCoord = (x + transform.position.x + heightOffsetX) / chunkSize * (1.0f / 4.0f);
                yCoord = (y + transform.position.z + heightOffsetY) / chunkSize * (1.0f / 4.0f);
                firstOctave = Mathf.PerlinNoise(xCoord, yCoord);
                firstOctaveGradient = Mathf.Sqrt(Mathf.Pow((Mathf.PerlinNoise(xCoord+finiteStep,yCoord) - firstOctave)/finiteStep,2) + Mathf.Pow((Mathf.PerlinNoise(xCoord,yCoord+finiteStep) - firstOctave)/finiteStep,2));
                //firstOctave *= (1 / (1 + firstOctaveGradient));
                firstOctave *= Mathf.Exp(-Mathf.Pow(firstOctaveGradient,2));
                heightmap[x, y] += firstOctave * 32;

                xCoord = (x + transform.position.x + heightOffsetX) / chunkSize * (2.0f / 4.0f);
                yCoord = (y + transform.position.z + heightOffsetX) / chunkSize * (2.0f / 4.0f);
                secondOctave = Mathf.PerlinNoise(xCoord, yCoord);
                secondOctaveGradient = Mathf.Sqrt(Mathf.Pow((Mathf.PerlinNoise(xCoord+finiteStep,yCoord) - secondOctave)/finiteStep,2) + Mathf.Pow((Mathf.PerlinNoise(xCoord,yCoord+finiteStep) - secondOctave)/finiteStep,2));
                secondOctaveGradient += firstOctaveGradient;
                secondOctave *= (1 / (1 + secondOctaveGradient));
                heightmap[x, y] += secondOctave * 16;

                xCoord = (x + transform.position.x + heightOffsetY) / chunkSize * (4.0f / 4.0f);
                yCoord = (y + transform.position.z + heightOffsetY) / chunkSize * (4.0f / 4.0f);
                thirdOctave = Mathf.PerlinNoise(xCoord, yCoord);
                thirdOctaveGradient = Mathf.Sqrt(Mathf.Pow((Mathf.PerlinNoise(xCoord+finiteStep,yCoord) - thirdOctave)/finiteStep,2) + Mathf.Pow((Mathf.PerlinNoise(xCoord,yCoord+finiteStep) - thirdOctave)/finiteStep,2));
                thirdOctaveGradient += secondOctaveGradient;
                thirdOctave *= (1 / (1 + thirdOctaveGradient));
                heightmap[x, y] += thirdOctave * 8;
                
                xCoord = (x + transform.position.x + heightOffsetY) / chunkSize * (8.0f / 4.0f);
                yCoord = (y + transform.position.z + heightOffsetX) / chunkSize * (8.0f / 4.0f);
                fourthOctave = Mathf.PerlinNoise(xCoord, yCoord);
                fourthOctaveGradient = Mathf.Sqrt(Mathf.Pow((Mathf.PerlinNoise(xCoord+finiteStep,yCoord) - fourthOctave)/finiteStep,2) + Mathf.Pow((Mathf.PerlinNoise(xCoord,yCoord+finiteStep) - fourthOctave)/finiteStep,2));
                fourthOctaveGradient += thirdOctave;
                fourthOctave *= (1 / (1 + fourthOctaveGradient));
                heightmap[x, y] += fourthOctave * 4; 
 
                
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
                float xCoord = (x + transform.position.x + temperatureOffsetX) / chunkSize * (1/temperatureScale);
                float yCoord = (y + transform.position.z + temperatureOffsetY) / chunkSize * (1/temperatureScale);
                temperatureMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
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
                float xCoord = (x + transform.position.x + erosionOffsetX) / chunkSize * (1 / erosionScale);
                float yCoord = (y + transform.position.z + erosionOffsetY) / chunkSize * (1 / erosionScale);
                erosionMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
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
        UnityEngine.Vector3[] vertices = new UnityEngine.Vector3[(chunkSize + 1) * (chunkSize + 1)];
        int[] triangles = new int[chunkSize * chunkSize * 6];

        int vert = 0;
        int tris = 0;
        for (int y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                vertices[vert] = new UnityEngine.Vector3(x, heightmap[x, y], y);
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
        // ! BRING IT BACK
        //MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        //meshCollider.sharedMesh = mesh;
    }
}