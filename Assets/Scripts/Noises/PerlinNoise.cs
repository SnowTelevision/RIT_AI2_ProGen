using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public float improvedPerlinFBMwarping(int warps, float factor, Vector2 coord, Vector2[,] grad, Vector2[] randomWarpStartCoords, int perlins, float gain, float lacunarity)
    {
        float warpedValue = improvedPerlinFBM(coord, grad, perlins, gain, lacunarity);
        Vector2 originalCoord = coord; // The unwarpped coordinate
        Vector2 lastCoord = Vector2.zero; // The coordinate of last warping

        for (int i = 0; i < warps; i++)
        {
            //randomWarpStartCoord = new Vector2(BetterRandom.betterRandom(0, 1000) / 100f, BetterRandom.betterRandom(0, 1000) / 100f);

            coord.x = improvedPerlinFBM(originalCoord + lastCoord * factor + randomWarpStartCoords[i], grad, perlins, gain, lacunarity);
            coord.y = improvedPerlinFBM(originalCoord + lastCoord * factor + randomWarpStartCoords[i + warps], grad, perlins, gain, lacunarity);

            lastCoord = coord;
        }
        if (warps > 0)
        {
            warpedValue = improvedPerlinFBM(originalCoord + coord * factor, grad, perlins, gain, lacunarity);
        }

        return warpedValue;
    }

    public float improvedPerlinFBM(Vector2 coord, Vector2[,] grad, int perlins, float gain, float lacunarity)
    {
        float value = 0;
        float amplitude = 0.5f;

        for (int i = 1; i < perlins + 1; i++)
        {
            value += amplitude * improvedFadePerlin(coord, grad);
            coord *= lacunarity;
            amplitude *= gain * (0.5f / gain);
        }

        return value * gain;
    }

    public float improvedFadePerlin(Vector2 coord, Vector2[,] grad)
    {
        Vector2 unitCoord = Floor2D(coord); // Integer part of the coordinate
        Vector2 fractCoord = Fract2D(coord); // Fractional part of the coordinate

        ///
        /// In the 2D array the coordinates should be like this:
        /// (0, 0) (1, 0)
        /// (0, 1) (1, 1)
        ///
        
        float tl = randomDot(fractCoord, grad[Mathf.RoundToInt(unitCoord.x) % grad.GetLength(0), Mathf.RoundToInt(unitCoord.y) % grad.GetLength(1)]);
        float tr = randomDot(fractCoord - Vector2.right, grad[Mathf.RoundToInt((unitCoord + Vector2.right).x) % grad.GetLength(0), Mathf.RoundToInt((unitCoord + Vector2.right).y) % grad.GetLength(1)]);
        float bl = randomDot(fractCoord + Vector2.down, grad[Mathf.RoundToInt((unitCoord - Vector2.down).x) % grad.GetLength(0), Mathf.RoundToInt((unitCoord - Vector2.down).y) % grad.GetLength(1)]);
        float br = randomDot(fractCoord - Vector2.right + Vector2.down, grad[Mathf.RoundToInt((unitCoord + Vector2.right - Vector2.down).x) % grad.GetLength(0), Mathf.RoundToInt((unitCoord + Vector2.right - Vector2.down).y) % grad.GetLength(1)]);
        
        float lerpX = (6 * Mathf.Pow(fractCoord.x, 5) - 15 * Mathf.Pow(fractCoord.x, 4) + 10 * Mathf.Pow(fractCoord.x, 3));
        float lerpY = (6 * Mathf.Pow(fractCoord.y, 5) - 15 * Mathf.Pow(fractCoord.y, 4) + 10 * Mathf.Pow(fractCoord.y, 3));
        
        return Mathf.Lerp(Mathf.Lerp(tl, tr, lerpX), Mathf.Lerp(bl, br, lerpX), lerpY) + 0.5f;
    }

    public float randomDot(Vector2 coord, Vector2 grad) // Return the fractional part of the sine of the dot product of the 2D coordinate vector and a random 2D vector
    {
        return Vector2.Dot(coord, grad);
    }

    //public Vector2 Lerp2D(Vector2 a2D, Vector2 b2D, float t) // Linear lerp a 2D vector to another 2D vector
    //{
    //    return new Vector2(a2D.x * (1 - t) + b2D.x * t, a2D.y * (1 - t) + b2D.y * t);
    //}

    public Vector2 Fract2D(Vector2 f2D) // Return the fractional part of a 2D vector
    {
        return (f2D - Floor2D(f2D));
    }

    public Vector2 Floor2D(Vector2 f2D)
    {
        return new Vector2(Mathf.Floor(f2D.x), Mathf.Floor(f2D.y)); // Floor a 2D vector
    }

    public float Fract(float f) // Return the fractional part of a float
    {
        return (f - Mathf.Floor(f));
    }
}
