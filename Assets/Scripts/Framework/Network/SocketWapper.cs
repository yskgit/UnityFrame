#if Network
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HallProtoConstructs;
using ProtoBuf;
using UnityEngine;

#region 网络连接相关类

public struct ReceiveMsg
{
    public int ProtoId;
    public byte[] ContentBytes;
    public ReceiveMsg(int protoId, byte[] contentBytes)
    {
        ProtoId = protoId;
        ContentBytes = contentBytes;
    }
}

public struct SocketHandle
{
    public string Name;
    public SocketHandle(string name)
    {
        Name = name;
    }
}

public sealed class ResponseProto
{
    private readonly int _protoId;
    private readonly FieldInfo _fieldInfo;
    private readonly Type _classType;
    private readonly Type _parmaType;

    public int GetProtoId()
    {
        return _protoId;
    }

    public FieldInfo GetFieldInfo()
    {
        return _fieldInfo;
    }

    public Type GetClassType()
    {
        return _classType;
    }

    public Type GetParamType()
    {
        return _parmaType;
    }

    public ResponseProto(int protoId, FieldInfo fieldInfo, Type classType, Type paramType)
    {
        _protoId = protoId;
        _fieldInfo = fieldInfo;
        _classType = classType;
        _parmaType = paramType;
    }

    public override string ToString()
    {
        string str = "protoId=" + _protoId + ",";
        str = str + "method=" + _fieldInfo.Name + ",";
        str = str + "classType=" + _classType.Name + ",";
        str = str + "paramType=" + _parmaType.Name + ",";
        return str;
    }
}

#endregion

public abstract class SocketWapper<T> : SingletonBehaviour<T> where T : SingletonBehaviour<T>
{
    #region 变量

    /// <summary>
    /// socket连接成功
    /// </summary>
    public event Action HandleConnectSuccess;
    /// <summary>
    /// socket连接失败
    /// </summary>
    public event Action HandleConnectFailed;
    /// <summary>
    /// socket断开连接
    /// </summary>
    public event Action HandleDisconnect;
    /// <summary>
    /// 发送消息完成
    /// </summary>
    public event Action HandleSendMsgComplete;
    /// <summary>
    /// 发送消息成功，并成功接收消息
    /// </summary>
    public event Action HandleSendMsgSuccess;
    /// <summary>
    /// 关闭socket事件
    /// </summary>
    public event Action HandleCloseSocket;
    /// <summary>
    /// socket重连
    /// </summary>
    public event Action HandleReconnet;
    /// <summary>
    /// socket重连次数超出限制
    /// </summary>
    public event Action HandleReconnetTimeOut;
    /// <summary>
    /// socket大的重连次数超出限制。总的重连次数就为 HandleReconnetTimeOut x HandleCannotConnectWithServer 次。
    /// </summary>
    public event Action HandleCannotConnectWithServer;

    private SocketClient _socketClient;
    private Coroutine _heartbeatCoroutine;
    private Coroutine _connectCoroutine;
    private float _heartbeatInterval;
    private float _totalReconnectTime;//总的重连时间
    protected float _reconnectTimes;//重连次数
    protected int _bigReconnectTimes; //重连次数，超过这个数就只能退出游戏
    protected int _reconnectCount; //重连计数
    protected int _bigReconnectCount; //重连计数

    /// <summary>
    /// 优先级最高的消息，如登录，注册，被踢下线等
    /// </summary>
    private readonly Queue<ReceiveMsg> _highestPriorityReceiveMsgs = new Queue<ReceiveMsg>();
    /// <summary>
    /// 游戏中的消息接受，如读取邮件、拉取战绩列表等
    /// </summary>
    private readonly Queue<ReceiveMsg> _receiveMsgs = new Queue<ReceiveMsg>();
    /// <summary>
    /// socket 的回调
    /// </summary>
    private readonly Queue<SocketHandle> _socketHandles = new Queue<SocketHandle>();

    ///// <summary>
    ///// 优先权限最高的protoId。1:踢出协议;3:心跳协议;20001:要求登录协议;20002:要求注册协议;20003:登录成功协议
    ///// </summary>
    //private readonly List<int> _highestPriorityProtoIds = new List<int>()
    //{
    //    20001,20002,20003,1,3
    //};

    /// <summary>
    /// 是否阻塞消息。切换界面的时候
    /// </summary>
    private bool _blockReceiveMsg = true;

    /// <summary>
    /// 存储子类所有的消息事件
    /// </summary>
    protected readonly Dictionary<int, ResponseProto> ResponseProtoDictionary = new Dictionary<int, ResponseProto>();

    /// <summary>
    /// 发送消息时传过来的数据，用于发送消息后，弹出网络加载界面，服务器返回_responseId 后，消除网络加载界面
    /// </summary>
    private int[] _responseProtoIds;

    /// <summary>
    /// 发送消息计时，用于发送消息超时显示提示界面
    /// </summary>
    private float _sendTimer;

    /// <summary>
    /// 用于发送消息计时
    /// </summary>
    private bool _sendingMsg;

    /// <summary>
    /// 发送超时时间
    /// </summary>
    private float _sendTimeOut;

    /// <summary>
    /// 是否被踢下线。如果被踢下线，则不进行socket重连操作。
    /// </summary>
    private bool _beKickOff;

    #endregion

    #region 初始化数据

    protected override void Awake()
    {
        base.Awake();
        _heartbeatInterval = float.Parse(FileHelper.ReadConfig("HeartbeatInterval"));
        _sendTimeOut = float.Parse(FileHelper.ReadConfig("SendTimeOut"));
        _totalReconnectTime = float.Parse(FileHelper.ReadConfig("TotalReconnectTime"));
        _reconnectTimes = int.Parse(FileHelper.ReadConfig("ReconnectTimes"));
        _bigReconnectTimes = int.Parse(FileHelper.ReadConfig("BigReconnectTimes"));

        GenerateResponseProtoDictionary();
    }

    /// <summary>
    /// 根据c#类名生成 URLMapRecord
    /// </summary>
    /// <returns></returns>
    private void GenerateResponseProtoDictionary()
    {
        Type socketWapperType = this.GetType();

        //1、获取到所有事件
        var events = socketWapperType.GetEvents(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        for (int i = 0; i < events.Length; i++)
        {
            //2、获取到包含“ResponseEventAttribute”特性的事件
            var resp = events[i].GetCustomAttributes(typeof(ResponseEventAttribute), true);
            if (resp.Length <= 0)
            {
                continue;
            }

            //3、获取到FieldInfo
            string handleName = events[i].Name;

            var field = socketWapperType.GetField(handleName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field == null)
            {
                LogUtil.LogWarning(string.Format("类“{0}”没有事件{1}!!!", socketWapperType.Name, handleName));
                continue;
            }
            //4、获取参数类型
            var eventType = events[i].EventHandlerType;
            var methodInfo = eventType.GetMethod("Invoke", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (methodInfo == null)
            {
                continue;
            }
            var parameters = methodInfo.GetParameters();
            if (parameters.Length <= 0)
            {
                LogUtil.LogWarning(string.Format("方法“{0}”没有参数!!!", methodInfo.Name));
                continue;
            }
            var paramType = parameters[0].ParameterType;
            //5、获取到protoId
            int protoId = ((ResponseEventAttribute)resp[0]).ProtoId;
            ResponseProto proto = new ResponseProto(protoId, field, socketWapperType, paramType);

            if (ResponseProtoDictionary.ContainsKey(protoId))
            {
                LogUtil.LogError("重复的ResponseProto。protoId = " + protoId);
                continue;
            }
            ResponseProtoDictionary.Add(protoId, proto);
        }
    }

    protected abstract SceenType _sceenType
    {
        get;
    }

    #endregion

    #region 连接socket

    /// <summary>
    /// 连接socket
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public virtual void StartSocketConnect(string ip, int port)
    {
        //创建客户端对象
        _socketClient = new SocketClient(ip, port);

        //绑定向服务器发送消息后的处理事件
        _socketClient.HandleSendMsgComplete = () =>
        {
            _socketHandles.Enqueue(new SocketHandle("HandleSendMsgComplete"));
        };

        //绑定当收到服务器发送的消息后的处理事件
        _socketClient.HandleRecMsg = OnReceiveMsg;

        //绑定连接socket成功回调
        _socketClient.HandleConnectSuccess = () =>
        {
            _socketHandles.Enqueue(new SocketHandle("HandleConnectSuccess"));
        };

        //绑定连接socket失败回调
        _socketClient.HandleConnectFailed = () =>
        {
            _socketHandles.Enqueue(new SocketHandle("HandleConnectFailed"));
        };

        //绑定关闭socket回调
        _socketClient.HandleClose = () =>
        {
            _socketHandles.Enqueue(new SocketHandle("HandleSocketClose"));
        };

        Connect();
    }

    /// <summary>
    /// 连接socket，并开启socket连接计时
    /// </summary>
    private void Connect()
    {
        //开始运行客户端
        _socketClient.StartClient();
        _connectCoroutine = StartCoroutine(ConnectTimer());
    }

    /// <summary>
    /// socket重连。返回值：重连操作是否正常执行。
    /// </summary>
    protected virtual bool Reconnect()
    {
        if (_beKickOff)
        {
            LogUtil.Log("用户已经被踢下线，不进行断线重连操作!!!");
            return false;
        }

        if (UIController.instance.GetCurrentSceenType() != _sceenType)
        {
            LogUtil.Log(string.Format("当前场景为{0}，不能执行{1}的重连操作!!!", UIController.instance.GetCurrentSceenType(), name));
            return false;
        }

        _reconnectCount++;

        if (_reconnectCount > _reconnectTimes)
        {
            _bigReconnectCount++;
            if (_bigReconnectCount >= _bigReconnectTimes)
            {
                OnCannotConnetWithServer();
            }
            else
            {
                OnReconnectTimeOut();
            }

            return false;
        }

        Connect();
        if (HandleReconnet != null)
        {
            HandleReconnet.Invoke();
        }
        return true;
    }

    /// <summary>
    /// 4秒没连接上socket，重连
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConnectTimer()
    {
        yield return new WaitForSeconds(_totalReconnectTime / _reconnectTimes);
        LogUtil.Log(string.Format("连接时间超过{0}秒！！！socket连接失败，关闭socket，重新连接。", _totalReconnectTime / _reconnectTimes));

        OnConnectFailedCallback();
        _socketClient.Close();
        Reconnect();
    }

    /// <summary>
    /// 总的连接次数用完
    /// </summary>
    protected virtual void OnReconnectTimeOut()
    {
        _reconnectCount = 0;
        if (HandleReconnetTimeOut != null)
        {
            HandleReconnetTimeOut.Invoke();
        }
    }

    /// <summary>
    /// 两次大的超时后，判定为连接不上服务器
    /// </summary>
    protected virtual void OnCannotConnetWithServer()
    {
        LogUtil.Log("已经重连两次，无法连接上服务器，弹出退出游戏提示框!!!");
        if (HandleCannotConnectWithServer != null)
        {
            HandleCannotConnectWithServer.Invoke();
        }
    }

    #endregion

    #region 心跳

    /// <summary>
    /// 重置心跳
    /// </summary>
    public void ResetHeartbeat()
    {
        if (_socketClient == null)
        {
            return;
        }

        //LogUtil.Log("重置心跳");
        StopHeartbeat();
        //LogUtil.Log("开始心跳");
        if (_beKickOff)
        {
            return;
        }

        _heartbeatCoroutine = StartCoroutine(DoHeartbeatRequest());
    }

    /// <summary>
    /// 停止心跳
    /// </summary>
    private void StopHeartbeat()
    {
        if (_heartbeatCoroutine != null)
        {
            //LogUtil.Log("停止心跳协程");
            StopCoroutine(_heartbeatCoroutine);
        }
    }

    private IEnumerator DoHeartbeatRequest()
    {
        //LogUtil.Log("发送心跳请求");
        yield return new WaitForSeconds(_heartbeatInterval);
        StartHeatbeatReq();
        _heartbeatCoroutine = StartCoroutine(DoHeartbeatRequest());
    }

    public void StartHeatbeatReq()
    {
        StopHeartbeat();
        DoSocketRequest(2, new RequestHeartbeatProto() { noop = true });
    }

    #endregion

    #region 发送消息

    /// <summary>
    /// 向服务端发送消息的接口，显示网络加载界面
    /// responseProtoIds:发送消息时传过来的id，用于发送消息后，弹出网络加载界面，服务器返回_responseId 后，消除网络加载界面
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <param name="protoId"></param>
    /// <param name="content"></param>
    /// <param name="responseProtoIds"></param>
    public virtual void DoSocketRequest<TReq>(int protoId, TReq content, params int[] responseProtoIds) where TReq : class
    {
        if (_socketClient == null)
        {
            return;
        }
        _responseProtoIds = responseProtoIds;
        DoSocketRequest(protoId, content);
    }

    /// <summary>
    /// 向服务端发送消息的接口，不显示网络加载界面
    /// </summary>
    /// <typeparam name="TReq"></typeparam>
    /// <param name="protoId"></param>
    /// <param name="content"></param>
    public virtual void DoSocketRequest<TReq>(int protoId, TReq content) where TReq : class
    {
        if (_socketClient == null)
        {
            return;
        }
        _sendingMsg = true;
        _socketClient.Send(protoId, content);
    }

    #endregion

    #region 接收消息处理

    /// <summary>
    /// 接收到消息的回调
    /// </summary>
    /// <param name="socketClient"></param>
    /// <param name="protoId"></param>
    /// <param name="recieveBytes"></param>
    private void OnReceiveMsg(SocketClient socketClient, int protoId, byte[] recieveBytes)
    {
        ////由于unity的api只能在主线程中执行，所以只能把消息放到队列中，在update里接受消息！
        //if (_highestPriorityProtoIds.Contains(protoId))
        //{
        //    _highestPriorityReceiveMsgs.Enqueue(new ReceiveMsg(protoId, recieveBytes));
        //    //if (LOGIN_SUCCESS_PROTO_ID == protoId) //接收到登录成功的消息后，就可以接受其他消息了
        //    //{
        //    //    _blockReceiveMsg = false;
        //    //}
        //}
        //else
        //{
        //    _receiveMsgs.Enqueue(new ReceiveMsg(protoId, recieveBytes));
        //}

        _receiveMsgs.Enqueue(new ReceiveMsg(protoId, recieveBytes));
    }

    /// <summary>
    /// 在update里接收消息到主线程
    /// </summary>
    protected virtual void Update()
    {
        //接收最高级别的消息队列
        if (_highestPriorityReceiveMsgs.Count > 0)
        {
            DoReceiveMsg(_highestPriorityReceiveMsgs.Dequeue());
        }

        //接收游戏内逻辑消息的处理
        if (!_blockReceiveMsg && _receiveMsgs.Count > 0)
        {
            DoReceiveMsg(_receiveMsgs.Dequeue());
        }

        //处理socket自身相关操作。连接成功、连接失败、发送消息回调、socket关闭回调
        if (_socketHandles.Count > 0)
        {
            DoHandleEvent(_socketHandles.Dequeue());
        }

        if (_sendingMsg)
        {
            _sendTimer += Time.deltaTime;
            if (_sendTimer >= _sendTimeOut)
            {
                LogUtil.Log("Send time out!!!");
                ResetSendTime();
                OnSocketDisconnected();
            }
        }
    }

    /// <summary>
    /// 阻塞接收消息。消息缓存到队列中，解除阻塞的时候执行。如：界面跳转时，阻塞消息的接收
    /// </summary>
    public void BlockReceiveMsg(bool isTrue)
    {
        _blockReceiveMsg = isTrue;
    }

    /// <summary>
    /// 重置发送消息的计时和状态
    /// </summary>
    private void ResetSendTime()
    {
        LogUtil.Log("重置消息发送计时!!!");
        _sendTimer = 0f;
        _sendingMsg = false;
    }

    private void SetEmptyArray(int[] array, int empty)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = empty;
        }
    }

    /// <summary>
    /// 处理队列中的消息
    /// </summary>
    /// <param name="msg"></param>
    private void DoReceiveMsg(ReceiveMsg msg)
    {
        ResetSendTime();//收到任何消息，重置发送消息的计时和状态
        ResetHeartbeat();

        if (_responseProtoIds != null && _responseProtoIds.Contains(msg.ProtoId))
        {
            //置空发送消息时缓存的接收对应消息protoid列表
            SetEmptyArray(_responseProtoIds, -1);
            OnSendSuccessCallback();
        }

        ResponseProto response;
        if (ResponseProtoDictionary.TryGetValue(msg.ProtoId, out response))
        {
            MemoryStream stream = new MemoryStream(msg.ContentBytes);
            var paramType = response.GetParamType();
            var content = Serializer.NonGeneric.Deserialize(paramType, stream);
            stream.Close();

            var field = response.GetFieldInfo();
            var eventDelegate = field.GetValue(this) as MulticastDelegate;
            if (eventDelegate == null)
            {
                LogUtil.Log(string.Format("事件“{0}”没有注册方法", field.Name));
                return;
            }

            foreach (var eventHandler in eventDelegate.GetInvocationList())
            {
                eventHandler.Method.Invoke(eventHandler.Target, new object[] { content });
            }
        }
        else
        {
            LogUtil.Log("ProtoDictionary 字典里没有对应的 protoId:" + msg.ProtoId);
        }

        //        //        count++;
        //        int protoId = msg.ProtoId;
        //        List<URLMapRecord> records;
        //        if (GameManager.instance.GetUrlMapRecords().TryGetValue(protoId, out records))
        //        {
        //            LogUtil.Log(string.Format("收到服务器消息，protoId为{0}", protoId));
        //            foreach (var record in records)
        //            {
        //                LogUtil.Log(record.GetClassType().Name);
        //                //必须是在场景中存在且在Hierarchy层次结构中active为true的组件
        //                MonoBehaviour monoObj = FindObjectOfType(record.GetClassType()) as MonoBehaviour;
        //                if (!monoObj)
        //                {
        //                    continue;
        //                }
        //                try
        //                {
        //                    //拿到数据，执行方法
        //                    MemoryStream stream = new MemoryStream(msg.ContentBytes);
        //                    object paramobj = Serializer.NonGeneric.Deserialize(record.GetParamType(), stream);
        //                    stream.Close();
        //                    record.GetMethodInfo().Invoke(monoObj, new object[] { paramobj });
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine(e);
        //                }
        //            }
        //            //            LogUtil.Log("count = " + count);
        //        }
        //        else
        //        {
        //            LogUtil.LogWarning("错误的protoId = " + protoId);
        //        }
    }

    #endregion

    #region socket连接成功、连接失败、发送消息完成、发送消息成功、关闭socket、socket断开连接

    /// <summary>
    /// socket的一系列操作的回调
    /// </summary>
    /// <param name="handle"></param>
    private void DoHandleEvent(SocketHandle handle)
    {
        string handleName = handle.Name;
        switch (handleName)
        {
            case "HandleSendMsgComplete":
                OnSendCompleteCallback();
                break;
            case "HandleConnectSuccess":
                OnConnectSuccessCallback();
                break;
            //socket检测的未连接成功弃用。连接socket计时时间内未连接上，视为未连接成功
            case "HandleConnectFailed":
                OnConnectFailedCallback();
                break;
            case "HandleSocketClose":
                OnCloseCallback();
                break;
            default:
                LogUtil.LogWarning(string.Format("没有{0}对应的方法", handleName));
                break;
        }
    }

    /// <summary>
    /// socket开始的回调
    /// </summary>
    protected virtual void OnConnectSuccessCallback()
    {
        _reconnectCount = 0;
        _bigReconnectCount = 0;
        if (_connectCoroutine != null)
        {
            StopCoroutine(_connectCoroutine);
        }

        if (HandleConnectSuccess != null)
        {
            HandleConnectSuccess.Invoke();
        }

        ResetHeartbeat();
    }

    /// <summary>
    /// socket开始的回调
    /// </summary>
    protected virtual void OnConnectFailedCallback()
    {
        if (HandleConnectFailed != null)
        {
            HandleConnectFailed.Invoke();
        }
    }

    /// <summary>
    /// 关闭socket的回调
    /// </summary>
    protected virtual void OnCloseCallback()
    {
        if (HandleCloseSocket != null)
        {
            HandleCloseSocket.Invoke();
        }
    }

    /// <summary>
    /// 消息发送完毕的回调。不代表发送成功。物理断线的情况下socket是可以发送消息，但是服务端是接收不到消息的。
    /// </summary>
    protected virtual void OnSendCompleteCallback()
    {
        if (HandleSendMsgComplete != null)
        {
            HandleSendMsgComplete.Invoke();
        }
    }

    /// <summary>
    /// 发送消息成功回调
    /// </summary>
    protected virtual void OnSendSuccessCallback()
    {
        if (HandleSendMsgSuccess != null)
        {
            HandleSendMsgSuccess.Invoke();
        }
    }

    /// <summary>
    /// socket断开了连接。发送消息超时判断为socket断开连接
    /// </summary>
    protected virtual void OnSocketDisconnected()
    {
        _socketClient.Close();
        StopHeartbeat();
        Reconnect();
        if (HandleDisconnect != null)
        {
            HandleDisconnect.Invoke();
        }
    }

    #endregion

    #region 关闭socket处理

    //必须要在退出游戏时断开socket和thread，不然下次unity启动游戏会假死，未响应。
    //因为unity是单线程的，所以在关闭游戏时，只会关闭掉主线程，不会去管理其他的线程
    protected virtual void OnDestroy()
    {
        CloseSocket();
    }

    /// <summary>
    /// 关闭socket
    /// </summary>
    public void CloseSocket()
    {
        if (_socketClient != null)
        {
            LogUtil.Log(string.Format("关闭socket--{0}", name));
            _socketClient.Close();
        }
    }

    #endregion

    #region 被踢下线的处理

    protected void DoKickOff()
    {
        _beKickOff = true;
        StopHeartbeat();
        CloseSocket();
    }

    public void ManualDestroy()
    {
        LogUtil.Log("手动销毁：" + name);
        Destroy(gameObject);
    }

    #endregion
}
#endif