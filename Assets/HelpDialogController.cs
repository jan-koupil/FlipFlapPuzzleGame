using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HelpDialogController : MonoBehaviour
{
    public Action OnClose = null;

    private GameObject _closeBtn;

    public void Awake()
    {
        _closeBtn = transform.Find("CloseHelpBtn").gameObject;
    }
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(_closeBtn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCloseBtnClick()
    {
        OnClose?.Invoke();
        Destroy(gameObject);
    }


}
