using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildingFace 
{
    public enum BuildingFaceType{
        REGULAR,
        CONNECTION,
        ROOF,
    }

    [SerializeField]
    public Vector2[] baseShape;
    Mesh mesh;
    Building building;

    Vector3[] vertices;
    Vector2[] uvs;
    int[] tris;

    public float floorHeight = 0.1f;
    public int floors = 10;
    public float startHeight = 0;
    public float height;
    float uvScale;

    BuildingFaceType faceType;
    public BuildingFaceType FaceType { set => faceType = value; get => faceType; }



    public BuildingFace(Building building, Mesh mesh)
    {
        InitParam(building, mesh);
    }

    public void InitParam(Building building, Mesh mesh)
    {
        this.building = building;
        this.mesh = mesh;
    }

    void InitProperty()
    {
        int verticesCount = baseShape.Length * (4 + 6);
        vertices = new Vector3[verticesCount];
        uvs = new Vector2[verticesCount];

        int trisCount = baseShape.Length * (2 + 2) * 3;
        tris = new int[trisCount];

        height = GetHeight();
        uvScale = 1.0f / 32 / floorHeight;

        curVerticesIndex = 0;
        curTrisIndex = 0;
    }

    public float GetHeight()
    {
        if (faceType == BuildingFaceType.REGULAR)
            return floorHeight * floors;
        else
            return height;
    }


    int curVerticesIndex = 0;
    int curTrisIndex = 0;
    public void ConstructMesh()
    {
        InitProperty();

        ConstructSideFaces();
        ConstructTopAndBottomFaces();

        ApplyDataToMesh();
    }

    float GetTopY()
    {
        return startHeight + height;
    }

    float GetBottomY()
    {
        return startHeight;
    }

    Vector2 GetQuadUV(float bottomLineLength, float edgeDistance, int x, int y)
    {
        if (faceType == BuildingFaceType.CONNECTION)
            return new Vector2(0, 0);
        else 
            return new Vector2(bottomLineLength + edgeDistance * x, 0 + height * y) * uvScale;
    }

    void ConstructSideFaces()
    {
        var sideCount = baseShape.Length;

        // Bottom line accumulated length, used to calculate UV
        float bottomLineLength = 0;
        for (int i = 0; i < sideCount; i++)
        {
            var leftBottomPosi = new Vector3(baseShape[i].x, GetBottomY(), baseShape[i].y);
            var leftTopPosi = new Vector3(baseShape[i].x, GetTopY(), baseShape[i].y);
            var rightBottomPosi = new Vector3(baseShape.Looped(i + 1).x, GetBottomY(), baseShape.Looped(i + 1).y);
            var rightTopPosi = new Vector3(baseShape.Looped(i + 1).x, GetTopY(), baseShape.Looped(i + 1).y);

            var edgeDistance = Vector3.Distance(leftBottomPosi, rightBottomPosi);

            var leftBottomIndex = curVerticesIndex;

            uvs[curVerticesIndex] = GetQuadUV(bottomLineLength, edgeDistance, 0, 0);
            vertices[curVerticesIndex++] = leftBottomPosi;
            uvs[curVerticesIndex] = GetQuadUV(bottomLineLength, edgeDistance, 0, 1);
            vertices[curVerticesIndex++] = leftTopPosi;

            uvs[curVerticesIndex] = GetQuadUV(bottomLineLength, edgeDistance, 1, 0);
            vertices[curVerticesIndex++] = rightBottomPosi;
            uvs[curVerticesIndex] = GetQuadUV(bottomLineLength, edgeDistance, 1, 1);
            vertices[curVerticesIndex++] = rightTopPosi;


            var leftTopIndex = leftBottomIndex + 1;
            var rightBottomIndex = leftBottomIndex + 2;
            var rightTopIndex = leftBottomIndex + 3;
            // Tris
            tris[curTrisIndex++] = leftBottomIndex;
            tris[curTrisIndex++] = rightTopIndex;
            tris[curTrisIndex++] = rightBottomIndex;

            tris[curTrisIndex++] = leftBottomIndex;
            tris[curTrisIndex++] = leftTopIndex;
            tris[curTrisIndex++] = rightTopIndex;

            bottomLineLength += edgeDistance;
        }
    }

    public void ConstructTopAndBottomFaces()
    {
        var sideCount = baseShape.Length;

        var shapeCenter = GetShapeCenter();

        var bottomCenterPosi = new Vector3(shapeCenter.x, GetBottomY(), shapeCenter.y);
        var topCenterPosi = new Vector3(shapeCenter.x, GetTopY(), shapeCenter.y);
        for (int i = 0; i < sideCount; i++)
        {
            var leftBottomPosi = new Vector3(baseShape[i].x, GetBottomY(), baseShape[i].y);
            var leftTopPosi = new Vector3(baseShape[i].x, GetTopY(), baseShape[i].y);
            var rightBottomPosi = new Vector3(baseShape.Looped(i + 1).x, GetBottomY(), baseShape.Looped(i + 1).y);
            var rightTopPosi = new Vector3(baseShape.Looped(i + 1).x, GetTopY(), baseShape.Looped(i + 1).y);

            // Bottom
            var leftBottomIndex = curVerticesIndex;
            uvs[curVerticesIndex] = new Vector2(0, 0);
            vertices[curVerticesIndex++] = leftBottomPosi;
            uvs[curVerticesIndex] = new Vector2(0, 0);
            vertices[curVerticesIndex++] = rightBottomPosi;
            uvs[curVerticesIndex] = new Vector2(0, 0);
            vertices[curVerticesIndex++] = bottomCenterPosi;


            var rightBottomIndex = leftBottomIndex + 1;
            var bottomCenterIndex = leftBottomIndex + 2;

            tris[curTrisIndex++] = leftBottomIndex;
            tris[curTrisIndex++] = rightBottomIndex;
            tris[curTrisIndex++] = bottomCenterIndex;

            // Top
            var leftTopIndex = leftBottomIndex + 3;
            var topCenterIndex = leftBottomIndex + 4;
            var rightTopIndex = leftBottomIndex + 5;

            uvs[curVerticesIndex] = new Vector2(0, 0);
            vertices[curVerticesIndex++] = leftTopPosi;
            uvs[curVerticesIndex] = new Vector2(0, 0);
            vertices[curVerticesIndex++] = topCenterPosi;
            uvs[curVerticesIndex] = new Vector2(0, 0);
            vertices[curVerticesIndex++] = rightTopPosi;

            tris[curTrisIndex++] = leftTopIndex;
            tris[curTrisIndex++] = topCenterIndex;
            tris[curTrisIndex++] = rightTopIndex;

        }
    }

    Vector2 GetShapeCenter()
    {
        var ret = Vector2.zero;
        foreach(var vec in baseShape)
        {
            ret += vec;
        }
        ret /= baseShape.Length;

        return ret;
    }

    void ApplyDataToMesh()
    {
        //mesh.Clear();


        //mesh.vertices = vertices;
        //mesh.triangles = tris;
        //mesh.uv = uvs;
        //mesh.RecalculateNormals();

        int vertexOffset = building.vertexList.Count;
        foreach(var v in vertices)
        {
            building.vertexList.Add(v);
        }

        foreach(var tri in tris)
        {
            building.triList.Add(tri + vertexOffset);
        }

        foreach(var uv in uvs)
        {
            building.uvList.Add(uv);
        }
            
    }

}
