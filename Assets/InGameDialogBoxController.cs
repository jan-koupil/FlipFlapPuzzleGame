using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InGameDialogBoxController : MonoBehaviour
{
    private TMP_Text _message;
    private TMP_Text _continueText;
    public Action OnClose = null;

    private List<GameObject> _UISelectables = new List<GameObject>();

    private GameObject _continueBtn;

    public void Awake()
    {
        _message = transform.Find("MessageText").gameObject.GetComponent<TMP_Text>();
        _continueText = transform.Find("ContinueBtn/ContinueText").gameObject.GetComponent<TMP_Text>();

        _UISelectables.Add(transform.Find("ContinueBtn").gameObject);
        _UISelectables.Add(transform.Find("BackToMenuBtn").gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(_UISelectables[0]);
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
    public void SetModeGameOver()
    {
        _message.text = "Game Over";
        _continueText.text = "Try again";
        OnClose = () => SceneManager.LoadScene("GameScene");
    }
    public void SetModeVictory()
    {
        _message.text = "Good job!";
        _continueText.text = "Next level";
        OnClose = () => SceneManager.LoadScene("GameScene");
    }

    public void SetModePaused(int level, string code)
    {
        _message.text = $"Game paused\nLevel {level}, code {code}";
        _continueText.text = "Continue";
    }
    public void OnCloseBtnClick()
    {
        OnClose?.Invoke();
        Destroy(gameObject);
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
