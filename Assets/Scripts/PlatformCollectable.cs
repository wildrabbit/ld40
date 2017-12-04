using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectableState: int
{
    Disabled = -1,
    Crystal = 0,
    Platform
}

public class PlatformCollectable : MonoBehaviour
{
    public float transformOffset = 1.1f;
    CollectableState _state;
    Player _player;
    Collectable _collectable;
    Platform _platform;
    AudioSource _source;

	// Use this for initialization
	void Start ()
    {
        _player = FindObjectOfType<Player>();
        _collectable = GetComponentInChildren<Collectable>();
        _platform = GetComponentInChildren<Platform>();
        _source = GetComponent<AudioSource>();
        SetState(CollectableState.Crystal, true);
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (!_player.Depleting && (_player.transform.position.y - transform.position.y) <  -transformOffset && _state == CollectableState.Crystal)
        {
            SetState(CollectableState.Platform);
        }
	}

    public void Reset()
    {
        _platform.Reset();
        _collectable.Reset();
        SetState(CollectableState.Crystal);
    }

    public void SetState(CollectableState newState, bool force = false)
    {
        _collectable.OnCollectStarted -= SetDisabled;

        if (newState == _state && !force) return;
        if (newState == CollectableState.Crystal)
        {
            _platform.gameObject.SetActive(false);
            _collectable.gameObject.SetActive(true);
            _collectable.OnCollectStarted += SetDisabled;
            _collectable.Reset();
        }
        else if (newState == CollectableState.Platform)
        {
            _source.Play();
            _platform.gameObject.SetActive(true);
            _collectable.gameObject.SetActive(false);
            _platform.SetSolid(true);
        }
        _state = newState;
    }

    void SetDisabled()
    {
        SetState(CollectableState.Disabled);
    }
}
