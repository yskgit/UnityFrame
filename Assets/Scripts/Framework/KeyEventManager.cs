using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyEventManager : SingletonBehaviour<KeyEventManager>
{
    /// <summary>
    /// 当前选中的物体。不同于EventSystem.current.currentSelectedGameObject。
    /// EventSystem.current.currentSelectedGameObject可为null，而CurrentSelectedObj为MySelectable回调过来的GameObject，不可能为空。
    /// </summary>
    public GameObject CurrentSelectedObj;
    /// <summary>
    /// 上次选中的物体。当EventSystem.current.currentSelectedGameObject=null时，CurrentSelectedObj和LastSelectedObj为同一物体。
    /// </summary>
    public GameObject LastSelectedObj;//

    /// <summary>
    /// 取消选中物体时的事件
    /// </summary>
    public event Action<GameObject> HandleDeselectObj;

    /// <summary>
    /// 选中物体时的事件
    /// </summary>
    public event Action<GameObject> HandleSelectObj;

    /// <summary>
    /// 上键按下的回调
    /// </summary>
    public event Action HandleUpKeyDown;

    /// <summary>
    /// 上键抬起的回调
    /// </summary>
    public event Action HandleUpKeyUp;

    /// <summary>
    /// 下键按下的回调
    /// </summary>
    public event Action HandleDownKeyDown;

    /// <summary>
    /// 下键抬起的回调
    /// </summary>
    public event Action HandleDownKeyUp;

    /// <summary>
    /// 左键按下的回调
    /// </summary>
    public event Action HandleLeftKeyDown;

    /// <summary>
    /// 左键抬起的回调
    /// </summary>
    public event Action HandleLeftKeyUp;

    /// <summary>
    /// 右键按下的回调
    /// </summary>
    public event Action HandleRightKeyDown;

    /// <summary>
    /// 右键抬起的回调
    /// </summary>
    public event Action HandleRightKeyUp;

    /// <summary>
    /// 当前选中物体变化时的事件
    /// </summary>
    public event Action HandleEscapeKeyDown;

    //检测数字键 
    //0
    public event Action HandleAlpha0KeyDown;
    public event Action HandleAlpha0KeyUp;
    //1
    public event Action HandleAlpha1KeyDown;
    public event Action HandleAlpha1KeyUp;
    //2
    public event Action HandleAlpha2KeyDown;
    public event Action HandleAlpha2KeyUp;
    //3
    public event Action HandleAlpha3KeyDown;
    public event Action HandleAlpha3KeyUp;
    //4
    public event Action HandleAlpha4KeyDown;
    public event Action HandleAlpha4KeyUp;
    //5
    public event Action HandleAlpha5KeyDown;
    public event Action HandleAlpha5KeyUp;
    //6
    public event Action HandleAlpha6KeyDown;
    public event Action HandleAlpha6KeyUp;
    //7
    public event Action HandleAlpha7KeyDown;
    public event Action HandleAlpha7KeyUp;
    //8
    public event Action HandleAlpha8KeyDown;
    public event Action HandleAlpha8KeyUp;
    //9
    public event Action HandleAlpha9KeyDown;
    public event Action HandleAlpha9KeyUp;

    /// <summary>
    /// *
    /// </summary>
    public event Action HandleStarKeyUp;

    /// <summary>
    /// #
    /// </summary>
    public event Action HandleSharpKeyUp;

    /// <summary>
    /// 快进
    /// </summary>
    public event Action HandleSpeedKeyUp;

    /// <summary>
    /// 菜单键
    /// </summary>
    public event Action HandleMenuKeyDown;

    /// <summary>
    /// 当前选中物体变化时的事件
    /// </summary>
    public event Action HandleConfirmKeyDown;

    /// <summary>
    /// 记录当前的 UIManager ，只有当前的 UIManager 才能响应键盘、遥控器事件
    /// </summary>
    private UIManager _currentUIManager;

    private int _confirmKeyCode = -1;//取得机顶盒确定键code
    private bool _canClick;//用来控制点击事件

    // Update is called once per frame
    void Update()
    {
        DoSuperviseInputEvent();
    }

    /// <summary>
    /// 禁用或者启用导航事件。上下左右键
    /// </summary>
    public void EnableNavigationEvent(bool isTrue)
    {
        EventSystem.current.sendNavigationEvents = isTrue;
    }

    /// <summary>
    /// 禁用或者启用按钮点击事件。上下左右键
    /// </summary>
    public void EnableClickEvent(bool isTrue)
    {
        _canClick = isTrue;
    }

    /// <summary>
    /// 禁用键盘输入、鼠标点击事件，即禁用所有交互事件！禁用后不能选中物体！
    /// 禁用键盘的时候，不会触发MySelectable的OnDeselect！！！
    /// </summary>
    /// <param name="isTrue"></param>
    public void EnableKeyboardEvent(bool isTrue)
    {
        //禁止键盘事件原理：销毁StandaloneInputModule组件
        var module = EventSystem.current.gameObject.GetComponent<StandaloneInputModule>();
        if (isTrue)
        {
            if (module)
            {
                return;
            }
            EventSystem.current.gameObject.AddComponent<StandaloneInputModule>();
        }
        else
        {
            if (!module)
            {
                return;
            }
            Destroy(module);
        }
    }

    private bool CheckIsCanSuperviseInputEvent()
    {
        if (!_currentUIManager || !_currentUIManager.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (LoadingWindow.instance && LoadingWindow.instance.IsShow())
        {
            return false;
        }

#if Network

        if (LoadingWebWindow.instance && LoadingWebWindow.instance.IsShow())
        {
            return false;
        }

#endif

        return true;
    }

    /// <summary>
    /// 监测键盘或者遥控器输入
    /// </summary>
    private void DoSuperviseInputEvent()
    {
        //if (!Input.anyKey)//Input.anyKey监测不到按键抬起
        //{
        //    return;
        //}

        //if (!_enableKeyEvent)
        //{
        //    return;
        //}

        _currentUIManager = UIController.instance.CurrentUIManager;

        if (!CheckIsCanSuperviseInputEvent())
        {
            return;
        }

        //确定下键盘或者模拟器或者遥控板“确定”键的响应KeyCode
        if (_confirmKeyCode == -1)
        {
            //电脑键盘响应的KeyCode，13
            if (Input.GetKeyDown(KeyCode.Return))
            {
                _confirmKeyCode = (int)KeyCode.Return;
            }

            //安卓模拟器确定键传值为10
            if (Input.GetKeyDown((KeyCode)10))
            {
                _confirmKeyCode = 10;
            }

            //经测试，小米机顶盒确定键响应了KeyCode.Return和JoystickButton0和Joystick1Button0
            if (Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                _confirmKeyCode = (int)KeyCode.JoystickButton0;
            }

            if (Input.GetKeyDown(KeyCode.Joystick1Button0))
            {
                _confirmKeyCode = (int)KeyCode.Joystick1Button0;
            }
        }

        //由上一步确定好的“确定”按键code执行对应事件
        if (_confirmKeyCode != -1 && Input.GetKeyDown((KeyCode)_confirmKeyCode))
        {
            if (!_canClick)
            {
                return;
            }
            if (HandleConfirmKeyDown != null)
            {
                HandleConfirmKeyDown.Invoke();
            }
            //Debug.Log("_enterKeyCode = " + _enterKeyCode);
            _currentUIManager.OnConfirmKeyDown(null);
        }

        if (TipsWindow.instance.IsShow()) //tips界面显示的时候，屏蔽掉其他所有按键响应，只响应确定按钮
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))//安卓的返回键
        {
            if (HandleEscapeKeyDown != null)
            {
                HandleEscapeKeyDown.Invoke();
            }
            _currentUIManager.OnEscapeKeyDown(null);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (HandleLeftKeyDown != null)
            {
                HandleLeftKeyDown.Invoke();
            }
            _currentUIManager.OnLeftArrowKeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            if (HandleLeftKeyUp != null)
            {
                HandleLeftKeyUp.Invoke();
            }
            _currentUIManager.OnLeftArrowKeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (HandleRightKeyDown != null)
            {
                HandleRightKeyDown.Invoke();
            }
            _currentUIManager.OnRightArrowKeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            if (HandleRightKeyUp != null)
            {
                HandleRightKeyUp.Invoke();
            }
            _currentUIManager.OnRightArrowKeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (HandleDownKeyDown != null)
            {
                HandleDownKeyDown.Invoke();
            }
            _currentUIManager.OnDownArrowKeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            if (HandleDownKeyUp != null)
            {
                HandleDownKeyUp.Invoke();
            }
            _currentUIManager.OnDownArrowKeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (HandleUpKeyDown != null)
            {
                HandleUpKeyDown.Invoke();
            }
            _currentUIManager.OnUpArrowKeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (HandleUpKeyUp != null)
            {
                HandleUpKeyUp.Invoke();
            }
            _currentUIManager.OnUpArrowKeyUp(null);
        }

        //监听数字键
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (HandleAlpha0KeyDown != null)
            {
                HandleAlpha0KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha0KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            if (HandleAlpha0KeyUp != null)
            {
                HandleAlpha0KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha0KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (HandleAlpha1KeyDown != null)
            {
                HandleAlpha1KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha1KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            if (HandleAlpha1KeyUp != null)
            {
                HandleAlpha1KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha1KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (HandleAlpha2KeyDown != null)
            {
                HandleAlpha2KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha2KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            if (HandleAlpha2KeyUp != null)
            {
                HandleAlpha2KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha2KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (HandleAlpha3KeyDown != null)
            {
                HandleAlpha3KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha3KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            if (HandleAlpha3KeyUp != null)
            {
                HandleAlpha3KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha3KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (HandleAlpha4KeyDown != null)
            {
                HandleAlpha4KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha4KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            if (HandleAlpha4KeyUp != null)
            {
                HandleAlpha4KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha4KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (HandleAlpha5KeyDown != null)
            {
                HandleAlpha5KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha5KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha5))
        {
            if (HandleAlpha5KeyUp != null)
            {
                HandleAlpha5KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha5KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (HandleAlpha6KeyDown != null)
            {
                HandleAlpha6KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha6KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha6))
        {
            if (HandleAlpha6KeyUp != null)
            {
                HandleAlpha6KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha6KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            if (HandleAlpha7KeyDown != null)
            {
                HandleAlpha7KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha7KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha7))
        {
            if (HandleAlpha7KeyUp != null)
            {
                HandleAlpha7KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha7KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (HandleAlpha8KeyDown != null)
            {
                HandleAlpha8KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha8KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha8))
        {
            if (HandleAlpha8KeyUp != null)
            {
                HandleAlpha8KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha8KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (HandleAlpha9KeyDown != null)
            {
                HandleAlpha9KeyDown.Invoke();
            }
            _currentUIManager.OnAlpha9KeyDown(null);
        }

        if (Input.GetKeyUp(KeyCode.Alpha9))
        {
            if (HandleAlpha9KeyUp != null)
            {
                HandleAlpha9KeyUp.Invoke();
            }
            _currentUIManager.OnAlpha9KeyUp(null);
        }

        if (Input.GetKeyDown(KeyCode.Menu))
        {
            if (HandleMenuKeyDown != null)
            {
                HandleMenuKeyDown.Invoke();
            }
            _currentUIManager.OnMenuKeyDown(null);
        }
    }

    /// <summary>
    /// *点击事件
    /// </summary>
    public void OnStarKeyUp(string keyCode)
    {
        if (HandleStarKeyUp != null)
        {
            HandleStarKeyUp.Invoke();
        }
        _currentUIManager.OnStarKeyUp(null);
    }

    /// <summary>
    /// #点击事件
    /// </summary>
    public void OnSharpKeyUp(string keyCode)
    {
        if (HandleSharpKeyUp != null)
        {
            HandleSharpKeyUp.Invoke();
        }
        _currentUIManager.OnSharpKeyUp(null);
    }

    /// <summary>
    /// 快进点击事件
    /// </summary>
    public void OnSpeedKeyUp(string keyCode)
    {
        if (HandleSpeedKeyUp != null)
        {
            HandleSpeedKeyUp.Invoke();
        }
        _currentUIManager.OnSpeedKeyUp(null);
    }

    /// <summary>
    /// 当前选中物体改变时的回调。由MySelectable回调过来，当MySelectable失去焦点的时候调用
    /// </summary>
    /// <param name="obj"></param>
    public void OnDeselectObj(GameObject obj)
    {
        if (HandleDeselectObj != null && obj)
        {
            LastSelectedObj = obj;
            HandleDeselectObj.Invoke(obj);
        }
    }

    /// <summary>
    /// 当前选中物体时的回调。由MySelectable回调过来，当MySelectable得到焦点的时候调用
    /// </summary>
    public void OnSelectObj(GameObject obj)
    {
        if (HandleSelectObj != null && obj)
        {
            CurrentSelectedObj = obj;
            HandleSelectObj.Invoke(obj);
        }
    }
}
