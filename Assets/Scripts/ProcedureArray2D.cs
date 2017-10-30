using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ProcedureArray2D : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>

    public int width;
    public int height; // Length of the terrain
    public int depth; // Maximum height of the terrain

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
    public Terrain terrain; // The terrain to be modified

    // Use this for initialization
    void Start()
    {
        hFrequencies = new float[] { 1, 2, 4, 8, 16, 32 };
        vFrequencies = new float[] { 1, 2, 4, 8, 16, 32 };
        hAmplitudes = new float[] { h1Amplitude, h2Amplitude, h4Amplitude, h8Amplitude, h16Amplitude, h32Amplitude };
        vAmplitudes = new float[] { v1Amplitude, v2Amplitude, v4Amplitude, v8Amplitude, v16Amplitude, v32Amplitude };

        map = new float[128, 128];

        GenerateMap();

        //string raw = "";
        //for (int y = 0; y < width; y++)
        //{

        //    for (int x = 0; x < width; x++)
        //    {
        //        raw += map[x, y].ToString("F2") + " ";
        //    }
        //    raw += '\n';
        //}

        //File.WriteAllText("C:\\UnityProjects\\AI2\\TestOutput\\wave.txt", raw);
    }

    // Update is called once per frame
    void Update()
    {
        //terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    public TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = map.GetLength(0);
        //terrainData.heightmapResolution = width + 1;

        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, map);
        return terrainData;
    }

    public void SmoothMap() // Lerp values so the map become smoother
    {

    }

    public void GenerateMap()
    {
        for (int i = 0; i < map.GetLength(0); i++) // Fill in vertical arrays
        {
            float[] newCombinedWave = CombinedWave(vFrequencies, vAmplitudes);

            for (int k = 0; k < map.GetLength(0); k++)
            {
                map[i, k] += newCombinedWave[k];
            }
        }

        for (int i = 0; i < map.GetLength(0); i++) // Fill in horizontal arrays
        {
            float[] newCombinedWave = CombinedWave(vFrequencies, vAmplitudes);

            for (int k = 0; k < map.GetLength(0); k++)
            {
                map[k, i] += newCombinedWave[k];
            }
        }

        for (int i = 0; i < map.GetLength(0); i++) // Normalize height map data to a scale of 1
        {
            for (int k = 0; k < map.GetLength(0); k++)
            {
                map[k, i] /= 6f;
            }
        }
    }

    public float[] CombinedWave(float[] frequencies, float[] amplitudes) // Combining waves of different frequencies
    {
        float[] newCombinedWave = new float[map.GetLength(0)];

        for (int i = 0; i < frequencies.Length; i++)
        {
            float[] newWave = Noise(frequencies[i]); // For each frequency generate its array

            for (int k = 0; k < map.GetLength(0); k++)
            {
                newCombinedWave[k] += amplitudes[i] * newWave[k]; // Adding the current array with its amplitude to the combined result
            }
        }

        return newCombinedWave;
    }

    public float[] Noise(float frequency) // Generate a shifted sine wave 
    {
        float[] newWave = new float[map.GetLength(0)];
        float phaseShift = BetterRandom.betterRandom(0, Mathf.RoundToInt(10000000 * Mathf.PI)) / 10000000f;

        for (int i = 0; i < map.GetLength(0); i++)
        {
            newWave[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / map.GetLength(0) + phaseShift);
        }

        return newWave;
    }
}
