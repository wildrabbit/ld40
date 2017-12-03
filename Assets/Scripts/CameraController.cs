using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public float topSnapThreshold = 0.2f;
    public float bottomSnapThreshold = 0.2f;
    public float levelHeight;

    public bool snapToPlatform = false;

    Vector3 _startPos;
    Camera _cam;
    Player _player;
    Transform _target;

    float _topSnap;
    float _botSnap;

    Tweener motion;
    
    public void SnapToHeight(float height)
    {
        _cam.transform.DOMoveY(height, 0.5f).SetEase(Ease.InBack);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }

	// Use this for initialization
    public void SetCamera(Camera c)
    {
        _cam = c;
    }
	void Start ()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        SetPlayer(GameObject.Find("Player").GetComponent<Player>());
        SetCamera(GetComponent<Camera>());
        _startPos = new Vector3(transform.position.x, -_cam.orthographicSize, transform.position.z);
        var startTween = _cam.transform.DOMoveY(_startPos.y, 2f).SetEase(Ease.InOutQuint).OnComplete(StartTransitionComplete);

        float size = _cam.orthographicSize * 2;
        _topSnap = -size * (topSnapThreshold + 0.5f);
        _botSnap = -levelHeight + size * (bottomSnapThreshold + 0.5f);

    }

    void StartTransitionComplete()
    {
        _player.EnableMovement(true);
        SetTarget(_player.transform);
    }

    // Update is called once per frame
    void Update ()
    {
        if (_target == null) return;
        float targetHeight = _target.position.y;
        if (targetHeight < _topSnap && targetHeight > _botSnap)
        {
            Player player = _target.GetComponent<Player>();
            if (player != null && player.Jumping)
            {
                return;
            }
        }
        else targetHeight = FollowNSnap();

        if (motion == null)
        {
            motion = _cam.transform.DOLocalMoveY(targetHeight, 0.3f).OnComplete(() => motion = null);
        }
        else
        {
            motion.ChangeEndValue(targetHeight, 0.3f);
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

    public void Reset()
    {
        _cam.transform.position = _startPos;
        motion = null;
    }
}
