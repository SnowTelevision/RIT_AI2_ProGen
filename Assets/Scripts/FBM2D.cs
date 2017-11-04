using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static float improvedFBM(Vector2 coord, int octaves, float gain, float lacunarity, Vector2 randomVector2D)
    {
        float value = 0;
        float amplitude = 0.5f;

        for (int i = 0; i < octaves; i++)
        {
            value += amplitude * improvedFadePerlin(coord, randomVector2D.x, randomVector2D.y);
            coord *= lacunarity;
            amplitude *= gain;
        }

        return value;
    }

    public static float improvedFadePerlin(Vector2 coord, float v2x, float v2y)
    {
        Vector2 unitCoord = Floor2D(coord); // Integer part of the coordinate
        Vector2 fractCoord = Fract2D(coord); // Fractional part of the coordinate

        ///
        /// In the 2D array the coordinates should be like this:
        /// (0, 0) (1, 0)
        /// (0, 1) (1, 1)
        ///

        // Random height of four corners of the unit square (the integer part of the coordinate is the top left corner)
        float tl = randomDot(unitCoord, v2x, v2y);
        float tr = randomDot(unitCoord + Vector2.right, v2x, v2y);
        float bl = randomDot(unitCoord - Vector2.down, v2x, v2y);
        float br = randomDot(unitCoord + Vector2.right - Vector2.down, v2x, v2y);

        Vector2 six = Vector2.one * 6;
        Vector2 fifteen = Vector2.one * 15;
        Vector2 ten = Vector2.one * 10;
        Vector2 improvedFade = 6 * Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, fractCoord)))) -
                               15 * Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, fractCoord))) +
                               10 * Vector2.Scale(fractCoord, Vector2.Scale(fractCoord, fractCoord)); // 6 * t^5 - 15 * t^4 + 10 * t^3

        return Mathf.Lerp(tl, tr, improvedFade.x) + (bl - tl) * improvedFade.y * (1 - improvedFade.x) + (br - tr) * improvedFade.x * improvedFade.y;
    }

    public static float randomDot(Vector2 coord, float v2x, float v2y) // Return the fractional part of the sine of the dot product of the 2D coordinate vector and a random 2D vector
    {
        Vector2 randV2 = new Vector2(v2x, v2y);

        return Fract(Mathf.Sin(Vector2.Dot(coord, randV2)));
    }

    //public Vector2 Lerp2D(Vector2 a2D, Vector2 b2D, float t) // Linear lerp a 2D vector to another 2D vector
    //{
    //    return new Vector2(a2D.x * (1 - t) + b2D.x * t, a2D.y * (1 - t) + b2D.y * t);
    //}

    public static Vector2 Fract2D(Vector2 f2D) // Return the fractional part of a 2D vector
    {
        return (f2D - Floor2D(f2D));
    }

    public static Vector2 Floor2D(Vector2 f2D)
    {
        return new Vector2(Mathf.Floor(f2D.x), Mathf.Floor(f2D.y)); // Floor a 2D vector
    }

    public static float Fract(float f) // Return the fractional part of a float
    {
        return (f - Mathf.Floor(f));
    }
}
