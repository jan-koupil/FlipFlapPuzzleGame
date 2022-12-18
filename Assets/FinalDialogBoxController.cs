using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalDialogBoxController : MonoBehaviour
{  
    private TMP_Text _message;
    private TMP_Text _continueText;
    
    private List<GameObject> _UISelectables = new List<GameObject>();


    private void Awake()
    {
        _message = transform.Find("MessageText").gameObject.GetComponent<TMP_Text>();
        _continueText = transform.Find("ContinueBtn/ContinueText").gameObject.GetComponent<TMP_Text>();

        _UISelectables.Add(transform.Find("ContinueBtn").gameObject);
        _UISelectables.Add(transform.Find("BackToMenuBtn").gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.Hide();       
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
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
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            int UIIndex = FindSelectedIndex();
            if (UIIndex > -1)
            {
                UIIndex++;
                UIIndex %= _UISelectables.Count;
                EventSystem.current.SetSelectedGameObject(_UISelectables[UIIndex]);
            }
        }
    }
    public void Show()
    {
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_UISelectables[0]);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void SetModeGameOver()
    {
        _message.text = "Game Over";
        _continueText.text = "Try again";
    }
    public void SetModeVictory()
    {
        _message.text = "Good job!";
        _continueText.text = "Next level";
    }

    public void PlayLevel()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private int FindSelectedIndex()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        return _UISelectables.IndexOf(selected);
    }
}
