using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    protected MyButton[] _btns;
    protected GameObject[] _objs;
    protected Text[] _texts;
    protected Sprite[] _sprites;
    protected Image[] _images;
    protected MyToggle[] _toggles;
    protected MyToggleGroup[] _toggleGroups;

    private Transform _topLeftTrans;
    private Transform _topCenterTrans;
    private Transform _topRightTrans;
    private Transform _centerLeftTrans;
    private Transform _centerTrans;
    private Transform _centerRightTrans;
    private Transform _bottomLeftTrans;
    private Transform _bottomCenterTrans;
    private Transform _bottomRightTrans;

    /// <summary>
    /// ShowWindow时缓存的SelectedObj。用于返回当前界面时，自动重新选中“_TempSelectedObj”物体
    /// </summary>
    private GameObject _tempSelectedObj;

    /// <summary>
    /// 设置选中的物体
    /// </summary>
    protected GameObject CurrentSelectedObj
    {
        get { return UIController.instance.CurrentSelectedObj; }
        set
        {
            UIController.instance.CurrentSelectedObj = value;
        }
    }

    /// <summary>
    /// 是否显示背景图
    /// </summary>
    protected bool ShowBg
    {
        set
        {
            UIController.instance.ShowBg(value);
        }
    }

    /// <summary>
    /// 此窗体的缓存资源是否是永久的
    /// </summary>
    public virtual bool IsPermanent
    {
        get { return false; }
    }

    /// <summary>
    /// 需要提前缓存的资源集合
    /// </summary>
    public virtual string[] CacheAssets
    {
        get { return null; }
    }

    /// <summary>
    /// 执行顺序：第一。加载UI。子类重写此方法时必须调用且最先基类调用此方法。
    /// </summary>
    public virtual void InitUI()
    {
        ShowBg = true;

        var buttonCon = GetComponent<MyButtonContainer>();
        _btns = buttonCon == null ? null : buttonCon.MyButtons;
        var objCon = GetComponent<GameObjectContainer>();
        _objs = objCon == null ? null : objCon.Objs;
        var textCon = GetComponent<TextContainer>();
        _texts = textCon == null ? null : textCon.Texts;
        var spriteCon = GetComponent<SpriteContainer>();
        _sprites = spriteCon == null ? null : spriteCon.Sprites;
        var imageCon = GetComponent<ImageContainer>();
        _images = imageCon == null ? null : imageCon.Images;
        var toggleCon = GetComponent<MyToggleContainer>();
        _toggles = toggleCon == null ? null : toggleCon.MyToggles;
        var toggleGroupCon = GetComponent<MyToggleGroupContainer>();
        _toggleGroups = toggleGroupCon == null ? null : toggleGroupCon.ToggleGroups;

        _topLeftTrans = transform.Find("top_left");
        _topCenterTrans = transform.Find("top_center");
        _topRightTrans = transform.Find("top_right");
        _centerLeftTrans = transform.Find("center_left");
        _centerTrans = transform.Find("center");
        _centerRightTrans = transform.Find("center_right");
        _bottomLeftTrans = transform.Find("bottom_left");
        _bottomCenterTrans = transform.Find("bottom_center");
        _bottomRightTrans = transform.Find("bottom_right");

        SetAdaptiveTransformPos();

        //第一次进入界面时不能播放选中声音
        AudioManager.instance.IsCanPlayBtnAudio = false;
    }

    /// <summary>
    /// 设置自适应物体（九宫格的九个格子物体）的位置
    /// </summary>
    private void SetAdaptiveTransformPos()
    {
        float width = GameManager.instance.GetScreenWidth();
        float height = GameManager.instance.GetScreenHeight();

        if (_topLeftTrans)
        {
            _topLeftTrans.localPosition = new Vector3(-width / 2f, height / 2, 0f);
        }
        if (_topCenterTrans)
        {
            _topCenterTrans.localPosition = new Vector3(0f, height / 2, 0f);
        }
        if (_topRightTrans)
        {
            _topRightTrans.localPosition = new Vector3(width / 2f, height / 2, 0f);
        }
        if (_centerLeftTrans)
        {
            _centerLeftTrans.localPosition = new Vector3(-width / 2f, 0f, 0f);
        }
        if (_centerTrans)
        {
            _centerTrans.localPosition = new Vector3(0f, 0f, 0f);
        }
        if (_centerRightTrans)
        {
            _centerRightTrans.localPosition = new Vector3(width / 2f, 0f, 0f);
        }
        if (_bottomLeftTrans)
        {
            _bottomLeftTrans.localPosition = new Vector3(-width / 2f, -height / 2, 0f);
        }
        if (_bottomCenterTrans)
        {
            _bottomCenterTrans.localPosition = new Vector3(0f, -height / 2, 0f);
        }
        if (_bottomRightTrans)
        {
            _bottomRightTrans.localPosition = new Vector3(width / 2f, -height / 2, 0f);
        }
    }

    /// <summary>
    /// 执行顺序：第二。加载数据
    /// </summary>
    /// <param name="args"></param>
    public virtual void InitData(object[] args)
    {

    }

    ///// <summary>
    ///// 执行顺序：第三
    ///// </summary>
    //protected virtual void OnEnable()
    //{
    //    ShowBg = true;
    //}

    /// <summary>
    /// 返回到当前UIManager
    /// </summary>
    /// <param name="isChangeWindow"></param>
    public virtual void ReturnBackToThisWindow(bool isChangeWindow)
    {
        //返回界面时不能播放选中声音
        AudioManager.instance.IsCanPlayBtnAudio = false;

        //showwindow的情况才会在返回当前界面的时候选中原来选中的物体
        if (!isChangeWindow && _tempSelectedObj)
        {
            CurrentSelectedObj = _tempSelectedObj;
        }
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    /// <summary>
    /// 跳转窗口，会隐藏掉之前的界面，默认有loading界面。详见 UIController.ChangeWindow
    /// </summary>
    /// <param name="targetWindowName">跳转到的界面名字</param>
    /// <param name="args">传递的参数</param>
    /// <param name="onFinished">界面跳转成功回调</param>
    /// <param name="isShowLoading">是否显示加载界面</param>
    /// <param name="isDestroy">是否销毁fromWindow</param>
    /// <param name="isHideFormer">是否隐藏fromWindow</param>
    public void ChangeWindow(string targetWindowName, object[] args = null, Action onFinished = null, bool isShowLoading = true, bool isDestroy = false, bool isHideFormer = true)
    {
        if (this != UIController.instance.CurrentUIManager)
        {
            return;
        }
        UIController.instance.ChangeWindow(targetWindowName, name, args, onFinished, isShowLoading, isDestroy, isHideFormer);
    }

    /// <summary>
    /// 弹出窗口，不会隐藏掉之前的界面，默认无loading界面。详见 UIController.ShowWindow
    /// </summary>
    /// <param name="targetWindowName">跳转到的界面名字</param>
    /// <param name="args">传递的参数</param>
    /// <param name="onFinished">界面跳转成功回调</param>
    /// <param name="isShowLoading">是否显示加载界面</param>
    /// <param name="isDestroy">是否销毁fromWindow</param>
    /// <param name="isHideFormer">是否隐藏fromWindow</param>
    public void ShowWindow(string targetWindowName, object[] args = null, Action onFinished = null, bool isShowLoading = false, bool isDestroy = false, bool isHideFormer = false)
    {
        if (this != UIController.instance.CurrentUIManager)
        {
            return;
        }
        _tempSelectedObj = CurrentSelectedObj;
        UIController.instance.ShowWindow(targetWindowName, name, args, onFinished, isShowLoading, isDestroy, isHideFormer);
    }

    /// <summary>
    /// 返回之前的界面。depth为返回的层级数，onFinished为返回成功后的回调
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="onFinished"></param>
    public void ReturnBack(int depth = 1, Action onFinished = null)
    {
        UIController.instance.ReturnBack(depth, onFinished, name);
    }

    /// <summary>
    /// 返回指定名字的界面
    /// </summary>
    /// <param name="targetWindow"></param>
    /// <param name="onFinished"></param>
    public void ReturnBack(string targetWindow, Action onFinished = null)
    {
        UIController.instance.ReturnBack(targetWindow, onFinished, name);
    }

    public void LoadHeadSprite(Image headImg)
    {
        AtlasHelper.LoadHeadSprite(HallSocketWrapper.instance.PlayerData.gender, HallSocketWrapper.instance.PlayerData.portrait, headImg);
    }

    #region 按钮事件

    /// <summary>
    /// 确认键点击事件监听
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnConfirmKeyDown(BaseEventData eventData)
    {
        ////暂时屏蔽下面的代码，因为测试结果为机顶盒可以响应确定按钮的点击事件!!
        //Debug.Log("OnConfirmKeyDown!!");
        //if (!CurrentSelectedObj)
        //{
        //    Debug.Log("CurrentSelectedObj is null");
        //    return;
        //}

        ////检测有没有MyButton组件
        //MyButton myButton = CurrentSelectedObj.GetComponent<MyButton>();
        //if (myButton && myButton.onClick != null)
        //{
        //    Debug.Log("btn name = " + myButton.name);
        //    myButton.OnPointerClick(eventData as PointerEventData);
        //    return;
        //}

        ////如果包含MyToggle组件，则执行MyToggle的事件
        //MyToggle myToggle = CurrentSelectedObj.GetComponent<MyToggle>();
        //if (myToggle != null)
        //{
        //    myToggle.OnPointerClick(eventData as PointerEventData);
        //}
    }

    /// <summary>
    /// 左键按下事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnLeftArrowKeyDown(BaseEventData eventData)
    {

    }

    /// <summary>
    /// 左键抬起事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnLeftArrowKeyUp(BaseEventData eventData)
    {

    }

    /// <summary>
    /// 右键按下事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnRightArrowKeyDown(BaseEventData eventData)
    {
    }

    /// <summary>
    /// 右键抬起事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnRightArrowKeyUp(BaseEventData eventData)
    {

    }

    /// <summary>
    /// 上键按下事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnUpArrowKeyDown(BaseEventData eventData)
    {

    }

    /// <summary>
    /// 上键抬起事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnUpArrowKeyUp(BaseEventData eventData)
    {

    }

    /// <summary>
    /// 下键按下事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnDownArrowKeyDown(BaseEventData eventData)
    {

    }

    /// <summary>
    /// 下键抬起事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnDownArrowKeyUp(BaseEventData eventData)
    {

    }

    /// <summary>
    /// 退出按钮按下事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnEscapeKeyDown(BaseEventData eventData)
    {

    }


    public virtual void OnAlpha0KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha0KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha1KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha1KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha2KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha2KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha3KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha3KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha4KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha4KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha5KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha5KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha6KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha6KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha7KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha7KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha8KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha8KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha9KeyDown(BaseEventData eventData)
    {
    }

    public virtual void OnAlpha9KeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnStarKeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnSharpKeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnSpeedKeyUp(BaseEventData eventData)
    {
    }

    public virtual void OnMenuKeyDown(BaseEventData eventData)
    {
    }
    #endregion
}
