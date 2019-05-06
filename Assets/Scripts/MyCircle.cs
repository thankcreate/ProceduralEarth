using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCircle : MonoBehaviour
{
    [Range(0, 100)]
    public int segments = 50;

    float xradius = 5;

    float yradius = 5;
    LineRenderer line;

    public Transform refPlanet;
    public Transform sun;

    void Start()
    {
        xradius = Vector3.Distance(refPlanet.position, sun.position);
        yradius = Vector3.Distance(refPlanet.position, sun.position);

        line = gameObject.GetComponent<LineRenderer>();

        line.SetVertexCount(segments + 1);
        line.useWorldSpace = false;
        CreatePoints();
    }

    void CreatePoints()
    {
        float x;
        float y;
        float z;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, 0, z));

            angle += (360f / segments);
        }
    }
}