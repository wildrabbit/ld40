using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlatformManager : MonoBehaviour
{
    PlatformCollectable[] _platformCollectables;

    Platform[] _platforms;
    Platform _rootPlatform;

    List<Collectable> _standaloneCollectables;
    Player _player;

    private void Awake()
    {
        _standaloneCollectables = new List<Collectable>();
    }
    // Use this for initialization
    void Start ()
    {
        _platformCollectables = FindObjectsOfType<PlatformCollectable>();
        _platforms = FindObjectsOfType<Platform>();
        _rootPlatform = GameObject.FindGameObjectWithTag("Respawn").GetComponent<Platform>();
        _rootPlatform.SetSolid(false);

        _standaloneCollectables = new List<Collectable>(FindObjectsOfType<Collectable>().Where(x => x.standalone));
        DisableStandaloneCollectables();
        _player = FindObjectOfType<Player>();
        _player.GlideFinished += OnGlideFinished;
        _player.DepleteBegun += () => SetSolid(true);
	}

    void DisableStandaloneCollectables()
    {
        foreach(Collectable c in _standaloneCollectables)
        {
            c.gameObject.SetActive(false);
        }
    }

    void OnGlideFinished()
    {
        foreach (Collectable c in _standaloneCollectables)
        {
            c.gameObject.SetActive(true);
        }
    }

    public void SetSolid(bool param)
    {
        _rootPlatform.SetSolid(true);
        //foreach (Platform p in _platforms)
        //{
        //    p.SetSolid(param);
        //}
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void Reset()
    {
        foreach (PlatformCollectable p in _platformCollectables)
        {
            p.Reset();
        }
        DisableStandaloneCollectables();
    }

}
