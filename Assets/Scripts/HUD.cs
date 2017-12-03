﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HUD : MonoBehaviour
{
    public Sprite itemNormal;
    public Sprite itemBoost;
    public Color dangerTint;

    public Image _collectedIcon;
    public Text _collectedLabel;
    public Text _boostHint;
    public Text _gameFinished;

    Player _player;
    Level _levelData;

    public const string kCounterLabelPattern = "{0:00}/{1:00}";
    public const string kBoostHint = "Boost ready. Hold and release \"Charge\". Cost: {0}";

    public const string kWon = "You did it!";
    public const string kLost = "Uh oh...";

    // Use this for initialization
    void Start () {
        _levelData = FindObjectOfType<Level>();
        _player = FindObjectOfType<Player>();
        _collectedIcon = transform.Find("CollectedIcon").GetComponent<Image>();
        _collectedLabel = transform.FindRecursive("Collected").GetComponent<Text>();
        _boostHint = transform.Find("BoostHint").GetComponent<Text>();
        _gameFinished = transform.Find("GameFinished").GetComponent<Text>();

        _gameFinished.enabled = false;

        _player.GameFinished += OnGameFinished;
        Refresh();
	}

    void OnGameFinished(bool won)
    {
        StartCoroutine(GameOver(won));
    }

    IEnumerator GameOver(bool won)
    {
        _gameFinished.enabled = true;
        _gameFinished.text = won ? kWon : kLost;
        
        _player.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.0f);
        yield return new WaitUntil(() => Input.anyKeyDown);
        _gameFinished.enabled = false;
        yield return FindObjectOfType<BottomCheck>().RestartStuff();        
    }

    void Refresh()
    {
        _collectedIcon.sprite = (_player.CanUseBoost) ? itemBoost : itemNormal;
        if (!_player.CanUseBoost)
        {
            _collectedIcon.color = _player.Collected < _levelData.required ? dangerTint : Color.white;
            _boostHint.enabled = false;
        }
        else
        {
            _collectedIcon.color = Color.white;
            _boostHint.enabled = true;
            _boostHint.text = string.Format(kBoostHint, _player.boostCost);
        }

        _collectedLabel.text = string.Format(kCounterLabelPattern, _player.Collected, _levelData.required);
    }

    // Update is called once per frame
    void Update () {
        Refresh();		
	}
}