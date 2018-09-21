using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActivityUIManager : UIManager
{
    private MyButton _closeBtn;

    public override void InitUI()
    {
        base.InitUI();

        _closeBtn = _btns[0];
        _closeBtn.onClick.AddListener(OnCloseBtnClick);

        CurrentSelectedObj = _closeBtn.gameObject;
    }

    public override void InitData(object[] args)
    {
        //        DoAnimation();
    }

    public override void ReturnBackToThisWindow(bool isChangeWindow)
    {
        base.ReturnBackToThisWindow(isChangeWindow);
        if (isChangeWindow)
        {
            //            DoAnimation();
        }
    }

    public override void OnEscapeKeyDown(BaseEventData eventData)
    {
        base.OnEscapeKeyDown(eventData);
        ReturnBack();
    }

    private void OnCloseBtnClick()
    {
        ReturnBack();
    }
}
