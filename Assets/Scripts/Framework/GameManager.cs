using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : SingletonBehaviour<GameManager>
{

#if Network
    public const string SECRET = "what does the fox say";
#endif

    /// <summary>
    /// 所有UI的根节点
    /// </summary>
    public Transform RootTrans;

    /// <summary>
    /// 游戏通用背景
    /// </summary>
    public GameObject Bg;

    public float ScreenWidth = 1280f;
    public float ScreenHeight = 720f;

    private float _bgScale;
    private float _screenWidth;
    private float _screenHeight;
    private float _screenScale;

    protected override void Awake()
    {
        base.Awake();
        Screen.fullScreen = false;
        Debug.Log("Screen.fullScreen = " + Screen.fullScreen);
        //Screen.SetResolution(1280, 720, false);
        //Debug.Log("Screen.fullScreen = " + Screen.fullScreen);

        LogUtil.SetLogLevel(FileHelper.ReadConfig("LogLevel"));

        InitScreenAdaptionArguments();

        RootTrans.GetComponent<CanvasScaler>().scaleFactor *= GameManager.instance.GetScreenScale();

        CreateSingletonComponentAndWindow();
    }

    /// <summary>
    /// 初始化设置屏幕适配参数。按照游戏屏幕适配，而非屏幕分辨率。
    /// </summary>
    private void InitScreenAdaptionArguments()
    {
        //标准比例16:9 1280x720
        float windowScale;
        float widthFactor = Screen.width / ScreenWidth;//宽度比因子
        float hightFactor = Screen.height / ScreenHeight;//高度比因子

        //游戏缩放比例参照因子小的一方。背景图缩放比例参考大的一方。
        //因为如果参照因子大的一方，如16:10的分辨率2160x1350，1280x720如果按照大比例（高度）因子放大则为2400x1350
        //较标准分辨率放大hightFactor（1.875）倍。
        //2160x1350分辨率想要放置好2400x1350分辨率的UI，则需要把2400边界的物体缩距回来，可能会造成重叠或者其他意外情况！！！
        //较标准分辨率放大widthFactor（1.5）倍。
        //而如果参照因子小的一方（宽度）放大则为2160x1250，则只需要把1250边界的UI再放大一点出去即可。
        if (widthFactor < hightFactor)
        {
            windowScale = widthFactor;
            _screenWidth = 1280f;//直接缩放的是最上一层Canvas的比例，所以宽高的表示还是编辑器下的数字
            _screenHeight = Screen.height / widthFactor;
            _bgScale = hightFactor / widthFactor;
        }
        else
        {
            windowScale = hightFactor;
            _screenWidth = Screen.width / hightFactor;
            _screenHeight = 720f;
            _bgScale = widthFactor / hightFactor;
        }

        _screenScale = windowScale;

        LogUtil.Log("windowScale = " + windowScale + " Width = " + _screenWidth + " Height = " + _screenHeight + " BgScale = " + _bgScale);
        LogUtil.Log("Screen.width  = " + Screen.width + " Screen.height = " + Screen.height);
    }

    private void Start()
    {
        UIController.instance.ShowWindow("ui_win_splash");

        //LogUtility.Log("gamemanager_start_0");
        //LogUtility.LogWarning("gamemanager_start_1");
        //LogUtility.LogError("gamemanager_start_2");
    }

    public float GetBgScale()
    {
        return _bgScale;
    }

    public float GetScreenWidth()
    {
        return _screenWidth;
    }

    public float GetScreenHeight()
    {
        return _screenHeight;
    }

    public float GetScreenScale()
    {
        return _screenScale;
    }

    /// <summary>
    /// 创建单例窗口、单例组件，并挂载到Root节点下。
    /// </summary>
    private void CreateSingletonComponentAndWindow()
    {
        Util.CreateGameObject<UIController>();
        Util.CreateGameObject<ObjectCache>();
        Util.CreateGameObject<ResourceManager>();
        Util.CreateGameObject<AudioManager>();
        Util.CreateGameObject<SDKWrapper>();
        Util.CreateGameObject<KeyEventManager>();
        Util.CreateGameObject<AssetBundleManager>();

        Util.CreateSingletonWindow<LoadingWindow>("ui_win_loading");
        Util.CreateSingletonWindow<TipsWindow>("ui_win_tips");
        Util.CreateSingletonWindow<AnnouncementWindow>("ui_win_announcement");

#if Network

        Util.CreateSingletonWindow<LoadingWebWindow>("ui_win_loadingWeb");
        Util.CreateGameObject<HallSocketWrapper>();

#endif
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
