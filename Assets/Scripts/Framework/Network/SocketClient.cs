using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Runtime.InteropServices;
using System.Threading;
using ProtoBuf;

/// <summary>
/// Socket客户端
/// </summary>
public class SocketClient
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="ipOrDomain">监听的IP地址或域名</param>
    /// <param name="port">监听的端口</param>
    public SocketClient(string ipOrDomain, int port)
    {
        _ipOrDomain = ipOrDomain;
        _port = port;
    }

    #endregion

    #region 内部成员

    private Socket _socket;
    /// <summary>
    /// socket连接的ip或者域名
    /// </summary>
    private readonly string _ipOrDomain = "";
    /// <summary>
    /// socket连接地址的端口号
    /// </summary>
    private readonly int _port;
    /// <summary>
    /// 是否可以接受消息。用于断开socket连接的瞬间如果接收到消息，不执行接收消息的逻辑，_isRec控制“立即”不执行while循环块
    /// </summary>
    private bool _isRec;
    ///// <summary>
    ///// 是否可以断线重连检测。用于断开socket连接时，断线重连的线程里“立即”不检测socket！
    ///// </summary>
    //private bool _isReconnect;
    /// <summary>
    /// socket接收消息的线程
    /// </summary>
    private Thread _sockeRecThread;
    /// <summary>
    /// 检测socket连接的线程
    /// </summary>
    private Thread _checkConnectionThread;
    /// <summary>
    /// 线程锁
    /// </summary>
    private static readonly object _lockObj = new object();

    ///// <summary>
    ///// 现阶段socket框架未使用本地socket断线检测机制，使用的是心跳检测机制。发送消息超时即判断为socket失去连接。
    ///// </summary>
    ///// <returns></returns>
    //public bool CheckIsConnected()
    //{
    //    return CheckIsConnected(_socket);
    //}

    ///// <summary>
    ///// 检测socket是否断开连接。
    ///// </summary>
    ///// <param name="socket"></param>
    ///// <returns></returns>
    //private bool CheckIsConnected(Socket socket)
    //{
    //    lock (_lockObj)
    //    {
    //        if (socket == null)
    //        {
    //            LogUtil.Log("_socket is null");
    //            return false;
    //        }

    //        if (!socket.Connected)
    //        {
    //            LogUtil.Log("socket is disconnected");
    //            return false;
    //        }

    //        //socketState = !(isReadable && socket.Available == 0 || !socket.Connected);

    //        //1、如果socket缓存里还有数据（ _socket.Available != 0），则socket连接正常；
    //        //2、如果socket缓存里没有数据（ _socket.Available == 0），isReadable为不可读，则连接正常，否则连接已经断开。
    //        bool isReadable = socket.Poll(1000, SelectMode.SelectRead);
    //        bool socketState = _socket.Available != 0 || !isReadable;
    //        LogUtil.Log("socketState : " + socketState + " -----isReadable = " + isReadable + " Available = " + socket.Available + " Connected = " + socket.Connected);
    //        return socketState;
    //    }
    //}

    /// <summary>
    /// 开始接受客户端消息
    /// </summary>
    public void StartRecMsg()
    {
        //在这个线程中接受服务器返回的数据
        if (!_socket.Connected)
        {
            //与服务器断开连接跳出循环
            LogUtil.LogWarning("StartRecMsg error._socket is not connected!!!");
            return;
        }

        //接受数据保存至bytes当中
        byte[] bytes = new byte[1024];
        //Receive方法中会一直等待服务端回发消息
        //如果没有回发会一直在这里等着。
        LogUtil.Log("ready to receive");
        int i;
        List<byte> tempBytes = new List<byte>();
        while (_isRec && ((i = _socket.Receive(bytes)) > 0))//如果bytes不够大，则receive方法会继续接受缓冲区字节流
        {
            //收取到的实际字节流
            byte[] receiveBytes = new byte[i];
            Array.Copy(bytes, receiveBytes, i);

            //LogUtil.Log("i = " + i);//读取到的字节数量
            //LogUtil.Log("_socket.Available = " + _socket.Available); //接收缓冲区中还剩下的数组的字节数量
            //LogUtil.Log("receiveBytes.Length = " + receiveBytes.Length);

            //处理粘包和半包
            //半包：包太大，一次性接收不完。多次接收组合
            //粘包：一次性接收到多个包。接收到即拆分入队列，用一个index标记拆分索引。
            int index = 0;
            while (index < receiveBytes.Length)
            {
                if (tempBytes.Count > 0)//上一次分包后的半包数据。也就是上一次分包后，剩下的有用字节，和之后读取到的字节可以组成一个完整包
                {
                    tempBytes.AddRange(receiveBytes);
                    receiveBytes = tempBytes.ToArray();
                }

                //methodId半包检测
                int methodId = 0;
                if (index + 4 <= receiveBytes.Length)
                {
                    methodId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(receiveBytes, index));
                    index += 4;
                }
                else//半包，methodId没读取完。methodId读取需要四个字节
                {
                    byte[] overBytes = new byte[receiveBytes.Length - index];
                    Array.Copy(receiveBytes, index, overBytes, 0, receiveBytes.Length - index);
                    tempBytes = overBytes.ToList();
                    break;
                }

                //contentLength半包检测
                int contentLength = 0;
                if (index + 4 <= receiveBytes.Length)
                {
                    contentLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(receiveBytes, index));
                    index += 4;
                }
                else//半包，contentLength没读取完。contentLength读取需要四个字节
                {
                    byte[] overBytes = new byte[receiveBytes.Length - index + 4];
                    Array.Copy(receiveBytes, index - 4, overBytes, 0, receiveBytes.Length - index + 4);//需要包含methodid的字节
                    tempBytes = overBytes.ToList();
                    break;
                }

                //contentData半包检测
                if (index + contentLength <= receiveBytes.Length)
                {
                    LogUtil.Log(string.Format("收到消息,methodId:{0},消息长度:{1}", methodId, contentLength));
                    //成功分包
                    byte[] packageBytes = new byte[contentLength + 8];//分包数据
                    Array.Copy(receiveBytes, index - 8, packageBytes, 0, contentLength + 8);
                    index += contentLength;
                    DoReceive(packageBytes);

                    tempBytes.Clear();
                }
                else//半包，contentLength没读取完。contentLength读取需要四个字节
                {
                    byte[] overBytes = new byte[receiveBytes.Length - index + 8];
                    Array.Copy(receiveBytes, index - 8, overBytes, 0, receiveBytes.Length - index + 8);//需要包含methodid和contentLength的字节
                    tempBytes = overBytes.ToList();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 处理接受到的单个包
    /// </summary>
    /// <param name="contentBytes"></param>
    private void DoReceive(byte[] contentBytes)
    {
        //协议id。方法id
        int protoId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(contentBytes, 0));
        //数据内容
        byte[] objBytes = new byte[contentBytes.Length - 8];
        Array.Copy(contentBytes, 8, objBytes, 0, contentBytes.Length - 8);

        DoHandleRecMsg(protoId, objBytes);
    }

    #endregion

    #region 外部接口

    private string GetIPV4(string ipOrDomain)
    {
        //ipOrDomain为ip
        IPAddress ipAddress;
        if (IPAddress.TryParse(ipOrDomain, out ipAddress))
        {
            return ipOrDomain;
        }

        //ipOrDomain为域名或者主机名
        IPAddress[] ips = Dns.GetHostEntry(ipOrDomain).AddressList;
        //遍历获得的IP集以得到IPV4地址
        foreach (IPAddress ip in ips)
        {
            //筛选出IPV4地址
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        //如果没有则返回IPV6地址
        return ips[0].ToString();
    }

    private void InitSocket()
    {
        //实例化 套接字 （ip4寻址协议，流式传输，TCP协议）
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //设置心跳.设置了心跳后，socket.connected会正常只是当前socket的连接状态.
        //windows本身在发送消息后，如果未有任何操作，将会在2小时后，发送心跳到对端确认连接。socket.IOControl的原理是缩短了windows检测时间
        //_socket.IOControl(IOControlCode.KeepAliveValues, GetKeepAlive(1, 5000, 1000), null);
    }

    private void BeginConnect()
    {
        LogUtil.Log("BeginConnect connect!!!");
        string ip = GetIPV4(_ipOrDomain);
        IPAddress address = IPAddress.Parse(ip);
        //创建网络节点对象 包含 ip和port
        IPEndPoint endpoint = new IPEndPoint(address, _port);
        //将 监听套接字  绑定到 对应的IP和端口
        _socket.BeginConnect(endpoint, ConnectedCallback, _socket);

        //事实证明 ：以下代码在windows7中适用，windows10不适用。因为如果断网，socket连接超时时间为二三十秒，而在windows10上瞬间检测到，并调用ConnectedCallback。
        //IAsyncResult connResult = _socket.BeginConnect(endpoint, ConnectedCallback, _socket);
        //connResult.AsyncWaitHandle.WaitOne(4000, true);  //等待4秒
        ////4秒未连上，关闭socket
        //if (!connResult.IsCompleted)
        //{
        //    LogUtil.Log("Not connect WaitOne 4000");
        //    Close();
        //}
    }

    /// <summary>
    /// 开始服务，连接服务端
    /// </summary>
    public void StartClient()
    {
        LogUtil.Log("Start connect socket!!!");
        InitSocket();
        //新开一条线程连接socket，否则“BeginConnect”方法里的connResult.AsyncWaitHandle.WaitOne(4000, true)会阻塞主线程4秒!!!
        Thread connectThread = new Thread(BeginConnect);
        connectThread.Start();
        //BeginConnect();



        //try
        //{

        ////设置心跳
        //_socket.IOControl(IOControlCode.KeepAliveValues, GetKeepAlive(1, 5000, 5000), null);
        //    ////设置超时时间
        //    //_socket.SendTimeout = 5000;
        //    //创建 ip对象//兼容域名和ip参数传递

        //    string ip = GetIPV4(_ipOrDomain);
        //    IPAddress address = IPAddress.Parse(ip);
        //    //创建网络节点对象 包含 ip和port
        //    IPEndPoint endpoint = new IPEndPoint(address, _port);
        //    //将 监听套接字  绑定到 对应的IP和端口
        //    _socket.BeginConnect(endpoint, ConnectedCallback, _socket);
        //    //_socket.BeginConnect(endpoint, asyncResult =>
        //    //{
        //    //    try
        //    //    {
        //    //        //这里做一个超时的监测，当连接超过10秒还没成功表示超时
        //    //        //如果连接成功，不会挂起5秒，直接执行下一句代码
        //    //        //如果服务器未开启，也不会挂起。预测可能是网络延迟的时候会尝试5秒的连接！！！
        //    //        bool receiveResponse = asyncResult.AsyncWaitHandle.WaitOne(5000);
        //    //        //在关闭socket的时候，线程也被关闭掉，但是如果线程已经执行到_socket.BeginConnect，那么_socket会被置空，但是回调还是会执行!!!
        //    //        if (_socket == null)//所以在此处还需添加一个“_socket == null”的判断
        //    //        {
        //    //            return;
        //    //        }
        //    //        LogUtil.Log("receiveResponse = " + receiveResponse + "      _socket.Connected = " + _socket.Connected);

        //    //        DateTime endSateTime = DateTime.Now;
        //    //        TimeSpan sTime = endSateTime - startSateTime;
        //    //        LogUtil.Log("StartClient cost time : " + sTime.TotalMilliseconds);

        //    //        //开启断线检测
        //    //        if (_checkConnectionThread == null)
        //    //        {
        //    //            _checkConnectionThread = new Thread(CheckSocketConnection)
        //    //            {
        //    //                IsBackground = true
        //    //            };
        //    //            _isReconnect = true;
        //    //            _checkConnectionThread.Start();
        //    //        }

        //    //        DoHandleClientStarted(_socket.Connected);

        //    //        if (!_socket.Connected)
        //    //        {
        //    //            return;
        //    //        }

        //    //        _socket.EndConnect(asyncResult);

        //    //        //开始接受服务器消息
        //    //        //与socket建立连接成功，开启线程接受服务端数据。
        //    //        _sockeRecThread = new Thread(StartRecMsg)
        //    //        {
        //    //            IsBackground = true
        //    //        };
        //    //        _isRec = true;
        //    //        _sockeRecThread.Start();
        //    //    }
        //    //    catch (Exception e)
        //    //    {
        //    //        LogUtil.LogWarning(e.ToString());
        //    //    }
        //    //}, null);
        //}
        //catch (Exception e)
        //{
        //    LogUtil.LogWarning(e.ToString());
        //}
    }

    /// 异步连接回调函数
    /// 
    /// 
    private void ConnectedCallback(IAsyncResult iar)
    {
        LogUtil.Log("ConnectedCallback!!!!");
        Socket socket = (Socket)iar.AsyncState;
        if (socket == null)
        {
            LogUtil.Log("socket is null");
            return;
        }

        try
        {
            ////开启断线检测
            //if (_checkConnectionThread == null)
            //{
            //    _checkConnectionThread = new Thread(CheckSocketConnection)
            //    {
            //        IsBackground = true
            //    };
            //    _isReconnect = true;
            //    _checkConnectionThread.Start();
            //}

            if (socket.Connected)
            {
                DoHandleConnectSuccess();
            }
            else
            {
                DoHandleConnectFailed();
            }

            if (!socket.Connected)
            {
                return;
            }

            socket.EndConnect(iar);

            //开始接受服务器消息
            //与socket建立连接成功，开启线程接受服务端数据。
            _sockeRecThread = new Thread(StartRecMsg)
            {
                IsBackground = true
            };
            _isRec = true;
            _sockeRecThread.Start();
        }
        catch (Exception e)
        {
            LogUtil.LogWarning(e.ToString());
        }
    }

    ///// <summary>
    ///// 开始进行socket断线检查，断线后，自动断线重连
    ///// </summary>
    //private void CheckSocketConnection()
    //{
    //    while (_isReconnect)
    //    {
    //        Thread.Sleep(3000);
    //        try
    //        {
    //            if (!CheckIsConnected(_socket))
    //            {
    //                Reconnect();
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            LogUtil.LogWarning(e.ToString());
    //        }
    //    }
    //}

    ///// <summary>
    ///// 心跳参数
    ///// </summary>
    ///// <param name="onOff">是否启用Keep-Alive</param>
    ///// <param name="keepAliveTime">多长时间开始第一次探测</param>
    ///// <param name="keepAliveInterval">探测时间间隔</param>
    ///// <returns></returns>
    //private byte[] GetKeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
    //{
    //    //设置心跳。为 Socket 设置低级操作模式，网线拔出、交换机掉电、客户端机器掉电等网络异常断开
    //    //eg:byte[] inValue = new byte[] { 1, 0, 0, 0, 0x88, 0x13, 0, 0, 0xd0, 0x07, 0, 0 };// 首次探测时间5 秒, 间隔侦测时间2 秒  

    //    //uint dummy = 0;
    //    //byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
    //    //BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0); //是否启用Keep-Alive
    //    //BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy)); //多长时间开始第一次探测
    //    //BitConverter.GetBytes((uint)2000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2); //探测时间间隔
    //    //byte[] buffer = new byte[12];
    //    //BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
    //    //BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
    //    //BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);

    //    uint dummy = 0;
    //    byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
    //    BitConverter.GetBytes((uint)onOff).CopyTo(inOptionValues, 0);
    //    BitConverter.GetBytes((uint)keepAliveTime).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
    //    BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);

    //    return inOptionValues;
    //}

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <typeparam name="TReq">发送数据的类型</typeparam>
    /// <param name="protoId">发送消息Id</param>
    /// <param name="content">发送内容</param>
    public void Send<TReq>(int protoId, TReq content) where TReq : class
    {
        lock (_lockObj)
        {
            //if (!CheckIsConnected(_socket))//断线等待断线重连
            //{
            //    //Reconnect();
            //    return;
            //}
            if (!_socket.Connected)
            {
                return;
            }

            Type type = typeof(TReq);
            var attributes = type.GetCustomAttributes(typeof(ProtoContractAttribute), false);
            if (attributes.Length <= 0)
            {
                LogUtil.LogWarning(string.Format("发送消息错误，类{0}没有“ProtoContractAttribute”特性!", type.Name));
                return;
            }

            //组装格式: 方法id + 数据长度 + 数据
            //方法id
            //win7 64位为小端模式，所以需要小端转大端。IPAddress.NetworkToHostOrder方法就是大小端互转
            byte[] methodIdBytes = BitConverter.GetBytes(IPAddress.NetworkToHostOrder(protoId));
            //数据
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, content);
            byte[] msgBytes = ms.ToArray();
            ms.Close();
            //数据长度
            byte[] dataLengthBytes = BitConverter.GetBytes(IPAddress.NetworkToHostOrder(msgBytes.Length));

            List<byte> msgList = new List<byte>();
            msgList.AddRange(methodIdBytes);
            msgList.AddRange(dataLengthBytes);
            msgList.AddRange(msgBytes);
            byte[] sendMsgBytes = msgList.ToArray();

            try
            {
                _socket.BeginSend(sendMsgBytes, 0, sendMsgBytes.Length, SocketFlags.None, SendCallback, _socket);
            }
            catch (Exception e)
            {
                LogUtil.LogWarning(e.ToString());
            }
        }
    }

    private void SendCallback(IAsyncResult iar)
    {
        Socket socket = (Socket)iar.AsyncState;
        if (socket == null)
        {
            LogUtil.Log("socket is null");
            return;
        }

        socket.EndSend(iar);
        DoHandleSendMsgComplete();
    }

    /// <summary>
    /// socket自身断线重连时需要的操作
    /// </summary>
    private void SelfClose()
    {
        try
        {
            LogUtil.Log("self Close Socket");
            if (_sockeRecThread != null)
            {
                _sockeRecThread.Abort();
            }
            if (_socket != null && _socket.Connected)
            {
                _socket.Disconnect(false);
            }
            if (_socket != null)
            {
                _socket.Close();
            }
        }
        catch (Exception e)
        {
            LogUtil.LogWarning(e.ToString());
        }
    }

    /// <summary>
    /// 外界关闭Socket
    /// </summary>
    public void Close()
    {
        LogUtil.Log("Close Socket!!!");
        try
        {
            _isRec = false;
            //_isReconnect = false;
            if (_sockeRecThread != null)
            {
                _sockeRecThread.Abort();
            }
            if (_checkConnectionThread != null)
            {
                _checkConnectionThread.Abort();
            }
            if (_socket != null && _socket.Connected)
            {
                _socket.Disconnect(false);
            }
            if (_socket != null)
            {
                _socket.Close();//如果beginConnect还未结束，可以释放掉正在连接的beginConnect，并调用beginConnect里的回调!!!
            }
        }
        catch (Exception e)
        {
            LogUtil.LogWarning(e.ToString());
        }

        DoHandleClientClose();
    }

    #endregion

    #region 事件处理

    //private void DoHandleException(Exception ex)
    //{
    //    if (ex is SocketException)
    //    {
    //        LogUtil.LogWarning("Socket Error,ErrorCode is " + ((SocketException)ex).SocketErrorCode);
    //    }
    //    else
    //    {
    //        LogUtil.LogWarning("Exception : " + ex);
    //    }

    //    if (HandleException != null)
    //    {
    //        HandleException(ex);
    //    }
    //}

    //private void DoHandleClientStarted(bool success)
    //{
    //    LogUtil.Log(string.Format("client socket start,success = {0}", success));
    //    if (HandleClientStarted != null)
    //    {
    //        HandleClientStarted(success);
    //    }
    //}

    /// <summary>
    /// socket连接成功
    /// </summary>
    private void DoHandleConnectSuccess()
    {
        LogUtil.Log("DoHandleConnectSuccess!!!");
        if (HandleConnectSuccess != null)
        {
            HandleConnectSuccess();
        }
    }

    /// <summary>
    /// socket连接失败
    /// </summary>
    private void DoHandleConnectFailed()
    {
        LogUtil.Log("DoHandleConnectFailed!!!");
        if (HandleConnectFailed != null)
        {
            HandleConnectFailed();
        }
    }

    //private void DoHandleDisconnect()
    //{
    //    LogUtil.Log("socket is Disconnected!!!");
    //    if (HandleClientStarted != null)
    //    {
    //        //HandleClientStarted(success);
    //    }
    //}

    /// <summary>
    /// 接受到消息
    /// </summary>
    /// <param name="protoId"></param>
    /// <param name="contentBytes"></param>
    private void DoHandleRecMsg(int protoId, byte[] contentBytes)
    {
        LogUtil.Log("DoHandleRecMsg!!!");
        if (HandleRecMsg != null)
        {
            HandleRecMsg(this, protoId, contentBytes);
        }
    }

    /// <summary>
    /// 发送消息回调
    /// </summary>
    private void DoHandleSendMsgComplete()
    {
        LogUtil.Log("DoHandleSendMsgComplete!!!");
        if (HandleSendMsgComplete != null)
        {
            HandleSendMsgComplete();
        }
    }

    private void DoHandleClientClose()
    {
        LogUtil.Log("DoHandleClientClose!!!");
        if (HandleClose != null)
        {
            HandleClose();
        }
    }

    /// <summary>
    /// 客户端连接建立成功后回调
    /// </summary>
    public Action HandleConnectSuccess { get; set; }

    /// <summary>
    /// 客户端连接建立失败后回调
    /// </summary>
    public Action HandleConnectFailed { get; set; }

    /// <summary>
    /// 处理接受消息的委托
    /// </summary>
    public Action<SocketClient, int, byte[]> HandleRecMsg { get; set; }

    /// <summary>
    /// 客户端连接发送消息后回调
    /// </summary>
    public Action HandleSendMsgComplete { get; set; }

    /// <summary>
    /// 客户端连接关闭后回调
    /// </summary>
    public Action HandleClose { get; set; }

    ///// <summary>
    ///// 异常处理程序
    ///// </summary>
    //public Action<Exception> HandleException { get; set; }

    #endregion
}
