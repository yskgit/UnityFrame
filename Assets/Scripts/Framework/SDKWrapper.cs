using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDKWrapper : SingletonBehaviour<SDKWrapper>
{
    public string GetAccount()
    {
#if IsTest

        return GetLocalAccount();

#elif HB

#if UNITY_ANDROID && !UNITY_EDITOR

        AndroidJavaObject jo = GetCurrentActivity();
        string account = jo.Call<string>("getAccount", null);
        LogUtil.Log("account = " + account);

        return account;

#else

        return GetLocalAccount();

#endif

#else

        return "ysk";

#endif
    }

    public void OnWebviewClick()
    {
#if OpenWebview
        LogUtil.Log("OnWebviewClick!!!");
        AndroidJavaClass cls = new AndroidJavaClass("com.jyhd.game.util.CYWebViewHelper");
        cls.CallStatic("openWebView");
#endif
    }

    public void OnWebviewClick(string url)
    {
#if OpenWebview
        LogUtil.Log("OnWebviewClick!!!");
        AndroidJavaClass cls = new AndroidJavaClass("com.jyhd.game.util.CYWebViewHelper");
        cls.CallStatic("openWebView", url);
#endif
    }

    private AndroidJavaObject GetCurrentActivity()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return jc.GetStatic<AndroidJavaObject>("currentActivity");
    }

    private string GetLocalAccount()
    {
        string account;
        if (!MemoryHelper.HasKey("account"))
        {
            int id = Random.Range(1, 200000);
            account = "jyhd" + id;
            MemoryHelper.SetValue("account", account);
        }
        else
        {
            account = MemoryHelper.GetValue<string>("account");
        }
        LogUtil.Log("account = " + account);
        return account;
    }
}
