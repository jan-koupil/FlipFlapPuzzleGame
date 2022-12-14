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
    [SerializeField] GameObject HelpWindow;

    private GameData _gameData;
    //private Color _defaultBgColor;
    private Color _selectedBgColor;
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
        _UISelectables.Add(transform.Find("HelpBtn").gameObject);
        _UISelectables.Add(transform.Find("ExitBtn").gameObject);
    }

    void Start()
    {
        //_defaultBgColor = _codeInput.colors.normalColor;
        _selectedBgColor = _codeInput.colors.selectedColor;
        _gameData.ResetCameraState();
        RenderLevelNo();
        //EventSystem.current.SetSelectedGameObject(transform.Find("StartBtn").gameObject);
        EventSystem.current.SetSelectedGameObject(_UISelectables[0]);
        //transform.Find("StartBtn").gameObject.GetComponent<UnityEngine.UIElements.Button>().Focus(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
        {
            SelectPreviousElement();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            SelectNextElement();
        }
        else if (EventSystem.current.currentSelectedGameObject == _codeInput.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Load();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SelectNextElement();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SelectPreviousElement();
            }
        }
    }

    private void SelectPreviousElement()
    {
        int UIIndex = FindSelectedIndex();
        if (UIIndex > -1)
        {
            UIIndex--;
            if (UIIndex < 0)
                UIIndex += _UISelectables.Count;
            EventSystem.current.SetSelectedGameObject(_UISelectables[UIIndex]);
        }
    }

    private void SelectNextElement()
    {
        int UIIndex = FindSelectedIndex();
        if (UIIndex > -1)
        {
            UIIndex++;
            UIIndex %= _UISelectables.Count;
            EventSystem.current.SetSelectedGameObject(_UISelectables[UIIndex]);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void ShowHelp()
    {
        gameObject.SetActive(false);

        GameObject hw = Instantiate(HelpWindow, this.transform.parent);
        var hwCtrl = hw.GetComponent<HelpDialogController>();
        hwCtrl.OnClose = () => { 
            gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(transform.Find("HelpBtn").gameObject);
        };
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Load()
    {
        string code = _codeInput.text.ToUpper().Trim();
        int levelNo = Level.FindLevel(code);
        //Debug.Log(_codeInput.colors.normalColor);
        if (levelNo > 0)
        {

            ColorBlock cb = _codeInput.colors;
            //cb.normalColor = _defaultBgColor;
            cb.normalColor = _selectedBgColor;
            _codeInput.colors = cb;

            _gameData.Level = levelNo;
            //_codeInput.text = "";
            RenderLevelNo();
            EventSystem.current.SetSelectedGameObject(_UISelectables[0]); //Focus Play Btn
        }
        else
        {
            ColorBlock cb = _codeInput.colors;
            cb.normalColor = new Color(255, 0, 0);
            cb.selectedColor = new Color(255, 0, 0);
            _codeInput.colors = cb;
            EventSystem.current.SetSelectedGameObject(_codeInput.gameObject);
            _codeInput.caretPosition = _codeInput.text.Length - 1;
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
