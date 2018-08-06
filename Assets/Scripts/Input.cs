using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput
{
    public bool any;
    public bool jumpWasPressed;
    public bool jumpPressed;

    public bool boostWasPressed;
    public bool boostPressed;

    public float xAxis;

    public bool escJustPressed;

    public void Init()
    {
        any = false;
        jumpWasPressed = false;
        jumpPressed = false;
        boostWasPressed = false;
        boostPressed = false;

        xAxis = 0f;

        escJustPressed = false;
    }

    public void Update()
    {
        any = Input.anyKeyDown;
        escJustPressed = Input.GetKeyDown(KeyCode.Escape);
        jumpWasPressed = jumpPressed;
        jumpPressed = Input.GetButtonDown("Jump");
        boostWasPressed = boostPressed;
        boostPressed = Input.GetButtonDown("Fire1");
        xAxis = Input.GetAxis("Horizontal");
    }
}
