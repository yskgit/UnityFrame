using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MySelectable : Selectable 
    , IPointerClickHandler //屏蔽 IPointerClickHandler 可以屏蔽掉鼠标点击触发点击事件
{
    /// <summary>
    /// 当前物体被选中时调用
    /// </summary>
    public event Action<MySelectable> HandleSelected;
    /// <summary>
    /// 当前物体被失去焦点时调用
    /// </summary>
    public event Action<MySelectable> HandleDeselected;
    /// <summary>
    /// 当前物体被点击的时候调用
    /// </summary>
    public event Action<MySelectable> HandleClick;
    /// <summary>
    /// 是否播放音乐
    /// </summary>
    [Header("是否播放音乐（选中、点击的音乐）")]
    public bool MPlayAudio = true;
    /// <summary>
    /// 选中当前物体的时候播放的声音文件
    /// </summary>
    [Header("选中的时候播放的声音文件")]
    public string SelectedAudioName = "buttonSelect";
    /// <summary>
    /// 点击当前物体的时候播放的声音文件
    /// </summary>
    [Header("点击的时候播放的声音文件")]
    public string ClickAudioName = "buttonClick";
    /// <summary>
    /// 选中当前物体时放大的倍数
    /// </summary>
    [Header("选中时放大的倍数")]
    public float Scale = 1.0f;
    /// <summary>
    /// 选中当前物体时额外显示的物体，如选中框
    /// </summary>
    [Header("选中时额外显示的物体，如选中框")]
    public GameObject OnSelectedShowObj;
    /// <summary>
    /// 是否间隔时间执行点击事件（不能连续点击当前物体），结合“InternalTime”使用
    /// </summary>
    [Header("是否间隔时间执行点击事件（不能连续点击当前物体），结合“InternalTime”使用")]
    public bool Internal;
    /// <summary>
    /// 点击事件间隔时间
    /// </summary>
    [Header("点击事件间隔时间")]
    public float InternalTime = 1.0f;

    /// <summary>
    /// 记录上一次点击时间。用于计算与上一次点击的间隔时间，从而判断当前是否为“连续点击”事件。
    /// </summary>
    private float _lastClickTime;

    protected override void OnEnable()
    {
        base.OnEnable();

        //往“KeyEventManager”里注册的“确定”点击事件 “OnBtnClick”
        if (KeyEventManager.instance)
        {
            KeyEventManager.instance.HandleConfirmKeyDown += OnBtnClick;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        //注销“KeyEventManager”里注册的“确定”点击事件 “OnBtnClick”
        if (KeyEventManager.instance)
        {
            KeyEventManager.instance.HandleConfirmKeyDown -= OnBtnClick;
        }
    }

    /// <summary>
    /// 获取焦点时，unity自动调用。也可以手动调用，eventData传null。
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        //物体被选中的时
        //放大 Scale 倍
        transform.localScale = new Vector3(Scale, Scale, Scale);
        //显示选中框
        ShowOnSelectedShowObj(true);
        //播放选中音乐
        PlayAudio(SelectedAudioName);
        //执行选中委托
        if (HandleSelected != null)
        {
            HandleSelected(this);
        }
        //通知 KeyEventManager 当前选中物体
        KeyEventManager.instance.OnSelectObj(gameObject);
    }

    /// <summary>
    /// 失去焦点时，unity自动调用。也可以手动调用，eventData传null。
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        ShowOnSelectedShowObj(false);
        transform.localScale = Vector3.one;
        if (HandleDeselected != null)
        {
            HandleDeselected(this);
        }
        KeyEventManager.instance.OnDeselectObj(gameObject);
    }

    /// <summary>
    /// 显示或隐藏选中框
    /// </summary>
    /// <param name="isTrue"></param>
    protected void ShowOnSelectedShowObj(bool isTrue)
    {
        if (OnSelectedShowObj)
        {
            OnSelectedShowObj.SetActive(isTrue);
        }
    }

    /// <summary>
    /// 播放音乐。
    /// </summary>
    /// <param name="audioName"></param>
    private void PlayAudio(string audioName)
    {
        if (!MPlayAudio)
        {
            return;
        }
        if (!string.IsNullOrEmpty(audioName))
        {
            AudioManager.instance.PlayAudio(EAndioType.Btn, audioName);
        }
    }

    /// <summary>
    /// 键盘、遥控器“确定”按钮的触发事件。
    /// </summary>
    private void OnBtnClick()
    {
        if (UIController.instance.CurrentSelectedObj && gameObject == UIController.instance.CurrentSelectedObj)
        {
            OnPointerClick(null);
        }
    }

    /// <summary>
    /// 鼠标点击会触发此点击事件
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        PlayAudio(ClickAudioName);

        if (Internal && Time.realtimeSinceStartup - _lastClickTime < InternalTime)
        {
            return;
        }
        _lastClickTime = Time.realtimeSinceStartup;

        if (HandleClick != null)
        {
            HandleClick(this);
        }
    }
}
