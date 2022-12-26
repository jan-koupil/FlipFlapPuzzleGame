using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class StartMessageBoxController : MonoBehaviour
{
    private TMP_Text _message;    
    public Action OnClose = null;
    public int LevelNo { get; set; }
    public string LevelCode { get; set; }

    private void Awake()
    {
        _message = transform.Find("MessageText").gameObject.GetComponent<TMP_Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(transform.Find("ContinueBtn").gameObject);
    }

    public void Update ()
    {
        _message.text = $"Level: {LevelNo}\nCode: {LevelCode}";
    }

    public void OnCloseBtnClick()
    {
        OnClose?.Invoke();

        Destroy(gameObject);
    }
}
