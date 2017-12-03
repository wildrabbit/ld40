using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResizer : MonoBehaviour
{
    Camera cam;
	// Use this for initialization
	void Start ()
    {
        cam = GetComponent<Camera>();
        cam.aspect = 9/16;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
