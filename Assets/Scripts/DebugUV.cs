using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUV : MonoBehaviour
{
    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        DebugInfo();
    }

    void DebugInfo()
    {

        Debug.Log("vertices count: " + mesh.vertices.Length);
        Debug.Log("tris count: " + mesh.triangles.Length);
        Debug.Log("uv count: " + mesh.uv.Length);

        //foreach (var da in mesh.uv)
        //{
        //    Debug.Log(da);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
