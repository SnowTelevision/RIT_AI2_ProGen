using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineWavesDomainWarping : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>

    public int actualTerrainSize;
    public int smoothMapSize;
    public int smoothFactor;

    public float s1Amplitude; // The amplitude for wave with frequency 1
    public float s2Amplitude; // The amplitude for wave with frequency 2
    public float s4Amplitude; // The amplitude for wave with frequency 4
    public float s8Amplitude; // The amplitude for wave with frequency 8
    public float s16Amplitude; // The amplitude for wave with frequency 16
    public float s32Amplitude; // The amplitude for wave with frequency 32
    public int s1Count; // The count for wave with frequency 1
    public int s2Count; // The count for wave with frequency 2
    public int s4Count; // The count for wave with frequency 4
    public int s8Count; // The count for wave with frequency 8
    public int s16Count; // The count for wave with frequency 16
    public int s32Count; // The count for wave with frequency 32
    public int s1Warp; // The warps for wave with frequency 1
    public int s2Warp; // The warps for wave with frequency 2
    public int s4Warp; // The warps for wave with frequency 4
    public int s8Warp; // The warps for wave with frequency 8
    public int s16Warp; // The warps for wave with frequency 16
    public int s32Warp; // The warps for wave with frequency 32

    public float[] frequencies; // The array for frequencies
    public float[] amplitudes; // The array for amplitudes
    public int[] counts; // The counts for how many 2D sine waves for each frequency
    public int[] warps; // The counts for how many time each 2D sine wave will warp for each frequency
    public float[,] map; // The output map
    public float[,] smoothMap; // The smoothed map
    public float heightScale; // Maximum height of the combined wave, the values in height map need to be divided by this number

    // Use this for initialization
    void Start()
    {
        frequencies = new float[] { 1, 2, 4, 8, 16, 32 };
        amplitudes = new float[] { s1Amplitude, s2Amplitude, s4Amplitude, s8Amplitude, s16Amplitude, s32Amplitude };
        counts = new int[] { s1Count, s2Count, s4Count, s8Count, s16Count, s32Count };
        warps = new int[] { s1Warp, s2Warp, s4Warp, s8Warp, s16Warp, s32Warp };

        smoothMapSize = actualTerrainSize * smoothFactor - smoothFactor + 1;

        map = new float[actualTerrainSize, actualTerrainSize];
        smoothMap = new float[smoothMapSize, smoothMapSize];

        GenerateMap();
        SmoothMap();
    }

    // Update is called once per frame
    void Update()
    {
        //GetComponent<Terrain>().terrainData.heightmapResolution = actualTerrainSize + 1;
        //GetComponent<Terrain>().terrainData.size = new Vector3(actualTerrainSize, 20, actualTerrainSize);
        //GetComponent<Terrain>().terrainData.SetHeights(0, 0, map);
    }

    public void SmoothMap() // Lerp values so the map become smoother
    {
        for (int i = 0; i < actualTerrainSize; i++) // Smooth one direction (vertical?)
        {
            for (int k = 0; k < actualTerrainSize - 1; k++)
            {
                for (int j = 0; j < smoothFactor; j++)
                {
                    smoothMap[i * smoothFactor, k * smoothFactor + j] = map[i, k] + (map[i, k + 1] - map[i, k]) *
                                                                        (6 * Mathf.Pow(((float)j / (float)smoothFactor), 5) - 15 * Mathf.Pow(((float)j / (float)smoothFactor), 4) + 10 * Mathf.Pow(((float)j / (float)smoothFactor), 3)); // Perlin's improved fade function
                }

                if (k == actualTerrainSize - 1) // Filling the last column
                {
                    smoothMap[i * smoothFactor, k * smoothFactor + smoothFactor] = map[i, k + 1];
                }
            }
        }

        for (int i = 0; i < actualTerrainSize - 1; i++) // Smooth the other direction (horizontal?)
        {
            for (int j = 0; j < smoothFactor; j++)
            {
                for (int m = 0; m < actualTerrainSize - 1; m++)
                {
                    for (int n = 0; n < smoothFactor; n++)
                    {
                        smoothMap[m * smoothFactor + n, i * smoothFactor + j] = smoothMap[m * smoothFactor, i * smoothFactor + j] + (smoothMap[m * smoothFactor + smoothFactor, i * smoothFactor + j] - smoothMap[m * smoothFactor, i * smoothFactor + j]) *
                                                                                (6 * Mathf.Pow(((float)n / (float)smoothFactor), 5) - 15 * Mathf.Pow(((float)n / (float)smoothFactor), 4) + 10 * Mathf.Pow(((float)n / (float)smoothFactor), 3)); // Perlin's improved fade function
                    }
                }
            }

            if (i == actualTerrainSize - 1) // Filling the last row
            {
                for (int m = 0; m < actualTerrainSize - 1; m++)
                {
                    for (int n = 0; n < smoothFactor; n++)
                    {
                        smoothMap[m * smoothFactor + n, i * smoothFactor + smoothFactor] = smoothMap[m * smoothFactor, i * smoothFactor + smoothFactor] + (smoothMap[m * smoothFactor + smoothFactor, i * smoothFactor + smoothFactor] - smoothMap[m * smoothFactor, i * smoothFactor + smoothFactor]) *
                                                                                           (6 * Mathf.Pow(((float)n / (float)smoothFactor), 5) - 15 * Mathf.Pow(((float)n / (float)smoothFactor), 4) + 10 * Mathf.Pow(((float)n / (float)smoothFactor), 3)); // Perlin's improved fade function
                    }
                }
            }
        }

        for (int i = 0; i < actualTerrainSize; i++)
        {
            for (int j = 0; j < actualTerrainSize; j++)
            {
                map[i, j] = smoothMap[i, j];

                if (map[i, j] < 0)
                {
                    map[i, j] = 0;
                }
            }
        }
    }

    public void GenerateMap()
    {
        for (int f = 0; f < frequencies.Length; f++)
        {
            for (int c = 0; c < counts[f]; c++)
            {
                float phaseShift = BetterRandom.betterRandom(0, Mathf.RoundToInt(20000000 * Mathf.PI)) / 10000000f;
                int centerI = BetterRandom.betterRandom(0, actualTerrainSize - 1);
                int centerJ = BetterRandom.betterRandom(0, actualTerrainSize - 1);

                for (int i = 0; i < actualTerrainSize; i++)
                {
                    for (int j = 0; j < actualTerrainSize; j++)
                    {
                        map[i, j] += (Mathf.Sin(Mathf.Pow(Mathf.Pow(2 * Mathf.PI * frequencies[f] * (i - centerI) / actualTerrainSize + phaseShift, 2) +
                                                          Mathf.Pow(2 * Mathf.PI * frequencies[f] * (j - centerJ) / actualTerrainSize + phaseShift, 2), 0.5f)) * amplitudes[f]) / (float)counts[f];
                    }
                }
            }
        }

        for (int i = 0; i < frequencies.Length; i++) // Calculating the scale of the height map
        {
            heightScale += amplitudes[i];// * counts[i];
        }

        for (int i = 0; i < actualTerrainSize; i++) // Normalize height map data to a scale of 1
        {
            for (int k = 0; k < actualTerrainSize; k++)
            {
                map[k, i] /= heightScale;
            }
        }

        for (int i = 0; i < actualTerrainSize; i++) // Convert negative value to positive
        {
            for (int k = 0; k < actualTerrainSize; k++)
            {
                map[k, i] = Mathf.Abs(map[k, i]);
            }
        }
    }

    public float WarpingFunction(float x, int warpCount)
    {
        float y;

        y = 0;

        return y;
    }
}
