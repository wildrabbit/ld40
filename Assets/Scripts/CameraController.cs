using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class CameraController : MonoBehaviour
{
    public float topSnapThreshold = 0.2f;
    public float bottomSnapThreshold = 0.2f;
    public float levelHeight;
    public float deathZoneOffset = 0.0f;
    public float platformSnapOffset = 2.0f;

    public float camSpeed = 10f;

    public bool snapToPlatform = false;

    public BottomCheck _bottomChecker;

    Vector3 _startPos;
    Camera _cam;
    Player _player;
    Transform _target;

    float _topSnap;
    float _botSnap;

    Tweener motion;

    bool _frozen;
    float _targetHeight;

    bool _platformSnapping;

    public event Action<Vector2> OnPlatformSnapFinished;
    
    public void SnapToHeight(float height)
    {
        Debug.Log("Snapping height");
        _cam.transform.DOMoveY(height, 0.5f).SetEase(Ease.InBack);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetPlayer(Player player)
    {
        _player = player;
        _player.PlatformStepped += OnPlatformStepped;
    }

    void OnPlatformStepped(Collider2D col)
    {
        _targetHeight = col.transform.position.y + _cam.orthographicSize - platformSnapOffset;
        if (_cam.transform.position.y > _targetHeight) return; // Don't go down.

        _platformSnapping = true;
        float speed = camSpeed / 2;
        float time = Mathf.Max(Mathf.Abs(_targetHeight - _cam.transform.position.y) / speed, 0.025f);
        Debug.Log("Step on platform");
        motion = _cam.transform.DOMoveY(_targetHeight, time).OnComplete(() =>
        {
            if (OnPlatformSnapFinished != null) OnPlatformSnapFinished(GetCameraDeathZone());
            _platformSnapping = false;
        }).SetDelay(0.1f);
    }

	// Use this for initialization
    public void SetCamera(Camera c)
    {
        _cam = c;
    }
	void Start ()
    {
        _bottomChecker = GameObject.FindWithTag("Finish").GetComponent<BottomCheck>();
        levelHeight = Mathf.Abs(_bottomChecker.transform.position.y);
        Screen.SetResolution(432, 768, false);
        _frozen = false;
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        SetPlayer(GameObject.Find("Player").GetComponent<Player>());
        SetCamera(GetComponent<Camera>());
        _startPos = new Vector3(transform.position.x, -_cam.orthographicSize, transform.position.z);
        float size = _cam.orthographicSize * 2;
        _topSnap = -size * (topSnapThreshold + 0.5f);
        _botSnap = -levelHeight + size * (bottomSnapThreshold + 0.5f);

        _targetHeight = _startPos.y;
        float time = Mathf.Max(Mathf.Abs(_targetHeight - _cam.transform.position.y) / camSpeed, 0.01f);

        var startTween = _cam.transform.DOMoveY(_targetHeight, time).SetEase(Ease.InOutQuint).OnComplete(StartTransitionComplete);
        _player.Pause(true);
    }

    void StartTransitionComplete()
    {
        _player.Pause(false);
        _player.EnableMovement(true);
        SetTarget(_player.transform);
    }

    // Update is called once per frame
    void Update ()
    {
        float oldTarget = _targetHeight;
        if (_frozen) return;
        if (_target == null) return;
        if (_platformSnapping) return;

        _targetHeight = _target.position.y;
        if (_targetHeight < _topSnap && _targetHeight > _botSnap)
        {
            if (_player != null && (_player.Jumping))
                return;
        }
        else _targetHeight = FollowNSnap();

        if (_player.Depleting && _targetHeight < _cam.transform.position.y)
        {
            motion.Kill();
            motion = null;
            return;
        }

        float time = Mathf.Max(Mathf.Abs(_targetHeight - _cam.transform.position.y) / camSpeed, 0.01f);

        if (motion == null)
        {
            motion = _cam.transform.DOLocalMoveY(_targetHeight, time).OnComplete(() => motion = null);
        }
        else
        {
            motion.ChangeEndValue(_targetHeight, time);
        }
    }

    float FollowNSnap()
    {
        if (_target.position.y > _topSnap)
        {
            return -_cam.orthographicSize;
        }
        else if (_target.position.y < _botSnap)
        {
            return -levelHeight + _cam.orthographicSize;
        }
        return _target.position.y;
    }

    public void Freeze()
    {
        if (motion != null)
        {
            motion.Kill();
            motion = null;
        }
        _frozen = true;

    }
    public void Reset()
    {
        _cam.transform.position = _startPos;
        motion = null;
        _frozen = false;
    }

    public Vector2 GetCameraDeathZone()
    {
        return (Vector2)_cam.transform.position - new Vector2(0, _cam.orthographicSize + deathZoneOffset);
    }
}
