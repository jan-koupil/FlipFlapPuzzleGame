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

    public float RollSpeed = 3;

    private bool _isRolling;
    private bool _gameOver;

    private const int _tileSize = 1;
    
    private List<GameObject> _floor = new();
    private List<GameObject> _targetTiles = new();
    private List<GameObject> _flippers = new();
    private List<GameObject> _passives = new();

    private enum TileTipe : byte { Flipping, Passive, Floor, Target, Hole }


    void Start()
    {
        _gameOver = false;
        
        string textMap = 
            "XXXXXXX\n" +
            "XFXXPXX\n" +
            "XXPXXXX\n" +
            "XXX XXX\n" +
            "XXTTXXX\n" +
            "XXTXXXX\n" +
            "XXXXXXX\n";

        BuildGame(ParseTextMap(textMap));
    }

    // Update is called once per frame
    void Update()
    {
        if (_isRolling) return;

        if (!_gameOver && Input.anyKey) {
            Bounds bounds = GetFlipperBounds(_flippers);

            if (Input.GetKey(KeyCode.W))
                StartFlipping(Vector3.forward);
            else if (Input.GetKey(KeyCode.S))
                StartFlipping(Vector3.back);
            else if (Input.GetKey(KeyCode.A))
                StartFlipping(Vector3.left);
            else if (Input.GetKey(KeyCode.D))
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

    private Vector3 ScaleVector (Vector3 vector, Vector3 scale)
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
            Vector3 extents = ScaleVector(currentBounds.extents, scale);
            Vector3 maxV = flipper.transform.position + extents;
            Vector3 minV = flipper.transform.position - extents;
            if (first)
            {
                bounds = new Bounds(flipper.transform.position, extents * 2);
                first = false;
            }
            else
            { 
                bounds.Encapsulate(maxV);
                bounds.Encapsulate(minV);
            }
        }

        return bounds;
    }

    IEnumerator Flip(List<GameObject> flippers, Vector3 anchor, Vector3 axis)
    {
        _isRolling = true;

        for (int i = 0; i < (180 / RollSpeed); i++)
        {
            foreach(var flipper in flippers)
            {
                flipper.transform.RotateAround(anchor, axis, RollSpeed);
            }
            yield return null;
            //yield return new WaitForSeconds(0.01f);
        }

        _isRolling = false;
        MergeAdjacentTiles();
    }

    private void MergeAdjacentTiles()
    {
        bool runAgain = false;

        foreach (GameObject flipper in _flippers.ToArray())
        {
            foreach(GameObject passive in _passives.ToArray())
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

        FindAndBreakOverlaps();
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
        // You can even access the rigidbody with no effort
        currentRb.detectCollisions = true;
        _flippers.Remove(overlappingTile);
        _gameOver = true;
    }

    private void BuildGame(TileTipe[,] map)
    {
        _floor = new();
        _targetTiles = new();
        _flippers = new();
        _passives = new();

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
                    case TileTipe.Flipping:
                        PutFlipperAt(location);
                        PutFloorAt(location);
                        break;

                    case TileTipe.Passive:
                        PutFlipperAt(location, true);
                        PutFloorAt(location);
                        break;

                    case TileTipe.Floor:
                        PutFloorAt(location);
                        break;

                    case TileTipe.Target:
                        PutFloorAt(location, true);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    private TileTipe[,] ParseTextMap(string textMap)
    {
        string[] lines = textMap.Replace('\r','\n').Replace("\n\n", "\n").Split('\n');
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

        TileTipe[,] map = new TileTipe[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[y, x] = lines[y][x] switch
                {
                    'X' => TileTipe.Floor,
                    'F' => TileTipe.Flipping,
                    'P' => TileTipe.Passive,
                    'T' => TileTipe.Target,
                    _ => TileTipe.Hole
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

}
