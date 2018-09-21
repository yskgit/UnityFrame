using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class QuitGameUIManager : UIManager
{
    private GameObject _popWindow;

    public override void InitUI()
    {
        base.InitUI();

        _popWindow = _objs[0];
        _popWindow.transform.localScale = Vector3.zero;

        _btns[0].onClick.AddListener(() =>
        {
            GameManager.instance.QuitGame();
        });
        _btns[1].onClick.AddListener(() =>
        {
            ReturnBack();
        });

        CurrentSelectedObj = _btns[1].gameObject;
    }

    public override void InitData(object[] args)
    {
        base.InitData(args);

        DoTweenHelper.DoScale(_popWindow.transform, Vector3.one, 0.5f, Ease.OutBack, null);
    }
}
