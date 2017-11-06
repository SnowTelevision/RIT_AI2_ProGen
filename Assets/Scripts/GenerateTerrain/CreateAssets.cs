using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CreateAssets : MonoBehaviour
{
    /// <summary>
    /// map[x,y] = terrainData.GetHeights()
    /// map[0,0] = (0, 0, 0)
    /// map[x,0] = (0, 0, x)
    /// map[0,y] = (y, 0, 0)
    /// map[x,y] = (y, 0, x)
    /// </summary>

    public AssetSetting[] assets;

    Terrain terrain;                                                             //create a neat reference to our terrain
    TerrainData terrainData;                                                                             //create a neat reference to our terrain data

    //int alphaMapResolution;
    int alphaMapHeight;
    int alphaMapWidth;

    float[,] heights;                              //get all the height points for the terrain... this will be where ware are going paint our textures on

    // Use this for initialization
    void Start()
    {
        terrain = this.gameObject.GetComponent<Terrain>();                                                              //create a neat reference to our terrain
        terrainData = terrain.terrainData;                                                                              //create a neat reference to our terrain data

        //alphaMapResolution = terrainData.alphamapResolution;
        alphaMapHeight = terrainData.heightmapHeight;
        alphaMapWidth = terrainData.heightmapWidth;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.GetComponent<Terrain>() == null)   // if this game obeject does not have a terrain attached, well then fuckit dont run
        {
            return;
        }

        heights = terrainData.GetHeights(0, 0, alphaMapWidth, alphaMapHeight);                                 //get all the height points for the terrain... this will be where ware are going paint our textures on

        // non-parallel

        for (var zRes = alphaMapHeight - 2; zRes >= 0; zRes--)
        {
            for (var xRes = alphaMapWidth - 2; xRes >= 0; xRes--)
            {
        //for (var zRes = 0; zRes < alphaMapHeight - 1; zRes++)
        //{
        //    for (var xRes = 0; xRes < alphaMapWidth - 1; xRes++)
        //    {
                var normalizedX = (float)xRes / (alphaMapWidth - 1);                        //gets the normalised X position based on the map resolution                     
                var normalizedZ = (float)zRes / (alphaMapHeight - 1);                       //gets the normalised Y position based on the map resolution 

                float angle = terrainData.GetSteepness(normalizedX, normalizedZ);                       //Get the ANGLE/STEEPNESS at this point: returns the angle between 0 and 90
                float elevation = heights[zRes, xRes];                                                  //Get the HEIGHT at this point: return between 0 and 1 (0=lowest trough, .5f=Water level. 1f=highest peak)

                for (var i = 0; i < assets.Length; i++)                                               //Loop through all our trextures and apply them accoding to the rules defined
                {
                    var assetSetting = assets[i];                                                   //get the setting instance based on index
                    if (Time.realtimeSinceStartup - assetSetting.lastPlacingTime < assetSetting.placementFrequency)
                    {
                        continue;
                    }

                    bool createAsset = true;                                                           // Default is true to apply this texture, if any of the selected condition fails, this will be change to false

                    if (assetSetting.elevationRange)
                    {
                        for (int m = -assetSetting.elevationCheckRange; m < assetSetting.elevationCheckRange + 1; m++) // Find out which coordinates amongst the area has the lowest or the highest elev
                        {
                            if (zRes + m < 0 || zRes + m >= alphaMapHeight) //Prevent index out of bound
                            {
                                continue;
                            }

                            for (int n = -assetSetting.elevationCheckRange; n < assetSetting.elevationCheckRange + 1; n++)
                            {
                                if (xRes + n < 0 || xRes + n >= alphaMapWidth) //Prevent index out of bound
                                {
                                    continue;
                                }

                                if (heights[zRes + m, xRes + n] > assetSetting.maxElevation || heights[zRes + m, xRes + n] < assetSetting.minElevation)
                                {
                                    createAsset = false;
                                }
                            }
                        }
                    }
                    if (createAsset && assetSetting.angleRange)
                    {
                        if (angle > assetSetting.maxAngle || angle < assetSetting.minAngle)
                        {
                            createAsset = false;
                        }
                    }

                    float lowestElevation = elevation; // Lowest elevation in the surrounding 8 coordinates
                    float highestElevation = elevation; // Highest elevation in the surrounding 8 coordinates
                    Vector2 lowestDir = Vector2.zero; // The direction of the lowest elevation
                    Vector2 highestDir = Vector2.zero; // The direction of the highest elevation

                    ///
                    /// z to x, x to y
                    ///
                    for (int m = -1; m < 2; m++) // Find out which coordinates amongst the area has the lowest or the highest elev
                    {
                        if(zRes + m < 0 || zRes + m >= alphaMapHeight) //Prevent index out of bound
                        {
                            continue;
                        }

                        for (int n = -1; n < 2; n++)
                        {
                            if (xRes + n < 0 || xRes + n >= alphaMapWidth) //Prevent index out of bound
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

                    if (createAsset && assetSetting.specSlope)
                    {
                        switch (assetSetting.slopeType)
                        {
                            case SlopeType.flat:
                                for (int x = -assetSetting.slopeCheckRange; x <= assetSetting.slopeCheckRange; x++)
                                {
                                    if (!createAsset)
                                    {
                                        break;
                                    }

                                    if ((zRes + x * highestDir.x) < 0 ||
                                        (zRes + x * highestDir.x) >= alphaMapHeight) //Prevent index out of bound
                                    {
                                        createAsset = false;
                                        break;
                                    }

                                    for (int y = -assetSetting.slopeCheckRange; y <= assetSetting.slopeCheckRange; y++)
                                    {
                                        if ((xRes + y * highestDir.y) < 0 ||
                                            (xRes + y * highestDir.y) >= alphaMapWidth) //Prevent index out of bound
                                        {
                                            createAsset = false;
                                            break;
                                        }

                                        if (terrainData.GetSteepness((float)(xRes + y) / (alphaMapWidth - 1), (float)(zRes + x) / (alphaMapHeight - 1)) > 15)
                                        {
                                            createAsset = false;
                                            break;
                                        }
                                    }
                                }
                                break;

                            case SlopeType.upHill:
                                for (int r = 1; r <= assetSetting.slopeCheckRange; r++)
                                {
                                    if ((zRes + r * highestDir.x) < 0 ||
                                        (zRes + r * highestDir.x) >= alphaMapHeight - 1 ||
                                        (xRes + r * highestDir.y) < 0 ||
                                        (xRes + r * highestDir.y) >= alphaMapWidth - 1 ||
                                        (zRes + r * lowestDir.x) < 0 ||
                                        (zRes + r * lowestDir.x) >= alphaMapHeight - 1 ||
                                        (xRes + r * lowestDir.y) < 0 ||
                                        (xRes + r * lowestDir.y) >= alphaMapWidth - 1) // Prevent index out of bound
                                    {
                                        createAsset = false;
                                        break;
                                    }
                                    if (assetSetting.isPair) // If asset is pair
                                    {
                                        if ((zRes + assetSetting.pairRange * highestDir.x) < 0 ||
                                            (zRes + assetSetting.pairRange * highestDir.x) >= alphaMapHeight - 1 ||
                                            (xRes + assetSetting.pairRange * highestDir.y) < 0 ||
                                            (xRes + assetSetting.pairRange * highestDir.y) >= alphaMapWidth - 1 ||
                                            (zRes + assetSetting.pairRange * lowestDir.x) < 0 ||
                                            (zRes + assetSetting.pairRange * lowestDir.x) >= alphaMapHeight - 1 ||
                                            (xRes + assetSetting.pairRange * lowestDir.y) < 0 ||
                                            (xRes + assetSetting.pairRange * lowestDir.y) >= alphaMapWidth - 1) // Prevent pair index out of bound
                                        {
                                            createAsset = false;
                                            break;
                                        }
                                    }

                                    if (heights[(zRes + Mathf.RoundToInt(r * highestDir.x)), (xRes + Mathf.RoundToInt(r * highestDir.y))] <
                                        heights[(zRes + Mathf.RoundToInt((r - 1) * highestDir.x)), (xRes + (r - 1) * Mathf.RoundToInt(highestDir.y))] || 
                                        heights[(zRes + Mathf.RoundToInt(r * lowestDir.x)), (xRes + Mathf.RoundToInt(r * lowestDir.y))] >
                                        heights[(zRes + Mathf.RoundToInt((r - 1) * lowestDir.x)), (xRes + (r - 1) * Mathf.RoundToInt(lowestDir.y))]) // Check if the slope is going up in the check range
                                    {
                                        createAsset = false;
                                        break;
                                    }
                                    if(assetSetting.isPair)
                                    {
                                        if (heights[(zRes + Mathf.RoundToInt(assetSetting.pairRange * highestDir.x)), (xRes + Mathf.RoundToInt(assetSetting.pairRange * highestDir.y))] <
                                            heights[(zRes + Mathf.RoundToInt(assetSetting.slopeCheckRange * lowestDir.x)), (xRes + Mathf.RoundToInt(assetSetting.slopeCheckRange * lowestDir.y))] ||
                                            heights[(zRes + Mathf.RoundToInt(assetSetting.pairRange * highestDir.x)), (xRes + Mathf.RoundToInt(assetSetting.pairRange * highestDir.y))] <
                                            heights[zRes, xRes]) // Check if the pair's height is lower than itself
                                        {
                                            createAsset = false;
                                            break;
                                        }
                                    }
                                }
                                break;

                            case SlopeType.downHill:
                                for (int r = 1; r <= assetSetting.slopeCheckRange; r++)
                                {
                                    if ((zRes + r * lowestDir.x) < 0 ||
                                        (zRes + r * lowestDir.x) >= alphaMapHeight - 1 ||
                                        (xRes + r * lowestDir.y) < 0 ||
                                        (xRes + r * lowestDir.y) >= alphaMapWidth - 1 ||
                                        (zRes + r * highestDir.x) < 0 ||
                                        (zRes + r * highestDir.x) >= alphaMapHeight - 1 ||
                                        (xRes + r * highestDir.y) < 0 ||
                                        (xRes + r * highestDir.y) >= alphaMapWidth - 1) //Prevent index out of bound
                                    {
                                        createAsset = false;
                                        break;
                                    }
                                    if (assetSetting.isPair) // If asset is pair
                                    {
                                        //print(xRes + assetSetting.pairRange * lowestDir.y + " " + (alphaMapWidth - 1) + " " + xRes + Mathf.RoundToInt(assetSetting.pairRange * lowestDir.x));
                                        if ((zRes + assetSetting.pairRange * lowestDir.x) < 0 ||
                                            (zRes + assetSetting.pairRange * lowestDir.x) >= alphaMapHeight - 1 ||
                                            (xRes + assetSetting.pairRange * lowestDir.y) < 0 ||
                                            (xRes + assetSetting.pairRange * lowestDir.y) >= alphaMapWidth - 1 ||
                                            (zRes + assetSetting.pairRange * highestDir.x) < 0 ||
                                            (zRes + assetSetting.pairRange * highestDir.x) >= alphaMapHeight - 1 ||
                                            (xRes + assetSetting.pairRange * highestDir.y) < 0 ||
                                            (xRes + assetSetting.pairRange * highestDir.y) >= alphaMapWidth - 1) // Prevent pair index out of bound
                                        {
                                            createAsset = false;
                                            break;
                                        }
                                    }

                                    if (heights[(zRes + Mathf.RoundToInt(r * lowestDir.x)), (xRes + Mathf.RoundToInt(r * lowestDir.y))] >
                                        heights[(zRes + Mathf.RoundToInt((r - 1) * lowestDir.x)), (xRes + (r - 1) * Mathf.RoundToInt(lowestDir.y))] ||
                                        heights[(zRes + Mathf.RoundToInt(r * highestDir.x)), (xRes + Mathf.RoundToInt(r * highestDir.y))] <
                                        heights[(zRes + Mathf.RoundToInt((r - 1) * highestDir.x)), (xRes + (r - 1) * Mathf.RoundToInt(highestDir.y))]) // Check if the slope is going down in the check range
                                    {
                                        createAsset = false;
                                        break;
                                    }
                                    if (assetSetting.isPair)
                                    {
                                        if (heights[(zRes + Mathf.RoundToInt(assetSetting.pairRange * lowestDir.x)), (xRes + Mathf.RoundToInt(assetSetting.pairRange * lowestDir.y))] >
                                            heights[(zRes + Mathf.RoundToInt(assetSetting.slopeCheckRange * highestDir.x)), (xRes + Mathf.RoundToInt(assetSetting.slopeCheckRange * highestDir.y))] ||
                                            heights[(zRes + Mathf.RoundToInt(assetSetting.pairRange * lowestDir.x)), (xRes + Mathf.RoundToInt(assetSetting.pairRange * lowestDir.y))] >
                                            heights[zRes, xRes]) // Check if the pair's height is higher than itself
                                        {
                                            createAsset = false;
                                            break;
                                        }
                                    }
                                }
                                break;
                        }
                    }

                    if (createAsset)
                    {
                        assetSetting.lastPlacingTime = Time.realtimeSinceStartup;
                        GameObject newAsset = Instantiate(assetSetting.asset);
                        newAsset.transform.position = new Vector3(xRes, heights[zRes, xRes] * terrainData.size.y + assetSetting.heightOffset, zRes);
                        //print(new Vector3(xRes, heights[zRes, xRes], zRes));

                        if (assetSetting.pairAsset)
                        {
                            GameObject pair = Instantiate(assetSetting.pairAsset);
                            if (assetSetting.slopeType.Equals(SlopeType.upHill))
                            {
                                newAsset.GetComponent<AssetBehavior>().pair = pair;
                                newAsset.GetComponent<AssetBehavior>().pair.transform.position = new Vector3(xRes + assetSetting.pairRange * highestDir.y,
                                                                                                             heights[zRes + Mathf.RoundToInt(assetSetting.pairRange * highestDir.x), xRes + Mathf.RoundToInt(assetSetting.pairRange * highestDir.y)] * terrainData.size.y + assetSetting.heightOffset,
                                                                                                             zRes + assetSetting.pairRange * highestDir.x);
                                newAsset.GetComponent<AssetBehavior>().pair.GetComponent<AssetBehavior>().pair = newAsset;
                            }
                            else if (assetSetting.slopeType.Equals(SlopeType.downHill))
                            {
                                newAsset.GetComponent<AssetBehavior>().pair = pair;
                                newAsset.GetComponent<AssetBehavior>().pair.transform.position = new Vector3(xRes + assetSetting.pairRange * lowestDir.y,
                                                                                                             heights[zRes + Mathf.RoundToInt(assetSetting.pairRange * lowestDir.x), xRes + Mathf.RoundToInt(assetSetting.pairRange * lowestDir.y)] * terrainData.size.y + assetSetting.heightOffset,
                                                                                                             zRes + assetSetting.pairRange * lowestDir.x);
                                newAsset.GetComponent<AssetBehavior>().pair.GetComponent<AssetBehavior>().pair = newAsset;
                            }
                        }
                    }
                }
            }
        }
    }
}

[Serializable]
public class AssetSetting
{
    public GameObject asset;
    public float heightOffset; // What is the offset for the placement height?

    public bool isPair; // If there is another pairing asset
    public GameObject pairAsset;
    public int pairRange; // How far should its pair being placed

    public bool elevationRange; // If the texture has to fit within an elevation range
    public float minElevation;
    public float maxElevation;
    public int elevationCheckRange; // How far the nearby elevation has to be within the same range

    public bool angleRange; // If the texture has to fit on a certain angle range (steeper land means mountain top/river bedrock)
    public float minAngle;
    public float maxAngle;

    public int slopeCheckRange; // How large is the "surrounding area"
    public bool specSlope; // Do we place it on a specific slope type
    public SlopeType slopeType; // Do we place it when it is on up hill or down hill or flat ground

    public float placementFrequency; // At least how long do we wait until placing the next asset
    public float lastPlacingTime; // The last time we placed this asset
}

public enum SlopeType
{
    flat,
    upHill,
    downHill,
}
