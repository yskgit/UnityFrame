using System;
using System.Collections;
using System.Collections.Generic;
using GameProtos;

public static partial class ProtoId
{
    /// <summary>
    /// 游戏场景登录
    /// </summary>
    public const int REQUEST_LOGIN = 30001;
    /// <summary>
    /// 请求退出房间
    /// </summary>
    public const int REQUEST_LEAVE_ROOM = 30002;
    /// <summary>
    /// 发送聊天内容
    /// </summary>
    public const int REQUEST_MESSAGE = 30004;
    /// <summary>
    /// 申请解散房间
    /// </summary>
    public const int REQUEST_DISMISS_ROOM = 30005;
    /// <summary>
    /// 准备请求
    /// </summary>
    public const int REQUEST_READY = 30007;
    /// <summary>
    /// 走棋请求
    /// </summary>
    public const int REQUEST_STEP = 30008;
    /// <summary>
    /// 悔棋
    /// </summary>
    public const int REQUEST_UNDO = 30009;
    /// <summary>
    /// 同意悔棋
    /// </summary>
    public const int REQUEST_AGREE_UNDO = 30010;
    /// <summary>
    /// 认输
    /// </summary>
    public const int REQUEST_SURRENDER = 30011;
    /// <summary>
    /// 进入房间信息，单播
    /// </summary>
    public const int NOTICE_LOGIN_GAME = 40001;
    /// <summary>
    /// 进入房间信息，单播
    /// </summary>
    public const int ENTER_ROOM = 40002;
    /// <summary>
    /// 单个玩家进入通知
    /// </summary>
    public const int ENTER_ROOM_NOTIFY = 40003;
    /// <summary>
    /// 单局结束后结算
    /// </summary>
    public const int SETTLEMENT = 40007;
    /// <summary>
    /// 房间牌局进行完或者中途解散后，整体结算
    /// </summary>
    public const int TOTAL_SETTLEMENT = 40008;
    /// <summary>
    /// 语音
    /// </summary>
    public const int MESSAGE = 40009;
    /// <summary>
    /// 投票信息。投票结果
    /// </summary>
    public const int VOTE_INFO = 40012;
    /// <summary>
    /// 开始游戏
    /// </summary>
    public const int GAME_START = 40013;
    /// <summary>
    /// 有人下线
    /// </summary>
    public const int ON_LINE_RESULT = 40015;
    /// <summary>
    /// 走棋结果
    /// </summary>
    public const int STEP_RESULT = 40016;
    /// <summary>
    /// 准备结果
    /// </summary>
    public const int READY_RESULT = 40017;
    /// <summary>
    /// 解散房间
    /// </summary>
    public const int DISMISS_ROOM = 40018;
    /// <summary>
    /// 收到有人申请悔棋的消息
    /// </summary>
    public const int RECEIVE_UNDO_REQUEST = 40019;
    /// <summary>
    /// 悔棋处理
    /// </summary>
    public const int RECEIVE_UNDO_RESULT = 40020;
    /// <summary>
    /// 通知走棋
    /// </summary>
    public const int NOTIFY_STEP = 40021;
    /// <summary>
    /// 投票离开房间是否成功
    /// </summary>
    public const int VOTE_DISMISS_ROOM = 40022;
}

public class GameSocketWrapper : SocketWapper<GameSocketWrapper>
{
    public int RoomId;

    [ResponseEvent(ProtoId.KICK_OFF)]
    public event Action<KickProto> HandleKickOff;

    /// <summary>
    /// 提示登录。socket连接上后，提示登录。全局只需要一个提示登录即可，所以此处为私有变量，只在此容器里登录。
    /// </summary>
    [ResponseEvent(ProtoId.NOTICE_LOGIN_GAME)]
    private event Action<NoticeLoginProto> _handleNoticeLogin;

    [ResponseEvent(ProtoId.ENTER_ROOM)]
    public event Action<EnterRoomProto> HandleEnterRoom;

    [ResponseEvent(ProtoId.ENTER_ROOM_NOTIFY)]
    public event Action<EnterRoomNotifyProto> HandleEnterRoomNotify;

    [ResponseEvent(ProtoId.SETTLEMENT)]
    public event Action<SettlementProto> HandleSettlement;

    [ResponseEvent(ProtoId.TOTAL_SETTLEMENT)]
    public event Action<TotalSettlementProto> HandleTotalSettlement;

    [ResponseEvent(ProtoId.MESSAGE)]
    public event Action<MessageProto> HandleMessageResult;

    [ResponseEvent(ProtoId.VOTE_INFO)]
    public event Action<VoteInfoProto> HandleVoteInfo;

    [ResponseEvent(ProtoId.GAME_START)]
    public event Action<GameBoardProto> HandleGameBoard;

    //[ResponseEvent(ProtoId.DISMISS_ROOM_RESULT)]
    //public event Action<DismissResultProto> HandleDismissRoom;

    [ResponseEvent(ProtoId.ON_LINE_RESULT)]
    public event Action<OfflineResultProto> HandleOffLineResult;

    [ResponseEvent(ProtoId.STEP_RESULT)]
    public event Action<StepResultProto> HandleStep;

    [ResponseEvent(ProtoId.READY_RESULT)]
    public event Action<ReadyResultProto> HandleReady;

    [ResponseEvent(ProtoId.DISMISS_ROOM)]
    public event Action<RoomDismissProto> HandleDismissRoom;

    [ResponseEvent(ProtoId.RECEIVE_UNDO_REQUEST)]
    public event Action<ApplyTakeBackProto> HandleUndoRequest;

    [ResponseEvent(ProtoId.RECEIVE_UNDO_RESULT)]
    public event Action<TakeBackResultProto> HandleUndoResult;

    [ResponseEvent(ProtoId.NOTIFY_STEP)]
    public event Action<NotifyStepActionProto> HandleNotifyStep;

    [ResponseEvent(ProtoId.VOTE_DISMISS_ROOM)]
    public event Action<VoteDissmissRoom> HandleVoteDismissRoom;

    protected override SceenType _sceenType
    {
        get { return SceenType.Game; }
    }

    protected override void Awake()
    {
        base.Awake();

        HandleKickOff += DoKickOff;
        _handleNoticeLogin += DoHandleNoticeLogin;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        HandleKickOff -= DoKickOff;
        _handleNoticeLogin -= DoHandleNoticeLogin;
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
            case KickReason.ROOM_OVER:
                tips = "房间结束，请退出后重新进入。";
                break;
            case KickReason.ROOM_AHEAD_DISMISSED:
                tips = "房间未打完第一局，提前解散，请退出后重新进入。";
                break;
            case KickReason.ILLEGAL_STEP:
                tips = "非法行棋，请退出后重新进入。";
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

    private void DoHandleNoticeLogin(NoticeLoginProto proto)
    {
        LogUtil.Log("提示登录!!!LoginProto = " + proto);
        RequestLoginProto requestLoginProto = new RequestLoginProto()
        {
            account = SDKWrapper.instance.GetAccount(),
            code = proto.code,
            gameType = int.Parse(FileHelper.ReadConfig("GameType"))
        };
        string signStr = Util.GetMd5EncryptStr(requestLoginProto.account + requestLoginProto.code + GameManager.SECRET);
        requestLoginProto.sign = signStr;
        LogUtil.Log("开始登录!!!loginProto = " + requestLoginProto);
        //发送信息向服务器端  
        DoSocketRequest(ProtoId.REQUEST_LOGIN, requestLoginProto, ProtoId.ENTER_ROOM);
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
        bool normal = base.Reconnect();
        if (normal)
        {
            LogUtil.Log(string.Format("开始socket重连!!! socket重连次数为：{0}", _reconnectCount));
            LoadingWebWindow.instance.Show(GetFormatLoadingWebTips());
        }
        return normal;
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
}
