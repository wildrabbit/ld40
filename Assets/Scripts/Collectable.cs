using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Collectable : MonoBehaviour
{
    Player _player;
    CircleCollider2D _colliderRef;
    SpriteRenderer _viewRef;
    public bool standalone = false;
    public int amount = 5;

    Tweener defaultTween;
    public TweenCallback OnCollected;
    public TweenCallback OnCollectStarted;

    AudioSource _source;

    // Use this for initialization
    void Awake () {
        _colliderRef = GetComponent<CircleCollider2D>();
        _viewRef = GetComponentInChildren<SpriteRenderer>();
        _source = GetComponent<AudioSource>();
	}

    private void Start()
    {
        _player = FindObjectOfType<Player>();
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
            _source.Play();
            _colliderRef.enabled = false;
            defaultTween.Kill();
            if (OnCollectStarted != null)
                OnCollectStarted();
            var seq = DOTween.Sequence();
            seq.Append(_viewRef.transform.DOScale(1.1f, 0.025f));
            seq.Append(_viewRef.transform.DOScale(0.8f, 0.3f));
            seq.Insert(0.025f, _viewRef.DOFade(0.0f, 0.3f));
            seq.AppendCallback(OnCollected);
            _player.OnCollected(this);
        }
    }

    public void Reset()
    {
        _colliderRef.enabled = true;
        _viewRef.transform.localScale = Vector3.one;
        _viewRef.color = new Color(_viewRef.color.r, _viewRef.color.g, _viewRef.color.b, 1f);
        gameObject.SetActive(true);
    }

}
