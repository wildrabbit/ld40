using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    Player _player;
    Animator _animator;

    int groundParamID;
    int jumpParamID;
    int glideParamID;
    int moveParamID;

    private void Start()
    {
        groundParamID = Animator.StringToHash("grounded");
        jumpParamID = Animator.StringToHash("jumping");
        glideParamID = Animator.StringToHash("gliding");
        moveParamID = Animator.StringToHash("moving");

        _player = GetComponent<Player>();
        _player.JumpStart += Jump;
        _animator = GetComponentInChildren<Animator>();
    }

    public void Update()
    {
        _animator.SetBool(groundParamID, _player.Grounded);
        _animator.SetBool(glideParamID, _player.Gliding);
        _animator.SetBool(moveParamID, _player.Moving);
    }

    public void Jump()
    {
        _animator.SetTrigger(jumpParamID);
    }

}
