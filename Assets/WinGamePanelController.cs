using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WinGamePanelController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject backBtn = transform.Find("BackBtn").gameObject;
        EventSystem.current.SetSelectedGameObject(backBtn);

    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
