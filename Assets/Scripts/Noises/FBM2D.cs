using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class FBM2D : MonoBehaviour
{
    public float gain; // How much the higher frequency octave will influence the previous result
    public float lacunarity; // Resolution of the map (higher the value higher the resolution in the same area

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public float improvedFBM(Vector2 coord, int octaves, float gain, float lacunarity, Vector2 randomVector, float randomSinMulti)
    {
        float value = 0;
        float amplitude = 0.5f;

        //Parallel.For(1, octaves + 1, i =>
        //{
        //    //Interlocked.Add(ref value, amplitude * improvedFadePerlin(coord, randomVector2D.x, randomVector2D.y, randomSinMulti));
        //    value += amplitude * improvedFadePerlin(coord, randomVector2D.x, randomVector2D.y, randomSinMulti);
        //    coord *= lacunarity;
        //    amplitude *= gain * (0.5f / gain);
        //});

        for (int i = 1; i < octaves + 1; i++)
        {
            value += amplitude * improvedFadeSin(coord, randomVector, randomSinMulti);
            coord *= lacunarity;
            amplitude *= gain * (0.5f / gain);
        }

        return value * gain;
    }

    public float improvedFadeSin(Vector2 coord, Vector2 randomVector, float randomSinMulti)
    {
        Vector2 unitCoord = Floor2D(coord); // Integer part of the coordinate
        Vector2 fractCoord = Fract2D(coord); // Fractional part of the coordinate

        ///
        /// In the 2D array the coordinates should be like this:
        /// (0, 0) (1, 0)
        /// (0, 1) (1, 1)
        ///

        // Random height of four corners of the unit square (the integer part of the coordinate is the top left corner)
        float tl = randomDot(unitCoord, randomVector, randomSinMulti);
        float tr = randomDot(unitCoord + Vector2.right, randomVector, randomSinMulti);
        float bl = randomDot(unitCoord - Vector2.down, randomVector, randomSinMulti);
        float br = randomDot(unitCoord + Vector2.right - Vector2.down, randomVector, randomSinMulti);
        
        //float lerpX = (6 * Mathf.Pow(fractCoord.x, 5) - 15 * Mathf.Pow(fractCoord.x, 4) + 10 * Mathf.Pow(fractCoord.x, 3));
        //float lerpY = (6 * Mathf.Pow(fractCoord.y, 5) - 15 * Mathf.Pow(fractCoord.y, 4) + 10 * Mathf.Pow(fractCoord.y, 3));
        //
        //return Mathf.Lerp(Mathf.Lerp(tl, tr, lerpX), Mathf.Lerp(bl, br, lerpX), lerpY);
        
        Vector2 improvedFade = 6 * Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, fractCoord)))) -
                               15 * Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, fractCoord))) +
                               10 * Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, fractCoord)); // 6 * t^5 - 15 * t^4 + 10 * t^3
        
        return Mathf.Lerp(tl, tr, improvedFade.x) + (bl - tl) * improvedFade.y * (1 - improvedFade.x) + (br - tr) * improvedFade.x * improvedFade.y;
    }

    public float randomDot(Vector2 coord, Vector2 randomVector, float randomSinMulti) // Return the fractional part of the sine of the dot product of the 2D coordinate vector and a random 2D vector
    {
        return Fract(Mathf.Sin(Vector2.Dot(coord, randomVector)) * randomSinMulti);
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
