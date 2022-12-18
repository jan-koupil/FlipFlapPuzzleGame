using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] GameObject FloorTile;
    [SerializeField] GameObject FlipTile;

    [SerializeField] Material FlippingMaterial;
    [SerializeField] Material PassiveMaterial;
    [SerializeField] Material FloorMaterial;
    [SerializeField] Material TargetMaterial;

    [SerializeField] float RollSpeed = 3;
    [SerializeField] float RiseSpeed = 0.005f;
    [SerializeField] float RiseHeight = 0.15f;
    [SerializeField] float FinalMenuDelay = 1.5f;

    [SerializeField] TMP_Text FlipCountDisplay;
    [SerializeField] TMP_Text BestFlipCountDisplay;
    [SerializeField] GameObject FinalDialogBox;
    [SerializeField] GameObject StartMessageBox;

    private StartMessageBoxController _startMessageBoxController;
    private FinalDialogBoxController _finalDialogBoxController;
    private Level _level;

    /// <summary>
    /// Shows wheter Flippy is in motion or can be controlled
    /// </summary>
    private bool _isRolling;
    private GameState _gameState;

    private const int _tileSize = 1;
    private const float _flipOverFlipperDist = 0.25f;

    private List<GameObject> _floor = null;
    private List<GameObject> _targetTiles = null;
    private List<GameObject> _flippers = null;
    private List<GameObject> _passives = null;

    private TileType[,] _gameMap;
    private GameData _gameData;

    private enum GameState : byte { Init, Running, Win, Fail }
    private enum TileType : byte { Hole, Flipping, Passive, Floor, Target }

    private void Awake()
    {
        _startMessageBoxController = StartMessageBox.GetComponent<StartMessageBoxController>();
        _finalDialogBoxController = FinalDialogBox.GetComponent<FinalDialogBoxController>();
        _gameData = GameObject.FindObjectOfType<GameData>();

    }

    void Start()
    {
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
        _startMessageBoxController.Show(_gameData.Level, _level.Code);
    }

    // Update is called once per frame
    void Update()
    {
        if (
            Input.GetKeyDown(KeyCode.Backspace) && 
            _gameState != GameState.Win && 
            !_startMessageBoxController.IsVisible
        )
        { 
            SetUpGame(_gameMap);
            StartGame();
        }

        if (_isRolling || _gameState != GameState.Running) 
            return;

        if (Input.anyKey)
        {

            if (Input.GetKey(KeyCode.A))
                StartFlipping(Vector3.forward);
            else if (Input.GetKey(KeyCode.Z))
                StartFlipping(Vector3.back);
            else if (Input.GetKey(KeyCode.N))
                StartFlipping(Vector3.left);
            else if (Input.GetKey(KeyCode.M))
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

        for (int i = 0; i < (180 / RollSpeed); i++)
        {
            foreach (var flipper in flippers)
            {
                flipper.transform.RotateAround(anchor, axis, RollSpeed);
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

    IEnumerator Highlight(List<GameObject> endObjects)
    {
        float steps = RiseHeight * _tileSize / RiseSpeed;
        for (int i = 0; i < steps; i++)
        {
            foreach (GameObject go in endObjects)
            {
                go.transform.position += Vector3.up * RiseSpeed * _tileSize;
            }
            //yield return null;
            yield return new WaitForSeconds(0.01f);
        }
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
        overlappingTile.transform.localScale *= 0.98f;
        Rigidbody currentRb = overlappingTile.AddComponent<Rigidbody>();
        currentRb.detectCollisions = true;
        _flippers.Remove(overlappingTile);
        _passives.Add(overlappingTile);

        GameOver();
    }

    private void GameOver()
    {
        _gameState = GameState.Fail;
        _finalDialogBoxController.SetModeGameOver();
        StartCoroutine(DelayMenuShow(FinalMenuDelay));
    }


    IEnumerator DelayMenuShow(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        _finalDialogBoxController.Show();
    }


    private void SetUpGame(TileType[,] map)
    {
        DestroyAll(_floor);
        DestroyAll(_targetTiles);
        DestroyAll(_flippers);
        DestroyAll(_passives);

        _floor = new();
        _targetTiles = new();
        _flippers = new();
        _passives = new();

        _finalDialogBoxController.Hide();

        _gameState = GameState.Init;
        _gameData.CurrentFlips = 0;
        RenderFlipCount();

        int height = map.GetLength(0);
        int width = map.GetLength(1);
        int shiftX = width / 2;
        int shiftY = height / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 location = new((x - shiftX) * _tileSize, 0f, (y - shiftY) * _tileSize);
                switch (map[y, x])
                {
                    case TileType.Flipping:
                        PutFlipperAt(location);
                        PutFloorAt(location);
                        break;

                    case TileType.Passive:
                        PutFlipperAt(location, true);
                        PutFloorAt(location);
                        break;

                    case TileType.Floor:
                        PutFloorAt(location);
                        break;

                    case TileType.Target:
                        PutFloorAt(location, true);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    public void StartGame()
    {
        _startMessageBoxController.Hide();
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
                    'F' => TileType.Flipping,
                    'P' => TileType.Passive,
                    'T' => TileType.Target,
                    _ => TileType.Hole
                };
            }
        }

        return map;
    }

    private void PutFloorAt(Vector3 location, bool isTarget = false)
    {
        location.y = -0.25f * _tileSize;
        GameObject tile = Instantiate(FloorTile, location, Quaternion.identity);
        Material material = isTarget ? TargetMaterial : FloorMaterial;
        foreach (Transform child in tile.transform)
            child.gameObject.GetComponent<MeshRenderer>().material = material;

        _floor.Add(tile);

        if (isTarget)
            _targetTiles.Add(tile);
    }
    private void PutFlipperAt(Vector3 location, bool isPassive = false)
    {
        location.y = 0.05f * _tileSize;
        GameObject tile = Instantiate(FlipTile, location, Quaternion.identity);
        Material material = isPassive ? PassiveMaterial : FlippingMaterial;
        tile.GetComponent<MeshRenderer>().material = material;

        if (isPassive)
            _passives.Add(tile);
        else
            _flippers.Add(tile);
    }

    private void Win()
    {
        StartCoroutine(Highlight(_targetTiles));
        StartCoroutine(Highlight(_flippers));
        _gameState = GameState.Win;
        _gameData.BestFlips = _gameData.CurrentFlips;

        _finalDialogBoxController.SetModeVictory();
        StartCoroutine(DelayMenuShow(FinalMenuDelay));

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

    private void RenderFlipCount()
    {
        int best = _gameData.BestFlips;
        BestFlipCountDisplay.text = best != 0 ? _gameData.BestFlips.ToString() : "-";
        FlipCountDisplay.text = _gameData.CurrentFlips.ToString();
    }

    private void StartFlipping(Vector3 direction)
    {
        if (_isRolling || _gameState != GameState.Running)
            return;

        Bounds bounds = GetFlipperBounds(_flippers);

        _gameData.CurrentFlips++;
        RenderFlipCount();

        var flipDirRay = new Ray(bounds.center, direction * -1);
        bounds.IntersectRay(flipDirRay, out float distance);

        var anchor = flipDirRay.GetPoint(distance);
        var axis = Vector3.Cross(Vector3.up, direction);

        StartCoroutine(Flip(_flippers, anchor, axis));
    }

    public void FlipUp()
    {
        StartFlipping(Vector3.forward);
    }
    public void FlipDown()
    {
        StartFlipping(Vector3.back);
    }
    public void FlipLeft()
    {
        StartFlipping(Vector3.left);
    }
    public void FlipRight()
    {
        StartFlipping(Vector3.right);
    }
}



