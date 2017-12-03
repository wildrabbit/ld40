using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    Platform[] _platforms;
    Platform _rootPlatform;

	// Use this for initialization
	void Start ()
    {
        _platforms = FindObjectsOfType<Platform>();
        _rootPlatform = GameObject.FindGameObjectWithTag("Respawn").GetComponent<Platform>();
        foreach (Platform p in _platforms)
        {
            p.SetSolid(false);
        }
	}

    public void SetSolid(bool param)
    {
        foreach (Platform p in _platforms)
        {
            p.SetSolid(param);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void Reset()
    {
        foreach (Platform p in _platforms)
        {
            p.Reset();
        }
    }
}
