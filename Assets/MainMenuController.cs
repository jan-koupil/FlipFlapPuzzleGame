using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private GameData _gameData;
    private Color _defaultBgColor;
    private TMP_Text _levelText;
    private TMP_InputField _codeInput;

    private List<GameObject> _UISelectables = new List<GameObject>();

    void Awake()
    {
        _gameData = GameObject.FindObjectOfType<GameData>();
        _levelText = transform.Find("LevelText").gameObject.GetComponent<TMP_Text>();
        _codeInput = transform.Find("CodeInput").gameObject.GetComponent<TMP_InputField>();

        _UISelectables.Add(transform.Find("StartBtn").gameObject);
        _UISelectables.Add(transform.Find("CodeInput").gameObject);
        _UISelectables.Add(transform.Find("LoadBtn").gameObject);
        _UISelectables.Add(transform.Find("ExitBtn").gameObject);
    }

    void Start()
    {
        _defaultBgColor = _codeInput.colors.normalColor;
        RenderLevelNo();
        //EventSystem.current.SetSelectedGameObject(transform.Find("StartBtn").gameObject);
        EventSystem.current.SetSelectedGameObject(_UISelectables[0]);
        //transform.Find("StartBtn").gameObject.GetComponent<UnityEngine.UIElements.Button>().Focus(); ;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
        {
            int UIIndex = FindSelectedIndex();
            if (UIIndex > -1)
            {
                UIIndex--;
                UIIndex %= _UISelectables.Count;
                EventSystem.current.SetSelectedGameObject(_UISelectables[UIIndex]);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            int UIIndex = FindSelectedIndex();
            if (UIIndex > -1)
            {
                UIIndex++;
                UIIndex %= _UISelectables.Count;
                EventSystem.current.SetSelectedGameObject(_UISelectables[UIIndex]);
            }
        }
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
        //Debug.Log(_codeInput.colors.normalColor);
        if (levelNo > 1)
        {

            ColorBlock cb = _codeInput.colors;
            cb.normalColor = _defaultBgColor;
            _codeInput.colors = cb;

            _gameData.Level = levelNo;
            _codeInput.text = "";
            RenderLevelNo();
            EventSystem.current.SetSelectedGameObject(_UISelectables[0]); //Focus Play Btn
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

    private int FindSelectedIndex()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        return _UISelectables.IndexOf(selected);   
    }
}
