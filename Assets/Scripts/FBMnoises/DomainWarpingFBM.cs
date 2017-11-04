using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomainWarpingFBM : MonoBehaviour
{
    //public int warpings; // How many times do we apply domain warping
    //public float factor; // This is used to multiply the last fbm vector2 result when input to the new fbm

    public Vector2 randomVector2D;
    public float randomSinMulti; // This number will be multiplied by the sine value

    public float warpedValue;

    // Use this for initialization
    void Start()
    {
        //do
        //{
        //    randomVector2D = new Vector2(BetterRandom.betterRandom(-10000000, 10000000) / 100000f, BetterRandom.betterRandom(-10000000, 10000000) / 100000f);
        //} while (randomVector2D.x == 0 || randomVector2D.y == 0 || randomVector2D.x == randomVector2D.y);


    }

    // Update is called once per frame
    void Update()
    {

    }

    public static float improvedFBMwarping(int warps, float factor, Vector2 coord, int octaves, float gain, float lacunarity, Vector2 randomVector2D, float randomSinMulti, Vector2[] randomWarpStartCoords)
    {
        float warpedValue = FBM2D.improvedFBM(coord, octaves, gain, lacunarity, randomVector2D, randomSinMulti);
        Vector2 originalCoord = coord; // The unwarpped coordinate
        Vector2 lastCoord = coord; // The coordinate of last warping

        //Vector2 randomWarpStartCoord = Vector2.zero;

        for (int i = 0; i < warps; i++)
        {
            //randomWarpStartCoord = new Vector2(BetterRandom.betterRandom(0, 1000) / 100f, BetterRandom.betterRandom(0, 1000) / 100f);

            coord.x = FBM2D.improvedFBM(lastCoord + randomWarpStartCoords[i], octaves, gain, lacunarity, randomVector2D, randomSinMulti);
            coord.y = FBM2D.improvedFBM(lastCoord + randomWarpStartCoords[i + warps], octaves, gain, lacunarity, randomVector2D, randomSinMulti);

            warpedValue = FBM2D.improvedFBM(originalCoord + coord * factor, octaves, gain, lacunarity, randomVector2D, randomSinMulti);
        }

        return warpedValue;
    }
}
