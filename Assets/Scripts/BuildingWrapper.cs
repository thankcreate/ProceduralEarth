using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingWrapper : MonoBehaviour
{ 
    public float destScaleY;
    public float lerpFactor = 3;
    bool needLerpY = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(needLerpY)
        {
            var ls = transform.localScale;
            ls.y = Mathf.Lerp(ls.y, destScaleY, Time.deltaTime * lerpFactor);
            transform.localScale = ls;
        }
    }

    public void ScaleToY(float toY, float lerpFactor)
    {
        this.lerpFactor = lerpFactor;
        needLerpY = true;
        destScaleY = toY;
    }
}
