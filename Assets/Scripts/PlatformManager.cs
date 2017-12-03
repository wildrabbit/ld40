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
        if (_rootPlatform != null)
        {
            _rootPlatform.SetSolid(true);
        }
	}

    public void SetSolid(bool param)
    {
        foreach (Platform p in _platforms)
        {
            p.SetSolid(param);
        }
        if (_rootPlatform != null)
        {
            _rootPlatform.SetSolid(true);
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
        if (_rootPlatform != null)
        {
            _rootPlatform.SetSolid(true);
        }
    }
}
