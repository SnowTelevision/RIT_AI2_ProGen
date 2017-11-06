using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class DomainWarpingFBMTest : MonoBehaviour
{
    public bool useAlone; // Does this script runs alone or used to combined with other noise
    public int depth;  // height from above
    public int width;     // make a int named width and set it to a default of 256
    public int height;    // make int named height and set it to a default of 256 [Length of terrain]
    public float Scale;
    public int octaves; // How many octaves to be combined
    public float gain; // Percentage of height with a scale from 0-1 (if the value is larger than 1 then the area larger than 1 will be cut to flat
    public float lacunarity; // similar to scale
    public int warpings; // How many times do we apply domain warping
    public float factor; // This is used to multiply the last fbm vector2 result when input to the new fbm

    public bool randomStartCoord;      // Do you want a random part of the terrain?
    public bool updateRealtime; // Will the terrain update in the runtime as the values in the inspector changes
    public bool animate;         // Do you want the terrain to be animated and move?
    public float animateSpeed;          // If animating, how fast?

    public Terrain terrain;

    public float offsetX;
    public float offsetY;

    public Vector2 randomVector2D;
    public float randomSinMulti; // This number will be multiplied by the sine value
    public Vector2[] randomWarpStartCoords; // Random start coorinations for each warping

    public float[,] map; // Storing the heightmap data (for the noise combination)
    //public Vector2 coord;
    public DomainWarpingFBM domainWarpingFBM;

    public void Start()
    {
        offsetX = Random.Range(0, 99999);
        offsetY = Random.Range(0, 99999);

        do
        {
            randomVector2D = new Vector2(BetterRandom.betterRandom(-10000000, 10000000) / 100000f, BetterRandom.betterRandom(-10000000, 10000000) / 100000f);
        } while (randomVector2D.x == 0 || randomVector2D.y == 0 || randomVector2D.x == randomVector2D.y);

        randomSinMulti = BetterRandom.betterRandom(10000, 100000) + (BetterRandom.betterRandom(1000000, 10000000) / 10000000f);

        randomWarpStartCoords = new Vector2[20];
        for (int i = 0; i < 10; i++) // Generate random start coorinations for each warping
        {
            randomWarpStartCoords[i] = new Vector2(BetterRandom.betterRandom(0, 1000) / 100f, BetterRandom.betterRandom(0, 1000) / 100f);
            randomWarpStartCoords[i + 10] = new Vector2(BetterRandom.betterRandom(0, 1000) / 100f, BetterRandom.betterRandom(0, 1000) / 100f);
        }

        terrain = GetComponent<Terrain>();      //for Terrain Data
        terrain.terrainData.heightmapResolution = width;
        terrain.terrainData.alphamapResolution = width;
        terrain.terrainData.size = new Vector3(width, depth, height);

        map = new float[width, height]; // Storing the heightmap data (for the noise combination)
        if (useAlone)
        {
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
        }
        else if (!useAlone)
        {
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
            map = GenerateHeights(); // Storing the heightmap data (for the noise combination)
        }

        domainWarpingFBM = GetComponent<DomainWarpingFBM>();
    }


    void Update()
    {
        if (useAlone)
        {
            if (updateRealtime)
            {
                GenerateTerrain(terrain.terrainData);
                //GenerateHeights();

                if (animate == true)
                {
                    offsetX += Time.deltaTime * animateSpeed;
                }
            }
        }
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
    }

    public float[,] GenerateHeights()
    {
        Parallel.For(0, width, x =>
        {
            Parallel.For(0, height, y =>
            {
                map[x, y] = CalculateHeight(x, y);      //generate some perlin noise value
                //Interlocked.Exchange(ref map[x, y], CalculateHeight(x, y));
            });
        });

        //map[0, width - 1] = 1;
        //map[1, width - 1] = 1;
        //map[0, width - 2] = 1;
        //map[0, width - 2] = 1;
        //for (int x = 0; x < width; x++)
        //{
        //    for (int y = 0; y < height; y++)
        //    {
        //        map[x, y] = CalculateHeight(x, y);      //generate some perlin noise value
        //    }
        //}

        return map;
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

        //Vector2 coord = Vector2.zero;
        //Interlocked.Exchange(ref coord.x, xCord);
        //Interlocked.Exchange(ref coord.y, yCord);

        Vector2 coord;
        coord.x = xCord;
        coord.y = yCord;

        return domainWarpingFBM.improvedFBMwarping(warpings, factor, coord, octaves, gain, lacunarity, randomVector2D, randomSinMulti, randomWarpStartCoords);
    }
}
