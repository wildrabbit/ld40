using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomCheck : MonoBehaviour
{
    CameraController _camController;
    Collider2D _collider2D;
    SpriteRenderer _view;
    Vector2 _startPos;
    Player _player;
    Collectable[] _collectables;
    private void Start()
    {
        _player = FindObjectOfType<Player>();
        _collider2D = GetComponent<Collider2D>();
        _camController = FindObjectOfType<CameraController>();
        _view = GetComponentInChildren<SpriteRenderer>();
        _startPos = new Vector2(transform.position.x, transform.position.y);
        _collectables = FindObjectsOfType<Collectable>();

    }

    public void SetTrigger(bool asTrigger)
    {
        _view.enabled = !asTrigger;
        _collider2D.isTrigger = asTrigger;
    }

    private void Update()
    {
        if (_collider2D.isTrigger)
        {
            transform.position = _camController.GetCameraDeathZone();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            StartCoroutine(RestartStuff());
        }
    }

    public IEnumerator RestartStuff()
    {
        _camController.Freeze();
        yield return new WaitForSeconds(0.5f);
        if (_camController != null)
        {
            _camController.Reset();
        }
        yield return new WaitForSeconds(0.5f);
        foreach (Collectable coll in _collectables)
        {
            coll.Reset();
        }
        yield return new WaitForSeconds(0.5f);
        if (_player != null)
        {
            _player.Reset();
        }
        yield return new WaitForSeconds(0.5f);
        Reset();
    }

    public void Reset()
    {
        transform.position = _startPos;
        SetTrigger(false);
    }
}
