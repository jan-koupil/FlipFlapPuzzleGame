using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BtnPanelController : MonoBehaviour
{
    [SerializeField] GameObject HelpWindow;
    [SerializeField] Camera Camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void DisableAll()
    {

    }

    public void EnableAll()
    {

    }

    public void ZoomIn()
    {

    }

    public void ZoomOut()
    {

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
