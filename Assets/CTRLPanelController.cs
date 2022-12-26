using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTRLPanelController : MonoBehaviour
{
    public Action UpAction;
    public Action DownAction;
    public Action LeftAction;
    public Action RightAction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnUpBtn()
    {
        UpAction?.Invoke();
    }

    public void OnDownBtn()
    {
        DownAction?.Invoke();

    }
    public void OnLeftBtn()
    {
        LeftAction?.Invoke();

    }
    public void OnRightBtn()
    {
        RightAction?.Invoke();
    }
}
