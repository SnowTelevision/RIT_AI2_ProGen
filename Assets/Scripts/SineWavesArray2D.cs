﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineWavesArray2D : MonoBehaviour
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

    public float[] frequencies; // The array for frequencies
    public float[] amplitudes; // The array for amplitudes
    public float[,] map; // The output map
    public float[,] smoothMap; // The smoothed map
    public float heightScale; // Maximum height of the combined wave, the values in height map need to be divided by this number

    // Use this for initialization
    void Start()
    {
        frequencies = new float[] { 1, 2, 4, 8, 16, 32 };
        amplitudes = new float[] { s1Amplitude, s2Amplitude, s4Amplitude, s8Amplitude, s16Amplitude, s32Amplitude };

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
            float phaseShift = BetterRandom.betterRandom(0, Mathf.RoundToInt(20000000 * Mathf.PI)) / 10000000f;
            int centerI = BetterRandom.betterRandom(0, actualTerrainSize - 1);
            int centerJ = BetterRandom.betterRandom(0, actualTerrainSize - 1);

            for (int i = 0; i < actualTerrainSize; i++)
            {
                for (int j = 0; j < actualTerrainSize; j++)
                {
                    map[i,j] += Mathf.Sin(Mathf.Pow(Mathf.Pow(2 * Mathf.PI * frequencies[f] * (i - centerI) / actualTerrainSize + phaseShift, 2) +
                                                    Mathf.Pow(2 * Mathf.PI * frequencies[f] * (j - centerJ) / actualTerrainSize + phaseShift, 2), 0.5f)) * amplitudes[f];
                }
            }
        }

        for (int i = 0; i < frequencies.Length; i++) // Calculating the scale of the height map
        {
            heightScale += amplitudes[i];
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
