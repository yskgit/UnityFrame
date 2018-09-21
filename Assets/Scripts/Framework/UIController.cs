using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 单个实例窗体的数据信息
/// </summary>
public class UIWindow
{
    public string WindowName;
    public GameObject WindowObj;
    public UIManager MUIManager;
    /// <summary>
    /// 是否是跳转窗口。true：跳转窗口；false：弹出窗口
    /// </summary>
    public bool IsChangeWindow;
    /// <summary>
    /// 是否有加载界面。true：有加载界面；false：没有加载界面
    /// </summary>
    public bool IsShowLoading;
    /// <summary>
    /// 是否隐藏之前的界面
    /// </summary>
    public bool IsHideFormer;
    /// <summary>
    /// 从哪个界面跳转过来的
    /// </summary>
    public string WindowFrom;
    /// <summary>
    /// 跳转界面传入的参数
    /// </summary>
    public object[] Args;
    public UIWindow(GameObject windowObj, UIManager uiManager, bool isChangeWindow, string windowName, bool isShowLoading, bool isHideFormer, string from, object[] args)
    {
        WindowObj = windowObj;
        MUIManager = uiManager;
        IsChangeWindow = isChangeWindow;
        WindowName = windowName;
        IsShowLoading = isShowLoading;
        IsHideFormer = isHideFormer;
        WindowFrom = from;
        Args = args;
    }
}

/// <summary>
/// 场景类型。大厅、游戏等。用于区分当前是在哪个场景
/// </summary>
public enum SceenType
{
    Hall,
    Game
}

public class UIController : SingletonBehaviour<UIController>
{
    /// <summary>
    /// 当前界面的 UIManager
    /// </summary>
    public UIManager CurrentUIManager
    {
        get
        {
            if (_uiWindows.Count <= 0)
            {
                return null;
            }
            return _uiWindows.Peek().MUIManager;
        }
    }

    /// <summary>
    /// 设置选中的物体
    /// </summary>
    public GameObject CurrentSelectedObj
    {
        get
        {
            return EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        }
        set
        {
            if (!EventSystem.current)
            {
                return;
            }

            //如果多次（大于一次）选中同一物体，则不会触发选中物体的OnSelect方法
            //所以，先置空SetSelectedGameObject，再赋值可以实现多次选中同一物体
            if (null != value)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            EventSystem.current.SetSelectedGameObject(value);
        }
    }

    /// <summary>
    /// 存储所有实例窗口的集合
    /// </summary>
    private readonly Stack<UIWindow> _uiWindows = new Stack<UIWindow>();

    /// <summary>
    /// 当前场景
    /// </summary>
    private SceenType _currentSceenType;

    /// <summary>
    /// 所有的单例窗口，如通用提示窗口，加载界面窗口，跑马灯等。
    /// </summary>
    private List<GameObject> _singletonWindows = new List<GameObject>();

    /// <summary>
    /// 跳转界面。默认隐藏上一个界面。默认显示loading界面。“上一个界面”和“loading界面”都可用参数空值显示、隐藏、销毁。
    /// 从targetWindow返回到fromWindow时，调用fromWindow的“UIManager”的“ReturnBackToThisWindow”方法；
    /// 在“ReturnBackToThisWindow”里可以得知是通过“changewindow”的方式切换界面的。
    /// 从而可以在“ReturnBackToThisWindow”方法里执行返回到“fromWindow”的事件，如设置当前选中物体。
    /// </summary>
    /// <param name="targetWindowName">跳转到的界面名字</param>
    /// <param name="fromWindow">从哪个界面跳转。暂时只做记录作用，可以传空值</param>
    /// <param name="args">传递的参数</param>
    /// <param name="onFinished">界面跳转成功回调</param>
    /// <param name="isShowLoading">是否显示加载界面</param>
    /// <param name="isDestroy">是否销毁fromWindow</param>
    /// <param name="isHideFormer">是否隐藏fromWindow</param>
    public void ChangeWindow(string targetWindowName, string fromWindow = "", object[] args = null, Action onFinished = null, bool isShowLoading = true, bool isDestroy = false, bool isHideFormer = true)
    {
        SwitchWindow(true, targetWindowName, args, onFinished, fromWindow, isShowLoading, isDestroy, isHideFormer);
    }

    public void ChangeWindowWithCurrentUIManager(string targetWindowName, object[] args = null, Action onFinished = null, bool isShowLoading = true, bool isDestroy = false, bool isHideFormer = true)
    {
        if (CurrentUIManager)
        {
            CurrentUIManager.ChangeWindow(targetWindowName, args, onFinished, isShowLoading, isDestroy, isHideFormer);
        }
    }

    /// <summary>
    /// 弹出界面。默认不隐藏上一个界面。默认不显示loading界面。“上一个界面”和“loading界面”都可用参数空值显示、隐藏、销毁。
    /// 从targetWindow返回到fromWindow时，调用fromWindow的“UIManager”的“ReturnBackToThisWindow”方法；
    /// 在“ReturnBackToThisWindow”里可以得知是通过“changewindow”的方式切换界面的。
    /// 从而可以在“ReturnBackToThisWindow”方法里执行返回到“fromWindow”的事件，如设置当前选中物体。
    /// </summary>
    /// <param name="targetWindowName">跳转到的界面名字</param>
    /// <param name="fromWindow">从哪个界面跳转。暂时只做记录作用，可以传空值</param>
    /// <param name="args">传递的参数</param>
    /// <param name="onFinished">界面跳转成功回调</param>
    /// <param name="isShowLoading">是否显示加载界面</param>
    /// <param name="isDestroy">是否销毁fromWindow</param>
    /// <param name="isHideFormer">是否隐藏fromWindow</param>
    public void ShowWindow(string targetWindowName, string fromWindow = "", object[] args = null, Action onFinished = null, bool isShowLoading = false, bool isDestroy = false, bool isHideFormer = false)
    {
        SwitchWindow(false, targetWindowName, args, onFinished, fromWindow, isShowLoading, isDestroy, isHideFormer);
    }

    public void ShowWindowWithCurrentUIManager(string targetWindowName, object[] args = null, Action onFinished = null, bool isShowLoading = false, bool isDestroy = false, bool isHideFormer = false)
    {
        if (CurrentUIManager)
        {
            CurrentUIManager.ShowWindow(targetWindowName, args, onFinished, isShowLoading, isDestroy, isHideFormer);
        }
    }

    private void SwitchWindow(bool changeWindow, string targetWindowName, object[] args, Action onFinished, string windowFrom, bool isShowLoading, bool isDestroy, bool isHideFormer)
    {
        if (!string.IsNullOrEmpty(windowFrom))
        {
            LogUtil.Log(string.Format("从界面“{0}”跳转到了“{1}界面”", windowFrom, targetWindowName));
        }

        //检测窗体集合中是否已有该窗体。不允许加载重复窗体
        if (CheckIsDuplicateWindow(targetWindowName))
        {
            LogUtil.LogWarning("Duplicate Window，targetWindowName is :" + targetWindowName);
            return;
        }

        //通过反射加载UIManager类
        Type managerType = LoadUIManager(targetWindowName);
        if (managerType == null)
        {
            return;
        }

        //加载窗体
        GameObject window = LoadWindow(targetWindowName);
        if (!window)
        {
            return;
        }

        //用栈存储窗体，用于返回功能！！
        var uiManager = window.AddComponent(managerType) as UIManager;
        if (!uiManager)
        {
            LogUtil.LogWarning(string.Format("uiManager is null!!!fromWindow is {0}，targetWindowName is {1}", windowFrom, targetWindowName));
            return;
        }

        //在界面隐藏或者销毁之前，取消选中当前界面的物体。否则当前选中物体的选中框不会消失，返回当前界面的时候可能会出现两个选中框
        CurrentSelectedObj = null;

        BlockReceiveMsg(true);

        //销毁 isDestroy = true 的界面；
        //如果 isDestroy = false ，隐藏 isHideFormer = true 的界面
        if (_uiWindows.Count > 0)
        {
            var preWindow = _uiWindows.Peek().WindowObj;
            if (isDestroy)
            {
                preWindow = _uiWindows.Pop().WindowObj;
                preWindow.SetActive(false);
                Destroy(preWindow);
            }
            else
            {
                //windowFrom 界面所有可交互组件变为不可交互
                Util.EnableWindowSelectable(_uiWindows.Peek().WindowObj, false);
                if (isHideFormer)
                {
                    preWindow.SetActive(false);
                }
            }
        }

        //窗体入栈存储
        _uiWindows.Push(new UIWindow(window, uiManager, changeWindow, targetWindowName, isShowLoading, isHideFormer, windowFrom, args));

        //提前加载资源到内存。加载到 ObjectCache 存放。
        if (uiManager.CacheAssets != null && uiManager.CacheAssets.Length > 0)//有资源需要提前加载到内存
        {
            //开始缓存资源
            ObjectCache.instance.CacheAssetsAsync(uiManager.IsPermanent, uiManager.name, uiManager.CacheAssets, null, () =>
            {
                //没有显示加载界面的时候，缓存资源完成后加载界面数据。
                //因为显示了加载界面的情况，是加载界面关闭的时候执行加载界面数据！
                if (!isShowLoading)
                {
                    DoInitWindowData(_uiWindows.Peek());
                    BlockReceiveMsg(false);

                    if (onFinished != null)
                    {
                        onFinished.Invoke();
                    }
                }
            });
        }
        else//没有资源需要提前加载到内存
        {
            //没有显示加载界面的时候，缓存资源完成后加载界面数据。
            //因为显示了加载界面的情况，是加载界面关闭的时候执行加载界面数据！
            if (!isShowLoading)
            {
                DoInitWindowData(_uiWindows.Peek());
                BlockReceiveMsg(false);

                if (onFinished != null)
                {
                    onFinished.Invoke();
                }
            }
        }

        //根据isLoading显示加载界面（协程）。
        //显示加载界面必须在“加载资源”之后，因为在加载资源方法执行的时候 ObjectCache.instance.IsCaching 置为true，
        //而加载界面的关闭需要依赖资源是否已经加载完成，即 ObjectCache.instance.IsCaching 是否为 false！
        if (isShowLoading)
        {
            //显示loading界面，ShowLodingWindow 里监测了缓存资源的加载是否完成(ObjectCache.instance.IsCaching)。
            //显示loading界面分为两种情况（为了应对策划的“假”加载界面需求！！！）：
            //1、缓存加载完了，Loading还没完，那Loading完了就加载数据！！！
            //2、缓存没加载完，Loading加载完了，那Loading界面必须等到缓存加载完了才能关闭！！！
            //也就是说：loading界面关闭之前，资源已经加载完了！！！
            ShowLodingWindow(() =>
            {
                DoInitWindowData(_uiWindows.Peek());
                BlockReceiveMsg(false);

                if (onFinished != null)
                {
                    onFinished.Invoke();
                }
            });
        }
    }

    /// <summary>
    /// 阻塞接收消息
    /// </summary>
    /// <param name="isTrue"></param>
    private void BlockReceiveMsg(bool isTrue)
    {
        if (HallSocketWrapper.instance)
        {
            HallSocketWrapper.instance.BlockReceiveMsg(isTrue);
        }

        if (GameSocketWrapper.instance)
        {
            GameSocketWrapper.instance.BlockReceiveMsg(isTrue);
        }
    }

    /// <summary>
    /// 检测窗体集合中是否已有该窗体
    /// </summary>
    /// <param name="windowName"></param>
    /// <returns></returns>
    private bool CheckIsDuplicateWindow(string windowName)
    {
        if (_uiWindows.Count <= 0)
        {
            return false;
        }

        UIWindow[] windows = _uiWindows.ToArray();
        for (int i = 0; i < windows.Length; i++)
        {
            if (windowName.Equals(windows[i].WindowName))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 加载UIManager。通过窗体名字后缀得到 UIManager 类名，反射获取UIManager
    /// </summary>
    /// <param name="windowName"></param>
    /// <returns></returns>
    private Type LoadUIManager(string windowName)
    {
        if (!windowName.StartsWith("ui_win_"))
        {
            LogUtil.LogWarning(string.Format("错误的windowName:{0}，格式应该以ui_win_开头", windowName));
            return null;
        }

        //获取类名前缀
        string classPre = windowName.Replace("ui_win_", "");
        //前缀首字母转换为大写
        string classFirstWord = classPre[0].ToString();
        classFirstWord = classFirstWord.ToUpper();
        classPre = classPre.Remove(0, 1);
        classPre = classFirstWord + classPre;
        //得到UIManager的类名
        string uiManagerName = classPre + "UIManager";
        //检查是否继承了UIManager
        Type uimanagerType = Type.GetType(uiManagerName);
        if (uimanagerType == null)
        {
            LogUtil.LogWarning(string.Format("不存在类名为{0}的类", uiManagerName));
            return null;
        }
        if (uimanagerType.IsAssignableFrom(typeof(UIManager)))
        {
            LogUtil.LogWarning(string.Format("类{0}未继承UIManager", uiManagerName));
            return null;
        }
        //通过反射得到UIManager类
        return uimanagerType;
    }

    /// <summary>
    /// 加载窗体资源
    /// </summary>
    /// <param name="windowName"></param>
    /// <returns></returns>
    private GameObject LoadWindow(string windowName)
    {
        GameObject tempWindowObj = Resources.Load<GameObject>(windowName);
        if (!tempWindowObj)
        {
            LogUtil.LogWarning(string.Format("未找到资源：{0}", windowName));
            return null;
        }
        GameObject windowObj = Instantiate(tempWindowObj);
        windowObj.name = windowName;
        windowObj.transform.SetParent(GameManager.instance.RootTrans);
        windowObj.transform.localScale = Vector3.one;
        windowObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        windowObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        windowObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        windowObj.SetActive(true);
        Util.SetMaximumSortingOrder(windowObj);
        return windowObj;
    }

    /// <summary>
    /// 加载窗体数据
    /// </summary>
    /// <param name="uiWindow"></param>
    private void DoInitWindowData(UIWindow uiWindow)
    {
        uiWindow.WindowObj.SetActive(true);
        uiWindow.MUIManager.InitUI();
        uiWindow.MUIManager.InitData(uiWindow.Args);
    }

    public void ReturnBackWithCurrentUIManager(string targetWindow, Action onFinished)
    {
        if (CurrentUIManager)
        {
            CurrentUIManager.ReturnBack(targetWindow, onFinished);
        }
    }

    /// <summary>
    /// 返回指定“targetWindow”界面。关闭栈里所有在“targetWindow”后入栈的窗体。
    /// </summary>
    /// <param name="targetWindow"></param>
    /// <param name="onFinished"></param>
    public void ReturnBack(string targetWindow, Action onFinished, string fromWindow = "")
    {
        //栈里没有窗体，错误的调用“ReturnBack”的情况。
        if (_uiWindows.Count <= 0)
        {
            LogUtil.Log("ReturnBack错误!!!栈里没有界面!!!");
            return;
        }

        var windowList = _uiWindows.ToList();
        if (!windowList.Exists(win => win.WindowName.Equals(targetWindow)))//窗口栈里没有“targetWindow”窗口
        {
            LogUtil.LogError(string.Format("ReturnBack错误!!!栈里没有界面{0}!!!", targetWindow));
            return;
        }

        windowList.Reverse();//因为ToList方法得到的元素顺序为后进栈的元素排在前面，所以需要反转
        //得到返回的层数。
        int index = windowList.FindIndex(win => win.WindowName.Equals(targetWindow));
        int depth = windowList.Count - 1 - index;

        if (depth <= 0)
        {
            LogUtil.LogError(string.Format("返回层数为0，没有实际返回意义，请检查返回界面名字“{0}”是否正确！", targetWindow));
            return;
        }

        LogUtil.Log(string.Format("从界面“{0}”返回界面“{1}”", fromWindow, targetWindow));

        //返回的层数永远小于栈里窗体数量，所有不用判断返回 depth 层级后栈里没有窗体的情况。
        ReturnBack(depth, onFinished, fromWindow);
    }

    public void ReturnBackWithCurrentUIManager(int depth, Action onFinished)
    {
        if (CurrentUIManager)
        {
            CurrentUIManager.ReturnBack(depth, onFinished);
        }
    }

    /// <summary>
    /// 返回界面，如果depth不合法，则销毁栈里所界面，并跳转到大厅界面。
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="onFinished"></param>
    public void ReturnBack(int depth, Action onFinished, string fromWindow = "")
    {
        //栈里没有窗体，错误的调用“ReturnBack”的情况。
        if (_uiWindows.Count <= 0)
        {
            LogUtil.LogError("ReturnBack错误!!!栈里没有界面!!!");
            return;
        }

        if (depth <= 0)
        {
            LogUtil.LogError(string.Format("返回深度错误,depth = {0}，depth应为大于0的整数！", depth));
        }

        //如果返回 depth 层级后，栈里将没有窗体，为了避免出错，此情况默认返回到主界面
        if (_uiWindows.Count <= depth)
        {
            LogUtil.LogError(string.Format("ReturnBack错误!!!depth不合法,如果返回{0}层，栈里将没有界面!!!", depth));
            //LogUtil.LogError(string.Format("ReturnBack错误!!!depth不合法,如果返回{0}层，栈里将没有界面!!!此情况默认返回到大厅界面!!!", depth));
            ////此处必须全部出栈，然后跳转窗口到大厅界面。
            ////eg：断线重连的情况，未加载大厅界面，如果游戏界面错误调用“ReturnBack”，则需要全部出栈，并跳转窗口到大厅界面。
            //while (_uiWindows.Count > 0)
            //{
            //    var window = _uiWindows.Pop();
            //    window.WindowObj.SetActive(false);
            //    Destroy(window.WindowObj);
            //}
            //ChangeWindow(string.Empty, "ui_win_hall");
            return;
        }

        //出栈并销毁depth层窗体
        //三个bool值用于返回到目标界面时，目标界面数据更新
        bool isChangeWindow = false;//eg:因为“ChangeWindow”默认隐藏了之前的窗体，ChangeWindow 为 true 时，重新选中默认物体
        bool isShowLoading = false;//eg:因为“isShowLoading”显示了加载界面，isShowLoading 为 true 时，显示loading界面
        bool isHideFormer = false;//eg:因为“isHideFormer”隐藏了之前的窗体，isHideFormer 为 true 时，显示目标界面
        for (int i = 0; i < depth; i++)
        {
            UIWindow uiWindow = _uiWindows.Pop();
            ObjectCache.instance.ClearGroup(uiWindow.MUIManager.name);
            if (i == depth - 1)
            {
                isChangeWindow = uiWindow.IsChangeWindow;
                isShowLoading = uiWindow.IsShowLoading;
                isHideFormer = uiWindow.IsHideFormer;
            }
            uiWindow.WindowObj.SetActive(false);//因为Destroy是在FixedUpdate执行，所以先SetActive false
            Destroy(uiWindow.WindowObj);
        }

        //回复当前窗体所有可交互组件的交互性
        UIWindow curWindow = _uiWindows.Peek();
        Util.EnableWindowSelectable(curWindow.WindowObj, true);
        LogUtil.Log(string.Format("成功从界面“{0}”返回界面“{1}”", fromWindow, curWindow.WindowName));

        if (isHideFormer)
        {
            curWindow.WindowObj.SetActive(true);
        }

        if (isShowLoading)//ChangeWindow 跳转窗口
        {
            ShowLodingWindow(() =>
            {
                CurrentUIManager.ReturnBackToThisWindow(true);
                if (onFinished != null)
                {
                    onFinished();
                }
            });
        }
        else
        {
            CurrentUIManager.ReturnBackToThisWindow(isChangeWindow);
            if (onFinished != null)
            {
                onFinished();
            }
        }
    }

    /// <summary>
    /// 显示loading界面
    /// </summary>
    /// <param name="onFinished"></param>
    private void ShowLodingWindow(Action onFinished)
    {
        LoadingWindow.instance.Show();
        StartCoroutine(Loading(onFinished));
    }

    IEnumerator Loading(Action onFinished)
    {
        //yield return new WaitForSeconds(LoadingWindow.DisplayTime);
        while (ObjectCache.instance.IsCaching)//等待资源缓存结束
        {
            yield return null;
        }
        LoadingWindow.instance.Close();
        if (onFinished != null)
        {
            onFinished();
        }
    }

    /// <summary>
    /// 显示游戏通用背景。
    /// </summary>
    /// <param name="isTrue"></param>
    public void ShowBg(bool isTrue)
    {
        GameManager.instance.Bg.SetActive(isTrue);
    }

    /// <summary>
    /// 获取当前场景类型
    /// </summary>
    /// <returns></returns>
    public SceenType GetCurrentSceenType()
    {
        return _currentSceenType;
    }

    /// <summary>
    /// 设置当前场景类型
    /// </summary>
    /// <param name="type"></param>
    public void SetCurrentSceenType(SceenType type)
    {
        _currentSceenType = type;
        switch (type)
        {
            case SceenType.Hall:
                HallSocketWrapper.instance.StartHeatbeatReq();
                break;
            case SceenType.Game:
                GameSocketWrapper.instance.StartHeatbeatReq();
                break;
            default:
                LogUtil.LogWarning("错误的场景类型：" + type);
                break;
        }
    }

    /// <summary>
    /// 获取已经加载窗体的 UIManager 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>返回值为 Null 说明当前加载窗体中不包含类型为 T 的 UIManager</returns>
    public T GetUIManager<T>() where T : UIManager
    {
        T uimanager = null;
        foreach (var uiWindow in _uiWindows)
        {
            if (uiWindow.MUIManager is T)
            {
                uimanager = (T)uiWindow.MUIManager;
            }
        }
        return uimanager;
    }

    /// <summary>
    /// 显示退出游戏界面
    /// </summary>
    public void ShowQuitGameWindow()
    {
        ShowWindowWithCurrentUIManager("ui_win_quitGame");
    }

    /// <summary>
    /// 得到游戏UI的根节点
    /// </summary>
    /// <returns></returns>
    public Transform GetRoot()
    {
        return GameManager.instance.RootTrans;
    }

    /// <summary>
    /// 添加单例窗口
    /// </summary>
    /// <param name="obj"></param>
    public void AddSingletonWindow(GameObject obj)
    {
        _singletonWindows.Add(obj);
    }

    /// <summary>
    /// 移除单例窗口
    /// </summary>
    /// <param name="obj"></param>
    public void RemoveSingletonWindow(GameObject obj)
    {
        if (_singletonWindows.Contains(obj))
        {
            _singletonWindows.Remove(obj);
        }
    }

    public List<GameObject> GetSingletonWindows()
    {
        return _singletonWindows;
    }
}
