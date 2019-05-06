using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySceneManager : MonoBehaviour
{
    Camera cam;

    Transform defaultCamParent;
    Vector3 defaultCamPosi;
    Quaternion defaultCamRot;

    Earth curPlanet;
    
    public float camBackDt = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;


        defaultCamParent = cam.transform.parent;
        defaultCamPosi = cam.transform.position;
        defaultCamRot = cam.transform.rotation;
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfNeedReturnToDefaultCam();
    }

    void CheckIfNeedReturnToDefaultCam()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (!curPlanet)
                return;

            cam.transform.DOMove(defaultCamPosi, camBackDt);
            cam.transform.DORotateQuaternion(defaultCamRot, camBackDt);
            cam.transform.parent = defaultCamParent;

            curPlanet.GetComponent<Interations>().enabled = false;
            curPlanet.MySendEventToAll("BACK");
            curPlanet.transform.parent.gameObject.MySendEventToAll("RESUME_ROTATE");
            curPlanet = null;

        }
    }



    public void SetCurPlanet(Earth pl)
    {
        curPlanet = pl;
    }



}
