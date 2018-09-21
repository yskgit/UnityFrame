using System.Collections;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace GameProtos
{
    //40001 提示登录
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

    //30001 请求登录
    [ProtoContract]
    public class RequestLoginProto
    {
        [ProtoMember(1)] public string account;
        [ProtoMember(2)] public int code;
        [ProtoMember(3)] public string sign;
        [ProtoMember(4)]
        public int gameType;

        public override string ToString()
        {
            return string.Format("account:{0},code:{1},sign:{2},gameType:{3}", account, code, sign, gameType);
        }
    }

    /**
     * 请求离开房间
     * 在牌局开始之前，房主离开则直接解散，其他玩家直接离开
     * 牌局开始之后，不能离开房间，需发送RequestActionProto --- DISMISS
     */
    //30002
    [ProtoContract]
    public class RequestLeaveRoomProto
    {
        public override string ToString()
        {
            return "离开房间";
        }

        //不用填写
        [ProtoMember(1)]
        public bool noop = true;
    }

    /**
     * 请求操作
     */
    //30003
    [ProtoContract]
    public class RequestActionProto
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        [ProtoMember(1)]
        public int action;

        //表示操作是否执行，如客户端倒拉时传true，不倒拉时传false
        [ProtoMember(2)]
        public bool isAction;

        [ProtoMember(3)]
        public ChessStep_proto chessStep;

        public override string ToString()
        {
            return string.Format("action:{0},isAction:{1},chessStep:{2}", action, isAction, chessStep);
        }
    }

    /**
 * 发送聊天内容
 */
    //30004
    [ProtoContract]
    public class RequestMessageProto
    {
        public override string ToString()
        {
            return "contentType:" + contentType;
        }
        [ProtoMember(1)]
        public int contentType;
    }

    /**
 * 解散房间
 * */
    //30005
    [ProtoContract]
    public class DismissProto
    {
        public override string ToString() { return "isAction：" + isAction; }

        // 是否同意
        [ProtoMember(1)]
        public bool isAction;
    }

    /**
* 上下线
* */
    //30006
    [ProtoContract]
    public class OnlineProto
    {
        public override string ToString() { return "isAction：" + isAction; }

        // 是否同意
        [ProtoMember(1)]
        public bool isAction;
    }

    /**
* 上下线
* */
    //30007
    [ProtoContract]
    public class ReadyProto
    {
        public override string ToString() { return "isAction：" + isAction; }

        // 是否同意
        [ProtoMember(1)]
        public bool isAction;
    }

    //30009
    /**
     * 申请悔棋
     * */
    [ProtoContract]
    public class TakeBackProto
    {
        public override string ToString() { return "playerId：" + playerId; }

        [ProtoMember(1)]
        public string playerId;
    }

    //30010 某个玩家（不）同意悔棋
    /**
 * 同意悔棋
 * */
    [ProtoContract]
    public class AgreeTakeBackProto
    {
        public override string ToString()
        {
            return string.Format("playerId:{0},isAgree:{1}", playerId, isAgree);
        }

        [ProtoMember(1)]
        public string playerId;

        [ProtoMember(2)]
        public bool isAgree;
    }

    /**
 * 认输
 * */
    [ProtoContract]
    public class SurrenderProto
    {
        public override string ToString() { return "玩家认输："; }

        [ProtoMember(1)]
        public int noop;
    }

    //   /**
    //* 走棋
    //* */
    //   //30008
    //   [ProtoContract]
    //   public class StepProto
    //   {
    //       public override string ToString() { return "chessStep: " + chessStep; }

    //       [ProtoMember(1)]
    //       public ChessStep_proto chessStep;
    //   }

    // 40002 进入房间信息，单播
    [ProtoContract]
    public class EnterRoomProto
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("roomId:{0},gameNum:{1},maxGameNum:{2},currentPlayerId:{3}",
                roomId, gameNum, maxGameNum, currentPlayerId));
            foreach (var item in playerList)
            {
                sb.Append("\n EnterRoomNotifyProto = " + item);
            }

            sb.Append("\n roomSettingModel = " + roomSettingModel);
            sb.Append("\n eatenRedChessList : ");
            foreach (var item in eatenRedChessList)
            {
                sb.Append(" " + item);
            }
            sb.Append("\n eatenBlackChessList : ");
            foreach (var item in eatenBlackChessList)
            {
                sb.Append(" " + item);
            }

            return sb.ToString();
        }

        //房间id
        [ProtoMember(1)]
        public int roomId;

        //房间规则
        [ProtoMember(2)]
        public RoomSettingModel roomSettingModel;

        //房间内坐着的人的信息
        [ProtoMember(3)]
        public List<EnterRoomNotifyProto> playerList;

        //当前局数
        [ProtoMember(4)]
        public int gameNum;

        //局数上限
        [ProtoMember(5)]
        public int maxGameNum;

        //当前走棋玩家
        [ProtoMember(6)]
        public string currentPlayerId;

        // 记录被吃的红棋子集合
        [ProtoMember(7)]
        public List<int> eatenRedChessList;

        // 记录被吃的黑棋子集合
        [ProtoMember(8)]
        public List<int> eatenBlackChessList;

        public EnterRoomProto()
        {
            eatenRedChessList = new List<int>();
            eatenBlackChessList = new List<int>();
        }

        public EnterRoomNotifyProto GetPlayerProto(string playerId)
        {
            return playerList.Find(proto => proto.userId.Equals(playerId));
        }

        /// <summary>
        /// 获取己方玩家信息
        /// </summary>
        /// <returns></returns>
        public EnterRoomNotifyProto GetPlayerProto()
        {
            return GetPlayerProto(HallSocketWrapper.instance.PlayerData.Account);
        }

        /// <summary>
        /// 获取敌方玩家信息
        /// </summary>
        /// <returns></returns>
        public EnterRoomNotifyProto GetEnemyProto()
        {
            return playerList.Find(proto => !proto.userId.Equals(HallSocketWrapper.instance.PlayerData.Account));
        }

        /// <summary>
        /// 检测是否有敌方玩家
        /// </summary>
        /// <returns></returns>
        public bool CheckHasEnemy()
        {
            return GetEnemyProto() != null;
        }
    }

    /// <summary>
    /// 房间状态。对应“EnterRoomProto”的roomState
    /// </summary>
    public static class RoomState
    {
        /// <summary>
        /// 准备。等待游戏开始
        /// </summary>
        public const int WAITING = 0;
        /// <summary>
        /// 牌局
        /// </summary>
        public const int PLAYING = 1;
        /// <summary>
        /// 单局 结算
        /// </summary>
        public const int SETTLEMENT = 2;
        /// <summary>
        /// 整体结算
        /// </summary>
        public const int OVER = 3;
    }

    // 40003 玩家进入通知
    [ProtoContract]
    public class EnterRoomNotifyProto
    {
        //座号//座号 0红色 1黑色
        [ProtoMember(1)]
        public int color;

        //用户id
        [ProtoMember(2)]
        public string userId;

        //用户昵称
        [ProtoMember(3)]
        public string nickname;

        //用户性别
        [ProtoMember(4)]
        public bool gender;

        //用户头像
        [ProtoMember(5)]
        public int portrait;

        //准备状态
        [ProtoMember(6)]
        public bool isReady;

        //是否在线
        [ProtoMember(7)]
        public bool isOnline;

        //分数
        [ProtoMember(8)]
        public int score;

        // 上次走棋信息
        [ProtoMember(9)]
        public ChessStep_proto LastChessStep;

        // level
        [ProtoMember(10)]
        public int level;

        public override string ToString()
        {
            return string.Format("userId:{0},color:{1},nickname:{2},gender:{3},portrait:{4},isReady:{5},isOnline:{6},score:{7},LastChessStep:{8},level:{9}",
            userId, color, nickname, gender, portrait, isReady, isOnline, score, LastChessStep, level);
        }
    }

    // 40007 单局结束后结算
    [ProtoContract]
    public class SettlementProto
    {
        // 1.排位算分   2.好友对战不算分   3.自由匹配不算分
        [ProtoMember(1)]
        public int mode;

        //结算信息
        [ProtoMember(2)]
        public List<ChangementModel> changementList;

        //赢家Id
        [ProtoMember(3)]
        public string winnerPlayerId;

        //当前局数
        [ProtoMember(4)]
        public int gameNum;

        // 总局数
        [ProtoMember(5)]
        public int maxGameNum;

        // 总局数
        [ProtoMember(6)]
        public bool surrender;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("mode:{0},winnerPlayerId:{1}", mode, winnerPlayerId));
            sb.Append("\n changemode : ");
            foreach (var item in changementList)
            {
                sb.Append("\n" + item);
            }
            return sb.ToString();
        }

        public SettlementProto()
        {
            changementList = new List<ChangementModel>();
        }

        public ChangementModel GetPlayerChangementModel(string playerId)
        {
            return changementList.Find(item => item.playerId.Equals(playerId));
        }
    }

    // 40009 房间牌局进行完或者中途解散后，整体结算
    [ProtoContract]
    public class TotalSettlementProto
    {
        //结算信息
        [ProtoMember(1)]
        public List<TotalChangementModel> changementList;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in changementList)
            {
                sb.Append("\n changemode = " + item);
            }
            return sb.ToString();
        }
    }

    // 整体结算model
    [ProtoContract]
    public class TotalChangementModel
    {
        //玩家Id
        [ProtoMember(1)]
        public string playerId;

        //分数得失
        [ProtoMember(2)]
        public int score;

        //胜利次数
        [ProtoMember(3)]
        public int winNum;

        public override string ToString()
        {
            return string.Format("playerId:{0},score:{1},winNum:{2}", playerId, score, winNum);
        }
    }

    // 单局结算model
    [ProtoContract]
    public class ChangementModel
    {
        //玩家Id
        [ProtoMember(1)]
        public string playerId;

        //分数得失
        [ProtoMember(2)]
        public int scoreDelta;

        //总分数
        [ProtoMember(3)]
        public int scoreTotal;

        //分数换算的等级
        [ProtoMember(4)]
        public int level;

        public override string ToString()
        {
            return string.Format("playerId:{0},score:{1},scoreTotal:{2}", playerId, scoreDelta, scoreTotal);
        }
    }

    //// 40005 轮到某个玩家进行操作
    //[ProtoContract]
    //public class ActionProto
    //{
    //    //玩家Id
    //    [ProtoMember(1)]
    //    public string playerId;

    //    //操作类型
    //    [ProtoMember(2)]
    //    public int action;

    //    //倒计时
    //    [ProtoMember(3)]
    //    public int timeout;

    //    public override string ToString()
    //    {
    //        return string.Format("playerId:{0},action{1},timeout:{2}", playerId, action, timeout);
    //    }
    //}

    public static class ActionType
    {
        /// <summary>
        /// 上下线
        /// </summary>
        public const int ONLINE = 1;
        /// <summary>
        /// 准备
        /// </summary>
        public const int ZHUN_BEI = 2;
        /// <summary>
        /// 推送的棋盘信息
        /// </summary>
        public const int BOARD_INFO = 3;
        /// <summary>
        /// 走棋
        /// </summary>
        public const int ZOU_QI = 4;
        /// <summary>
        /// 解散房间
        /// </summary>
        public const int DIMISS = 5;
    }

    // 40009 语音
    [ProtoContract]
    public class MessageProto
    {
        [ProtoMember(1)]
        public string playerId;

        [ProtoMember(2)]
        public int contentType;

        public override string ToString()
        {
            return string.Format("playerId:{0},contentType:{1}", playerId, contentType);
        }
    }

    /// <summary>
    /// 实际棋子走棋结构
    /// </summary>
    [ProtoContract]
    public class ChessStep_proto
    {
        /// <summary>
        /// 起始棋盘格子
        /// </summary>
        [ProtoMember(1)]
        public int chessPosFrom_x;
        /// <summary>
        /// 落子棋盘格子
        /// </summary>
        [ProtoMember(2)]
        public int chessPosFrom_y;
        /// 落子棋盘格子
        [ProtoMember(3)]
        public int chessPosTo_x;
        [ProtoMember(4)]
        public int chessPosTo_y;
        /// <summary>
        /// 起始棋盘格子下的棋子
        /// </summary>
        [ProtoMember(5)]
        public int chessIdFrom;
        /// <summary>
        /// 落子棋盘格子下的棋子。为空说明为走棋，不为空说明此步为吃棋
        /// </summary>
        [ProtoMember(6)]
        public int chessIdTo;
        /// <summary>
        /// 走棋的角色
        /// </summary>
        [ProtoMember(7)]
        public string playerId;
        /// <summary>
        /// 当前走棋是第几步
        /// </summary>
        [ProtoMember(8)]
        public int stepCount;

        ///// <summary>
        ///// 此步是红方还是黑方走棋
        ///// </summary>
        //public E_Color round()
        //{
        //    return chessIdFrom <= 7 ? E_Color.Black : E_Color.Red;
        //}

        /// <summary>
        /// 此步是否是吃棋
        /// </summary>
        public bool isEat()
        {
            return chessIdTo != 0;
        }

        //public override string ToString()
        //{
        //    return string.Format("{0}走棋:({1},{2})({3}) -> ({4},{5})({6}),stepCount:{7}", playerId, chessPosFrom_y
        //        , chessPosFrom_x, ChessHelper.GetChessName(chessIdFrom), chessPosTo_y, chessPosTo_x, ChessHelper.GetChessName(chessIdTo), stepCount);
        //}
    }

    // 房间规则模型
    [ProtoContract]
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

        // 游戏模式  1.排位算分   2.好友对战不算分   3.自由匹配不算分
        [ProtoMember(4)]
        public int mode;

        public override string ToString()
        {
            return string.Format("gameNumType:{0},isShare:{1},isRedSide:{2},mode:{3}", gameNumType, isShare, isRedSide,
                mode);
        }
    }

    // 40004 玩家离开通知
    [ProtoContract]
    public class LeaveRoomNotifyProto
    {
        //座号
        [ProtoMember(1)]
        public string playerId;
    }

    // 操作失败  40010
    [ProtoContract]
    public class ActionFailProto
    {
        [ProtoMember(1)]
        public int code;
        public override string ToString()
        {
            return string.Format("自己操作失败：{0}", code);
        }
    }

    /**
     * 房间状态同步
     */
    //40011
    [ProtoContract]
    public class RoomStateProto
    {

        public override string ToString()
        {
            return "state:" + state;
        }

        [ProtoMember(1)]
        public int state;
    }

    /**
     * 投票信息同步
     * */
    //40012
    [ProtoContract]
    public class VoteInfoProto
    {
        //投票结果
        [ProtoMember(1)]
        //角色名 + 投票状态 0 不同意 1 同意 2 未选择
        public List<VoteInfo> voteList;

        //投票发起者
        [ProtoMember(2)]
        public string startVotePlayerId;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("{0} 发起了投票，已经通过的人有：", startVotePlayerId));
            foreach (var item in voteList)
            {
                sb.Append("\n VoteInfo = " + item);
            }

            return string.Format("{0} 发起了投票，已经通过的人数：{1}", startVotePlayerId, voteList.Count);
        }

        public VoteInfoProto()
        {
            voteList = new List<VoteInfo>();
        }
    }

    [ProtoContract]
    public class VoteInfo
    {
        public override string ToString()
        {
            return string.Format("playerId:{0},voteState:{1}", playerId, voteState);
        }

        // 投票者名字
        [ProtoMember(1)]
        public string playerId;

        //投票结果 0 不同意 1 同意 2 未选择
        [ProtoMember(2)]
        public int voteState;
    }

    //40013
    [ProtoContract]
    public class GameBoardProto
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("boardInfo = \n");
            int index = 0;
            foreach (var item in boardInfo)
            {
                sb.Append(" " + item);
                if (index >= 8)
                {
                    index = 0;
                    sb.Append("\n");
                    continue;
                }
                index++;
            }
            return sb.ToString();
        }

        //棋盘信息
        [ProtoMember(1)]
        public List<byte> boardInfo;

    }

    //40014 解散房间消息
    [ProtoContract]
    public class DismissResultProto
    {
        public override string ToString()
        {
            return string.Format("voterId:{0},isAction:{1}", voterId, isAction);
        }

        //响应者
        [ProtoMember(1)]
        public string voterId;

        //是否同意
        [ProtoMember(2)]
        public bool isAction;

    }

    //40015
    [ProtoContract]
    public class OfflineResultProto
    {
        public override string ToString()
        {
            return "playerId = " + playerId;
        }

        //上线或下线
        [ProtoMember(1)]
        public string playerId;

    }

    //40016
    [ProtoContract]
    public class StepResultProto
    {
        public override string ToString()
        {
            return string.Format("steperId:{0},chessStep:{1}", steperId, chessStep);
        }

        //走棋者
        [ProtoMember(1)]
        public string steperId;

        //走棋
        [ProtoMember(2)]
        public ChessStep_proto chessStep;
    }

    //40017
    [ProtoContract]
    public class ReadyResultProto
    {
        public override string ToString()
        {
            return string.Format("readyPlayerId:{0},isAction:{1}", readyPlayerId, isAction);
        }

        //走棋者
        [ProtoMember(1)]
        public string readyPlayerId;

        //准备或者取消准备
        [ProtoMember(2)]
        public bool isAction;

    }

    //40018 // 有人离开
    [ProtoContract]
    public class RoomDismissProto
    {
        public override string ToString()
        {
            return string.Format("reason:{0},description:{1},playerId:{2}", reason, description, playerId);
        }

        [ProtoMember(1)]
        public byte reason;

        [ProtoMember(2)]
        public string description;

        [ProtoMember(3)]
        public string playerId;
    }

    public enum DismissReason : byte
    {
        //房间不存在
        ROOM_NOT_EXIST = 1,
        //房间被解散
        ROOM_DISMISSED = 2,
        //离开房间
        LEAVE_ROOM = 3,
        //已在房间
        ALREADY_IN_ROOM = 4,
        //房间结束
        ROOM_OVER = 5,
        //房间未打完第一局，提前解散
        ROOM_AHEAD_DISMISSED = 6,
        //有玩家离开
        PLAYER_LEAVE = 7,
    }

    //40019 悔棋通知
    [ProtoContract]
    public class ApplyTakeBackProto
    {
        public override string ToString() { return "playerId：" + playerId; }

        [ProtoMember(1)]
        public string playerId;
    }

    //40020 悔棋结果
    [ProtoContract]
    public class TakeBackResultProto
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("playerId:{0},isAgree:{1}", playerId, isAgree));
            sb.Append("\n lastChessStepOne:");
            if (lastChessStepOne != null)
            {
                sb.Append("\n " + lastChessStepOne);
            }
            sb.Append("\n lastChessStepTwo:");
            if (lastChessStepTwo != null)
            {
                sb.Append("\n " + lastChessStepTwo);
            }
            sb.Append("\n lastChessStepThree:");
            if (lastChessStepThree != null)
            {
                sb.Append("\n " + lastChessStepThree);
            }
            return sb.ToString();
        }
        [ProtoMember(1)]
        public string playerId;
        [ProtoMember(2)]
        public bool isAgree;
        // 倒数第一步
        [ProtoMember(3)]
        public ChessStep_proto lastChessStepOne;
        // 倒数第二步
        [ProtoMember(4)]
        public ChessStep_proto lastChessStepTwo;
        // 倒数第三步
        [ProtoMember(5)]
        public ChessStep_proto lastChessStepThree;
    }

    //40021 通知走棋
    [ProtoContract]
    public class NotifyStepActionProto
    {
        public override string ToString() { return string.Format("playerId:{0},leftTime:{1}", playerId, leftTime); }

        [ProtoMember(1)]
        public string playerId;

        [ProtoMember(2)]
        public int leftTime;
    }

    // 投票离开 区别于有人主动离开的情况。如：准备界面有人点击了取消按钮
    [ProtoContract]
    public class VoteDissmissRoom
    {
        public override string ToString() { return "isSuccess:" + isSuccess; }

        /// <summary>
        /// 投票离开房间是否成功
        /// </summary>
        [ProtoMember(1)]
        public bool isSuccess;
    }
}
