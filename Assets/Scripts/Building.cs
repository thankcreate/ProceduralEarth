using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [HideInInspector]
    public List<Vector3> vertexList;
    [HideInInspector]
    public List<int> triList;
    [HideInInspector]
    public List<Vector2> uvList;

    protected MeshFilter meshFilter;
    protected Mesh mesh;
    protected MeshRenderer meshRenderer;

    [HideIf("HideBuildingFaces")]
    public BuildingFace[] buildingFaces;
    public Material material;


    protected virtual bool HideBuildingFaces()
    {
        return false;
    }


    protected virtual void Initialize()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
        {
            meshFilter.sharedMesh = new Mesh();
        }
        mesh = meshFilter.sharedMesh;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        InitBuildingFaces();
    }

    protected virtual void InitBuildingFaces()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnValidate()
    {
        //Debug.Log("OnValidateBuilding");
        if (Application.isEditor)
            Generate();
    }

    public void Generate()
    {
        Initialize();
        GenerateMesh();
    }

    void GenerateMesh()
    {
        if (vertexList == null)
            vertexList = new List<Vector3>();

        if (triList == null)
            triList = new List<int>();

        if (uvList == null)
            uvList = new List<Vector2>();

        vertexList.Clear();
        triList.Clear();
        uvList.Clear();

        foreach (var bf in buildingFaces)
        {
            bf.InitParam(this, mesh);
            bf.ConstructMesh();
        }

        ApplyDataToMesh();
    }

    void ApplyDataToMesh()
    {
        mesh.Clear();

        mesh.vertices = vertexList.ToArray();
        mesh.triangles = triList.ToArray();
        mesh.uv = uvList.ToArray();
        mesh.RecalculateNormals();
    }


}
