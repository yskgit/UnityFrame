using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace HallProtoConstructs
{
    [ProtoContract]
    public class LoginProto
    {
        [ProtoMember(1)] public string account;
        [ProtoMember(2)] public int code;
        [ProtoMember(3)] public string sign;
        [ProtoMember(4)]
        public int gameType;
        [ProtoMember(5)]
        public int connectType;//0正常登陆，1断线重连
    }

    [ProtoContract]
    public class NoticeLoginProto
    {
        [ProtoMember(1)]
        public int code;

        public override string ToString()
        {
            return "code = " + code;
        }
    }

    /**
    * 要求注册协议
    * code为channel的hashCode，即为socket连接的识别码
*/
    [ProtoContract]
    public class NoticeRegisterProto
    {
        [ProtoMember(1)]
        public int code;
    }

    [ProtoContract]
    public class LoginSuccessProto
    {
        [ProtoMember(1)] public string nickname;//用户昵称
        [ProtoMember(2)] public bool gender;//用户性别
        [ProtoMember(3)] public int portrait;//用户头像编号
        [ProtoMember(4)] public int ticket;//房卡数量
        [ProtoMember(5)] public int roomId;//断线重连的房间号 为0则不是断线重连
        [ProtoMember(6)] public string host;
        [ProtoMember(7)] public int port;

        public override string ToString()
        {
            return "nickname:" + nickname + " gender:" + gender + " portrait:" + portrait + " ticket:" + ticket;
        }
    }

    [ProtoContract]
    public class LoginFailProto
    {
        [ProtoMember(1)] public byte LoginFailReason;
        public override string ToString()
        {
            return "LoginFailReason:" + LoginFailReason;
        }
    }

    [ProtoContract]
    public class RegisterProto
    {
        [ProtoMember(1)]
        public string account;
        [ProtoMember(2)]
        public string nickname;
        [ProtoMember(3)]
        public bool gender;
        [ProtoMember(4)]
        public int portrait;
        [ProtoMember(5)]
        public int code;
        [ProtoMember(6)]
        public string sign;

        public override string ToString()
        {
            return string.Format("account:{0},nickname:{1},gender:{2},portrait:{3},code:{4},sign:{5}", account, nickname, gender, portrait, code, sign);
        }
    }

    /**
 * 更新用户个人信息协议
 *
 * @return 成功不返回任何东西
 * nickname为用户昵称
 * gender为用户性别，true=》男，false=》女
 * portrait为用户头像编号
 */
    [ProtoContract]
    public class UpdateInfoProto
    {
        [ProtoMember(1)]
        public String nickname;
        [ProtoMember(2)]
        public bool gender;
        [ProtoMember(3)]
        public int portrait;
    }

    [ProtoContract]
    public class RequestGameHistoryListProto
    {
        [ProtoMember(1)]
        public int length;//单页长度
        [ProtoMember(2)]
        public int page;//页数
        [ProtoMember(3)]
        public int gameType;
    }

    [ProtoContract]
    public class GameHistoryListProto
    {
        [ProtoMember(1)]
        public List<GameHistoryModel> historyList;

        public GameHistoryListProto()
        {
            historyList = new List<GameHistoryModel>();
        }

    }

    [ProtoContract]
    public class GameHistoryModel
    {
        [ProtoMember(1)]
        public int id;
        [ProtoMember(2)]
        public List<UserModel> userNameList;
        [ProtoMember(3)]
        public List<int> resultDataList;
        [ProtoMember(4)]
        public long startTime;
        [ProtoMember(5)]
        public long endTime;

        public GameHistoryModel()
        {
            userNameList = new List<UserModel>();
            resultDataList = new List<int>();
        }
    }
    [ProtoContract]
    public class UserModel
    {
        [ProtoMember(1)]
        public string id;
        [ProtoMember(2)]
        public string nickName;
        [ProtoMember(3)]
        public bool gender;
        [ProtoMember(4)]
        public int portrait;
    }

    [ProtoContract]
    public class GameReplayProto
    {
        [ProtoMember(1)]
        public List<Byte> replayData;
    }

    [ProtoContract]
    public class RequestGoodsListProto
    {
        [ProtoMember(1)]
        public bool noop;
    }

    /**
 * 发起订单，获取流水号后调用sdk支付接口
 *
 * @return [BuyGoodsFlowProto]
 */
    [ProtoContract]
    public class RequestBuyGoodsProto
    {
        [ProtoMember(1)]
        public string itemPackageId;
    }

    /**
 * 仅用于平台商没有提供支付结果回调功能的备用方案，例如ut
 * 支付接口返回结果后，将流水号、结果、签名发给服务端结束订单
 * 签名为md5(flowId + (isSuccess?"1":"0") + SECRET_KEY)
 *
 * @return [BuyGoodsResultProto]
 */
    [ProtoContract]
    public class CallbackBuyGoodsProto
    {
        [ProtoMember(1)]
        public string flowId;

        [ProtoMember(2)]
        public bool isSuccess;

        [ProtoMember(3)]
        public string sign;
    }

    /**
 * 创建订单流水号
 */
    [ProtoContract]
    public class BuyGoodsFlowProto
    {
        //流水号
        [ProtoMember(1)]
        public string flowId;
    }

    /**
 * 订单结果，如果商品无法被购买，则不返回流水号，直接结束
 */
    [ProtoContract]
    public class BuyGoodsResultProto
    {
        //流水号
        [ProtoMember(1)]
        public sbyte code;//对应 BuyGoodsResultCode 的结果

        [ProtoMember(2)]
        public ItemPackageModel item;//
    }

    public enum BuyGoodsResultCode
    {
        //成功
        SUCCESS = 1,
        //失败
        FAIL = 0,
        //还未上架
        NOT_STARTED = -1,
        //已经过期
        EXPIRED = -2,
        //售罄
        SOLD_OUT = -3
    }

    [ProtoContract]
    public class RequestEmailsListProto
    {
        [ProtoMember(1)]
        public string emailDetailId;
    }

    [ProtoContract]
    public class GoodsListProto
    {
        [ProtoMember(1)]
        public List<ItemPackageModel> GoodList;

        public GoodsListProto()
        {
            GoodList = new List<ItemPackageModel>();
        }
    }
    [ProtoContract]
    public class ItemPackageModel
    {
        [ProtoMember(1)]
        public string id;//商品ID

        [ProtoMember(2)]
        public byte type;//商品类型

        [ProtoMember(3)]
        public int num;//商品個數

        [ProtoMember(4)]
        public int price;//价格

        [ProtoMember(5)]
        public long startTime;//开始时间

        [ProtoMember(6)]
        public long endTme;//结束时间

        [ProtoMember(7)]
        public int limit;//限量个数  如果为0时 则不显示

        [ProtoMember(8)]
        public int left;//剩余个数

        [ProtoMember(9)]
        public long curTime;// 当前时间

        [ProtoMember(10)]
        public string productId;// 商品Id

        public bool isClose;// 是否关闭点击

        public ItemPackageModel()
        {
            isClose = false;
        }
    }
    [ProtoContract]
    public class EmailsListProto
    {
        [ProtoMember(1)]
        public List<EmailProto> emails;

        public EmailsListProto()
        {
            emails = new List<EmailProto>();
        }
    }
    [ProtoContract]
    public class EmailProto
    {
        [ProtoMember(1)]
        public string id; //邮件ID

        [ProtoMember(2)]
        public string title;//邮件标题

        [ProtoMember(3)]
        public string content;//邮件内容

        [ProtoMember(4)]
        public List<ItemPackageModel> extra;//邮件附件

        [ProtoMember(5)]
        public long createTime;//邮件创建时间

        [ProtoMember(6)]
        public long expiryTime;//邮件结束时间

        public EmailProto()
        {
            extra = new List<ItemPackageModel>();
            isRead = false;
            isGet = false;
        }

        public bool isRead;
        public bool isGet;
    }
    [ProtoContract]
    public class RecieveEmailProto
    {
        [ProtoMember(1)]
        public string emailDetailId;//邮件id

        [ProtoMember(2)]
        public bool isRecieveExtra;//true为领取附件 false为删除邮件
    }
    [ProtoContract]
    public class RecieveEmailResult
    {
        [ProtoMember(1)]
        public string id;//邮件id

        [ProtoMember(2)]
        public sbyte code;//返回状态 数据类型为RecieveEmailCode

    }

    [ProtoContract]
    public enum RecieveEmailCode
    {
        //成功
        SUCCESS = 0,

        //已领取过
        ALREADY_RECIEVED = -1,

        //未知邮件信息，已过期或id错误
        NOT_FOUND = -2,

        //不是你的邮件
        NOT_YOURS = -3,
        //不领取附件删除
        DELETED = -4

    }

    [ProtoContract]
    public class RequestReplayCommand
    {
        [ProtoMember(1)]
        public int replayId;//对局ID

    }

    /**
    * 创建房间协议，内容待定 //todo
    * @return 成功[JoinRoomProto] ,失败[JoinRoomFailProto]
    */
    [ProtoContract] // 10004
    public class RequestCreateRoomProto
    {
        /**
         * 游戏类型
         * 石家庄麻将 = 1
         * 斗地主 = 2
         */

        [ProtoMember(1)]
        public int gameType;

        //设置
        [ProtoMember(2)]
        public string setting;

        public override string ToString()
        {
            return string.Format("gameType:{0},setting:{1}", gameType, setting);
        }
    }


    [ProtoContract]
    /**
      * 房间规则模型
      */
    public class RoomSettingModel
    {
        //游戏局数
        [ProtoMember(1)]
        public int gameNumType;

        //是否分摊房费
        [ProtoMember(2)]
        public bool isShare;

        //房主选边 是否为红
        [ProtoMember(3)]
        public bool isRedSide;

        // 游戏模式  1.ps    2.好友对战不算分   3.自由匹配不算分
        [ProtoMember(4)]
        public int mode;

        public String Encode()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(gameNumType);
            sb.Append(boolToChar(isShare));
            sb.Append(boolToChar(isRedSide));
            sb.Append(mode);
            return sb.ToString();
        }

        private char boolToChar(bool value)
        {
            return value ? '1' : '0';
        }

        //public RoomSettingModel(String setting)
        //{
        //    setting.
        //    char[] settingList = setting.toCharArray();
        //    gameNumType = charToInt(settingList[0]);
        //    isShare = charToBool(settingList[1]);
        //    topType = charToInt(settingList[2]);
        //    shuffle = charToBool(settingList[3]);
        //    hasLaiZi = charToBool(settingList[4]);
        //    menDaoLa = charToBool(settingList[5]);
        //}

        private int charToInt(char value)
        {
            return value - 48;
        }

        private bool charToBool(char value)
        {
            return value - 48 != 0;
        }

        public RoomSettingModel()
        {
        }
    }


    /**
 * 创建房间成功或加入房间成功，引导进入房间协议
 * roomId为房间号
 * host为游戏服地址
 * port为游戏服端口
 */
    [ProtoContract]
    public class JoinRoomProto
    {
        [ProtoMember(1)]
        public int roomId;
        [ProtoMember(2)]
        public string host;
        [ProtoMember(3)]
        public int port;

        public override string ToString()
        {
            return string.Format("roomId:{0},host:{1},port:{2}", roomId, host, port);
        }
    }

    /**
 * 创建房间或加入房间失败协议
 * reason为失败原因
 */
    [ProtoContract]
    public class CreateOrJoinRoomFailProto
    {
        [ProtoMember(1)]
        public byte code;
    }

    public enum JoinRoomFailCode
    {
        //无法连接到游戏服务器
        GAMESERVER_UNREACHABLE = 1,
        //房卡数量不足
        NOT_ENOUGH_TICKETS = 2,
        //房间已满
        ROOM_FULL = 3,
        //游戏类型不匹配
        GAME_TYPE_ERROR = 4,
        //无该房间信息
        ROOM_ID_ERROR = 5
    }

    /**
    * 申请加入房间协议
    *
    * @return 成功[JoinRoomProto] ,失败[JoinRoomFailProto]
    * roomId为要加入的房间号
*/
    [ProtoContract]
    public class RequestJoinRoomProto
    {
        [ProtoMember(1)]
        public int roomId;
        /**
* 游戏类型
* 石家庄麻将 = 1
* 斗地主 = 2
*/

        [ProtoMember(2)]
        public int gameType;
    }

    // 随机匹配队列 10012
    [ProtoContract]
    public class RandomGameQueueProto
    {
        /** 开关匹配
         * 0 开启   1 关闭
         * */
        [ProtoMember(1)]
        public int operationType;

        /** 游戏类型 */
        [ProtoMember(2)]
        public int gameType;

        /** 设置 */
        [ProtoMember(3)]
        public String setting;
    }
    /**
        * 随机匹配队列
        * 用于处理玩家加入匹配队列的请求的返回
        *
        */
    // 随机匹配队列  20015
    [ProtoContract]
    public class RandomGameQueueResultProto
    {
        /** 开关匹配
         * 0 开启   1 关闭
         * */
        [ProtoMember(1)]
        public int queueState;

        public override string ToString()
        {
            return string.Format("queueState:" + queueState);
        }
    }

    // @ProtoEntity(id = 20006, direction = ProtoEntity.ProtoDirection.S2C)
    [ProtoContract]
    public class BroadcastProto
    {
        [ProtoMember(1)]
        public String content;

        [ProtoMember(2)]
        public int level;
    }

    /**
 * 同步信息
 */
    [ProtoContract]
    public class SynchronizeInfoProto
    {
        [ProtoMember(1)]
        public string nickname;

        [ProtoMember(2)]
        public bool gender;

        [ProtoMember(3)]
        public int portrait;

        [ProtoMember(4)]
        public int ticket;

        public override string ToString()
        {
            return string.Format("nickname:{0},gender:{1},portrait:{2},ticket:{3}", nickname, gender, portrait, ticket);
        }
    }

    /**
 * 申请开启单机模式结果
 */
    //20017
    [ProtoContract]
    public class StartSingleModeResult
    {
        [ProtoMember(1)]
        public bool isSuccess;

        //0：初级 1：大师
        [ProtoMember(2)]
        public int level;

        [ProtoMember(3)]
        public byte code;

        public override string ToString()
        {
            return string.Format("isSuccess:{0},level:{1}", isSuccess, level);
        }

        public SingleModeFailCode GetFailCode()
        {
            return (SingleModeFailCode)Enum.Parse(typeof(SingleModeFailCode), code.ToString());
        }
    }

    public enum SingleModeFailCode
    {
        //成功
        SUCCESS = 1,

        //用户不存在
        GAMEUSER_NOTEXISTS = 2,

        //房卡数量不足
        ROOM_CARDNOTENOUGH = 3,
    }

    /**
    * 返回玩家当前积分
    * */
    //20018
    [ProtoContract]
    public class RankScoreProto
    {
        [ProtoMember(1)]
        public List<GameTypeWithScore> scoreInfos;

        public RankScoreProto()
        {
            scoreInfos = new List<GameTypeWithScore>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < scoreInfos.Count; i++)
            {
                sb.Append(@"\n " + scoreInfos[i]);
            }

            return sb.ToString();
        }
    }

    [ProtoContract]
    public class GameTypeWithScore
    {
        [ProtoMember(1)]
        public byte gameType;

        [ProtoMember(2)]
        public int score;

        [ProtoMember(3)]
        public int level;

        public override string ToString()
        {
            return string.Format("gameType:{0},score:{1},level:{2}", gameType, score, level);
        }
    }

    //20019 现在开启的活动类型
    [ProtoContract]
    public class ActivityState
    {
        [ProtoMember(1)]
        public int activityOpenState;

        public override string ToString()
        {
            return string.Format("activityOpenState:{0}", activityOpenState);
        }
    }

    /**
 * 申请开启单机模式
 */
    //10013
    [ProtoContract]
    public class StartSingleMode
    {
        /**
         * 等级 0：初级 1：大师
         */
        [ProtoMember(1)]
        public int level;
    }

    //20020
    [ProtoContract]
    public class LevelUpReward
    {
        [ProtoMember(1)]
        public int num;

        public override string ToString()
        {
            return string.Format("num:{0}", num);
        }
    }
}
