using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class RuleTbl
{
    private const string TBL_NAME = "rule";
    //public const string Id = "Id";
    public const string Content = "Content";

    public static string GetItemStr(string key, string colName)
    {
        string value = ResTableContainer.GetItemData(TBL_NAME, key, colName);
        if (string.IsNullOrEmpty(value))
        {
            Debug.Log(string.Format("character excel configuration error,GetItemStr key = {0}", key));
            return "";
        }

        //var tbl = ResTableContainer.GetTable(TBL_NAME);
        //var values = tbl.GetAllData();
        //for (int i = 0; i < values.Length; i++)
        //{
        //    Debug.Log(values[i]);
        //}

        return value;
    }
}

public class RuleUIManager : UIManager
{
    private RuleList _ruleList;

    private readonly string[] _rules = {
        @"1.游戏人数：四人局
2.麻将牌：万、筒、条、东、南、西、北、中、发、白，共136张牌 
3.基本规则：
1）可以碰、杠
2）庄家：由创建包间的玩家担任第一把庄家，闲家胡牌，庄家的下一家当庄，庄家胡牌则继续做庄。
3）流局：无人胡牌，则为流局
4）黄庄：黄庄黄杠：不计杠分
      黄庄重新开局后庄家不变
5）可一炮多响

4.特殊玩法：
1）可吃牌：勾选此规则就可以在牌局中吃牌，吃牌只能吃上家的牌。
2）带风牌：勾选此规则，牌局中就会有“东南西北”出现。
3）点炮可胡：勾选此规则才可点炮胡，否则只能自摸胡。
4）带混儿：混儿牌可代替任何牌，不能组合其他牌进行吃、碰、杠。玩家抓完牌后，庄家翻出下一张牌，此张牌的数字加一，为混儿牌。",
@"5）混儿悠：自摸胡牌时，用混儿做将，单吊任意牌胡牌
6）缺门胡：至少缺少万筒条其中一门牌才能胡牌。

5.胡牌类型和分数：
1）庄家加成：2分。庄家输赢分数均有加成 
2）基本牌型：
自摸：2分 
一条龙：2分。同一花色的牌1到9顺序相连，必须为“123”+“456”+“789”的形式，其余的牌也可组成胡牌牌型。
杠上开花：2分  
清一色：2分。胡牌时手中的牌均为有筒条万中的一种花色 
字一色：2分。胡牌时手中的牌均为东南西北中发白
十三幺：10分。牌组为“东、南、西、北、中、发、白、一筒、九筒、一万、九万、一条、九条、再加上中、发、白中的任意一样” 不得碰、杠
七对：2分 
豪华七对：4分。七对中有一副4张一样的牌
超级七对：8分。七对中有二副4张一样的牌",
        @"至尊豪华七对：16分。七对中有三副4张一样的牌
3）可选牌型：
门清：2分。没有吃、碰、杠的情况下胡牌。
碰碰胡：2分。
捉五魁：2分。四六万胡卡五万。
混儿悠：2分。自摸胡牌时，用混儿做将，单吊任意牌胡牌。
6.计分规则
1）底分1分
2）自摸胡：三家皆出分给赢家。
3）点炮一家出：点炮者出分给赢家，其他两家不出分。
4）点炮大包：只有点炮者出分，点炮者需出双倍的分数给赢家，且需要替另外两家出对应的胡牌分。
5）杠分计算：
明杠：点杠人-3分   杠牌者+3分
补杠：点碰人-3分   杠牌者+3分 
暗杠：三家-2分     杠牌者+6分
荒庄时，不计杠分，多个杠可累计"
    };

    public override string[] CacheAssets
    {
        get { return new[] { "ui_item_rule" }; }
    }

    public override void InitUI()
    {
        base.InitUI();

        _ruleList = _objs[0].GetComponent<RuleList>();
    }

    public override void InitData(object[] args)
    {
        //for (int i = 0; i < _rules.Length; i++)
        //{
        //    _rules[i] = RuleTbl.GetItemStr(i.ToString(), RuleTbl.Content);
        //}
        _ruleList.UpdateList(_rules.ToList());
    }

    public override void ReturnBackToThisWindow(bool isChangeWindow)
    {
        base.ReturnBackToThisWindow(isChangeWindow);
        if (isChangeWindow)
        {
            //            DoAnimation();
        }
    }

    public override void OnEscapeKeyDown(BaseEventData eventData)
    {
        base.OnEscapeKeyDown(eventData);
        ReturnBack();
    }

    public override void OnConfirmKeyDown(BaseEventData eventData)
    {
        base.OnConfirmKeyDown(eventData);
        ReturnBack();
    }
}
