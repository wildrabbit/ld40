using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomCheck : MonoBehaviour
{
    public float triggerHeight = 7.0f;
    PlatformManager _platformManager;
    CameraController _camController;
    Collider2D _collider2D;
    SpriteRenderer _view;
    Vector2 _startPos;
    Player _player;
    bool awaitingPlatform;
    private void Start()
    {
        _platformManager = FindObjectOfType<PlatformManager>();
        _player = FindObjectOfType<Player>();
        _collider2D = GetComponent<Collider2D>();
        _camController = FindObjectOfType<CameraController>();
        _view = GetComponentInChildren<SpriteRenderer>();
        _startPos = new Vector2(transform.position.x, transform.position.y);

        _player.PlatformStepped += OnPlatformStepped;
        _player.DepletionFinished += (endType) => RestartStuff();

        _camController.OnPlatformSnapFinished += (newPos) => { if (_collider2D.isTrigger) { transform.position = newPos; } };
        _player.GlideFinished += OnGlideFinished;
        awaitingPlatform = false;
    }
    void OnGlideFinished()
    {
        awaitingPlatform = true;
    }

    void OnPlatformStepped(Collider2D c)
    {
        if (awaitingPlatform && c.transform.position.y - _collider2D.transform.position.y >= triggerHeight)
        {
            SetTrigger(true);
            awaitingPlatform = false;
        }
    }
    public void SetTrigger(bool asTrigger)
    {
        Debug.Log("TRIGGER CALLED!" + asTrigger);
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
        Debug.Log("Restart!");
        _player.PlayLost();
        yield return new WaitForSeconds(0.7f);
        _player.Reset();
        _player.gameObject.SetActive(false);
        _camController.Freeze();
        yield return new WaitForSeconds(0.5f);
        if (_camController != null)
        {
            _camController.Reset();
        }
        _platformManager.Reset();
        yield return new WaitForSeconds(1.5f);
        Reset();
        yield return new WaitForSeconds(0.5f);
        if (_player != null)
        {
            _player.gameObject.SetActive(true);
        }
        
    }

    public void Reset()
    {
        transform.position = _startPos;
        awaitingPlatform = false;
        SetTrigger(false);
    }
}
