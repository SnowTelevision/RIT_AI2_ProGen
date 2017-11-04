using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PerlinTest : MonoBehaviour
{

    //EPISODE 2



    [Header("PREFRENCES")]
    public bool Randomized = true;      //BOOL NOT IN TUTORIAL {Do you want the terrain generated each time?}
    public bool Animate = true;         //BOOL NOT IN TUTORIAL {Do you want it to be animated and move?
    public float Speed = 5;             //FLOAT NOT IN TUTORIAL {If animating, how fast?)
    //if Animate is true, it works with colliders (add a rigid body to a cube and place the cube above terrain)

    [Space]

    public int depth;  //height from above

    public int width;     //make a int named width and set it to a default of 256
    public int height;    //make int named height and set it to a default of 256 [Length of terrain]

    public float Scale;

    public float offsetX;
    public float offsetY;

    public float[,] map; // Storing the heightmap data

    public float fbmWeight; // How much the fbm going to influence the perlin
    public int octaves;
    public float gain;
    public float lacunarity;

    public Vector2 randomVector2D;

    public void Start()
    {
        offsetX = Random.Range(0, 99999);
        offsetY = Random.Range(0, 99999);

        Terrain terrain = GetComponent<Terrain>();      //for Terrain Data
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        map = new float[width, height];

        map = GenerateHeights();

        randomVector2D = new Vector2(BetterRandom.betterRandom(-100000, 100000) / 1000f, BetterRandom.betterRandom(-100000, 100000) / 1000f) * (BetterRandom.betterRandom(0, 100000000) / 1000f);
    }


    void Update()
    {
        Terrain terrain = GetComponent<Terrain>();      //for Terrain Data
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        if (Animate == true)
        {
            offsetX += Time.deltaTime * Speed;
        }
    }


    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width;

        terrainData.size = new Vector3(width, depth, height);

        terrainData.SetHeights(0, 0, GenerateHeights());
        //terrainData.SetHeights(0, 0, GetComponent<ProcedureArray2D>().map);
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);      //generate some perlin noise value
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCord = (float)x / width * Scale;
        float yCord = (float)y / height * Scale;

        if (Randomized == true)
        {
            xCord += offsetX;
            yCord += offsetY;
        }

        Vector2 coord = new Vector2(xCord, yCord);

        return FBM2D.improvedFBM(coord, octaves, gain, lacunarity, randomVector2D) * fbmWeight + Mathf.PerlinNoise(xCord, yCord) * (1 - fbmWeight);
    }
}
