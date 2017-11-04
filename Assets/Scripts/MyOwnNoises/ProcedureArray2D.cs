using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ProcedureArray2D : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>

    public int actualTerrainSize;
    public int depth; // Maximum height of the terrain
    public int smoothMapSize;
    public int smoothFactor;

    public float h1Amplitude; // The amplitude for horizontal wave with frequency 1
    public float h2Amplitude; // The amplitude for horizontal wave with frequency 2
    public float h4Amplitude; // The amplitude for horizontal wave with frequency 4
    public float h8Amplitude; // The amplitude for horizontal wave with frequency 8
    public float h16Amplitude; // The amplitude for horizontal wave with frequency 16
    public float h32Amplitude; // The amplitude for horizontal wave with frequency 32
    public float v1Amplitude; // The amplitude for vertical wave with frequency 1
    public float v2Amplitude; // The amplitude for vertical wave with frequency 2
    public float v4Amplitude; // The amplitude for vertical wave with frequency 4
    public float v8Amplitude; // The amplitude for vertical wave with frequency 8
    public float v16Amplitude; // The amplitude for vertical wave with frequency 16
    public float v32Amplitude; // The amplitude for vertical wave with frequency 32

    public float[] hFrequencies; // The array for horizontal frequencies
    public float[] hAmplitudes; // The array for horizontal amplitudes
    public float[] vFrequencies; // The array for vertical frequencies
    public float[] vAmplitudes; // The array for vertical amplitudes
    public float[,] map; // The output map
    public float[,] smoothMap; // The smoothed map
    public Terrain terrain; // The terrain to be modified
    public float heightScale; // Maximum height of the combined wave, the values in height map need to be divided by this number

    // Use this for initialization
    void Start()
    {
        hFrequencies = new float[] { 1, 2, 4, 8, 16, 32 };
        vFrequencies = new float[] { 1, 2, 4, 8, 16, 32 };
        hAmplitudes = new float[] { h1Amplitude, h2Amplitude, h4Amplitude, h8Amplitude, h16Amplitude, h32Amplitude };
        vAmplitudes = new float[] { v1Amplitude, v2Amplitude, v4Amplitude, v8Amplitude, v16Amplitude, v32Amplitude };

        smoothMapSize = actualTerrainSize * smoothFactor - smoothFactor + 1;

        map = new float[actualTerrainSize, actualTerrainSize];
        smoothMap = new float[smoothMapSize, smoothMapSize];

        GenerateMap();

        //string raw = "";
        //for (int y = 0; y < actualTerrainSize; y++)
        //{

        //    for (int x = 0; x < actualTerrainSize; x++)
        //    {
        //        raw += map[x, y].ToString("F2") + " ";
        //    }
        //    raw += '\n';
        //}

        //File.WriteAllText("C:\\UnityProjects\\AI2\\TestOutput\\wave.txt", raw);

        SmoothMap();

        //raw = "";
        //for (int y = 0; y < actualTerrainSize; y++)
        //{

        //    for (int x = 0; x < actualTerrainSize; x++)
        //    {
        //        raw += map[x, y].ToString("F2") + " ";
        //    }
        //    raw += '\n';
        //}

        //File.WriteAllText("C:\\UnityProjects\\AI2\\TestOutput\\smooth.txt", raw);
    }

    // Update is called once per frame
    void Update()
    {
        //terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    public TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = actualTerrainSize + 1;

        terrainData.size = new Vector3(actualTerrainSize, depth, actualTerrainSize);
        terrainData.SetHeights(0, 0, map);

        //terrainData.heightmapResolution = smoothMapSize + 1;

        //terrainData.size = new Vector3(smoothMapSize, depth, smoothMapSize);
        //terrainData.SetHeights(0, 0, smoothMap);
        return terrainData;
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
        for (int i = 0; i < actualTerrainSize; i++) // Fill in vertical arrays
        {
            float[] newCombinedWave = CombinedWave(vFrequencies, vAmplitudes);

            for (int k = 0; k < actualTerrainSize; k++)
            {
                map[i, k] += newCombinedWave[k];
            }
        }

        for (int i = 0; i < actualTerrainSize; i++) // Fill in horizontal arrays
        {
            float[] newCombinedWave = CombinedWave(hFrequencies, hAmplitudes);

            for (int k = 0; k < actualTerrainSize; k++)
            {
                map[k, i] += newCombinedWave[k];
            }
        }

        for (int i = 0; i < hFrequencies.Length; i++) // Calculating the scale of the height map
        {
            heightScale += hAmplitudes[i];
            heightScale += vAmplitudes[i];
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
                map[k, i] = Mathf.Abs(map[k,i]);
            }
        }
    }

    public float[] CombinedWave(float[] frequencies, float[] amplitudes) // Combining waves of different frequencies
    {
        float[] newCombinedWave = new float[actualTerrainSize];

        for (int i = 0; i < frequencies.Length; i++)
        {
            float[] newWave = Noise(frequencies[i]); // For each frequency generate its array

            for (int k = 0; k < actualTerrainSize; k++)
            {
                newCombinedWave[k] += amplitudes[i] * newWave[k]; // Adding the current array with its amplitude to the combined result
            }
        }

        return newCombinedWave;
    }

    public float[] Noise(float frequency) // Generate a shifted sine wave 
    {
        float[] newWave = new float[actualTerrainSize];
        float phaseShift = BetterRandom.betterRandom(0, Mathf.RoundToInt(20000000 * Mathf.PI)) / 10000000f;

        for (int i = 0; i < map.GetLength(0); i++)
        {
            newWave[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / actualTerrainSize + phaseShift);
        }

        return newWave;
    }
}
