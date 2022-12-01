using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameScript : MonoBehaviour
{
    // Start is called before the first frame update
    
    public float RollSpeed = 3;
    public Material FlippingMaterial;
    public GameObject FloorTile;

    private bool _isRolling;
    private bool _gameOver;
    private const int _tileSize = 1;
    private List<GameObject> _floor = new();

    
    void Start()
    {
        _gameOver = false;
        for (int x = -6; x < 6; x++)
        {
            for (int z = -6; z < 6; z++)
            {
                _floor.Add(Instantiate(FloorTile, new Vector3(x * _tileSize, -0.25f, z * _tileSize), Quaternion.identity));
            }
        }
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
            var flippers = GetFlippers();
            Bounds bounds = GetFlipperBounds(flippers);

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
                StartCoroutine(Flip(flippers, anchor, axis));
            }
        }

    }

    private Vector3 ScaleVector (Vector3 vector, Vector3 scale)
    {
        return new Vector3(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z);
    }
    private GameObject[] GetFlippers()
    {
        return GameObject.FindGameObjectsWithTag("Flipping");
    }

    private Bounds GetFlipperBounds(GameObject[] flippers)
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

    IEnumerator Flip(GameObject[] flippers, Vector3 anchor, Vector3 axis)
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
        GameObject[] flippers = GetFlippers();
        GameObject[] passives = GameObject.FindGameObjectsWithTag("Passive");
        
        Debug.Log(flippers.Length);
        Debug.Log(passives.Length);

        bool runAgain = false;

        foreach (GameObject flipper in flippers)
        {
            foreach(GameObject passive in passives)
            {
                float distance = Vector3.Distance(flipper.transform.position, passive.transform.position);
                Debug.Log(distance);
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
        passiveTile.tag = "Flipping";
        passiveTile.GetComponent<MeshRenderer>().material = FlippingMaterial;
    }

    private void FindAndBreakOverlaps()
    {
        GameObject[] flippers = GetFlippers();

        foreach (GameObject flipper in flippers)
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
        overlappingTile.tag = "Broken";
        _gameOver = true;
    }
}
