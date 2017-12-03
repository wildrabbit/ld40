using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public bool _solid;
    Collider2D _colliderRef;
    SpriteRenderer _rendererRef;
	
    // Use this for initialization
	void Awake ()
    {
        _colliderRef = GetComponent<Collider2D>();
        _rendererRef = GetComponent<SpriteRenderer>();
    }

    public void SetSolid(bool solid)
    {
        _solid = solid;
        Color color = _rendererRef.color;
        color.a = (_solid) ? 1f : 0.3f;
        _colliderRef.enabled = _solid;
        _rendererRef.color = color;
    }
	
	// Update is called once per frame
	public void Reset()
    {
        SetSolid(false);
    }
}
