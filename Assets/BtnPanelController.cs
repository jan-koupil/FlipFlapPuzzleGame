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
        SetZoom();
        SetCamToogleColor();
    }

    //public void DisableAll()
    //{

    //}

    //public void EnableAll()
    //{

    //}

    public void ToggleCameraLock()
    {
        _gameData.LockedCamera = !_gameData.LockedCamera;
        SetCamToogleColor();
    }

    private void SetCamToogleColor()
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
}
