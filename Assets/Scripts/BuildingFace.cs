using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BuildingFace 
{
    public Vector2[] baseShape;
    public float height;
    public float floorHeight = 0.2f;
    public int floors = 10;
    MeshFilter meshFilter;
    Mesh mesh;
    MeshRenderer meshRenderer;

    Vector3[] vertices;
    Vector2[] uvs;
    int[] tris;
    
    public Material material;

    void InitProperty()
    {
        
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter.sharedMesh == null)
        {
            meshFilter.sharedMesh = new Mesh();
        }
        mesh = meshFilter.sharedMesh;

        meshRenderer = GetComponent<MeshRenderer>();


        int verticesCount = baseShape.Length * (4 + 6);
        vertices = new Vector3[verticesCount];
        uvs = new Vector2[verticesCount];

        int trisCount = baseShape.Length * (2 + 2) * 3;
        tris = new int[trisCount];


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
        height = floorHeight * floors;
        float uvScale = 1.0f / 32 / floorHeight;




        var sideCount = baseShape.Length;
        var verticesIndex = 0;
        var trisIndex = 0;

        // Bottom line accumulated length, used to calculate UV
        float bottomLineLength = 0;
        for(int i = 0; i < sideCount; i++)
        {
            var leftBottomPosi = new Vector3(baseShape[i].x, 0, baseShape[i].y);
            var leftTopPosi = new Vector3(baseShape[i].x, height, baseShape[i].y);
            var rightBottomPosi = new Vector3(baseShape.Looped(i + 1).x, 0, baseShape.Looped(i + 1).y);
            var rightTopPosi = new Vector3(baseShape.Looped(i + 1).x, height, baseShape.Looped(i + 1).y);

            var edgeDistance = Vector3.Distance(leftBottomPosi, rightBottomPosi);

            var leftBottomIndex = verticesIndex;

            uvs[verticesIndex] = new Vector2(bottomLineLength, 0) * uvScale;           
            vertices[verticesIndex++] = leftBottomPosi;
            uvs[verticesIndex] = new Vector2(bottomLineLength, height) * uvScale;
            vertices[verticesIndex++] = leftTopPosi;
            
            uvs[verticesIndex] = new Vector2(bottomLineLength + edgeDistance, 0) * uvScale;
            vertices[verticesIndex++] = rightBottomPosi;
            uvs[verticesIndex] = new Vector2(bottomLineLength + edgeDistance, height) * uvScale;
            vertices[verticesIndex++] = rightTopPosi;

            
            var leftTopIndex = leftBottomIndex + 1;
            var rightBottomIndex = leftBottomIndex + 2;
            var rightTopIndex = leftBottomIndex + 3;
            // Tris
            tris[trisIndex++] = leftBottomIndex;
            tris[trisIndex++] = rightTopIndex;
            tris[trisIndex++] = rightBottomIndex;

            tris[trisIndex++] = leftBottomIndex;
            tris[trisIndex++] = leftTopIndex;
            tris[trisIndex++] = rightTopIndex;            

            bottomLineLength += edgeDistance;
        }

        var bottomCenterPosi = new Vector3(0, 0, 0);
        var topCenterPosi = new Vector3(0, height, 0);
        for (int i = 0; i < sideCount; i++)
        {
            var leftBottomPosi = new Vector3(baseShape[i].x, 0, baseShape[i].y);
            var leftTopPosi = new Vector3(baseShape[i].x, height, baseShape[i].y);
            var rightBottomPosi = new Vector3(baseShape.Looped(i + 1).x, 0, baseShape.Looped(i + 1).y);
            var rightTopPosi = new Vector3(baseShape.Looped(i + 1).x, height, baseShape.Looped(i + 1).y);

            // Bottom
            var leftBottomIndex = verticesIndex;
            uvs[verticesIndex] = new Vector2(0, 0);
            vertices[verticesIndex++] = leftBottomPosi;
            uvs[verticesIndex] = new Vector2(0, 0);
            vertices[verticesIndex++] = rightBottomPosi;
            uvs[verticesIndex] = new Vector2(0, 0);
            vertices[verticesIndex++] = bottomCenterPosi;


            var rightBottomIndex = leftBottomIndex + 1;
            var bottomCenterIndex = leftBottomIndex + 2;

            tris[trisIndex++] = leftBottomIndex;
            tris[trisIndex++] = rightBottomIndex;
            tris[trisIndex++] = bottomCenterIndex;

            // Top
            var leftTopIndex = leftBottomIndex + 3;
            var topCenterIndex = leftBottomIndex + 4;
            var rightTopIndex = leftBottomIndex + 5;

            uvs[verticesIndex] = new Vector2(0, 0);
            vertices[verticesIndex++] = leftTopPosi;
            uvs[verticesIndex] = new Vector2(0, 0);
            vertices[verticesIndex++] = topCenterPosi;
            uvs[verticesIndex] = new Vector2(0, 0);
            vertices[verticesIndex++] = rightTopPosi;

            tris[trisIndex++] = leftTopIndex;
            tris[trisIndex++] = topCenterIndex;
            tris[trisIndex++] = rightTopIndex;

        }
        


        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }



}
