using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interations : MonoBehaviour
{
    Earth currentPlanet;
    Camera cam;
    
    

    [Title("Camera Control")]
    public float scrollScale = 1;
    public float rotateScale = 1;

    [Title("City Generation")]
    public GameObject testPrefab;
    public float magicFactor = 1.1f;
    public float cityGenInterval = 0.5f;
    public float degreePerClick = 8;
    public float contourFre = 1;
    public int verticalBlock = 2;
    public int horizontalBlock = 6;
    public float roadWidthFactor = 0.1f;
    int extendPerClick = 3;

    [Title("Central Park")]
    public Vector2 centralParkSize = new Vector2(0.2f, 0.03f);

    [Title("City Height")]
    [Tooltip("Overall freqency")]
    public float allHeightFreq = 1;
    public float allMaxHeight = 1.5f;
    public float allMinHeight = 1f;

    
    public float centerProportion = 0.25f;
    public float centerMaxHeight = 8f;
    public float centerMinHeight = 5f;
    public float centerExp = 1;

    public float middleProportion = 0.5f;
    public float middleMaxHeight = 3f;
    public float middleMinHeight = 1f;
    public float middleExp = 1;


    int buildingCount = 80;
    // Start is called before the first frame update
    void Start()
    {
        currentPlanet = GetComponent<Earth>();
        cam = Camera.main;
        PreCalc();
    }

    void PreCalc()
    {       
        var boundsSize = testPrefab.transform.localScale.x * 2 * magicFactor;
        buildingCount = (int)(2 * Mathf.PI / boundsSize);

        extendPerClick = (int)(degreePerClick / 360 * buildingCount);

        contourNoiseoffsetX = Random.Range(0f, 10f);
        contourNoiseoffsetY = Random.Range(0f, 10f);

        float parkMinimumEdge = 0.2f;
        //centralParkMin.x = Random.Range(parkMinimumEdge, 1 - centralParkSize.x - parkMinimumEdge);
        //centralParkMin.y = Random.Range(parkMinimumEdge, 1 - centralParkSize.y - parkMinimumEdge);
        centralParkMin.x = Random.Range(-middleProportion, middleProportion - centralParkSize.x);
        centralParkMin.y = Random.Range(-middleProportion, middleProportion - centralParkSize.y);
        centralParkMax.x = centralParkMin.x + centralParkSize.x;
        centralParkMax.y = centralParkMin.y + centralParkSize.y;

    }

    // Update is called once per frame
    void Update()
    {

        CheckIfSpaceClicked();
        CheckIfMouseClicked();

        CamControl();


        lastMousePosi = Input.mousePosition;
    }

    Vector3 lastMousePosi;
    void CamControl()
    {
        var scrollDis = scrollScale * Input.mouseScrollDelta.y;



        var worldForward = cam.transform.forward;
        var localForward = cam.transform.parent.InverseTransformDirection(worldForward);

        var lp = cam.transform.localPosition;
        lp += scrollDis * localForward;
        cam.transform.localPosition = lp;

        if (Input.GetMouseButton(0))
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000))
            {
                return;
            }

            var deltaPosi = Input.mousePosition - lastMousePosi;
            var dragDis = rotateScale * deltaPosi;
            var camParent = cam.transform.parent;
            var rt = camParent.localEulerAngles;
            rt.y += dragDis.x;
            rt.x += dragDis.y;
            camParent.localEulerAngles = rt;
        }
    }

    void CheckIfMouseClicked()
    {
        if(Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
    }

    bool inGeneration = false;
    private void HandleMouseDown()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 1000))
        {
            var hitPoint = hit.point;
            if(testPrefab)
            {                
                var onLand = currentPlanet.IsOnLand(hitPoint);
                if (onLand && !inGeneration)
                {
                    inGeneration = true;
                    StartCoroutine(GenerateCity(hitPoint));
                }
                    
                
            }
        }
    }

    IEnumerator GenerateCity(Vector3 hitPoint)
    {
        PreCalc();

        for (int i = 0; i < extendPerClick; i++)
        {
            bool atLeastOneGenerated = false;
            if (i == 0)
            {
                bool res = GenerateBuilding(0, 0, hitPoint);
                if (res)
                    atLeastOneGenerated = true;
            }
                

            int left = -i;
            int right = i;
            int top = -i;
            int bottom = i;


            for (int j = left; j < right; j++)
            {
                bool res = GenerateBuilding(top, j, hitPoint);
                if (res)
                    atLeastOneGenerated = true;
            }
            for (int j = top; j < bottom; j++)
            {
                bool res = GenerateBuilding(j, right, hitPoint);
                if (res)
                    atLeastOneGenerated = true;
            }
            for (int j = right; j > left; j--)
            {
                bool res = GenerateBuilding(bottom, j, hitPoint);
                if (res)
                    atLeastOneGenerated = true;
            }
            for (int j = bottom; j > top; j--)
            {
                bool res = GenerateBuilding(j, left, hitPoint);
                if (res)
                    atLeastOneGenerated = true;
            }

            if(atLeastOneGenerated)
                yield return new WaitForSeconds(cityGenInterval);
            else
            {
                break;
            }

        }

        inGeneration = false;
        yield return null;
    }



    bool GenerateBuilding(int polarX, int polarY, Vector3 hitPoint)
    {
        if(!IsValid(polarX, polarY))
        {
            return false;
        }
        
        var polarHitPoint = currentPlanet.GetPolarPosiFromWorld(hitPoint);

        float intervalX = 360f / buildingCount;
        float intervalY = 360f / buildingCount;

        // Debug.Log("PolarPoint: " + polarHitPoint);

        // Polar system offset from zero point(click point)
        var offsetX = intervalX * polarX + polarX / verticalBlock * roadWidthFactor * intervalX;
        var offsetY = intervalY * polarY + polarY / horizontalBlock * roadWidthFactor * intervalY;

        var polarBuilding = new Vector2(polarHitPoint.x + offsetX, 
            polarHitPoint.y + offsetY);

        var worldBuilding = currentPlanet.GetWorldTerrainPosiFromPoloar(polarBuilding);

        //var radialDir = worldBuilding - transform.position;
        //var worldDir = Quaternion.FromToRotation(Vector3.up, radialDir);
        var buildingEular = new Vector3(90 + polarHitPoint.x, polarHitPoint.y, 0);
        

        var onLand = currentPlanet.IsOnLand(worldBuilding);
        if (onLand)
        {
            var building = Instantiate(testPrefab, worldBuilding, Quaternion.identity , currentPlanet.buildingRoot);
            building.transform.localEulerAngles = buildingEular;
            var ls = building.transform.localScale;
            ls.y *= GetHeightFactorOverall(polarX, polarY) * GetHeightFactorRadial(polarX, polarY);
            building.transform.localScale = ls;

        }
        return onLand;
    }

    float GetHeightFactorOverall(int polarX, int polarY)
    {
        float ret = 1;

        var normX = 1.0f * polarX / extendPerClick;
        var normY = 1.0f * polarY / extendPerClick;
        var freq = allHeightFreq;
        var zeroToOneNoise = (Perlin.Noise(normX * freq + contourNoiseoffsetX,
            normY * freq + contourNoiseoffsetY) + 1) / 2;

        Debug.Log("zeroToOneNoise: " + zeroToOneNoise);
        var minHeight = allMinHeight;
        var maxHeight = allMaxHeight;

        ret = Mathf.Lerp(minHeight, maxHeight, zeroToOneNoise);

        return ret;
    }
    float GetHeightFactorRadial(int polarX, int polarY)
    {        
        float ret = 1;

        var normX = 1.0f * polarX / extendPerClick;
        var normY = 1.0f * polarY / extendPerClick;
        var distance = new Vector2(normX, normY).magnitude;


        if (distance < centerProportion)
            ret = MapToSin(0, centerProportion, centerMinHeight, centerMaxHeight, distance, centerExp);
        else if (distance < middleProportion)
            ret = MapToSin(centerProportion, middleProportion, middleMinHeight, middleMaxHeight, distance, middleExp);
        else
            ret = 1;

        return ret;
    }

    float MapToSin(float minX, float maxX, float minY, float maxY, float x, float exp)
    {
        var propX = Mathf.InverseLerp(minX, maxX, x);

        var sinZeroToOne = Mathf.Sin(propX * Mathf.PI / 2 + Mathf.PI / 2);
        var powed = Mathf.Pow(sinZeroToOne, exp);

        var ret = Mathf.Lerp(minY, maxY, powed);
        return ret;
    }

    float contourNoiseoffsetX;
    float contourNoiseoffsetY;
    bool IsValid(int polarX, int polarY)
    {
        var shapeValid = IsValidInMyNoiseShape(polarX, polarY);
        var inCentralPark = IsInCentralPark(polarX, polarY);
        var valid = shapeValid && !inCentralPark;
        return valid;
    }

    bool IsValidInMyNoiseShape(int polarX, int polarY)
    {
        bool valid = false;
        var max = extendPerClick;
        float freq = contourFre;
        var distance = new Vector2(polarX, polarY).magnitude;
        var normalized = new Vector2(polarX, polarY).normalized;

        var zeroToOneNoise = (Perlin.Noise(normalized.x * freq + contourNoiseoffsetX,
            normalized.y * freq + contourNoiseoffsetY) + 1) / 2;

        var min = extendPerClick / 3;
        var limitationDistance = Mathf.Lerp(min, max, zeroToOneNoise);

        valid = distance < limitationDistance;


        return valid;
    }

    Vector2 centralParkMin;
    Vector2 centralParkMax;
    bool IsInCentralPark(int polarX, int polarY)
    {
        var normX = 1.0f * polarX / extendPerClick;
        var normY = 1.0f * polarY / extendPerClick;
        if (normX > centralParkMin.x 
            && normY > centralParkMin.y
            && normX < centralParkMax.x
            && normY < centralParkMax.y)
        {
            return true;
        }
        return false;
    }

    void CheckIfSpaceClicked()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ChangeCurrentPlanet();
        }
    }

    void ChangeCurrentPlanet()
    {
        if (currentPlanet == null)
            return;

        var colorKeys = currentPlanet.earthHeightColor.colorKeys;
        for(int i = 0; i < colorKeys.Length; i++)
        {
            colorKeys[i].color = Random.ColorHSV();
            // colorKeys[i].time = Random.value;            
        }

        var layer0 = currentPlanet.noiseArray[0];
        layer0.noiseFrequency = Random.Range(0.3f, 3);
        layer0.noiseOffset = new Vector3(Random.value, Random.value, Random.value) * 10;
        layer0.noiseThreshould = 0;
        //layer0.noiseThreshould = Random.Range(0.0f, 0.8f);



        currentPlanet.earthHeightColor.SetKeys(colorKeys, currentPlanet.earthHeightColor.alphaKeys);
        currentPlanet.Generate();
    }

    [Button]
    public void ClearAllBuildings()
    {
        currentPlanet.ClearAllBuildings();
    }
}
