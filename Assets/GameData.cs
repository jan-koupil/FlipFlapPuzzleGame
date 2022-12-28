using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private Dictionary<int, int> _bestFlipList = new();

    public int Level { get; set; } = 1;
    public int CurrentFlips { get; set; } = 0;
    public int BestFlips {        
        get
        {
            if (_bestFlipList.TryGetValue(Level, out int bestFlips))
                return bestFlips;
            else
                return 0;
        }
        set
        {
            if (!_bestFlipList.ContainsKey(Level) || _bestFlipList[Level] >= value)
                _bestFlipList[Level] = value;
        } 
    }

    [SerializeField] int MinZoom = 1;
    [SerializeField] int MaxZoom = 10;

    private int _zoom = 5;

    public int Zoom
    {
        get { return _zoom; }
        set { 
            if (_zoom <= MaxZoom && _zoom >= MinZoom)
                _zoom = value; 
        }
    }

    public bool IsMaxZoom { get => _zoom == MaxZoom; }
    public bool IsMinZoom { get => _zoom == MinZoom; }

}
