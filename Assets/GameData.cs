using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System.IO;
using Newtonsoft.Json;

public class GameData : MonoBehaviour
{
    private const string SaveGameFilename = "BestFlips.json";
    private string _saveGameFullPath;

    void Awake()
    {
        DontDestroyOnLoad(this);
        _saveGameFullPath = Application.persistentDataPath + Path.DirectorySeparatorChar + SaveGameFilename;
        InitBestFlips();
        Zoom = InitZoom;
    }

    private Dictionary<int, int> _bestFlipList;

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
            { 
                _bestFlipList[Level] = value;
                SaveBestFlips();
            }
        } 
    }

    public bool ShowStartMenu { get; set; } = true;

    [SerializeField] int MinZoom = 1;
    [SerializeField] int MaxZoom = 10;
    [SerializeField] int InitZoom = 5;
    private int _zoom;

    public int Zoom
    {
        get { return _zoom; }
        set { 
            if (value <= MaxZoom && value >= MinZoom)
                _zoom = value; 
        }
    }

    public bool IsMaxZoom { get => _zoom == MaxZoom; }
    public bool IsMinZoom { get => _zoom == MinZoom; }

    private bool _lockedCamera = true;

    public bool LockedCamera
    {
        get { return _lockedCamera; }
        set 
        {
            if (CamUnlockEvent) return; //don't un/lock until previous lock is processed
            
            _lockedCamera = value;
            
            if (!_lockedCamera) //now i am unlocking camera, it will follow flippy
                CamUnlockEvent = true;
        }
    }

    public bool CamUnlockEvent { get; private set; }

    public void SetCamUnlockEventProcessed()
    {
        CamUnlockEvent = false;
    }
    
    public Vector3 CameraOffset { get; set; } = Vector3.zero;

    public void ResetCameraState()
    {
        LockedCamera = true;
        CameraOffset = Vector3.zero;
        SetCamUnlockEventProcessed();
    }

    private void InitBestFlips()
    {
        if (File.Exists(_saveGameFullPath))
        { 
            string json = File.ReadAllText(_saveGameFullPath);
            _bestFlipList = JsonConvert.DeserializeObject<Dictionary<int, int>>(json);
        }
        else
            _bestFlipList = new();

    }
    private void SaveBestFlips()
    {
        string json = JsonConvert.SerializeObject(_bestFlipList);
        File.WriteAllTextAsync(_saveGameFullPath, json);
    }
}
