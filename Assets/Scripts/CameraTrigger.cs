using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum CameraType
{
    Fixed,
    FollowTarget
}

public class CameraTrigger : MonoBehaviour
{
    //public CameraController camera;
    public Collider2D expected;
    public CameraType camType;
    public Collider2D nextCamTarget;

    // Use this for initialization
	void Start ()
    {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == expected)
        {
            /* cameraController.SetType(camType);
            if (cameraController.RequiresTarget())
            {
                cameraController.SetTarget(nextCamTarget());
            }
            */
        }
    }
}
