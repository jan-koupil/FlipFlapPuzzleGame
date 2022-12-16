using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public int CurrentSteps;
}
