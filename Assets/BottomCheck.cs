using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomCheck : MonoBehaviour
{
    CameraController _camController;

    private void Start()
    {
        _camController = FindObjectOfType<CameraController>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.Reset();
            }
            if (_camController != null)
            {
                _camController.Reset();
            }
        }
    }
}
