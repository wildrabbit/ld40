using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomCheck : MonoBehaviour
{
    PlatformManager _platformManager;
    CameraController _camController;
    Collider2D _collider2D;
    SpriteRenderer _view;
    Vector2 _startPos;
    Player _player;
    private void Start()
    {
        _platformManager = FindObjectOfType<PlatformManager>();
        _player = FindObjectOfType<Player>();
        _collider2D = GetComponent<Collider2D>();
        _camController = FindObjectOfType<CameraController>();
        _view = GetComponentInChildren<SpriteRenderer>();
        _startPos = new Vector2(transform.position.x, transform.position.y);

        _player.FirstPlatformAfterGlide += (col) => SetTrigger(true);
        _player.DepletionFinished += (endType) => RestartStuff();

        _camController.OnPlatformSnapFinished += (newPos) => transform.position = newPos;
    }

    public void SetTrigger(bool asTrigger)
    {
        _view.enabled = !asTrigger;
        _collider2D.isTrigger = asTrigger;
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
        _player.gameObject.SetActive(false);
        _camController.Freeze();
        yield return new WaitForSeconds(0.5f);
        if (_camController != null)
        {
            _camController.Reset();
        }
        _platformManager.Reset();
        yield return new WaitForSeconds(1.5f);
        if (_player != null)
        {
            _player.gameObject.SetActive(true);
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
