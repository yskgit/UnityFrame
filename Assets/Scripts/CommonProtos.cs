using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

/**
* 踢出协议
* reason为被踢原因
*/
[ProtoContract]
public class KickProto
{
    [ProtoMember(1)]
    public byte reason;//1、重复登录；2、非法操作，签名校验失败或者code与用户channel的hashCode不对应等
    [ProtoMember(2)]
    public string description;

    public override string ToString()
    {
        return string.Format("reason:{0},description:{1}", reason, description);
    }
}

/**
* 请求心跳协议
*/
[ProtoContract]
public class RequestHeartbeatProto
{
    [ProtoMember(1)]
    public bool noop;
}

/**
* 心跳协议
*/
[ProtoContract]
public class HeartbeatProto
{
    [ProtoMember(1)]
    public bool noop;
}

public enum KickReason
{
    //重复登录
    MUTI_LOGIN = 1,

    //非法操作，签名校验失败或者code与用户channel的hashCode不对应等
    ILLIGAL_ACTION = 2,

    //房间不存在
    ROOM_NOT_EXIST = 3,

    //房间被解散
    ROOM_DISMISSED = 4,

    //离开房间
    LEAVE_ROOM = 5,

    //已在房间
    ALREADY_IN_ROOM = 6,

    //房间结束
    ROOM_OVER = 7,

    //房间未打完第一局，提前解散
    ROOM_AHEAD_DISMISSED = 8,

    //非法行棋
    ILLEGAL_STEP = 9,
}

