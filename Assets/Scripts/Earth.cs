﻿using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Earth : MonoBehaviour
{
    [Title("Noise")]
    public EarthNoise[] noiseArray;
    [Title("Shape")]
    [Range(2, 256)]
    public int resolution = 10;
    [Range(1, 10)]
    public float radius = 1;


    //[Title("Noise")]
    //public float noiseFrequency;
    //public float noiseStrength;
    //public Vector3 noiseOffset;    
    //public float noiseThreshould;

    //[ValidateInput("GreaterAndEqualThanOne", "Layer must be more than one", InfoMessageType.Error)]
    //public int noiseMultiLayer = 1;

    //// larger than 1
    //[ValidateInput("GreaterThanOne", "Freq multiplier must be more than one", InfoMessageType.Error)]
    //public float noiseFrequencyMulti = 2;
    //// smaller than 1
    //[Range(0, 0.99f)]
    //public float noiseStrenthMulti = 0.5f;

    [Title("Earth Material")]
    //public Color earthAlbedoColor;
    public Material earthMaterial;
    public Gradient earthHeightColor;
    const int earthHeightColorResolution = 100;
    Texture2D heightColorTex;    
    
    float minFactor = float.MaxValue;
    float maxFactor = float.MinValue;

    [Title("Building Generation")]
    public Transform buildingRoot;
    public GameObject buildingPrefab;    
    public int buildingSpan = 2;
    List<GameObject> buildingList = new List<GameObject>();

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    EarthFace[] terrainFaces;

    Noise noise = new Noise();

    private void OnValidate()
    {
        //Generate();
        MakeHeightColorMapFromGradient();

        //earthMaterial.SetTexture("_heightColorTexture", heightColorTex);
        //earthMaterial.SetVector("_elevationMinMax", new Vector4(minFactor, maxFactor));
        earthMaterial.SetTexture("_HeightColorTex", heightColorTex);
    }

    private void Generate()
    {
        Initialize();
        GenerateMesh();
        GenerateColours();
    }

    void Initialize()
    {
        minFactor = float.MaxValue;
        maxFactor = float.MinValue;

        for (int i = this.buildingRoot.childCount; i > 0; --i)
            DestroyImmediate(this.buildingRoot.GetChild(0).gameObject);

        if (buildingList == null)
            buildingList = new List<GameObject>();
        buildingList.Clear();



        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new EarthFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = earthMaterial;
            terrainFaces[i] = new EarthFace(this, meshFilters[i].sharedMesh, resolution, directions[i]);
        }
    }

    void GenerateMesh()
    {
        foreach (EarthFace face in terrainFaces)
        {
            face.ConstructMesh();
        }

        
    }

     
    void MakeHeightColorMapFromGradient()
    {
        if (heightColorTex == null)
            heightColorTex = new Texture2D(earthHeightColorResolution, 1);

        Color[] colors = new Color[earthHeightColorResolution];
        for(int i = 0; i < earthHeightColorResolution; i++)
        {
            // Debug.Log("Picker:" + i / (earthHeightColorResolution - 1.0f));

            colors[i] = earthHeightColor.Evaluate(i / (earthHeightColorResolution - 1.0f));
        }

        heightColorTex.SetPixels(colors);
        heightColorTex.Apply();        
    }

    void GenerateColours()
    {
        //Debug.Log("Min " + minFactor);
        //Debug.Log("Max " + maxFactor);
        MakeHeightColorMapFromGradient();

        //earthMaterial.SetTexture("_heightColorTexture", heightColorTex);
        //earthMaterial.SetVector("_elevationMinMax", new Vector4(minFactor, maxFactor));
        earthMaterial.SetTexture("_HeightColorTex", heightColorTex);

        earthMaterial.SetFloat("_Min", minFactor);
        earthMaterial.SetFloat("_Max", maxFactor);
    }


    public float GetNoiseFactor(Vector3 pointOnUnitSphere, int index)
    {
        EarthNoise earthNoise = noiseArray[index];
        float factor = 0;
        var layerFreq = earthNoise.noiseFrequency;
        var layerStrenth = 1.0f;
        for (int i = 0; i < earthNoise.noiseMultiLayer; i++)
        {
            var layerFactor = 0.0f;
            if (index == 0)
                layerFactor = (noise.Evaluate(pointOnUnitSphere * layerFreq + earthNoise.noiseOffset) + 1) * 0.5f * layerStrenth;
            else
            {
                var decimalValue = 1 - Mathf.Abs(noise.Evaluate(pointOnUnitSphere * layerFreq + earthNoise.noiseOffset));
                layerFactor = decimalValue * decimalValue  * layerStrenth;
            }
                
            factor += layerFactor;
            layerFreq *= earthNoise.noiseFrequencyMulti;
            layerStrenth *= earthNoise.noiseStrenthMulti;
        }

        // Make a threshould so that only value > threshould get shown
        // Others all keep 0
        factor = Mathf.Max(0, factor - earthNoise.noiseThreshould);

        return  factor * earthNoise.noiseStrength;
    }

    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
    {
        float noiseFactor0 = GetNoiseFactor(pointOnUnitSphere, 0);
        float noiseFactor = noiseFactor0;
        //float noiseFactor = 0.001f;
        if (noiseFactor0 > 0)
        {
            for (int i = 1; i < noiseArray.Length; i++)
            {
                noiseFactor += GetNoiseFactor(pointOnUnitSphere, i) * noiseFactor0;
            }
        }

        float overAllFactor = radius * (1 + noiseFactor);

        if (overAllFactor > maxFactor)
            maxFactor = overAllFactor;
        if (overAllFactor < minFactor)
            minFactor = overAllFactor;

        var ret = pointOnUnitSphere * overAllFactor;
       // GenearteBuilding(ret);
        return ret;
    }

    public void GenearteBuilding(Vector3 terrainPosition)
    {
        if (terrainPosition.magnitude <= radius)
        {
            return;
        }
        var build = Instantiate(buildingPrefab, buildingRoot);
        // buildingList.Add(build);

        build.transform.localPosition = terrainPosition;
        build.transform.rotation = Quaternion.FromToRotation(build.transform.up, terrainPosition);
        build.GetComponent<Building>().Generate();
        //build.
    }

    [Button]
    public void Refresh()
    {
        Generate();
    }

    bool GreaterThanOne(float property)
    {
        return property > 1;
    }

    bool LessThenOne(float property)
    {
        return property < 1;
    }

    bool GreaterAndEqualThanOne(int property)
    {
        return property >= 1;
    }

    
}