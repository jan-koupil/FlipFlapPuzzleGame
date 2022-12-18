using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Level
{
    private static List<Level> _levelList = new();
    public string TextMap { get; private set; }
    public string Code { get; private set; }

    static Level()
    {
        InitLevels();
    }

    public static Level GetLevel(int levelNo)
    {
        //Debug.Log(levelNo.ToString());
        int index = levelNo - 1;
        if (index < _levelList.Count)
            return _levelList[index];
        else return null;
    }

    /// <summary>
    /// Finds level no. by level code
    /// </summary>
    /// <param name="code">Text code given by player</param>
    /// <returns>Number of level, otherwise 0</returns>
    public static int FindLevel(string code)
    {
        return _levelList.FindIndex(l => l.Code == code) + 1;
    }

    public Level(string textMap, string code)
    {
        TextMap = textMap;
        Code = code;
    }

    private static void InitLevels()
    {
        _levelList.Add(new Level(
            "XXXX\n" +
            "XFXX\n" +
            "XXTX\n" +
            "XXXX\n",
            "SQUARE"
        ));

        _levelList.Add(new Level(
            "FX     \n" +
            " XX    \n" +
            "  XX   \n" +
            "   XT  \n",
            "PATH"
        ));
        
        _levelList.Add(new Level(
            "XXXX\n" +
            "XFXX\n" +
            "TXPX\n" +
            "TXXX\n",
            "PAIR"
        ));

        _levelList.Add(new Level(
            "XXXXXXX\n" +
            "XFXXXXX\n" +
            "XXPXXXX\n" +
            "XXX PXX\n" +
            "XXTTXXX\n" +
            "XXTXXXX\n" +
            "XXXXXXX\n",
            "DOUGHNUT"
        ));


    }
}
