#if Network

using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingWebWindow : SingletonWindow<LoadingWebWindow>
{
    /// <summary>
    /// 当前选中物体的导航。用于显示网页加载界面的时候，当前UIManager不能导航
    /// </summary>
    //private Navigation _navigation;
    private Navigation.Mode _navigationMode;

    /// <summary>
    /// 打开 LoadingWebWindow 界面时缓存的物体。用于关闭 LoadingWebWindow 界面时重新选中 _tempObj。
    /// </summary>
    private GameObject _tempObj;

    /// <summary>
    /// LoadingWebWindow 的提示信息
    /// </summary>
    private Text _tips;

    protected override void Awake()
    {
        base.Awake();

        var texts = GetComponent<TextContainer>().Texts;

        _tips = texts[0];
        ShowTips(false);
    }

    /// <summary>
    /// 显示网络加载界面
    /// </summary>
    public override void Show()
    {
        base.Show();

        _tempObj = UIController.instance.CurrentSelectedObj;
        if (!_tempObj)
        {
            return;
        }

        //_navigation = _tempObj.GetComponent<Selectable>().navigation;
        //_navigationMode = _navigation.mode;

        _navigationMode = _tempObj.GetComponent<Selectable>().navigation.mode;

        Util.SetNavigationMode(_tempObj.GetComponent<MySelectable>(), Navigation.Mode.None);
    }

    /// <summary>
    /// 显示网络加载界面，并显示提示信息
    /// </summary>
    /// <param name="tips"></param>
    public void Show(string tips)
    {
        Show();

        ShowTips(true);
        _tips.text = tips;
    }

    private void ShowTips(bool isTrue)
    {
        _tips.gameObject.SetActive(isTrue);
    }

    /// <summary>
    /// 关闭网络加载界面
    /// </summary>
    public override void Close()
    {
        if (_tempObj)
        {
            Util.SetNavigationMode(_tempObj.GetComponent<MySelectable>(), _navigationMode);
            //_navigation.mode = _navigationMode;
            //_tempObj.GetComponent<Selectable>().navigation = _navigation;
        }

        ShowTips(false);

        base.Close();
    }
}

#endif