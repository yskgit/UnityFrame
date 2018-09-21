using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleList : ScrollList<string>
{
    protected override string TransName
    {
        get { return "ui_item_rule"; }
    }

    protected override Type ItemType
    {
        get { return typeof(RuleItem); }
    }
}
