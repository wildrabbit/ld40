using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Sprite itemNormal;
    public Sprite itemBoost;
    public Color dangerTint;

    public Image _collectedIcon;
    public Text _collectedLabel;

    Player _player;
    Level _levelData;

    public const string kCounterLabelPattern = "{0:00}/{1:00}";

    // Use this for initialization
    void Start () {
        _levelData = FindObjectOfType<Level>();
        _player = FindObjectOfType<Player>();
        _collectedIcon = transform.Find("CollectedIcon").GetComponent<Image>();
        _collectedLabel = transform.FindRecursive("Collected").GetComponent<Text>();

        Refresh();
	}

    void Refresh()
    {
        _collectedIcon.sprite = (_player.CanUseBoost) ? itemBoost : itemNormal;
        if (!_player.CanUseBoost)
        {
            _collectedIcon.color = _player.Collected < _levelData.required ? dangerTint : Color.white;
        }

        _collectedLabel.text = string.Format(kCounterLabelPattern, _player.Collected, _levelData.required);
    }

    // Update is called once per frame
    void Update () {
        Refresh();		
	}
}
