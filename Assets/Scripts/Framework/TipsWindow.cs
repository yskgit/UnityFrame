using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TipsWindow : SingletonWindow<TipsWindow>
{
    private GameObject _tempSelectedObj;//ShowTips时缓存的SelectedObj

    private Action _handleLeft;
    private Action _handleRight;
    private Action _handleCnter;

    private GameObject _twoBtnObj;
    private GameObject _oneBtnObj;

    private MyButton _centerBtn;
    private MyButton _leftBtn;
    private MyButton _rightBtn;

    private Text _tips;
    //    private Text _centerText;
    private Text _leftText;
    private Text _rightText;

    protected override void Awake()
    {
        base.Awake();

        var objs = GetComponent<GameObjectContainer>().Objs;
        var buttons = GetComponent<MyButtonContainer>().MyButtons;
        var texts = GetComponent<TextContainer>().Texts;

        _twoBtnObj = objs[0];
        _oneBtnObj = objs[1];

        _leftBtn = buttons[0];
        _leftBtn.onClick.AddListener(OnLeftBtnClick);
        _rightBtn = buttons[1];
        _rightBtn.onClick.AddListener(OnRightBtnClick);
        _centerBtn = buttons[2];
        _centerBtn.onClick.AddListener(OnCenterBtnClick);

        _tips = texts[0];
        _leftText = texts[1];
        _rightText = texts[2];
        //        _centerText = texts[3];

        DeactiveBtns();
    }

    /// <summary>
    /// 显示tips界面。
    /// 显示两个按钮。按钮点击的时候会执行传进来的委托，并关闭界面。
    /// </summary>
    /// <param name="tips">提示信息内容</param>
    /// <param name="handleLeft">左边按钮点击事件</param>
    /// <param name="handleRight">右边按钮点击事件</param>
    /// <param name="left">左边按钮文字显示</param>
    /// <param name="right">右边按钮文字显示</param>
    public void Show(string tips, Action handleLeft, Action handleRight, string left = "确定", string right = "取消")
    {
        if (!CheckIsCanShow())
        {
            return;
        }

        base.Show();

        SetTempSelectedObj();
        ShowTwoBtn(true);

        _tips.text = tips;
        _handleLeft = handleLeft;
        _handleRight = handleRight;

        _leftText.text = left;
        _rightText.text = right;

        UIController.instance.CurrentSelectedObj = _leftBtn.gameObject;
    }

    /// <summary>
    /// 显示tips界面。
    /// 显示一个按钮，按钮上的文字只能是默认的“确认”。按钮点击的时候会执行传进来的委托，并关闭界面。
    /// </summary>
    /// <param name="tips">提示信息内容</param>
    /// <param name="handleCenter">按钮点击事件</param>
    public void Show(string tips, Action handleCenter)
    {
        if (!CheckIsCanShow())
        {
            return;
        }

        base.Show();

        SetTempSelectedObj();
        ShowOneBtn(true);

        _tips.text = tips;
        _handleCnter = handleCenter;

        UIController.instance.CurrentSelectedObj = _centerBtn.gameObject;
    }

    /// <summary>
    /// 检测是否可以显示。预留出的接口，如果以后有需求需要tips界面不可顶替显示时使用
    /// </summary>
    /// <returns></returns>
    private bool CheckIsCanShow()
    {
        //return CheckIsSelfInit() && !IsShow();
        return true;
    }

    private void SetTempSelectedObj()
    {
        _tempSelectedObj = UIController.instance.CurrentSelectedObj;
    }

    private void DeactiveBtns()
    {
        ShowOneBtn(false);
        ShowTwoBtn(false);
    }

    private void DiselectBtns()
    {
        _centerBtn.OnDeselect(null);
        _leftBtn.OnDeselect(null);
        _rightBtn.OnDeselect(null);
    }

    private void ShowOneBtn(bool isTrue)
    {
        _oneBtnObj.SetActive(isTrue);
    }

    private void ShowTwoBtn(bool isTrue)
    {
        _twoBtnObj.SetActive(isTrue);
    }

    /// <summary>
    /// 关闭tips界面必须在按钮点击事件最开始执行!!!否则btn的ondeselected无法执行!!!
    /// </summary>
    public override void Close()
    {
        DiselectBtns();
        DeactiveBtns();
        //关闭tips界面后，重新选择之前选中的物体
        if (_tempSelectedObj)
        {
            UIController.instance.CurrentSelectedObj = _tempSelectedObj;
        }

        base.Close();
    }

    private void OnLeftBtnClick()
    {
        Close();
        if (_handleLeft != null)
        {
            _handleLeft();
        }
    }

    private void OnRightBtnClick()
    {
        Close();
        if (_handleRight != null)
        {
            _handleRight();
        }
    }

    private void OnCenterBtnClick()
    {
        Close();
        if (_handleCnter != null)
        {
            _handleCnter();
        }
    }
}
