using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RegisterUIManager : UIManager
{
    private const int CHANGE_NAME_FPS = 10;//摇筛子时，随机姓名的频率
    private const float DICE_TIME = 1.8f;//筛子摇动的持续时间
    private bool _isRandomingName;  //正在随机姓名不能进入游戏，不能切换按钮
    private string _sex;

    private string[] _xingArr;
    private string[] _maleMingArr;
    private string[] _femaleMingArr;

    private MyToggleGroup _toggleGroup;
    private Text _name;
    private MyButton _diceBtn;
    private MyToggle _maleToggle;
    private MyToggle _femaleToggle;

    public override void InitUI()
    {
        base.InitUI();
        _maleToggle = transform.Find("toggle/Toggle_male").gameObject.GetComponent<MyToggle>();
        _maleToggle.onValueChanged.AddListener((toogle) =>
        {
            if (!toogle.isOn)
            {
                SetRandomName(_xingArr, false);
            }
        });
        _femaleToggle = transform.Find("toggle/Toggle_female").gameObject.GetComponent<MyToggle>();
        _femaleToggle.onValueChanged.AddListener((toogle) =>
        {
            if (!toogle.isOn)
            {
                SetRandomName(_xingArr, true);
            }
        });

        CurrentSelectedObj = transform.Find("toggle/Toggle_male").gameObject;

        _toggleGroup = transform.Find("toggle").GetComponent<MyToggleGroup>();
        _name = transform.Find("nickname/Text_nickname").GetComponent<Text>();

        GameObject dice = transform.Find("nickname/Image_dice").gameObject;
        _diceBtn = dice.GetComponent<MyButton>();
        _diceBtn.onClick.AddListener(OnDiceClick);
        GameObject randomNameTipsObj = transform.Find("nickname/Image_tips").gameObject;
        dice.GetComponent<MyButton>().OnSelectedShowObj = randomNameTipsObj;

        transform.Find("Button_enterGame").GetComponent<MyButton>().onClick.AddListener(OnEnterGameClick);
    }

    public override void InitData(object[] args)
    {
        base.InitData(args);

        _xingArr = FileHelper.GetXingArray();
        _maleMingArr = FileHelper.GetMaleMingArray();
        _femaleMingArr = FileHelper.GetFemaleMingArray();

        _sex = "male";

        SetRandomName(_xingArr, CheckIsMale());
    }

    public override void OnEscapeKeyDown(BaseEventData eventData)
    {
        base.OnEscapeKeyDown(eventData);
        ShowWindow("ui_win_quitGame");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    /// <summary>
    /// 点击骰子
    /// </summary>
    private void OnDiceClick()
    {
        if (_isRandomingName)
        {
            return;
        }

        //获取选择的性别
        bool isMale = CheckIsMale();
        _sex = isMale ? "male" : "female";

        StartCoroutine(RandomName(isMale));
//        PlayBtnAudio(_diceBtn.ClickAudioName);
    }

    IEnumerator RandomName(bool isMale)
    {
        yield return null;

        UGUISpriteAnimation ani = transform.Find("nickname/Image_dice").GetComponent<UGUISpriteAnimation>();
        ani.PlayFromOldFrame();

        float diceTimer = DICE_TIME;
        float nameTimer = 0f;
        _isRandomingName = true;
        KeyEventManager.instance.EnableKeyboardEvent(false);
        while (diceTimer > 0)
        {
            diceTimer -= Time.deltaTime;
            nameTimer += Time.deltaTime;

            if (nameTimer >= 1f / CHANGE_NAME_FPS)
            {
                nameTimer = 0;
                SetRandomName(_xingArr, isMale);
            }

            yield return null;
        }
        ani.Stop();
        _isRandomingName = false;
        KeyEventManager.instance.EnableKeyboardEvent(true);
    }

    /// <summary>
    /// 随机名字
    /// </summary>
    private void SetRandomName(string[] xingArr, bool isMale)
    {
        string[] mingArr = isMale ? _maleMingArr : _femaleMingArr;
        int randomXingInt = Random.Range(0, xingArr.Length);
        int randomMingInt = Random.Range(0, mingArr.Length);

        string xingStr = xingArr[randomXingInt];
        string mingStr = mingArr[randomMingInt];

        string username = xingStr + mingStr;
        _name.text = username;
    }

    private void OnEnterGameClick()
    {
        if (_isRandomingName)
        {
            Debug.Log("random name is not over");
            return;
        }
        string username = _name.text;
        Debug.Log("set username,username = " + username);
        MemoryHelper.SetUserName(username);
        MemoryHelper.SetUserSex(_sex);

        ChangeWindow("ui_win_hall",null,null,true,true);
    }

    private bool CheckIsMale()
    {
        bool male = true;
        IEnumerable<MyToggle> toggles = _toggleGroup.ActiveToggles();
        foreach (MyToggle t in toggles)
        {
            //遍历这个存放MyToggle的按钮组IEnumerable，此乃C#的一个枚举集合，一般直接用foreach遍历    
            if (t.isOn)//遍历到一个被选择的MyToggle    
            {
                switch (t.name)//根据这个MyToggle的name，我们给string sex赋予不同的值    
                {
                    case "Toggle_male":
                        male = true;
                        break;
                    case "Toggle_female":
                        male = false;
                        break;
                }
                break;//就没必要遍历下去了，后面已经可以预见到，都是没被选择的MyToggle。    
            }
        }
        return male;
    }
}
