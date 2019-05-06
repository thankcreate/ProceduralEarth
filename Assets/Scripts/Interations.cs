using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interations : MonoBehaviour
{
    Earth currentPlanet;
    Camera cam;


    [Title("Camera Control")]
    public Transform rootY;
    public Transform rootX;
    public Transform camEarthAnchor;
    public float scrollScale = 1;
    public float rotateScale = 1;
    public float dragScale = 0.1f;
    public float mvScale = 0.1f;

    [Title("City Generation")]
    public GameObject[] buildingPrefabs;
    public bool sameDirEveryClick = true;
    public float magicFactor = 1.1f;
    public float cityGenInterval = 0.5f;
    public float heightGrowLerp = 3.0f;
    public float degreePerClick = 8;
    public float contourFre = 1;
    public int verticalBlock = 2;
    public int horizontalBlock = 6;
    public float roadWidthFactor = 0.1f;
    int extendPerClick = 3;
    public int cbdNumber = 1;
    public float cbdProportion = 1f;

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

    Vector2[] cbdPositions;
    int buildingCount = 80;
    // Start is called before the first frame update
    void Start()
    {
        currentPlanet = GetComponent<Earth>();
        cam = Camera.main;
        PreCalc();
        SaveOriginalInfo();
    }


    Vector3 oriRootYRot;
    Vector3 oriRootXRot;
    Vector3 oriEarthCamPosi;
    void SaveOriginalInfo()
    {
        oriRootXRot = rootX.localEulerAngles;
        oriRootYRot = rootY.localEulerAngles;
        oriEarthCamPosi = camEarthAnchor.localPosition;
    }

    void RestoreOriginalInfo()
    {
        rootX.localEulerAngles = oriRootXRot;
        rootY.localEulerAngles = oriRootYRot;
        cam.transform.localPosition = oriEarthCamPosi;
    }


    void PreCalc()
    {       
        var boundsSize = buildingPrefabs[0].transform.localScale.x * 2 * magicFactor;
        buildingCount = (int)(2 * Mathf.PI / boundsSize);

        extendPerClick = (int)(degreePerClick / 360 * buildingCount);

        Debug.Log("extendPerClick: " + extendPerClick);

        contourNoiseoffsetX = Random.Range(0f, 10f);
        contourNoiseoffsetY = Random.Range(0f, 10f);

        float parkMinimumEdge = 0.2f;
        //centralParkMin.x = Random.Range(parkMinimumEdge, 1 - centralParkSize.x - parkMinimumEdge);
        //centralParkMin.y = Random.Range(parkMinimumEdge, 1 - centralParkSize.y - parkMinimumEdge);

        var magOfStandardSize = degreePerClick / 25;
        var adjustedCentralParkSize = centralParkSize / magOfStandardSize;

        centralParkMin.x = Random.Range(-middleProportion, middleProportion - adjustedCentralParkSize.x);
        centralParkMin.y = Random.Range(-middleProportion, middleProportion - adjustedCentralParkSize.y);
        centralParkMax.x = centralParkMin.x + adjustedCentralParkSize.x;
        centralParkMax.y = centralParkMin.y + adjustedCentralParkSize.y;

        cbdPositions = new Vector2[cbdNumber];
        if(cbdNumber == 1)
        {
            cbdPositions[0] = Vector2.zero;
        }
        else
        {
            for(int i = 0; i < cbdNumber; i++)
            {
                cbdPositions[i] = new Vector2(Random.Range(-cbdProportion, cbdProportion), Random.Range(-cbdProportion, cbdProportion));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        CheckIfSpaceClicked();
        CheckIfMouseClicked();

        CamControl();
        CheckOtherKeys();

        lastMousePosi = Input.mousePosition;
    }

    private void CheckOtherKeys()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            ClearAllBuildings();
            if(cor != null)
            {                
                StopCoroutine(cor);                
                inGeneration = false;
            }
        }

        
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

        Vector3 arrow = Vector3.zero;
        if (Input.GetKey(KeyCode.DownArrow))
            arrow.y = -1;
        else if (Input.GetKey(KeyCode.UpArrow))
            arrow.y = 1;
        else if (Input.GetKey(KeyCode.LeftArrow))
            arrow.x = -1;
        else if (Input.GetKey(KeyCode.RightArrow))
            arrow.x = 1;

        if(arrow != Vector3.zero)
        {
            lp = cam.transform.localPosition;
            lp += arrow * mvScale * Time.deltaTime;
            cam.transform.localPosition = lp;

            //var camRootX = cam.transform.parent;
            //var camRootXLp = camRootX.localPosition;
            //camRootXLp += arrow * mvScale * Time.deltaTime;
            //camRootX.localPosition = camRootXLp;
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            RestoreOriginalInfo();
        }

        if (Input.GetMouseButton(0))
        {
            //var ray = cam.ScreenPointToRay(Input.mousePosition);
            //RaycastHit hit;
            //if (Physics.Raycast(ray, out hit, 1000) )
            //{
            //    return;
            //}
            if (Input.GetMouseButtonDown(0))
                return;

            var deltaPosi = Input.mousePosition - lastMousePosi;
            var dragDis = rotateScale * deltaPosi;
            var camRootX = cam.transform.parent;
            var rt = camRootX.localEulerAngles;
            rt.x += -dragDis.y;
            if(rt.x > 180)
            {
                rt.x = rt.x - 360;
            }
            rt.x = Mathf.Clamp(rt.x, -89.0f, 89.0f);
            camRootX.localEulerAngles = rt;

            var camRootY = cam.transform.parent.parent;
            rt = camRootY.localEulerAngles;
            rt.y += dragDis.x;
            camRootY.localEulerAngles = rt;
        }

        // Drag logic
        if (Input.GetMouseButton(2))
        {
            var deltaPosi = Input.mousePosition - lastMousePosi;

            lp = cam.transform.localPosition;
            lp += dragScale * deltaPosi * -1;
            cam.transform.localPosition = lp;
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
    Coroutine cor;
    IEnumerator lastEu;
    private void HandleMouseDown()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 1000))
        {
            var hitPoint = hit.point;
            if(buildingPrefabs[0])
            {                
                var onLand = currentPlanet.IsOnLand(hitPoint);
                if (onLand && !inGeneration)
                {
                    inGeneration = true;
                    var localHitPoint = transform.InverseTransformPoint(hitPoint);                    
                    cor =  StartCoroutine(GenerateCity(localHitPoint));                    
                }
                    
                
            }
        }
    }

    int counter = 0;
    IEnumerator GenerateCity(Vector3 localHitPoint)
    {

        PreCalc();

        counter = 0;
        Debug.Log("GenerateCity Begin");
        

        for (int i = 0; i < extendPerClick; i++)
        {
            var hitPoint = transform.TransformPoint(localHitPoint);
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
                Debug.Log("Reach limit at i: " + i);
                break;
            }

        }

        Debug.Log("GenerateCity End: " + counter + "buildings generated");
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
        Vector2 usedPolar = sameDirEveryClick ? polarHitPoint : polarBuilding;
        var buildingEular = new Vector3(90 + usedPolar.x, usedPolar.y, 0);
        

        var onLand = currentPlanet.IsOnLand(worldBuilding);
        if (onLand)
        {
            var prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            var building = Instantiate(prefab, worldBuilding, Quaternion.identity , currentPlanet.buildingRoot);
            building.SetActive(true);
            building.transform.localEulerAngles = buildingEular;

            var bw = building.GetComponent<BuildingWrapper>();
            if(bw)
            {
                var ls = building.transform.localScale;
                bw.ScaleToY(ls.y * GetHeightMultiplier(building, polarX, polarY), heightGrowLerp);
            }
            counter++;
        }
        return onLand;
    }

    const int STANDARD_HEIGHT = 14;
    float GetHeightMultiplier(GameObject go, int polarX, int polarY)
    {
        var ret = GetHeightFactorOverall(polarX, polarY) * GetHeightFactorRadial(polarX, polarY);
        var script = go.transform.GetChild(0).GetComponent<BuildingHighLevel>();
        var floorSum = 0;
        foreach(var section in script.sectionFloors)
        {
            floorSum += section;
        }

        ret /= 1.0f * floorSum / STANDARD_HEIGHT;
        return ret;
    }

    float GetHeightFactorOverall(int polarX, int polarY)
    {
        float ret = 1;

        var normX = 1.0f * polarX / extendPerClick;
        var normY = 1.0f * polarY / extendPerClick;
        var freq = allHeightFreq;
        var zeroToOneNoise = (Perlin.Noise(normX * freq + contourNoiseoffsetX,
            normY * freq + contourNoiseoffsetY) + 1) / 2;

        // Debug.Log("zeroToOneNoise: " + zeroToOneNoise);
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
        var normPosi = new Vector2(polarX, polarY) * 1.0f / extendPerClick;
        float minDistance = float.MaxValue;
        foreach(var cbdPosi in cbdPositions)
        {
            var dis = Vector2.Distance(cbdPosi, normPosi);
            minDistance = Mathf.Min(minDistance, dis);
        }
        var distance = minDistance;


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

        // color
        if (!currentPlanet.pureRandomTime)
        {
            currentPlanet.RestoreToOriginalGradientColor();
        }

        var colorKeys = currentPlanet.earthHeightColor.colorKeys;
        var baseRandom = Random.value;
        for(int i = 0; i < colorKeys.Length; i++)
        {
            var hue = baseRandom + i * currentPlanet.hueInterval;
            var color = Color.white;

            if(currentPlanet.pureRandomColor)
            {
                color = Random.ColorHSV();
            }
            else
            {
                color = Color.HSVToRGB(hue, 0.5f, 0.5f);
            }

            Debug.Log("HUES: " + hue);

            colorKeys[i].color = color;

            if(currentPlanet.pureRandomTime)
            {
                colorKeys[i].time = Random.value;
            }   
        }

        // basic layer
        var layer0 = currentPlanet.noiseArray[0];
        layer0.noiseFrequency = Random.Range(0.8f, 1.5f);
        layer0.noiseOffset = new Vector3(Random.value, Random.value, Random.value) * 10;
        layer0.noiseThreshould = 0;
        //layer0.noiseThreshould = Random.Range(0.0f, 0.8f);


        // ridge layer
        var layer1 = currentPlanet.noiseArray[1];
        layer1.noiseFrequency = Random.Range(1.6f, 2.0f);
        layer1.noiseOffset = new Vector3(Random.value, Random.value, Random.value) * 10;
        layer1.noiseMultiLayer = Random.Range(1, 3);

        currentPlanet.earthHeightColor.SetKeys(colorKeys, currentPlanet.earthHeightColor.alphaKeys);
        currentPlanet.Generate();
    }

    [Button]
    public void ClearAllBuildings()
    {
        currentPlanet.ClearAllBuildings();
    }
}
