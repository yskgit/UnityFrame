#if Network

using System;
using System.Collections.Generic;
using HallProtoConstructs;
using UnityEngine;

public class PlayerData
{
    public string nickname;//用户昵称
    public bool gender;//用户性别,gender为用户性别，true=》男，false=》女
    public int portrait;//用户头像编号
    public int ticket;//房卡数量
    public string Account;//账号id
}

public enum E_ActivityType : int
{
    None = 0,
    DoubleRoomCard = 1 << 0,
}

public static partial class ProtoId
{
    public const int KICK_OFF = 1;
    public const int HEART_BEAT = 3;

    public const int REQUEST_LOGIN_HALL = 10001;
    public const int REQUEST_REGISTER = 10002;
    public const int REQUEST_UPDATE_INFO = 10003;
    public const int REQUEST_CREATE_ROOM = 10004;
    public const int REQUEST_JOIN_ROOM = 10005;
    public const int REQUEST_GAME_HISTORY = 10006;
    public const int REQUEST_GAME_REPLAY = 10007;
    public const int REQUEST_RECIEVE_EMAIL = 10008;
    public const int REQUEST_GOODD_LIST = 10009;
    public const int REQUEST_BUY_GOODS = 10010;
    public const int REQUEST_CALLBACK_BUY_GOODS = 10011;

    public const int NOTICE_LOGIN = 20001;
    public const int NOTICE_REGISTER = 20002;
    public const int LOGIN_SUCCESS = 20003;
    public const int CREATE_OR_JOIN_ROOM_FAILED = 20004;
    public const int CREATE_OR_JOIN_ROOM_SUCCESS = 20005;
    public const int BROADCAST = 20006;
    public const int GET_HISTORY_SUCCESS = 20007;
    public const int GAME_REPLAY = 20008;
    public const int EMAIL_LIST = 20009;
    public const int EMAIL_SINGLE = 20010;
    public const int RECEIVE_EMAIL_RESULT = 20011;
    public const int GOOD_LIST = 20012;
    public const int BUY_GOOD_FLOW = 20013;
    public const int BUY_GOOD_RESULT = 20014;
    public const int RANDOM_GAME_QUENE_RESULT = 20015;
    public const int SYNCHRONIZE_INFO_PROTO = 20016;
    /// <summary>
    /// 开启单机游戏成功与否的结果
    /// </summary>
    public const int START_SINGLE_MODE_RESULT = 20017;
    /// <summary>
    /// 玩家积分
    /// </summary>
    public const int RANK_SCORE = 20018;
    public const int ACTIVITY_STATE = 20019;
    public const int GAIN_ROOM_CARD = 20020;
}

public class HallSocketWrapper : SocketWapper<HallSocketWrapper>
{
    /// <summary>
    /// 操作异常，被踢下线
    /// </summary>
    [ResponseEvent(ProtoId.KICK_OFF)]
    public event Action<KickProto> HandleKickOff;

    /// <summary>
    /// 服务器收到心跳请求的回应
    /// </summary>
    [ResponseEvent(ProtoId.HEART_BEAT)]
    public event Action<HeartbeatProto> HandleHeartbeat;

    /// <summary>
    /// socket连接成功后，提示登录
    /// </summary>
    [ResponseEvent(ProtoId.NOTICE_LOGIN)]
    public event Action<NoticeLoginProto> HandleNoticeLogin;

    /// <summary>
    /// 需要注册
    /// </summary>
    [ResponseEvent(ProtoId.NOTICE_REGISTER)]
    public event Action<NoticeRegisterProto> HandleNoticeRegister;

    /// <summary>
    /// 登录成功
    /// </summary>
    [ResponseEvent(ProtoId.LOGIN_SUCCESS)]
    public event Action<LoginSuccessProto> HandleLoginSuccess;

    /// <summary>
    /// 创建房间或加入房间失败协议
    /// </summary>
    [ResponseEvent(ProtoId.CREATE_OR_JOIN_ROOM_FAILED)]
    public event Action<CreateOrJoinRoomFailProto> HandleCreateOrJoinRoomFailed;

    /// <summary>
    /// 创建房间成功或加入房间成功，引导进入房间协议
    /// </summary>
    [ResponseEvent(ProtoId.CREATE_OR_JOIN_ROOM_SUCCESS)]
    public event Action<JoinRoomProto> HandleCreateOrJoinRoomSuccess;

    /// <summary>
    /// 战绩查询成功回调
    /// </summary>
    [ResponseEvent(ProtoId.GET_HISTORY_SUCCESS)]
    public event Action<GameHistoryListProto> HandleGameHistoryList;

    /// <summary>
    /// 观看对局回放回调
    /// </summary>
    [ResponseEvent(ProtoId.GAME_REPLAY)]
    public event Action<GameReplayProto> HandleGameReplay;

    /// <summary>
    /// 开始整局邮件接收
    /// </summary>
    [ResponseEvent(ProtoId.EMAIL_LIST)]
    public event Action<EmailsListProto> HandleEmailsList;

    /// <summary>
    /// 单个邮件发送
    /// </summary>
    [ResponseEvent(ProtoId.EMAIL_SINGLE)]
    public event Action<EmailProto> HandleEmailSingle;

    /// <summary>
    /// 领取附件后邮件回调
    /// </summary>
    [ResponseEvent(ProtoId.RECEIVE_EMAIL_RESULT)]
    public event Action<RecieveEmailResult> HandleRecieveEmailResult;

    /// <summary>
    /// 商城商品列表
    /// </summary>
    [ResponseEvent(ProtoId.GOOD_LIST)]
    public event Action<GoodsListProto> HandleGoodsList;

    /// <summary>
    /// 商城购买请求成功回调
    /// </summary>
    [ResponseEvent(ProtoId.BUY_GOOD_FLOW)]
    public event Action<BuyGoodsFlowProto> HandleBuyGoodsFlow;

    /// <summary>
    /// 商城查询成功回调。也可能是购买请求的时候，请求失败，如商品过期等，会返回此协议！！！
    /// </summary>
    [ResponseEvent(ProtoId.BUY_GOOD_RESULT)]
    public event Action<BuyGoodsResultProto> HandleBuyGoodsResult;

    /// <summary>
    /// 大厅匹配返回数据
    /// </summary>
    [ResponseEvent(ProtoId.RANDOM_GAME_QUENE_RESULT)]
    public event Action<RandomGameQueueResultProto> RandomGameQueueResult;

    /// <summary>
    /// 同步数据信息
    /// </summary>
    [ResponseEvent(ProtoId.SYNCHRONIZE_INFO_PROTO)]
    public event Action<SynchronizeInfoProto> HandleSynchronizeInfoProto;

    /// <summary>
    /// 公告
    /// </summary>
    [ResponseEvent(ProtoId.BROADCAST)]
    public event Action<BroadcastProto> BroadcastProto;

    /// <summary>
    /// 开启单机游戏结果
    /// </summary>
    [ResponseEvent(ProtoId.START_SINGLE_MODE_RESULT)]
    public event Action<StartSingleModeResult> HandleStartSingleModeResult;

    /// <summary>
    /// 收到玩家积分推送
    /// </summary>
    [ResponseEvent(ProtoId.RANK_SCORE)]
    public event Action<RankScoreProto> HandleScoreRank;

    /// <summary>
    /// 玩家排位升级协议
    /// </summary>
    [ResponseEvent(ProtoId.GAIN_ROOM_CARD)]
    public event Action<LevelUpReward> HandleLevelUpReward;

    /// <summary>
    /// 同步数据信息
    /// </summary>
    [ResponseEvent(20019)]
    public event Action<ActivityState> HandleActivityState;

    //缓存的数据
    public PlayerData PlayerData = new PlayerData();
    public List<ItemPackageModel> GoodList = new List<ItemPackageModel>();
    public List<EmailProto> EmailList = new List<EmailProto>();
    /// <summary>
    /// 玩家积分
    /// </summary>
    public int RankScore;
    /// <summary>
    /// 玩家积分对应的等级
    /// </summary>
    public int RankLevel;
    /// <summary>
    /// 现在开启的活动类型
    /// </summary>
    public int ActivityType;

    protected override void Awake()
    {
        base.Awake();

        HandleKickOff += DoKickOff;
        HandleEmailsList += InitEmails;
        HandleEmailSingle += AddEmail;
        HandleLoginSuccess += SetPlayerData;
        HandleHeartbeat += DoReceiveHeartbeat;
        HandleNoticeLogin += DoHandleNoticeLogin;
        RandomGameQueueResult += DoRandomGameQueueResult;
        BroadcastProto += BroadcastProtoResult;
        HandleSynchronizeInfoProto += SynchronizeInfoProto;
        HandleScoreRank += SetRankScore;
        HandleLevelUpReward += LevelUpReward;
        HandleActivityState += SetActivityState;
    }

    protected override SceenType _sceenType
    {
        get { return SceenType.Hall; }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        HandleKickOff -= DoKickOff;
        HandleEmailsList -= InitEmails;
        HandleEmailSingle -= AddEmail;
        HandleLoginSuccess -= SetPlayerData;
        HandleHeartbeat -= DoReceiveHeartbeat;
        HandleNoticeLogin -= DoHandleNoticeLogin;
        RandomGameQueueResult -= DoRandomGameQueueResult;
        BroadcastProto -= BroadcastProtoResult;
        HandleSynchronizeInfoProto -= SynchronizeInfoProto;
        HandleScoreRank -= SetRankScore;
        HandleLevelUpReward -= LevelUpReward;
        HandleActivityState -= SetActivityState;
    }

    private void DoKickOff(KickProto proto)
    {
        LogUtil.Log("DoKickOff!!!");
        DoKickOff();
        string tips;
        KickReason reason = (KickReason)proto.reason;
        switch (reason)
        {
            case KickReason.MUTI_LOGIN:
                tips = "你的帐号在其他地方登录，你被迫下线。请关闭游戏后重新登录。";
                break;
            case KickReason.ILLIGAL_ACTION:
                tips = "你的帐号操作异常，已被迫下线。请关闭游戏后重新登录。";
                break;
            case KickReason.ROOM_NOT_EXIST:
                tips = "包间号码输入错误，请重新输入";
                break;
            case KickReason.ROOM_DISMISSED:
                tips = "房主解散了包间！ ";
                break;
            case KickReason.LEAVE_ROOM:
                tips = "你离开了包间！";
                break;
            case KickReason.ALREADY_IN_ROOM:
                tips = "你已经加入了一个包间，请退出后重新进入。";
                break;
            default:
                LogUtil.LogWarning(string.Format("error kick reason {0} at {1}", proto.reason, name));
                return;
        }

        LogUtil.Log("tips = " + tips);
        TipsWindow.instance.Show(tips, () =>
        {
            GameManager.instance.QuitGame();
        });
    }

    private void InitEmails(EmailsListProto resp)
    {
        EmailList = resp.emails;
        UIController.instance.GetUIManager<HallUIManager>().UpdateInfo(E_Update.Email);
    }

    private void AddEmail(EmailProto resp)
    {
        EmailList.Insert(0, resp);
        UIController.instance.GetUIManager<HallUIManager>().UpdateInfo(E_Update.Email);
    }

    public bool CheckHasNotReadEmail()
    {
        List<EmailProto> lis = new List<EmailProto>();
        AOTSafe.Foreach<EmailProto>(EmailList, email =>
        {
            if (!email.isRead)
            {
                lis.Add(email);
            }
        });
        LogUtil.Log("NotReadEmails.Count = " + lis.Count);
        return lis.Count > 0;
    }

    private void SetPlayerData(LoginSuccessProto proto)
    {
        LogUtil.Log("SetPlayerData!!!");
        PlayerData = new PlayerData
        {
            nickname = proto.nickname,
            gender = proto.gender,
            portrait = proto.portrait,
            ticket = proto.ticket,
        };
        PlayerData.Account = SDKWrapper.instance.GetAccount();
    }

    public void RequestStore()
    {
        DoSocketRequest(10009, new RequestGoodsListProto { noop = true }, 20012);
    }

    private void SetGoodsList(GoodsListProto proto)
    {
        GoodList = proto.GoodList;
    }

    private void DoReceiveHeartbeat(HeartbeatProto proto)
    {
        LogUtil.Log("收到心跳包的回应");
    }

    private void DoHandleNoticeLogin(NoticeLoginProto proto)
    {
        LogUtil.Log("提示登录!!!proto.code = " + proto.code);
        if (HandleNoticeLogin == null)
        {
            return;
        }
        //HandleNoticeLogin.GetInvocationList().Length == 1说明为断线重连，重新登录，并且在有登录（如gamemanager）的界面不执行此内容

        LogUtil.Log("DoLogin!!!  HallSocketWrapper");
        LoginProto loginProto = new LoginProto()
        {
            account = SDKWrapper.instance.GetAccount(),
            code = proto.code,
            gameType = int.Parse(FileHelper.ReadConfig("GameType")),
            connectType = 0
        };
        string signStr = Util.GetMd5EncryptStr(loginProto.account + loginProto.code + GameManager.SECRET);
        loginProto.sign = signStr;
        //发送信息向服务器端  
        DoSocketRequest(10001, loginProto, 20002, 20003);
    }

    public override void DoSocketRequest<TReq>(int protoId, TReq content, params int[] responseProtoIds)
    {
        LoadingWebWindow.instance.Show();
        base.DoSocketRequest(protoId, content, responseProtoIds);
    }

    protected override void OnSendSuccessCallback()
    {
        LoadingWebWindow.instance.Close();
        base.OnSendSuccessCallback();
    }

    protected override void OnSocketDisconnected()
    {
        LogUtil.Log("socket断掉了！！！");
        base.OnSocketDisconnected();
    }

    public override void StartSocketConnect(string ip, int port)
    {
        LogUtil.Log("开始socket连接!!!");
        LoadingWebWindow.instance.Show();
        base.StartSocketConnect(ip, port);
    }

    protected override void OnConnectFailedCallback()
    {
        LogUtil.Log("socket连接失败!!!");
        base.OnConnectFailedCallback();
    }

    protected override void OnConnectSuccessCallback()
    {
        LogUtil.Log("socket连接成功!!!");
        LoadingWebWindow.instance.Close();
        base.OnConnectSuccessCallback();
    }

    protected override bool Reconnect()
    {
        bool reconnect = base.Reconnect();
        if (reconnect)
        {
            LogUtil.Log(string.Format("开始socket重连!!! socket重连次数为：{0}", _reconnectCount));
            LoadingWebWindow.instance.Show(GetFormatLoadingWebTips());
        }
        return reconnect;
    }

    private string GetFormatLoadingWebTips()
    {
        return string.Format("断线重连中 第[{0}/{1}]次尝试。。。", _reconnectCount, _reconnectTimes);
    }

    protected override void OnReconnectTimeOut()
    {
        LogUtil.Log(string.Format("已经重连{0}次了，弹出tips界面!!!", _reconnectCount - 1));
        base.OnReconnectTimeOut();
        LoadingWebWindow.instance.Close();
        TipsWindow.instance.Show("网络异常", () =>
        {
            Reconnect();
        }, () =>
        {
            GameManager.instance.QuitGame();
        }, "重连", "退出游戏");
    }

    protected override void OnCannotConnetWithServer()
    {
        base.OnCannotConnetWithServer();
        LoadingWebWindow.instance.Close();
        TipsWindow.instance.Show("哎呀，连接不上服务器了，请检查您的网络连接。", () =>
        {
            GameManager.instance.QuitGame();
        });
    }

    /// <summary>
    /// 接收返回的匹配结果
    /// </summary>
    /// <param name="result"></param>
    private void DoRandomGameQueueResult(RandomGameQueueResultProto result)
    {
        //UserManager.getInstance().matchQueueState = result.queueState;
    }

    private void BroadcastProtoResult(BroadcastProto broadcast)
    {
        AnnouncementWindow.instance.Show(broadcast.content);
    }

    private void SynchronizeInfoProto(SynchronizeInfoProto proto)
    {
        LogUtil.Log("接收到同步消息:" + proto);

        PlayerData.nickname = proto.nickname;
        PlayerData.gender = proto.gender;
        PlayerData.portrait = proto.portrait;
        PlayerData.ticket = proto.ticket;

        UIController.instance.GetUIManager<HallUIManager>().UpdateInfo(E_Update.RoomCard);
    }

    private void SetRankScore(RankScoreProto proto)
    {
        LogUtil.Log("收到玩家当前积分消息：proto = " + proto);

        RankScore = proto.scoreInfos.Find(item => item.gameType == int.Parse(FileHelper.ReadConfig("GameType"))).score;
        RankLevel = proto.scoreInfos.Find(item => item.gameType == int.Parse(FileHelper.ReadConfig("GameType"))).level;

        UIController.instance.GetUIManager<HallUIManager>().UpdateInfo(E_Update.LevelAndScore);
    }

    private void LevelUpReward(LevelUpReward proto)
    {
        LogUtil.Log("收到玩家当前积分消息：proto = " + proto);

        TipsWindow.instance.Show(string.Format("恭喜你，在竞技场中成功晋级，获得令牌*{0}枚。", proto.num), null);
    }

    private void SetActivityState(ActivityState proto)
    {
        LogUtil.Log("SetActivityState!!! proto = " + proto);
        ActivityType = proto.activityOpenState;
    }
}

#endif