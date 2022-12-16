using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public int CurrentFlips = 0;
    public int BestFlips = 0;
}
