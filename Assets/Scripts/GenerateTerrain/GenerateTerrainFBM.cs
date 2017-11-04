using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrainFBM : MonoBehaviour
{
    public float waveWeight; // How much the wave height map will change the base perlin map
    public float lineWeight;
    public int terrainSize;
    public int terrainDepth;
    public int depth;

    public SineWavesArray2D waveMap;
    public ProcedureArray2D arrayMap;
    public FBMTest fbmMap;
    public Terrain terrain; // The terrain to be modified

    // Use this for initialization
    void Start()
    {
        waveMap = GetComponent<SineWavesArray2D>();
        arrayMap = GetComponent<ProcedureArray2D>();
        fbmMap = GetComponent<FBMTest>();
        terrain = GetComponent<Terrain>();

        terrainDepth = depth;
        terrainSize = waveMap.actualTerrainSize;

        terrain.terrainData = MakeTerrain(terrain.terrainData);
    }

    // Update is called once per frame
    void Update()
    {
        terrain.terrainData = MakeTerrain(terrain.terrainData);
    }

    public TerrainData MakeTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = terrainSize + 1;

        terrainData.size = new Vector3(terrainSize, terrainDepth, terrainSize);
        terrainData.SetHeights(0, 0, combineMap());

        return terrainData;
    }

    public float[,] combineMap()
    {
        float[,] combinedMap = new float[terrainSize, terrainSize];

        for (int i = 0; i < terrainSize; i++)
        {
            for (int j = 0; j < terrainSize; j++)
            {
                combinedMap[i, j] = waveMap.map[i, j] * waveWeight * (1 - lineWeight) + arrayMap.map[i, j] * waveWeight * lineWeight + fbmMap.map[i, j] * (1 - waveWeight);
            }
        }

        return combinedMap;
    }
}
