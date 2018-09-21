using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DeviceInfo : MonoBehaviour {

    private readonly StringBuilder _deviceInfo = new StringBuilder();
    private string _androidMemoryInfo = "android内存信息";//android内存信息  剩余内存/最大内存
    //"maxMemory:" + maxMemory + " totalMemory:" + totalMemory + " freeMemory: "+freeMemory;
    //private string _appMemoryInfo = "app内存信息";//app内存信息
    private string _appMaxAssignMemory = "分配给app的最大内存";//分配给app的最大内存
    private string _appCurrentAssignMemory = "当前分配给app总内存";//当前分配给app总内存
    private string _appCurrentRestMemory = "当前分配给app总内存中剩余的内存";//当前分配给app总内存中剩余的内存
                                                               //System.out: memory: 256
                                                               //System.out: maxMemory: 256.0
    //System.out: totalMemory: 11.974937
    //System.out: freeMemory: 3.6257935
    //这说明我这个app在当前手机的最大分配内存是256m,现在已经分配了11m,这11m中有6m是空闲的

    private AndroidJavaObject _jo;

    void Start()
    {
        //ConsoleDeviceInfo();
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        _jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        InvokeRepeating("GetMemoryInfo", 0f, 1f);
#endif
    }

    private void GetMemoryInfo()
    {
        _jo.Call("getAppMemoryInfo");
        _jo.Call("getMaxAssignMemory");
        _jo.Call("getTotalAssignMemory");
        _jo.Call("getFreeAssignMemory");
    }

    private void ConsoleDeviceInfo()
    {
        _deviceInfo.AppendLine("设备与系统信息:");
        //设备的模型
        GetMessage("设备模型", SystemInfo.deviceModel);
        //设备的名称
        GetMessage("设备名称", SystemInfo.deviceName);
        //设备的类型
        GetMessage("设备类型（PC电脑，掌上型）", SystemInfo.deviceType.ToString());
        //系统内存大小
        GetMessage("系统内存大小MB", SystemInfo.systemMemorySize.ToString());
        //操作系统
        GetMessage("操作系统", SystemInfo.operatingSystem);
        //设备的唯一标识符
        GetMessage("设备唯一标识符", SystemInfo.deviceUniqueIdentifier);
        //显卡设备标识ID
        GetMessage("显卡ID", SystemInfo.graphicsDeviceID.ToString());
        //显卡名称
        GetMessage("显卡名称", SystemInfo.graphicsDeviceName);
        //显卡类型
        GetMessage("显卡类型", SystemInfo.graphicsDeviceType.ToString());
        //显卡供应商
        GetMessage("显卡供应商", SystemInfo.graphicsDeviceVendor);
        //显卡供应唯一ID
        GetMessage("显卡供应唯一ID", SystemInfo.graphicsDeviceVendorID.ToString());
        //显卡版本号
        GetMessage("显卡版本号", SystemInfo.graphicsDeviceVersion);
        //显卡内存大小
        GetMessage("显存大小MB", SystemInfo.graphicsMemorySize.ToString());
        //显卡是否支持多线程渲染
        GetMessage("显卡是否支持多线程渲染", SystemInfo.graphicsMultiThreaded.ToString());
        //支持的渲染目标数量
        GetMessage("支持的渲染目标数量", SystemInfo.supportedRenderTargetCount.ToString());
        Debug.Log(_deviceInfo);
    }

    void GetMessage(params string[] str)
    {
        if (str.Length == 2)
        {
            _deviceInfo.AppendLine(str[0] + ":" + str[1]);
        }
    }

    //public void SetAppMemoryInfo(string str)
    //{
    //    _appMemoryInfo = str;
    //}

    public void SetAndroidMemoryInfo(string str)
    {
        _androidMemoryInfo = str;
    }

    public void SetAppMaxAssignMemory(string str)
    {
        _appMaxAssignMemory = str;
    }

    public void SetAppCurrentAssignMemory(string str)
    {
        _appCurrentAssignMemory = str;
    }

    public void SetAppCurrentRestMemory(string str)
    {
        _appCurrentRestMemory = str;
    }

    private void OnGUI()
    {
        //GUIStyle bb = new GUIStyle();
        //bb.normal.background = null;    //这是设置背景填充的
        ////bb.normal.textColor = new Color(1.0f, 0.5f, 0.0f);   //设置字体颜色的
        //bb.normal.textColor = Color.black;   //设置字体颜色的
        //bb.fontSize = 20;       //当然，这是字体大小

        ////GUI.Label(new Rect((Screen.width / 2) - 100, 0, 200, 200), _appMemoryInfoStr, bb);
        ////GUI.Label(new Rect((Screen.width / 2) - 100, 20, 200, 200), _androidMemoryInfoStr, bb);

        //GUILayout.Label("安卓内存", bb);
        //GUILayout.Label("App内存", bb);

        GUILayout.BeginVertical();
        //android内存
        GUILayout.BeginHorizontal();
        GUILayout.Label("安卓内存信息", GUILayout.Width(160));
        GUILayout.TextField(_androidMemoryInfo);
        GUILayout.EndHorizontal();
        //分配给app的最大内存
        GUILayout.BeginHorizontal();
        GUILayout.Label("分配给app的最大内存", GUILayout.Width(160));
        GUILayout.TextField(_appMaxAssignMemory);
        GUILayout.EndHorizontal();
        //当前分配给app总内存
        GUILayout.BeginHorizontal();
        GUILayout.Label("当前分配给app总内存", GUILayout.Width(160));
        GUILayout.TextField(_appCurrentAssignMemory);
        GUILayout.EndHorizontal();
        //当前分配给app总内存中剩余的内存
        GUILayout.BeginHorizontal();
        GUILayout.Label("当前分配给app总内存中剩余的内存", GUILayout.Width(160));
        GUILayout.TextField(_appCurrentRestMemory);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}
