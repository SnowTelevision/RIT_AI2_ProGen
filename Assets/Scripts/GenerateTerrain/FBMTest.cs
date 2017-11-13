using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBMTest : MonoBehaviour
{
    public bool useAlone; // Does this script runs alone or used to combined with other noise
    public int depth;  // height from above
    public int width;     // make a int named width and set it to a default of 256
    public int height;    // make int named height and set it to a default of 256 [Length of terrain]
    public float Scale;
    public int octaves; // How many octaves to be combined
    public float gain; // Percentage of height with a scale from 0-1 (if the value is larger than 1 then the area larger than 1 will be cut to flat
    public float lacunarity; // similar to scale

    public bool randomStartCoord;      // Do you want a random part of the terrain?
    public bool updateRealtime; // Will the terrain update in the runtime as the values in the inspector changes
    public bool animate;         // Do you want the terrain to be animated and move?
    public float animateSpeed;          // If animating, how fast?

    public float offsetX;
    public float offsetY;

    public Vector2 randomVector2D;
    public float randomSinMulti; // This number will be multiplied by the sine value

    public float[,] map; // Storing the heightmap data (for the noise combination)

    public void Start()
    {
        offsetX = Random.Range(0, 99999);
        offsetY = Random.Range(0, 99999);

        do
        {
            randomVector2D = new Vector2(BetterRandom.betterRandom(-10000000, 10000000) / 100000f, BetterRandom.betterRandom(-10000000, 10000000) / 100000f);
        } while (randomVector2D.x == 0 || randomVector2D.y == 0 || randomVector2D.x == randomVector2D.y);

        randomSinMulti = BetterRandom.betterRandom(10000, 100000) + (BetterRandom.betterRandom(1000000, 10000000) / 10000000f);

        if (!useAlone)
        {
            map = new float[width, height]; // Storing the heightmap data (for the noise combination)
            map = GenerateHeights(); // Storing the heightmap data (for the noise combination)

            Terrain terrain = GetComponent<Terrain>();      //for Terrain Data
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
        }
    }


    void Update()
    {
        if (useAlone)
        {
            if (updateRealtime)
            {
                Terrain terrain = GetComponent<Terrain>();      //for Terrain Data
                terrain.terrainData = GenerateTerrain(terrain.terrainData);

                if (animate == true)
                {
                    offsetX += Time.deltaTime * animateSpeed;
                }
            }
        }
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width;

        terrainData.size = new Vector3(width, depth, height);

        terrainData.SetHeights(0, 0, GenerateHeights());

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

        if (randomStartCoord == true)
        {
            xCord += offsetX;
            yCord += offsetY;
        }

        Vector2 coord = new Vector2(xCord, yCord);

        return FindObjectOfType<FBM2D>().improvedFBM(coord, octaves, gain, lacunarity, new Vector2(), randomSinMulti);
    }
}
