﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBehavior : MonoBehaviour
{
    public float lastingTime; // How long should this last
    public GameObject pair;
    public DomainWarpingFBMTest terrain;

    // Use this for initialization
    void Start()
    {
        terrain = FindObjectOfType<DomainWarpingFBMTest>();
        lastingTime = terrain.width * terrain.animateSpeed;

        if (terrain.animate)
        {
            Destroy(gameObject, lastingTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(pair != null)
        {
            transform.LookAt(pair.transform);
        }

        if(terrain.updateRealtime)
        {
            if(terrain.animate)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - terrain.animateSpeed * Time.deltaTime * terrain.Scale * 2f);
            }
        }
    }
}
