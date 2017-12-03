using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Collectable : MonoBehaviour
{
    Player _player;
    CircleCollider2D _colliderRef;
    SpriteRenderer _viewRef;

    Tweener defaultTween;
    Tweener deathTween;

	// Use this for initialization
	void Start () {
        _player = FindObjectOfType<Player>();
        _colliderRef = GetComponent<CircleCollider2D>();
        _viewRef = GetComponentInChildren<SpriteRenderer>();
        defaultTween = _viewRef.transform.DOScale(1.1f, 0.15f).SetLoops(-1, LoopType.Yoyo);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>())
        {
            defaultTween.Kill();
            var seq = DOTween.Sequence();
            seq.Append(_viewRef.transform.DOScale(1.1f, 0.025f));
            seq.Append(_viewRef.transform.DOScale(0.8f, 0.3f));
            seq.Insert(0.025f, _viewRef.DOFade(0.0f, 0.3f));

            _player.OnCollected(this);
        }
    }

}
