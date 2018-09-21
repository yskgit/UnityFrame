using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine.Networking;
#if Network
using HallProtoConstructs;



#endif

public class SplashUIManager : UIManager
{
#if Network
    private bool _isInRoom;//用于进入游戏是否是断线重连的
#endif

    private Image _circle;
    private Image _jiayu;
    private Image _entertainment;
    private Image _fangkuai;
    private Image _meihua;
    private Image _hongtao;
    private Image _heitao;

    public override void InitUI()
    {
        base.InitUI();
        Image[] images = GetComponent<ImageContainer>().Images;

        _circle = images[0];
        _jiayu = images[1];
        _entertainment = images[2];
        _fangkuai = images[3];
        _meihua = images[4];
        _hongtao = images[5];
        _heitao = images[6];
    }

    public override void InitData(object[] args)
    {
        base.InitData(args);

        _circle.transform.localScale = Vector3.zero;
        _jiayu.transform.localPosition = new Vector3(116f, 80f, 0f);
        _entertainment.transform.localPosition = new Vector3(26f, -25f, 0f);
        InitColor(_jiayu);
        InitColor(_entertainment);
        InitColor(_fangkuai);
        InitColor(_meihua);
        InitColor(_hongtao);
        InitColor(_heitao);

        DoTweenHelper.DoScale(_circle.transform, Vector3.one, 0.7f, Ease.Flash, null);
        var splashRotation = _circle.GetComponent<SplashRotation>();
        splashRotation.RotateTime = 0.7f;
        splashRotation.StartRotate();

        //0.2+0.2+0.1+0.1+0.1+0.1 = 0.8f
        DoFadeAndMove(_jiayu, 0.2f, 0.2f, new Vector3(116f, 36f, 0f), () =>
           {
               DoFadeAndMove(_entertainment, 0.2f, 0.2f, new Vector3(26f, -55f, 0f), () =>
               {
                   Sequence seq = DOTween.Sequence();
                   seq.Append(DoTweenHelper.DoFade(_fangkuai, 1, 0.1f, Ease.Flash, null))
                       .Append(DoTweenHelper.DoFade(_meihua, 1, 0.1f, Ease.Flash, null))
                       .Append(DoTweenHelper.DoFade(_hongtao, 1, 0.1f, Ease.Flash, null))
                       .Append(DoTweenHelper.DoFade(_heitao, 1, 0.1f, Ease.Flash, () =>
                       {
                           //StartCoroutine(DelayEnterGame(1f));
                           EnterGame();
                       }));
               });
           });

        AudioManager.instance.PlayAudio(EAndioType.Bg, "bg_hall");
    }

    private void InitColor(MaskableGraphic trans)
    {
        Color color = trans.color;
        color.a = 0;
        trans.color = color;
    }

    private void DoFadeAndMove(MaskableGraphic trans, float fadeTime, float moveTime, Vector3 moveTarget, Action onFinished)
    {
        DoTweenHelper.DoFade(trans, 1, fadeTime, Ease.Flash, null);
        DoTweenHelper.DoLocalMove(trans.transform, moveTarget, moveTime, Ease.Flash, () =>
        {
            if (onFinished != null)
            {
                onFinished();
            }
        });
    }

    //    private IEnumerator DelayEnterGame(float time)
    //    {
    //        //yield return new WaitForSeconds(time);
    //        yield return new WaitForFixedUpdate();

    //#if Network

    //        AssetBundleManager.instance.CheckUpdateAsync((dic) =>
    //        {
    //            if (dic != null && dic.Count > 0)
    //            {

    //            }
    //        });

    //        StartNetworkGame();

    //#else

    //        StartStandaloneGame();

    //#endif

    //    }

    private void EnterGame()
    {
#if Network && HotFix

        Action onFinished = StartNetworkGame;
        ShowWindow("ui_win_download", new object[]{onFinished});

#elif Network

        StartNetworkGame();

#else

        StartStandaloneGame();

#endif
    }

    /// <summary>
    /// 开始单机游戏
    /// </summary>
    private void StartStandaloneGame()
    {
        LogUtil.Log("StartStandaloneGame!!!");
        ChangeWindow(MemoryHelper.HasKeyUsername() ? "ui_win_hall" : "ui_win_register", null, null, true, true);
    }

#if Network

    /// <summary>
    /// 开始联机游戏
    /// </summary>
    private void StartNetworkGame()
    {
        LogUtil.Log("StartNetworkGame!!!");
        RegisterEvent(true);
        StartSocketConnect();
    }


    /// <summary>
    /// 注册或者注销此界面需要接受的服务器通信事件
    /// </summary>
    /// <param name="isTrue"></param>
    private void RegisterEvent(bool isTrue)
    {
        if (isTrue)
        {
            HallSocketWrapper.instance.HandleNoticeLogin += DoNoticeLogin;
            HallSocketWrapper.instance.HandleNoticeRegister += DoNoticeRegister;
            HallSocketWrapper.instance.HandleLoginSuccess += DoLoginSuccess;
            HallSocketWrapper.instance.HandleCreateOrJoinRoomSuccess += DoCreateRoomSuccess;
        }
        else
        {
            HallSocketWrapper.instance.HandleNoticeLogin -= DoNoticeLogin;
            HallSocketWrapper.instance.HandleNoticeRegister -= DoNoticeRegister;
            HallSocketWrapper.instance.HandleLoginSuccess -= DoLoginSuccess;
            HallSocketWrapper.instance.HandleCreateOrJoinRoomSuccess -= DoCreateRoomSuccess;
        }
    }

    private void StartSocketConnect()
    {
        string ip = FileHelper.ReadConfig("LoginURL");
        string port = FileHelper.ReadConfig("Port");
        LogUtil.Log(string.Format("StartSocketConnect ip:{0},port:{1}", ip, port));
        HallSocketWrapper.instance.StartSocketConnect(ip, int.Parse(port));
    }

    private void DoNoticeLogin(NoticeLoginProto proto)
    {
        LogUtil.Log("DoNoticeLogin!!!    proto.code = " + proto.code);
        DoLogin(proto.code);
    }

    private void DoLogin(int code)
    {
        LogUtil.Log("DoLogin!!!");
        LoginProto loginProto = new LoginProto()
        {
            account = SDKWrapper.instance.GetAccount(),
            code = code,
            gameType = int.Parse(FileHelper.ReadConfig("GameType"))
        };
        string signStr = Util.GetMd5EncryptStr(loginProto.account + loginProto.code + GameManager.SECRET);
        loginProto.sign = signStr;
        //发送信息向服务器端  
        HallSocketWrapper.instance.DoSocketRequest(10001, loginProto, 20002, 20003);
    }

    private void DoNoticeRegister(NoticeRegisterProto proto)
    {
        LogUtil.Log("DoNoticeRegister!!!");
        RegisterEvent(false);
        ChangeWindow("ui_win_register", new object[] { proto.code }, null, true, true);
    }

    private void DoLoginSuccess(LoginSuccessProto proto)
    {
        LogUtil.Log("DoLoginSuccess!!!");
        StartCoroutine(ChangeWindow());
    }

    IEnumerator ChangeWindow()
    {
        yield return new WaitForFixedUpdate();
        RegisterEvent(false);
        if (!_isInRoom)
        {
            ChangeWindow("ui_win_hall", null, null, true, true);
        }
        else
        {
            LogUtil.Log("通知自动进入房间");
            ChangeWindow("ui_win_game", new object[] { true }, null, true, true);
        }
    }

    private void DoCreateRoomSuccess(JoinRoomProto proto)
    {
        _isInRoom = true;
        //UserManager.getInstance().UpdateLoginSuccess(proto);
        //CreateSingletonComponent<MJGameSocketWapper>("MJGameSocketWapper");
    }
#endif
}
