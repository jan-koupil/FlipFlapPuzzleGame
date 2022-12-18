using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class StartMessageBoxController : MonoBehaviour
{
    private TMP_Text _message;    
    private Button _continueBtn;    
    public bool IsVisible { get; private set; }

    private void Awake()
    {
        _message = transform.Find("MessageText").gameObject.GetComponent<TMP_Text>();
        _continueBtn = transform.Find("ContinueBtn").gameObject.GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.Hide();
    }

    // Update is called once per frame
    public void Show(int levelNo, string code)
    {
        _message.text = $"Level: {levelNo}\nCode: {code}";
        gameObject.SetActive(true);
        IsVisible = true;
        EventSystem.current.SetSelectedGameObject(transform.Find("ContinueBtn").gameObject);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        IsVisible = false;
    }
}
