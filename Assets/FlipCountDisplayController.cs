using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class FlipCountDisplayController : MonoBehaviour
{
    private TMP_Text _flipCountDisplay;
    private TMP_Text _bestFlipCountDisplay;
    private GameData _gameData;
    private int _lastFlips;

    private void Awake()
    {
        _gameData = GameObject.FindObjectOfType<GameData>();
        _flipCountDisplay = transform.Find("FlipCount").gameObject.GetComponent<TMP_Text>();
        _bestFlipCountDisplay = transform.Find("BestFlipCount").gameObject.GetComponent<TMP_Text>();

    }
    // Start is called before the first frame update
    void Start()
    {
        int best = _gameData.BestFlips;
        _bestFlipCountDisplay.text = best != 0 ? _gameData.BestFlips.ToString() : "-";
        _flipCountDisplay.text = _gameData.CurrentFlips.ToString();
        _lastFlips = _gameData.CurrentFlips;
    }

    // Update is called once per frame
    void Update()
    {
        if (_lastFlips != _gameData.CurrentFlips)
        { 
            _flipCountDisplay.text = _gameData.CurrentFlips.ToString();
            _lastFlips = _gameData.CurrentFlips;
        }
    }
}
