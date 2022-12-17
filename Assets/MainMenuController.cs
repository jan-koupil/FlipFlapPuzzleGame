using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] TMP_InputField _codeInput;
    [SerializeField] TMP_Text _levelText;
    private GameData _gameData;
    private Color _defaultBgColor;

    void Awake()
    {
        _gameData = GameObject.FindObjectOfType<GameData>();
        _defaultBgColor = _codeInput.colors.normalColor;
    }

    void Start()
    {
        RenderLevelNo();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Load()
    {
        string code = _codeInput.text.ToUpper();
        int levelNo = Level.FindLevel(code);
        Debug.Log(_codeInput.colors.normalColor);
        if (levelNo > 1)
        {

            ColorBlock cb = _codeInput.colors;
            cb.normalColor = _defaultBgColor;
            _codeInput.colors = cb;

            _gameData.Level = levelNo;
            _codeInput.text = "";
            RenderLevelNo();
        }
        else
        {
            ColorBlock cb = _codeInput.colors;
            cb.normalColor = new Color(255, 0, 0);
            _codeInput.colors = cb;
        }
    }

    private void RenderLevelNo()
    {
        _levelText.text = $"Level: {_gameData.Level}";
    }
}
