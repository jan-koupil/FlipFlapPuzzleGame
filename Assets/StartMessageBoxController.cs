using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StartMessageBoxController : MonoBehaviour
{
    public TMP_Text Message;    
    public Button ContinueBtn;    
    //public TMP_Text BackText;
    public bool IsVisible { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        this.Hide();
    }

    // Update is called once per frame
    public void Show(int levelNo, string code)
    {
        Message.text = $"Level {levelNo}: CODE \"{code}\"";
        gameObject.SetActive(true);
        IsVisible = true;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        IsVisible = false;
    }
    //public void ButtonPress()
    //{
    //    this.Hide();
    //}

}
