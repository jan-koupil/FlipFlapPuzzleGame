using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameScript : MonoBehaviour
{
    // Start is called before the first frame update
    
    public GameObject FloorTile;
    public GameObject FlipTile;

    public Material FlippingMaterial;
    public Material PassiveMaterial;
    public Material TargetMaterial;

    public float RollSpeed = 3;

    private bool _isRolling;
    private bool _gameOver;

    private const int _tileSize = 1;
    
    private List<GameObject> _floor = new();
    private List<GameObject> _target = new();
    private List<GameObject> _flippers = new();
    private List<GameObject> _passives = new();


    void Start()
    {
        _gameOver = false;        
        BuildGame();
    }

    // Update is called once per frame
    void Update()
    {
        //Bounds bounds = flippers.Aggregate(new Bounds(), (b, go) => b.Encapsulate());
        if (_isRolling) return;

        //if (Input.GetKeyDown(KeyCode.Space))
        //{

        //    Debug.Log("Next one");
        //    Debug.Log(bounds.center);
        //    Debug.Log(bounds.size);
        //    Debug.Log(bounds.extents);
        //    Debug.Log(bounds.max);
        //    Debug.Log(bounds.min);
        //}

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
        Rigidbody currentRb = overlappingTile.AddComponent<Rigidbody>();
        // You can even access the rigidbody with no effort
        currentRb.detectCollisions = true;
        _flippers.Remove(overlappingTile);
        _gameOver = true;
    }

    private void BuildGame()
    {
        for (int x = -6; x < 6; x++)
        {
            for (int z = -6; z < 6; z++)
            {
                _floor.Add(Instantiate(FloorTile, new Vector3(x * _tileSize, -0.25f, z * _tileSize), Quaternion.identity));
            }
        }

        GameObject firstTile = Instantiate(FlipTile, new Vector3(2 * _tileSize, 0.05f * _tileSize, 1 * _tileSize), Quaternion.identity);
        firstTile.GetComponent<MeshRenderer>().material = FlippingMaterial;
        _flippers.Add(firstTile);


        GameObject passive1 = Instantiate(FlipTile, new Vector3(0 * _tileSize, 0.05f * _tileSize, 1 * _tileSize), Quaternion.identity);
        passive1.GetComponent<MeshRenderer>().material = PassiveMaterial;
        _passives.Add(passive1);

        GameObject passive2 = Instantiate(FlipTile, new Vector3(-2 * _tileSize, 0.05f * _tileSize, 3 * _tileSize), Quaternion.identity);
        passive2.GetComponent<MeshRenderer>().material = PassiveMaterial;
        _passives.Add(passive2);

        GameObject passive3 = Instantiate(FlipTile, new Vector3(1 * _tileSize, 0.05f * _tileSize, 3 * _tileSize), Quaternion.identity);
        passive1.GetComponent<MeshRenderer>().material = PassiveMaterial;
        _passives.Add(passive3);

    }
}
