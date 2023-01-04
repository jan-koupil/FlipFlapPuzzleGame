using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BtnPanelController : MonoBehaviour
{
    [SerializeField] GameObject HelpWindow;
    [SerializeField] Camera Cam;

    private GameData _gameData;

    private GameObject _zoomInBtn;
    private GameObject _zoomOutBtn;
    private GameObject _camLockBtn;

    List<GameObject> _allButtons = new List<GameObject>();


    private void Awake()
    {
        _gameData = GameObject.FindObjectOfType<GameData>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _zoomInBtn = transform.Find("ZoomInBtn").gameObject;
        _zoomOutBtn = transform.Find("ZoomOutBtn").gameObject;
        _camLockBtn = transform.Find("CamLockBtn").gameObject;
        FindAllChildren();
        SetZoom();
        SetCamToggleColor();
    }

    private void FindAllChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
            _allButtons.Add(transform.GetChild(i).gameObject);
    }

    public void DisableAll()
    {
        //_allButtons.ForEach(b => { b.SetActive(false); });
        _allButtons.ForEach(b => { b.GetComponent<Button>().interactable = false; });        
    }

    public void EnableAll()
    {
        //_allButtons.ForEach(b => { b.SetActive(true); });
        _allButtons.ForEach(b => { b.GetComponent<Button>().interactable = true; });
    }

    public void Update()
    {
        if ( Input.GetKeyDown(KeyCode.C) )
        {
            ToggleCameraLock();
        }
        else if (Input.GetKeyDown(KeyCode.H) )
        {
            ShowHelp();
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            ZoomIn();
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            ZoomOut();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RestartLevel();
        }

    }

    public void ToggleCameraLock()
    {
        _gameData.LockedCamera = !_gameData.LockedCamera;
        SetCamToggleColor();
    }

    private void SetCamToggleColor()
    {
        _camLockBtn.GetComponent<Image>().color = _gameData.LockedCamera ? Color.yellow : Color.green;
    }

    public void ZoomIn()
    {
        _gameData.Zoom--;
        SetZoom();
    }

    public void ZoomOut()
    {
        _gameData.Zoom++;
        SetZoom();
    }

    private void SetZoom()
    {
        Cam.orthographicSize = _gameData.Zoom;
        _zoomInBtn.GetComponent<Button>().interactable = !_gameData.IsMinZoom;
        _zoomOutBtn.GetComponent<Button>().interactable = !_gameData.IsMaxZoom;
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

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartLevel()
    {
        _gameData.ShowStartMenu = false;
        SceneManager.LoadScene("GameScene");
    }
}
