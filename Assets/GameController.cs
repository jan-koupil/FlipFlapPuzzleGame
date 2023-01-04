using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Tilemaps.Tilemap;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] GameObject FloorTilePrefab;
    [SerializeField] GameObject FlipTilePrefab;

    [SerializeField] Material FlippingMaterial;
    [SerializeField] Material PassiveMaterial;
    [SerializeField] Material FrozenMaterial;
    [SerializeField] Material FloorMaterial;
    [SerializeField] Material TargetMaterial;
    [SerializeField] Material SandMaterial;

    [SerializeField] float FlipDuration = 1f/3;
    [SerializeField] float RiseDuration = 1f;
    [SerializeField] float RiseHeight = 0.15f;
    [SerializeField] float FinalMenuDelay = 1.5f;

    [SerializeField] GameObject MessageBoxCanvas;
    [SerializeField] GameObject InGameDialogBoxPrefab;
    [SerializeField] GameObject StartMessageBoxPrefab;
    [SerializeField] GameObject ControlPanelPrefab;

    [SerializeField] GameObject ButtonPanel;
    [SerializeField] Camera MainCamera;
    private GameObject _cameraPivot;

    private Level _level;

    /// <summary>
    /// Shows wheter Flippy is in motion or can be controlled
    /// </summary>
    private bool _isRolling;
    private GameState _gameState;

    private const int _tileSize = 1;
    private const float _flipOverFlipperDist = 0.25f;

    private List<GameObject> _floor = null;
    private List<GameObject> _sandTiles = null;
    private List<GameObject> _targetTiles = null;
    private List<GameObject> _flippers = null;
    private List<GameObject> _passives = null;
    private List<GameObject> _frozen = null;

    private TileType[,] _gameMap;
    private GameData _gameData;

    private enum GameState : byte { Init, Running, Win, Fail, Paused }
    private enum TileType : byte { Hole, Flipping, Passive, Floor, Target, Sand, Frozen }

    private void Awake()
    {
        _gameData = GameObject.FindObjectOfType<GameData>();
        _cameraPivot = MainCamera.transform.parent.gameObject;
    }

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        _gameState = GameState.Init;

        _level = Level.GetLevel(_gameData.Level);        
        if (_level == null) //end game
        {
            _gameData.Level--;
            SceneManager.LoadScene("WinGameScene");
            return;
        }
        _gameMap = ParseTextMap(_level.TextMap);

        SetUpGame(_gameMap);
        if (_gameData.ShowStartMenu)
            ShowStartMessageBox();
        else
            StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gameData.LockedCamera && _flippers.Count > 0)
        {
            GameObject flippy = _flippers.First();

            if (_gameData.CamUnlockEvent) //right now camera was unlocked
            {
                Vector3 camOffset = _cameraPivot.transform.position - flippy.transform.position;
                camOffset.y = 0;
                _gameData.CameraOffset = camOffset;
                _gameData.SetCamUnlockEventProcessed(); //release flag "now was unlocked"
            }

            Vector3 newPosition = flippy.transform.position + _gameData.CameraOffset;
            newPosition.y = _cameraPivot.transform.position.y;

            _cameraPivot.transform.position = newPosition;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowInGameDialog();
            return;
        }

        if (_isRolling || _gameState != GameState.Running) 
            return;

        if (Input.anyKey)
        {

            if (Input.GetKey(KeyCode.UpArrow))
                StartFlipping(Vector3.forward);
            else if (Input.GetKey(KeyCode.DownArrow))
                StartFlipping(Vector3.back);
            else if (Input.GetKey(KeyCode.LeftArrow))
                StartFlipping(Vector3.left);
            else if (Input.GetKey(KeyCode.RightArrow))
                StartFlipping(Vector3.right);

        }

    }

    private Vector3 ScaleVector(Vector3 vector, Vector3 scale)
    {
        return new Vector3(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z);
    }

    private Bounds GetFlipperBounds(List<GameObject> flippers)
    {
        bool first = true;
        Bounds bounds = new();

        foreach (GameObject flipper in flippers)
        {
            Bounds currentBounds = flipper.GetComponent<MeshFilter>().mesh.bounds;
            Vector3 scale = flipper.transform.localScale;
            Vector3 size = ScaleVector(currentBounds.size, scale);
            Bounds realBounds = new(flipper.transform.position, size);
            if (first)
            {
                bounds = realBounds;
                first = false;
            }
            else
            {
                bounds.Encapsulate(realBounds);
            }
        }

        return bounds;
    }

    IEnumerator Flip(List<GameObject> flippers, Vector3 anchor, Vector3 axis)
    {
        _isRolling = true;
        ReleaseSand();
        
        float angularVelocity = 180 / FlipDuration;
        float totalAngle = 0;
        float lastTime = Time.time;

        while (totalAngle < 180)
        {
            float deltaTime = Time.time - lastTime;
            lastTime = Time.time;
            float maxAngle = 180 - totalAngle;
            float angle = angularVelocity * deltaTime;

            if (angle < maxAngle)
            {
                totalAngle += angle;
            }
            else
            {
                angle = maxAngle;
                totalAngle = 180;
            }

            foreach (var flipper in flippers)
            {
                flipper.transform.RotateAround(anchor, axis, angle);
            }

            CheckFlipOverFlipper();
            yield return null;
            //yield return new WaitForSeconds(0.01f);
        }

        _isRolling = false;

        if (_gameState == GameState.Running) //when flipped over flipper, this would be unnecessary
        {
            MergeAdjacentTiles();
            RoundPositions();
        }

        FindAndBreakOverlaps();

        if (_gameState == GameState.Running)
            CheckVictory();
    }

    /// <summary>
    /// Checks if any of the flipping tiles is above a passive flipping tile - that is a gameover
    /// </summary>
    private void CheckFlipOverFlipper()
    {
        foreach (GameObject flipper in _flippers.ToArray())
        {
            foreach (GameObject passive in _passives.ToArray())
            {
                float distance = Vector3.Distance(flipper.transform.position, passive.transform.position);
                if (distance < _flipOverFlipperDist * _tileSize)
                {
                    BreakOverlap(flipper);
                }
            }
        }
    }

    private void RoundPositions()
    {
        foreach (var flipper in _flippers)
        {
            Vector3 coercedPos = flipper.transform.position;
            coercedPos.x = Mathf.Round(coercedPos.x);
            coercedPos.z = Mathf.Round(coercedPos.z);
            flipper.transform.position = coercedPos;
        }
    }

    IEnumerator RiseUp(GameObject[] objects, float riseHeight, float riseDuration, Action callback = null)
    {

        float maxHeight = riseHeight * _tileSize;
        float riseVelocity = maxHeight / riseDuration;
        float totalHeight = 0;
        float lastTime = Time.time;

        while (totalHeight < maxHeight)
        {
            float deltaTime = Time.time - lastTime;
            lastTime = Time.time;
            float maxStep = maxHeight - totalHeight;
            float step = riseVelocity * deltaTime;

            if (step <= maxStep)
            {
                totalHeight += step;
            }
            else
            {
                step = maxStep;
                totalHeight = maxHeight;
            }

            foreach (GameObject go in objects)
            {
                go.transform.position += Vector3.up * step;
            }
            yield return null;
//            yield return new WaitForSeconds(0.01f);
        }
        callback?.Invoke();
    }

    private void MergeAdjacentTiles()
    {
        bool runAgain = false;

        foreach (GameObject flipper in _flippers.ToArray())
        {
            foreach (GameObject passive in _passives.ToArray())
            {
                float distance = Vector3.Distance(flipper.transform.position, passive.transform.position);
                if (distance < 1.1f * _tileSize)
                {
                    ActivateTile(passive);
                    runAgain = true;
                }
            }
        }

        if (runAgain)
            MergeAdjacentTiles();
    }

    private void ActivateTile(GameObject passiveTile)
    {
        passiveTile.GetComponent<MeshRenderer>().material = FlippingMaterial;
        _flippers.Add(passiveTile);
        _passives.Remove(passiveTile);
    }

    private void FindAndBreakOverlaps()
    {
        foreach (GameObject flipper in _flippers.ToArray())
        {
            float minDist = float.MaxValue;

            foreach (GameObject floorTile in _floor)
            {
                float dist = Vector3.Distance(flipper.transform.position, floorTile.transform.position);
                if (dist < minDist)
                    minDist = dist;
            }

            if (minDist > 0.9 * _tileSize)
            {
                BreakOverlap(flipper);
            }
        }
    }

    private void BreakOverlap(GameObject overlappingTile)
    {
        DropTileDown(overlappingTile);

        _flippers.Remove(overlappingTile);
        _passives.Add(overlappingTile);

        GameOver();
    }


    private void ReleaseSand()
    {
        foreach (GameObject sandTile in _sandTiles.ToArray())
        {
            foreach (GameObject flipper in _flippers)
            {
                float dist = Vector3.Distance(flipper.transform.position, sandTile.transform.position);
                if (dist < _tileSize)
                    DestroySandTile(sandTile);
            }
        }
    }

    private void DestroySandTile(GameObject sandTile)
    {
        DropTileDown(sandTile);

        _sandTiles.Remove(sandTile);
        _floor.Remove(sandTile);
    }

    private void Unfreeze()
    {
        foreach (GameObject frozenTile in _frozen.ToArray())
        {
            foreach (GameObject flipper in _flippers)
            {
                float dist = Vector3.Distance(flipper.transform.position, frozenTile.transform.position);
                if (dist < _tileSize)
                    StartTileMelting(frozenTile);
            }
        }
    }

    private void StartTileMelting(GameObject tile)
    {
        //zmìnit materiál
        tile.GetComponent<MeshRenderer>().material = PassiveMaterial;

        //vyjet nahoru
        StartCoroutine(
            RiseUp(
                new GameObject[] { tile }, 
                0.1f - 0.01f, 
                RiseDuration, 
                () => {
                    while(_gameState != GameState.Running) {}
                    _frozen.Remove(tile);
                    _passives.Add(tile);
                }
            )
        );

        //po dobìhnutí callback pøesune mezi pasivy

    }

    void DropTileDown (GameObject tile)
    {
        tile.transform.localScale *= 0.99f;
        Rigidbody currentRb = tile.AddComponent<Rigidbody>();
        currentRb.detectCollisions = true;
        StartCoroutine(
            DelayTileDestruction(tile, 1.5f)
        );
    }

    IEnumerator DelayTileDestruction(GameObject tile, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        try { Destroy(tile); } catch (Exception) { };
    }

    private void GameOver()
    {
        _gameState = GameState.Fail;
        StartCoroutine(DelayMenuShow(FinalMenuDelay));
    }


    IEnumerator DelayMenuShow(float delayTime)
    {
        DisableControlButtons();

        yield return new WaitForSeconds(delayTime);

        if (_gameState != GameState.Win && _gameState != GameState.Fail)
            yield break;


        GameObject igdb = Instantiate(InGameDialogBoxPrefab, MessageBoxCanvas.transform);
        var igdbCtrl = igdb.GetComponent<InGameDialogBoxController>();

        if (_gameState == GameState.Win)
        {
            igdbCtrl.SetModeVictory();
            _gameData.ShowStartMenu = true;
        }
        else if (_gameState == GameState.Fail)
        {
            igdbCtrl.SetModeGameOver();
            _gameData.ShowStartMenu = false;
        }
    }

    private void SetUpGame(TileType[,] map)
    {
        DestroyAll(_floor);
        DestroyAll(_targetTiles);
        DestroyAll(_sandTiles);
        DestroyAll(_flippers);
        DestroyAll(_passives);
        DestroyAll(_frozen);

        _floor = new();
        _sandTiles = new();
        _targetTiles = new();
        _flippers = new();
        _passives = new();
        _frozen = new();

        _gameState = GameState.Init;
        _gameData.CurrentFlips = 0;

        int height = map.GetLength(0);
        int width = map.GetLength(1);
        int shiftX = width / 2;
        int shiftY = height / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 location = new((x - shiftX) * _tileSize, 0f, (y - shiftY) * _tileSize);
                TileType tileType = map[y, x];
                switch (tileType)
                {
                    case TileType.Flipping:
                    case TileType.Passive:
                    case TileType.Frozen:
                        PutFlipperAt(location, tileType);
                        PutFloorAt(location);
                        break;

                    case TileType.Floor:
                    case TileType.Sand:
                    case TileType.Target:
                        PutFloorAt(location, tileType);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    public void StartGame()
    {
        if (Application.isMobilePlatform)
            ShowControlPanel();

        _gameData.ShowStartMenu = true;
        _gameState = GameState.Running;

    }

    private void DestroyAll(List<GameObject> objects)
    {
        if (objects == null)
            return;

        foreach (GameObject go in objects)
            try { Destroy(go); } catch (Exception) { };
    }

    private TileType[,] ParseTextMap(string textMap)
    {
        string[] lines = textMap.Replace('\r', '\n').Replace("\n\n", "\n").Split('\n');
        int height = 0;
        int width = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length > 0)
            {
                height = i + 1;
                width = lines[i].Length;
            }
            else
            {
                break;
            }
        }

        TileType[,] map = new TileType[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[y, x] = lines[y][x] switch
                {
                    'X' => TileType.Floor,
                    'S' => TileType.Sand,
                    'F' => TileType.Flipping,
                    'P' => TileType.Passive,
                    'I' => TileType.Frozen,
                    'T' => TileType.Target,
                    _ => TileType.Hole
                };
            }
        }

        return map;
    }

    private void PutFloorAt(Vector3 location, TileType tileType = TileType.Floor)
    {
        location.y = -0.25f * _tileSize;
        GameObject tile = Instantiate(FloorTilePrefab, location, Quaternion.identity);
        Material material = tileType switch
        {
            TileType.Floor => FloorMaterial,
            TileType.Sand => SandMaterial,
            TileType.Target => TargetMaterial,
            _ => FloorMaterial
        };
        foreach (Transform child in tile.transform)
            child.gameObject.GetComponent<MeshRenderer>().material = material;

        _floor.Add(tile);

        if (tileType == TileType.Target)
            _targetTiles.Add(tile);
        else if (tileType == TileType.Sand)
            _sandTiles.Add(tile);
    }

    private void PutFlipperAt(Vector3 location, TileType tileType = TileType.Passive)
    {
        if (tileType == TileType.Frozen)
            location.y = -0.05f * _tileSize + 0.01f;
        else
            location.y =  0.05f * _tileSize;

        GameObject tile = Instantiate(FlipTilePrefab, location, Quaternion.identity);
        Material material = tileType switch
        {
            TileType.Flipping => FlippingMaterial,
            TileType.Passive => PassiveMaterial,
            TileType.Frozen => FrozenMaterial,
            _ => FlippingMaterial
        };
        tile.GetComponent<MeshRenderer>().material = material;

        if (tileType == TileType.Flipping)
            _flippers.Add(tile);
        else if (tileType == TileType.Passive)
            _passives.Add(tile);
        else if (tileType == TileType.Frozen)
            _frozen.Add(tile);
    }

    private void Win()
    {
        StartCoroutine(RiseUp(_targetTiles.ToArray(), RiseHeight, RiseDuration));
        StartCoroutine(RiseUp(_flippers.ToArray(), RiseHeight, RiseDuration));
        _gameState = GameState.Win;
        _gameData.BestFlips = _gameData.CurrentFlips;
        _gameData.ResetCameraState();
        StartCoroutine(DelayMenuShow(RiseDuration + FinalMenuDelay));

        _gameData.Level++;
    }

    private void CheckVictory()
    {
        if (_passives.Count > 0)
            return;

        foreach (var target in _targetTiles)
        {
            bool isHome = false;
            foreach (var flipper in _flippers)
            {
                if (Vector3.Distance(flipper.transform.position, target.transform.position) < 0.9 * _tileSize)
                {
                    isHome = true;
                    break;
                }
            }
            if (!isHome)
                return;
        }

        Win();
    }

    private void StartFlipping(Vector3 direction)
    {
        if (_isRolling || _gameState != GameState.Running)
            return;

        Bounds bounds = GetFlipperBounds(_flippers);

        _gameData.CurrentFlips++;

        var flipDirRay = new Ray(bounds.center, direction * -1);
        bounds.IntersectRay(flipDirRay, out float distance);

        var anchor = flipDirRay.GetPoint(distance);
        var axis = Vector3.Cross(Vector3.up, direction);

        StartCoroutine(Flip(_flippers, anchor, axis));
        Unfreeze();
    }

    private void ShowStartMessageBox()
    {
        DisableControlButtons();

        GameObject smb = Instantiate(StartMessageBoxPrefab, MessageBoxCanvas.transform);
        var smbCtrl = smb.GetComponent<StartMessageBoxController>();
        smbCtrl.LevelNo = _gameData.Level;
        smbCtrl.LevelCode = _level.Code;

        smbCtrl.OnClose = () => {
            StartGame();
            EnableControlButtons();
        };
    }

    private void ShowControlPanel()
    {
        GameObject ctrlPanel = Instantiate(ControlPanelPrefab, MessageBoxCanvas.transform);

        var panelController = ctrlPanel.GetComponent<CTRLPanelController>();

        panelController.DownAction = () => StartFlipping(Vector3.back);
        panelController.UpAction = () => StartFlipping(Vector3.forward);
        panelController.LeftAction = () => StartFlipping(Vector3.left);
        panelController.RightAction = () => StartFlipping(Vector3.right);
    }

    public void ShowInGameDialog()
    {
        if (_gameState != GameState.Running)
            return;

        _gameState = GameState.Paused;
        DisableControlButtons();

        GameObject igdb = Instantiate(InGameDialogBoxPrefab, MessageBoxCanvas.transform);
        var igdbCtrl = igdb.GetComponent<InGameDialogBoxController>();
        igdbCtrl.SetModePaused(_gameData.Level, _level.Code);
        igdbCtrl.OnClose = () =>
        {
            _gameState = GameState.Running;
            EnableControlButtons();
        };
    }

    private void EnableControlButtons()
    {
        var btnPanelCtrl = ButtonPanel.GetComponent<BtnPanelController>();
        btnPanelCtrl.EnableAll();
    }
    private void DisableControlButtons()
    {
        var btnPanelCtrl = ButtonPanel.GetComponent<BtnPanelController>();
        btnPanelCtrl.DisableAll();
    }
}
