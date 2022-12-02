using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GameScript : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject FloorTile;
    public GameObject FlipTile;

    public Material FlippingMaterial;
    public Material PassiveMaterial;
    public Material FloorMaterial;
    public Material TargetMaterial;

    public float RollSpeed;
    public float RiseSpeed;
    public float RiseHeight;

    private bool _isRolling;
    private GameState _gameState;

    private const int _tileSize = 1;

    private List<GameObject> _floor = null;
    private List<GameObject> _targetTiles = null;
    private List<GameObject> _flippers = null;
    private List<GameObject> _passives = null;

    private TileType[,] _gameMap;

    private enum GameState : byte { Running, Win, Fail }
    private enum TileType : byte { Hole, Flipping, Passive, Floor, Target }

    void Start()
    {
        _gameState = GameState.Running;

        string textMap =
            "XXXXXXX\n" +
            "XFXXXXX\n" +
            "XXPXXXX\n" +
            "XXX PXX\n" +
            "XXTTXXX\n" +
            "XXTXXXX\n" +
            "XXXXXXX\n";

        //string textMap =
        //    "XXXX\n" +
        //    "XFXX\n" +
        //    "XXTX\n" +
        //    "XXXX\n";

        //string textMap =
        //    "XXXX\n" +
        //    "XFXX\n" +
        //    "TXPX\n" +
        //    "TXXX\n";

        _gameMap = ParseTextMap(textMap);
        BuildGame(_gameMap);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && _gameState != GameState.Win)
            BuildGame(_gameMap);

        if (_isRolling || _gameState != GameState.Running) 
            return;

        if (Input.anyKey)
        {
            Bounds bounds = GetFlipperBounds(_flippers);

            if (Input.GetKey(KeyCode.A))
                StartFlipping(Vector3.forward);
            else if (Input.GetKey(KeyCode.Z))
                StartFlipping(Vector3.back);
            else if (Input.GetKey(KeyCode.N))
                StartFlipping(Vector3.left);
            else if (Input.GetKey(KeyCode.M))
                StartFlipping(Vector3.right);

            void StartFlipping(Vector3 direction)
            {
                var flipDirRay = new Ray(bounds.center, direction * -1);
                bounds.IntersectRay(flipDirRay, out float distance);
                var anchor = flipDirRay.GetPoint(distance);
                var axis = Vector3.Cross(Vector3.up, direction);
                StartCoroutine(Flip(_flippers, anchor, axis));
            }
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
            yield return null;
            //yield return new WaitForSeconds(0.01f);
        }

        _isRolling = false;

        MergeAdjacentTiles();
        FindAndBreakOverlaps();

        if (_gameState == GameState.Running)
            CheckVictory();
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
        overlappingTile.transform.localScale *= 0.95f;
        Rigidbody currentRb = overlappingTile.AddComponent<Rigidbody>();
        currentRb.detectCollisions = true;
        _flippers.Remove(overlappingTile);
        _gameState = GameState.Fail;
    }

    private void BuildGame(TileType[,] map)
    {
        DestroyAll(_floor);
        DestroyAll(_targetTiles);
        DestroyAll(_flippers);
        DestroyAll(_passives);

        _floor = new();
        _targetTiles = new();
        _flippers = new();
        _passives = new();

        _gameState = GameState.Running;

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
}
