using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EarthNoise 
{    
    public float noiseFrequency = 1;
    public float noiseStrength = 0.1f;
    public Vector3 noiseOffset;
    public float noiseThreshould = 0;

    public int noiseMultiLayer = 1;
    
    public float noiseFrequencyMulti = 2;
    // smaller than 1
    [Range(0, 0.99f)]
    public float noiseStrenthMulti = 0.5f;
}
