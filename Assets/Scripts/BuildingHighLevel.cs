﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHighLevel : Building
{
    public int[] sectionFloors;
    public bool needRoof = false;
    public bool needBase = false;
    public bool needSectionConnection = false;

    public int shapeEdgeCount = 4;
    public float floorHeight = 0.1f;

    public float baseHeightByFloor = 1f;
    public float connectionHeightByFloor = 0.5f;    
    public float roofHeightByFloor = 5f;



    public float decreaseUnit = 0.03f;
    public float connectionIncreaseUnit = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override bool HideBuildingFaces()
    {
        return true;
    }

    float currentHeight = 0;
    int buildingFaceIndex = 0;
    float currentUnit = 1;


    protected override void InitBuildingFaces()
    {
        currentHeight = 0;
        buildingFaceIndex = 0;
        currentUnit = 1;

        int sectionCount = sectionFloors.Length;
        int roofCount = needRoof ? 1 : 0;
        int baseCount = needBase ? 1 : 0;
        int edgeCount = needSectionConnection ? sectionCount : 0;

        int buildingFaceCount = sectionCount + roofCount + baseCount + edgeCount;
        buildingFaces = new BuildingFace[buildingFaceCount];

       
        // base
        if(needBase)
        {
            var bf = GenerateBuildingFace(currentUnit, 1);
            bf.startHeight = currentHeight;
            bf.height = floorHeight * baseHeightByFloor;
            bf.FaceType = BuildingFace.BuildingFaceType.CONNECTION;
            PushBuildingFace(bf);
        }

        for(int i = 0; i < sectionFloors.Length; i++)
        {            
            int floors = sectionFloors[i];
            var bf = GenerateBuildingFace(currentUnit, floors);
            bf.startHeight = currentHeight;
            bf.FaceType = BuildingFace.BuildingFaceType.REGULAR;
            PushBuildingFace(bf);

            if(needSectionConnection)
            {
                var edge = GenerateBuildingFace(bf.unit + connectionIncreaseUnit, 1);
                edge.startHeight = currentHeight;
                edge.height = floorHeight * connectionHeightByFloor;
                edge.FaceType = BuildingFace.BuildingFaceType.CONNECTION;
                PushBuildingFace(edge, false);
            }
        }

        if(needRoof)
        {
            var bf = GenerateBuildingFace(currentUnit, 1);
            bf.startHeight = currentHeight;
            bf.height = floorHeight * roofHeightByFloor;
            bf.FaceType = BuildingFace.BuildingFaceType.ROOF;
            PushBuildingFace(bf);
        }
    }

    BuildingFace GenerateBuildingFace(float unit, int floors)
    {
        var bf = new BuildingFace(this, mesh);
        bf.floorHeight = floorHeight;
        bf.floors = floors;
        bf.unit = unit;

        var angle = 0.0f;
        bf.baseShape = new Vector2[shapeEdgeCount];
        for (int i = 0; i < shapeEdgeCount; i++)
        {
            var stepAngle = Mathf.PI * 2 / shapeEdgeCount;
            angle += stepAngle;            
            bf.baseShape[i] = new Vector2(unit * Mathf.Cos(angle), unit * Mathf.Sin(angle));
            // Debug.Log(bf.baseShape[i]);
        }

        return bf;
    }

    void PushBuildingFace(BuildingFace bf, bool needDecreaseUnit = true)
    {
        buildingFaces[buildingFaceIndex++] = bf;
        currentHeight += bf.GetHeight();
        if(needDecreaseUnit)
            currentUnit -= decreaseUnit;
    }
}
