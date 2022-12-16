using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxController : MonoBehaviour
{  
    public TMP_Text Message;
    public Button BackBtn;
    public Button ContinueBtn;
    public TMP_Text ContinueText;
    public TMP_Text BackText;    

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);       
    }

    // Update is called once per frame
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void SetModeGameOver()
    {
        Message.text = "Game Over";
        BackText.text = "Back to menu";
        ContinueText.text = "Try again";
    }
    public void SetModeVictory()
    {
        Message.text = "Good job!";
        BackText.text = "Back to menu";
        ContinueText.text = "Next level";
    }

    private void Replay()
    {

    }
    private void BackToMenu()
    {

    }
    private void Advance()
    {

    }

}
