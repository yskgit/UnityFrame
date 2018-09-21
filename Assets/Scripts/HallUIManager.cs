using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[Flags]
public enum E_Update
{
    Default = 0,
    RoomCard = 1 << 0,//1
    Gold = 1 << 1,//2
    Email = 1 << 2,//4
    Head = 1 << 3,//8
    LevelAndScore = 1 << 4,//16
    All = Default | RoomCard | Gold | Email | Head | LevelAndScore
}

public class HallUIManager : UIManager
{
    //private readonly Vector3 MAJING_CREATE_ROOM_START_POS = new Vector3(-4.5f, 50f, 0f);
    //private readonly Vector3 MAJING_CREATE_ROOM_TARGET_POS = new Vector3(-9f, 57f, 0f);
    //private readonly Vector3 MAJING_ENTER_ROOM_START_POS = new Vector3(40f, 40f, 0f);
    //private readonly Vector3 MAJING_ENTER_ROOM_TARGET_POS = new Vector3(47f, 33f, 0f);
    //private readonly Vector3 FRAME_ENTER_ROOM_START_POS = new Vector3(0f, 0f, -3f);
    //private readonly Vector3 FRAME_ENTER_ROOM_TARGET_POS = new Vector3(0f, 0f, 3f);

    private MyButton _addRoomCardBtn;
    private MyButton _settingBtn;
    private MyButton _ruleBtn;
    private MyButton _createRoomBtn;
    private MyButton _enterRoomBtn;
    private MyButton _storeBtn;
    private MyButton _gradeBtn;
    private MyButton _activityBtn;
    private MyButton _emailBtn;
    private MyButton _profileBtn;
    private MyButton _headBtn;

    private MyButton _testBtn;

    private GameObject _majiang_CreateRoom;
    private GameObject _majiang_EnterRoom;
    private GameObject _frame_EnterRoom;
    private GameObject _hint_Email;

    private Text _nicknameText;
    private Text _playerIdText;
    private Text _roomCardNumText;

    private Image _headImg;

    private Coroutine _goldCoroutine;
    private Coroutine _roomCardCoroutine;

    protected override void OnEnable()
    {
        //HallSocketWrapper.instance.HandleGameHistoryList += GradesListProtoResp;
        //HallSocketWrapper.instance.HandleGoodsList += GoodsListProtoResp;
        //HallSocketWrapper.instance.HandleEmailSingle += ReceiveOneEmail;
        //HallSocketWrapper.instance.HandleEmailsList += ReceiveEmailList;
    }

    protected override void OnDisable()
    {
        //HallSocketWrapper.instance.HandleGameHistoryList -= GradesListProtoResp;
        //HallSocketWrapper.instance.HandleGoodsList -= GoodsListProtoResp;
        //HallSocketWrapper.instance.HandleEmailSingle -= ReceiveOneEmail;
        //HallSocketWrapper.instance.HandleEmailsList -= ReceiveEmailList;
    }

    //public override string[] CacheAssets
    //{
    //    get { return new[] {""}; }
    //}

    public override void InitUI()
    {
        base.InitUI();
        ShowBg = false;

        //MyButton[] _buttons = GetComponent<MyButtonContainer>().MyButtons;
        //        Text[] _texts = GetComponent<TextContainer>().Texts;

        //_addRoomCardBtn = _buttons[0];
        //_addRoomCardBtn.onClick.AddListener(OnAddRoomCardBtnClick);
        //_settingBtn = _buttons[1];
        //_settingBtn.onClick.AddListener(OnSettingBtnClick);
        //_ruleBtn = _buttons[2];
        //_ruleBtn.onClick.AddListener(OnRuleBtnClick);
        //_createRoomBtn = _buttons[3];
        //_createRoomBtn.onClick.AddListener(OnCreateRoomBtnClick);
        //_enterRoomBtn = _buttons[4];
        //_enterRoomBtn.onClick.AddListener(OnEnterRoomBtnClick);
        //_storeBtn = _buttons[5];
        //_storeBtn.onClick.AddListener(OnStoreBtnClick);
        //_gradeBtn = _buttons[6];
        //_gradeBtn.onClick.AddListener(OnGradeBtnClick);
        //_activityBtn = _buttons[7];
        //_activityBtn.onClick.AddListener(OnActivityBtnClick);
        //_emailBtn = _buttons[8];
        //_emailBtn.onClick.AddListener(OnEmailBtnClick);
        //_profileBtn = _buttons[9];
        //_profileBtn.onClick.AddListener(OnProfileBtnClick);
        //_headBtn = _buttons[10];
        //_headBtn.onClick.AddListener(OnHeadBtnClick);

        //_testBtn = transform.Find("testBtn").GetComponent<MyButton>();
        //_testBtn.onClick.AddListener(OnTestBtnClick);

        //_nicknameText = _texts[0];
        //_playerIdText = _texts[1];
        //_roomCardNumText = _texts[2];

        //_majiang_CreateRoom = _objs[0];
        //_frame_EnterRoom = _objs[1];
        //_majiang_EnterRoom = _objs[2];
        //_hint_Email = _objs[3];

        //_headImg = _images[0];

        //CurrentSelectedObj = _createRoomBtn.gameObject;
    }

    public override void InitData(object[] args)
    {
        DoAnimation();
        //LoadInfo();
        //UpdateEmailInfo();
    }

    public override void ReturnBackToThisWindow(bool isChangeWindow)
    {
        base.ReturnBackToThisWindow(isChangeWindow);
        if (isChangeWindow)
        {
            //DoAnimation();
            //CurrentSelectedObj = _createRoomBtn.gameObject;
            //UpdateInfo(E_Update.Email);
        }
    }

    public override void OnEscapeKeyDown(BaseEventData eventData)
    {
        base.OnEscapeKeyDown(eventData);
        ShowWindow("ui_win_quitGame"); 
    }

    private void DoAnimation()
    {
        //_majiang_CreateRoom.transform.localPosition = MAJING_CREATE_ROOM_START_POS;
        //_majiang_EnterRoom.transform.localPosition = MAJING_ENTER_ROOM_START_POS;
        //_frame_EnterRoom.transform.eulerAngles = FRAME_ENTER_ROOM_START_POS;

        //DoLocalMove(_majiang_CreateRoom.transform, MAJING_CREATE_ROOM_TARGET_POS, 1f, Ease.Flash, null).SetLoops(-1, LoopType.Yoyo);
        //DoLocalMove(_majiang_EnterRoom.transform, MAJING_ENTER_ROOM_TARGET_POS, 1f, Ease.Flash, null).SetLoops(-1, LoopType.Yoyo);
        //DoRotate(_frame_EnterRoom.transform, FRAME_ENTER_ROOM_TARGET_POS, 1f, Ease.Flash, null).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnWebviewClick()
    {
        Debug.Log("OnWebviewClick!!!");
        AndroidJavaClass cls = new AndroidJavaClass("com.jyhd.game.util.CYWebViewHelper");
        cls.CallStatic("openWebView");
    }

    public void UpdateInfo(E_Update updateData)
    {
        //if (updateData == E_Update.Default)
        //{
        //    return;
        //}
        //if ((E_Update.RoomCard & updateData) == E_Update.RoomCard)//更新房卡
        //{
        //    UpdateRoomCardInfo();
        //}
        //else if ((E_Update.Gold & updateData) == E_Update.Gold)//更新金币
        //{
        //    UpdateGoldInfo();
        //}
        //else if ((E_Update.Email & updateData) == E_Update.Email)//更新邮件提示
        //{
        //    UpdateEmailInfo();
        //}
        //else if ((E_Update.Head & updateData) == E_Update.Head)//更新头像图片
        //{
        //    UpdateHeadImg();
        //}
    }
}
