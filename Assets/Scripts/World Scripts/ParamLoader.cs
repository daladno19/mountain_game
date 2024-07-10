using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEditor;
using UnityEngine;

public class ParamLoader : MonoBehaviour
{
    public int seed = 52;
    public float scale = 1_000_000.0f;
    public Chunk chunk;
    void Start()
    {
        Random.InitState(seed);
        float temperatureOffsetX = Random.value * scale;
        float temperatureOffsetY = Random.value * scale;
        float erosionOffsetX = Random.value * scale;
        float erosionOffsetY = Random.value * scale;
        float heightOffsetX = Random.value * scale;
        float heightOffsetY = Random.value * scale;
        chunk.setParams(temperatureOffsetX,
                        temperatureOffsetY,
                        erosionOffsetX,
                        erosionOffsetY,
                        heightOffsetX,
                        heightOffsetY
                        );
    }
}
