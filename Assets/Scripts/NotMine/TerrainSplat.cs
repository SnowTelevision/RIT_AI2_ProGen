using System;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

[Serializable]
public class TerrainSplat : MonoBehaviour
{
    public TextureSetting[] Textures;

    void Update()
    {
        if (this.GetComponent<Terrain>() == null)   // if this game obeject does not have a terrain attached, well then fuckit dont run
        {
            return;
        }

        var terrain = this.gameObject.GetComponent<Terrain>();                                                              //create a neat reference to our terrain
        var terrainData = terrain.terrainData;                                                                              //create a neat reference to our terrain data

        terrainData.splatPrototypes = Textures.Select(s => new SplatPrototype { texture = s.Texture }).ToArray();                   //Get all the textures and assign it to the terrain's spaltprototypes
        terrainData.RefreshPrototypes();                                                                                            //gotta refresh my terraindata's prototypes after its manipulated

        int splatLengths = terrainData.splatPrototypes.Length;
        int alphaMapResolution = terrainData.alphamapResolution;
        int alphaMapHeight = terrainData.heightmapHeight;
        int alphaMapWidth = terrainData.heightmapWidth;

        var splatMap = new float[alphaMapResolution, alphaMapResolution, splatLengths];       //create a new splatmap array equal to our map's, we will store our new splat weights in here, then assight it to the map later
        var heights = terrainData.GetHeights(0, 0, alphaMapWidth, alphaMapHeight);                                 //get all the height points for the terrain... this will be where ware are going paint our textures on

        // non-parallel

        for (var zRes = 0; zRes < alphaMapHeight - 1; zRes++)
        {
            for (var xRes = 0; xRes < alphaMapWidth - 1; xRes++)
            {
                var splatWeights = new float[splatLengths];                                             //create a temp array to store all our 'none-normalised weights'
                var normalizedX = (float)xRes / (alphaMapWidth - 1);                        //gets the normalised X position based on the map resolution                     
                var normalizedZ = (float)zRes / (alphaMapHeight - 1);                       //gets the normalised Y position based on the map resolution 
                //var randomBlendNoise = ReMap(Mathf.PerlinNoise(xRes * .8f, zRes * .5f), 0, 1, .8f, 1);  //Get a random perlin value

                float angle = terrainData.GetSteepness(normalizedX, normalizedZ);                       //Get the ANGLE/STEEPNESS at this point: returns the angle between 0 and 90
                //Vector3 direction = terrainData.GetInterpolatedNormal(xRes, zRes);                      //Get the DIRECTION at this point: returns the direction of the normal as a Vector3
                float elevation = heights[zRes, xRes];                                                  //Get the HEIGHT at this point: return between 0 and 1 (0=lowest trough, .5f=Water level. 1f=highest peak)
                //float perlinElevation = heights[zRes, xRes] * randomBlendNoise;                         //Get a semi random height based on perlin noise, this is to give a more random blend, rather than straight horizontal lines.

                for (var i = 0; i < Textures.Length; i++)                                               //Loop through all our trextures and apply them accoding to the rules defined
                {
                    //var weighting = 0f;                                                                 //set the default weighting to 0, this means that if the image does not meet any of the criteria, then it will have no impact
                    var textureSetting = Textures[i];                                                   //get the setting instance based on index
                    //var calculatedHeight = textureSetting.RandomBlend ? perlinElevation : elevation;    //create a new height variable, and make it the actual height, unless the user selected to add a bit of randomness                                      
                    bool applyTexture = true;                                                           // Default is true to apply this texture, if any of the selected condition fails, this will be change to false

                    if (Textures[i].elevationRange)
                    {
                        if (elevation > textureSetting.maxElevation || elevation < textureSetting.minElevation)
                        {
                            applyTexture = false;
                        }
                    }
                    if (applyTexture && Textures[i].angleRange)
                    {
                        if (angle > textureSetting.maxAngle || angle < textureSetting.minAngle)
                        {
                            applyTexture = false;
                        }
                    }
                    if (applyTexture && Textures[i].isConcave || Textures[i].isConvex)
                    {
                        float lowestElevation = elevation; // Lowest elevation in the surrounding 8 coordinates
                        float highestElevation = elevation; // Highest elevation in the surrounding 8 coordinates
                        Vector2 lowestDir = Vector2.zero; // The direction of the lowest elevation
                        Vector2 highestDir = Vector2.zero; // The direction of the highest elevation

                        for (int m = -1; m < 2; m++) // Find out which coordinates amongst the area has the lowest or the highest elev
                        {
                            for (int n = -1; n < 2; n++)
                            {
                                if (zRes + m < 0 || zRes + m >= alphaMapHeight || xRes + n < 0 || xRes + n >= alphaMapWidth) //Prevent index out of bound
                                {
                                    continue;
                                }
                                if (heights[zRes + m, xRes + n] < lowestElevation)
                                {
                                    lowestElevation = heights[zRes + m, xRes + n];
                                    lowestDir.x = m;
                                    lowestDir.y = n;
                                }
                                if (heights[zRes + m, xRes + n] > highestElevation)
                                {
                                    highestElevation = heights[zRes + m, xRes + n];
                                    highestDir.x = m;
                                    highestDir.y = n;
                                }
                            }
                        }

                        if (Textures[i].isConcave)
                        {
                            bool isConcave = false;

                            if (lowestDir == Vector2.zero)
                            {
                                isConcave = true;
                            }
                            else
                            {
                                for (int r = 1; r <= textureSetting.checkingRange; r++)
                                {
                                    if ((zRes + r * lowestDir.x) < 0 ||
                                        (zRes + r * lowestDir.x) >= alphaMapHeight ||
                                        (xRes + r * lowestDir.y) < 0 ||
                                        (xRes + r * lowestDir.y) >= alphaMapWidth) //Prevent index out of bound
                                    {
                                        break;
                                    }

                                    if (heights[(zRes + Mathf.RoundToInt(r * lowestDir.x)), (xRes + Mathf.RoundToInt(r * lowestDir.y))] >
                                        heights[(zRes + Mathf.RoundToInt((r - 1) * lowestDir.x)), (xRes + (r - 1) * Mathf.RoundToInt(lowestDir.y))]) // If alone the downward slope, the slope goes back up
                                    {
                                        isConcave = true;
                                        break;
                                    }
                                }
                            }

                            if (!isConcave)
                            {
                                applyTexture = false;
                            }
                        }
                        if (Textures[i].isConvex)
                        {
                            bool isConvex = false;

                            if (highestDir == Vector2.zero)
                            {
                                isConvex = true;
                            }
                            else
                            {
                                for (int r = 1; r <= textureSetting.checkingRange; r++)
                                {
                                    if ((zRes + r * highestDir.x) < 0 ||
                                        (zRes + r * highestDir.x) >= alphaMapHeight ||
                                        (xRes + r * highestDir.y) < 0 ||
                                        (xRes + r * highestDir.y) >= alphaMapWidth) //Prevent index out of bound
                                    {
                                        break;
                                    }

                                    if (heights[(zRes + Mathf.RoundToInt(r * highestDir.x)), (xRes + Mathf.RoundToInt(r * highestDir.y))] <
                                        heights[(zRes + Mathf.RoundToInt((r - 1) * highestDir.x)), (xRes + Mathf.RoundToInt((r - 1) * highestDir.y))]) // If alone the downward slope, the slope goes back up
                                    {
                                        isConvex = true;
                                        break;
                                    }
                                }
                            }

                            if (!isConvex)
                            {
                                applyTexture = false;
                            }
                        }
                    }
                    //if (Textures[i].isConcave)
                    //{
                    //    bool isConcave = false;

                    //    for (int r = 1; r <= textureSetting.checkingRange; r++)
                    //    {
                    //        if (zRes + Mathf.RoundToInt((r * direction.normalized).z) < 0 ||
                    //           zRes + Mathf.RoundToInt((r * direction.normalized).z) >= alphaMapHeight ||
                    //           xRes + Mathf.RoundToInt((r * direction.normalized).x) < 0 ||
                    //           xRes + Mathf.RoundToInt((r * direction.normalized).x) >= alphaMapWidth) //Prevent index out of bound
                    //        {
                    //            break;
                    //        }

                    //        if (heights[zRes + Mathf.RoundToInt((r * direction.normalized).z), xRes + Mathf.RoundToInt((r * direction.normalized).x)] >
                    //           heights[zRes + Mathf.RoundToInt(((r - 1) * direction.normalized).z), xRes + Mathf.RoundToInt(((r - 1) * direction.normalized).x)]) // If alone the downward slope, the slope goes back up
                    //        {
                    //            isConcave = true;
                    //        }
                    //    }

                    //    if (!isConcave)
                    //    {
                    //        applyTexture = false;
                    //    }
                    //}
                    //if (Textures[i].isConvex)
                    //{
                    //    bool isConvex = false;

                    //    for (int r = 1; r <= textureSetting.checkingRange; r++)
                    //    {
                    //        if (xRes == 60 && zRes == 60)
                    //        {
                    //            print("r: " + r);
                    //            print("Direction: " + direction);
                    //            print("Direction norm: " + direction.normalized);
                    //            print(zRes + Mathf.RoundToInt((r * direction.normalized).z) + ", " + xRes + Mathf.RoundToInt((r * direction.normalized).x));
                    //        }

                    //        if (zRes + Mathf.RoundToInt((r * direction.normalized).z) < 0 ||
                    //           zRes + Mathf.RoundToInt((r * direction.normalized).z) >= alphaMapHeight ||
                    //           xRes + Mathf.RoundToInt((r * direction.normalized).x) < 0 ||
                    //           xRes + Mathf.RoundToInt((r * direction.normalized).x) >= alphaMapWidth) //Prevent index out of bound
                    //        {
                    //            break;
                    //        }

                    //        if (heights[zRes + Mathf.RoundToInt((r * direction.normalized).z), xRes + Mathf.RoundToInt((r * direction.normalized).x)] <
                    //           heights[zRes + Mathf.RoundToInt(((r - 1) * direction.normalized).z), xRes + Mathf.RoundToInt(((r - 1) * direction.normalized).x)]) // If alone the upward slope, the slope goes back down
                    //        {
                    //            isConvex = true;
                    //        }
                    //    }

                    //    if (!isConvex)
                    //    {
                    //        applyTexture = false;
                    //    }
                    //}

                    if (applyTexture)
                    {
                        splatWeights[i] = textureSetting.Impact;
                    }

                    //Original code
                    /*
                    switch (textureSetting.PlacementType)
                    {
                        case PlacementType.Angle:
                            if (Math.Abs(angle - textureSetting.Angle) < textureSetting.Precisision)                        //check if the specified angle is the same as the current angle (allow a variance based on the precision)
                                weighting = textureSetting.Impact;
                            break;
                        case PlacementType.Direction:
                            if (Vector3.SqrMagnitude(direction - textureSetting.Direction) < textureSetting.Precisision)    //check if the specified direction is the same as the current direction (allow a variance based on the precision)
                                weighting = textureSetting.Impact;
                            break;
                        case PlacementType.Elevation:
                            if (Math.Abs(textureSetting.Elevation = calculatedHeight) < textureSetting.Precisision)         //check if the specified elevation is the same as the current elevation (allow a variance based on the precision)
                                weighting = textureSetting.Impact;
                            break;
                        case PlacementType.ElevationRange:
                            if (calculatedHeight > textureSetting.MinRange && calculatedHeight < textureSetting.MaxRange)    //check if the current height is between the specified min and max heights
                                weighting = textureSetting.Impact;
                            break;
                    }

                    splatWeights[i] = weighting;
                    */
                }

                #region normalize
                //we need to make sure that the sum of our weights is not greater than 1, so lets normalise it
                var totalWeight = splatWeights.Sum();                               //sum all the splat weights,
                for (int i = 0; i < splatLengths; i++)        //Loop through each splatWeights
                {
                    splatWeights[i] /= totalWeight;                                 //Normalize so that sum of all texture weights = 1
                    splatMap[zRes, xRes, i] = splatWeights[i];                      //Assign this point to the splatmap array
                }
                #endregion
            }
        }

        //Parallel
        /*
        Parallel.For(0, alphaMapHeight - 1, zRes =>
        {
            Parallel.For(0, alphaMapWidth - 1, xRes =>
            {
                var splatWeights = new float[splatLengths];                                             //create a temp array to store all our 'none-normalised weights'
                var normalizedX = (float)xRes / (alphaMapWidth - 1);                        //gets the normalised X position based on the map resolution                     
                var normalizedZ = (float)zRes / (alphaMapHeight - 1);                       //gets the normalised Y position based on the map resolution 
                var randomBlendNoise = ReMap(Mathf.PerlinNoise(xRes * .8f, zRes * .5f), 0, 1, .8f, 1);  //Get a random perlin value

                float angle = terrainData.GetSteepness(normalizedX, normalizedZ);                       //Get the ANGLE/STEEPNESS at this point: returns the angle between 0 and 90
                Vector3 direction = terrainData.GetInterpolatedNormal(xRes, zRes);                      //Get the DIRECTION at this point: returns the direction of the normal as a Vector3
                float elevation = heights[zRes, xRes];                                                  //Get the HEIGHT at this point: return between 0 and 1 (0=lowest trough, .5f=Water level. 1f=highest peak)

                for (var i = 0; i < Textures.Length; i++)                                               //Loop through all our trextures and apply them accoding to the rules defined
                {
                    var textureSetting = Textures[i];                                                   //get the setting instance based on index                  
                    bool applyTexture = true;                                                           // Default is true to apply this texture, if any of the selected condition fails, this will be change to false

                    if (Textures[i].elevationRange)
                    {
                        if (elevation > textureSetting.maxElevation || elevation < textureSetting.minElevation)
                        {
                            applyTexture = false;
                        }
                    }
                    if (Textures[i].angleRange)
                    {
                        if (angle > textureSetting.maxAngle || angle < textureSetting.minAngle)
                        {
                            applyTexture = false;
                        }
                    }
                    if (Textures[i].isConcave || Textures[i].isConvex)
                    {
                        float lowestElevation = elevation; // Lowest elevation in the surrounding 8 coordinates
                        float highestElevation = elevation; // Highest elevation in the surrounding 8 coordinates
                        Vector2 lowestDir = Vector2.zero; // The direction of the lowest elevation
                        Vector2 highestDir = Vector2.zero; // The direction of the highest elevation

                        for (int m = -1; m < 2; m++) // Find out which coordinates amongst the area has the lowest or the highest elev
                        {
                            for (int n = -1; n < 2; n++)
                            {
                                if (zRes + m < 0 || zRes + m >= alphaMapHeight || xRes + n < 0 || xRes + n >= alphaMapWidth) //Prevent index out of bound
                                {
                                    continue;
                                }
                                if (heights[zRes + m, xRes + n] < lowestElevation)
                                {
                                    lowestElevation = heights[zRes + m, xRes + n];
                                    lowestDir.x = m;
                                    lowestDir.y = n;
                                }
                                if (heights[zRes + m, xRes + n] > highestElevation)
                                {
                                    highestElevation = heights[zRes + m, xRes + n];
                                    highestDir.x = m;
                                    highestDir.y = n;
                                }
                            }
                        }

                        if (Textures[i].isConcave)
                        {
                            bool isConcave = false;

                            if (lowestDir.x == zRes && lowestDir.y == xRes)
                            {
                                isConcave = true;
                            }
                            else
                            {
                                for (int r = 1; r <= textureSetting.checkingRange; r++)
                                {
                                    if ((zRes + r * lowestDir.x) < 0 ||
                                        (zRes + r * lowestDir.x) >= alphaMapHeight ||
                                        (xRes + r * lowestDir.y) < 0 ||
                                        (xRes + r * lowestDir.y) >= alphaMapWidth) //Prevent index out of bound
                                    {
                                        break;
                                    }

                                    if (heights[(zRes + Mathf.RoundToInt(r * lowestDir.x)), (xRes + Mathf.RoundToInt(r * lowestDir.y))] >
                                        heights[(zRes + Mathf.RoundToInt((r - 1) * lowestDir.x)), (xRes + (r - 1) * Mathf.RoundToInt(lowestDir.y))]) // If alone the downward slope, the slope goes back up
                                    {
                                        isConcave = true;
                                    }
                                }
                            }

                            if (!isConcave)
                            {
                                applyTexture = false;
                            }
                        }
                        if (Textures[i].isConvex)
                        {
                            bool isConvex = false;

                            if (highestDir.x == zRes && highestDir.y == xRes)
                            {
                                isConvex = true;
                            }
                            else
                            {
                                for (int r = 1; r <= textureSetting.checkingRange; r++)
                                {
                                    if (xRes == 60 && zRes == 60)
                                    {
                                        print("r: " + r);
                                        print((zRes + r * highestDir.x) + ", " + (xRes + r * highestDir.y));
                                    }

                                    if ((zRes + r * highestDir.x) < 0 ||
                                        (zRes + r * highestDir.x) >= alphaMapHeight ||
                                        (xRes + r * highestDir.y) < 0 ||
                                        (xRes + r * highestDir.y) >= alphaMapWidth) //Prevent index out of bound
                                    {
                                        break;
                                    }

                                    if (heights[(zRes + Mathf.RoundToInt(r * highestDir.x)), (xRes + Mathf.RoundToInt(r * highestDir.y))] <
                                        heights[(zRes + Mathf.RoundToInt((r - 1) * highestDir.x)), (xRes + Mathf.RoundToInt((r - 1) * highestDir.y))]) // If alone the downward slope, the slope goes back up
                                    {
                                        isConvex = true;
                                    }
                                }
                            }

                            if (!isConvex)
                            {
                                applyTexture = false;
                            }
                        }
                    }

                    if (applyTexture)
                    {
                        splatWeights[i] = textureSetting.Impact;
                    }
                }

                #region normalize
                //we need to make sure that the sum of our weights is not greater than 1, so lets normalise it
                var totalWeight = splatWeights.Sum();                               //sum all the splat weights,
                for (int i = 0; i < splatLengths; i++)        //Loop through each splatWeights
                {
                    splatWeights[i] /= totalWeight;                                 //Normalize so that sum of all texture weights = 1
                    splatMap[zRes, xRes, i] = splatWeights[i];                      //Assign this point to the splatmap array
                }
                #endregion
            });
        });*/

        terrainData.SetAlphamaps(0, 0, splatMap);
    }

    //Get a random periln value within acceptable range
    public float ReMap(float value, float sMin, float sMax, float mMin, float mMax)
    {
        return (value - sMin) * (mMax - mMin) / (sMax - sMin) + mMin;
    }
}

[Serializable]
public class TextureSetting
{
    [Tooltip("The texture you want to be placed")]
    public Texture2D Texture;
    [Tooltip("The type of placement")]
    //public PlacementType PlacementType;
    public bool elevationRange; // If the texture has to fit within an elevation range
    public float minElevation;
    public float maxElevation;

    public bool angleRange; // If the texture has to fit on a certain angle range (steeper land means mountain top/river bedrock)
    public float minAngle;
    public float maxAngle;

    public int checkingRange; // How large is the "surrounding area"
    public bool isConcave; // If the texture has to fit in a place belongs to a concave area (river bed)
    public bool isConvex; // If the texture has to fit in a place belongs to a convex area (mountain top)

    //[Tooltip("The exact height you want this texture to be displayed (.5 will be the middle of the hieght of the map)")]
    //[Range(0, 1)]
    //public float Elevation;

    //[Tooltip("The angle you want this texture to be displayed at (0-19 deggrees)")]
    //[Range(0, 90)]
    //public float Angle;

    //[Tooltip("The min and the max height you want this texture to be displayed (.5 will be the middle of the hieght of the map)")]
    //public Vector3 Direction;


    //public float MinRange;
    //public float MaxRange;

    //[Tooltip("Add some random variations to height based placement, this will give a smoother blend based on height")]
    //public bool RandomBlend;

    //[Tooltip("Comparing floats gives us a chance of losing floating point values. How precisly do you want your values to be interperetted (0.0001f beeing EXTREMELY precise, 0.9f being irrelevent almost)")]
    //public float Precisision;

    public int Impact;
}

//public enum PlacementType
//{
//    Elevation,
//    ElevationRange,
//    Angle,
//    Direction,
//}