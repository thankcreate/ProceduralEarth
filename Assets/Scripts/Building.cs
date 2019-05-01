using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Vector2[] baseShape;
    public float height;
    public float floorHeight = 0.2f;
    public int floors = 10;
    MeshFilter meshFilter;
    Mesh mesh;
    MeshRenderer meshRenderer;


    public Material material;

    BuildingFace[] buildingFaces;

    void InitProperty()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
        {
            meshFilter.sharedMesh = new Mesh();
        }
        mesh = meshFilter.sharedMesh;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
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
        Generate();
    }

    void Generate()
    {
        InitProperty();
        GenerateMesh();
    }

    void GenerateMesh()
    {
        
    }



}
