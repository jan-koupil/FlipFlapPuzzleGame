using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public int Level { get; set; } = 1;
    public int CurrentFlips { get; set; } = 0;
    public int BestFlips { get; set; } = 0;
}
