using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;

public class Earth : MonoBehaviour
{
    [Title("Preset")]
    public Preset earthPreset;
    [Title("Noise")]
    [Range(0, 0.1f)]
    public float overAllNoiseFactor = 1;
    public EarthNoise[] noiseArray;
    [Title("Shape")]
    public Transform meshRoot;
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
    public bool pureRandomColor;
    public float hueInterval = 0.1f;
    public bool pureRandomTime;

    const int earthHeightColorResolution = 100;
    Texture2D heightColorTex;    
    
    float minFactor = float.MaxValue;
    float maxFactor = float.MinValue;

    [Title("Building Generation")]
    public Transform buildingRoot;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    EarthFace[] terrainFaces;

    Noise noise = new Noise();

    void Start()
    {
        SaveOriginalGradientColor();
        GenerateColorTexture();
        ReadMinMaxHeightFromMaterial();
    }

    List<GradientColorKey> originalColorList;

    public void SaveOriginalGradientColor()
    {
        originalColorList = new List<GradientColorKey>();
        foreach (var c in earthHeightColor.colorKeys)
        {
            originalColorList.Add(c);
        }
    }

    public void RestoreToOriginalGradientColor()
    {        
        earthHeightColor.SetKeys(originalColorList.ToArray(), earthHeightColor.alphaKeys);
    }

    public void GenerateColorTexture()
    {
        //Generate();
        MakeHeightColorMapFromGradient();

        //earthMaterial.SetTexture("_heightColorTexture", heightColorTex);
        //earthMaterial.SetVector("_elevationMinMax", new Vector4(minFactor, maxFactor));
        earthMaterial.SetTexture("_HeightColorTex", heightColorTex);


    }

    public Noise GetNoise()
    {
        return noise;
    }

    void ReadMinMaxHeightFromMaterial()
    {
        minFactor = earthMaterial.GetFloat("_Min");
        maxFactor = earthMaterial.GetFloat("_Max");
    }

    private void OnValidate()
    {
        //Generate();
        MakeHeightColorMapFromGradient();

        //earthMaterial.SetTexture("_heightColorTexture", heightColorTex);
        //earthMaterial.SetVector("_elevationMinMax", new Vector4(minFactor, maxFactor));
        earthMaterial.SetTexture("_HeightColorTex", heightColorTex);
    }

    public  void Generate()
    {
        Debug.Log("OnValidateEarth");
        Initialize();
        GenerateMesh();
        GenerateColours();
    }

    public void ClearAllBuildings()
    {
        for (int i = this.buildingRoot.childCount; i > 0; --i)
            DestroyImmediate(this.buildingRoot.GetChild(0).gameObject);
    }

    public void DestroyAllMeshes()
    {
        for (int i = this.meshRoot.childCount; i > 0; --i)
            DestroyImmediate(this.meshRoot.GetChild(0).gameObject);

        for (int i = 0; i < meshFilters.Length; i++)
            meshFilters[i] = null;

    }

    void Initialize()
    {
        minFactor = float.MaxValue;
        maxFactor = float.MinValue;


        ClearAllBuildings();
        DestroyAllMeshes();


        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new EarthFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        //if(meshRoot.childCount == 0)
        //{

        //    for (int i = 0; i < meshFilters.Length; i++)
        //        meshFilters[i] = null;
        //}

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null || meshRoot.childCount == 0)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = meshRoot;
                meshObj.transform.localPosition = Vector3.zero;
                meshObj.transform.localEulerAngles = Vector3.zero;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            var mr = meshFilters[i].GetComponent<MeshRenderer>();
            mr.sharedMaterial = earthMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
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
        GenerateColorTexture();

        earthMaterial.SetFloat("_Min", minFactor);
        earthMaterial.SetFloat("_Max", maxFactor);
    }


    public float GetNoiseFactor(Vector3 pointOnUnitSphere, int index)
    {
        EarthNoise earthNoise = noiseArray[index];
        float factor = 0;
        var layerFreq = earthNoise.noiseFrequency;
        var layerStrenth = 1.0f;
        var lastLayerV = 1.0f; //only used in ridge generation
        for (int i = 0; i < earthNoise.noiseMultiLayer; i++)
        {
            var layerFactor = 0.0f;
            if (index == 0)
                layerFactor = (noise.Evaluate(pointOnUnitSphere * layerFreq + earthNoise.noiseOffset) + 1) * 0.5f * layerStrenth;
            else
            {
                var decimalValue = 1 - Mathf.Abs(noise.Evaluate(pointOnUnitSphere * layerFreq + earthNoise.noiseOffset));
                var powed = Mathf.Pow(decimalValue, 2);
                var multiplyByLastLayerV = powed * lastLayerV;
                lastLayerV = multiplyByLastLayerV;
                layerFactor = multiplyByLastLayerV * layerStrenth;
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

    public float CalculatePointFactorOnPlanet(Vector3 pointOnUnitSphere)
    {
        float noiseFactor0 = GetNoiseFactor(pointOnUnitSphere, 0);

        float noiseFactor = 0.0f;
        noiseFactor += noiseFactor0;
        if (noiseFactor0 > 0)
        {
            for (int i = 1; i < noiseArray.Length; i++)
            {
                noiseFactor += GetNoiseFactor(pointOnUnitSphere, i) * noiseFactor0;
            }
        }

        float overAllFactor = radius * (1 + noiseFactor * overAllNoiseFactor);

        if (overAllFactor > maxFactor)
            maxFactor = overAllFactor;
        if (overAllFactor < minFactor)
            minFactor = overAllFactor;

        var ret =  overAllFactor;
        return ret;
    }

    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
    {
       
        var ret = pointOnUnitSphere * CalculatePointFactorOnPlanet(pointOnUnitSphere);
       // GenearteBuilding(ret);
        return ret;
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

    public bool IsOnLand(Vector3 worldPosi)
    {
        var relativeHeight = GetRelativeHeight(worldPosi);
        // Debug.Log("RelativeHeight: " + relativeHeight);
        var landSep = earthHeightColor.colorKeys[1].time;

        bool onLand = false;

        if (relativeHeight > landSep)
            onLand = true;
        
        return onLand;
    }

    float GetRelativeHeight(Vector3 worldPosi)
    {
        var localPosi = transform.InverseTransformPoint(worldPosi);
        var unitLocalPosi = localPosi.normalized;

        var factor = CalculatePointFactorOnPlanet(unitLocalPosi);
        var lerp = Mathf.InverseLerp(minFactor, maxFactor, factor);
        return lerp;
    }



    public Vector2 GetPolarPosiFromWorld(Vector3 worldPosi)
    {
        var point = transform.InverseTransformPoint(worldPosi).normalized;

        return GetPolarPosiFromLocal(point);
    }


    public Vector2 GetPolarPosiFromLocal(Vector3 point)
    {
        Vector2 ret = Vector2.zero;
        

        ret.y = Mathf.Atan2(point.x, point.z);
        var xzLen = new Vector2(point.x, point.z).magnitude;
        ret.x = Mathf.Atan2(-point.y, xzLen);

        ret *= Mathf.Rad2Deg;
        return ret;
    }

    public Vector3 GetLocalPosiFromPolar(Vector2 polar)
    {
        Vector3 ret = Vector3.zero;

        var origin = new Vector3(0, 0, 1);
        var rotation = Quaternion.Euler(polar.x, polar.y, 0);
        ret = rotation * origin;
        return ret;
    }

    public Vector3 GetWorldPosiFromPolar(Vector2 polar)
    {
        var localPosi = GetLocalPosiFromPolar(polar);
        return transform.TransformPoint(localPosi);
    }

    

    public Vector3 GetWorldTerrainPosiFromPoloar(Vector2 polar)
    {
        var localPosi = GetLocalPosiFromPolar(polar).normalized;
        var localTerrainPosi = CalculatePointOnPlanet(localPosi);
        var worldTerrainPosi = transform.TransformPoint(localTerrainPosi);
        return worldTerrainPosi;
    }

    public void ApplyEarthPresetBack()
    {
        earthPreset.ApplyTo(this);
    }
}