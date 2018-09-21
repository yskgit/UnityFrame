using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuleItem : ListItemBase<string> {

    public override void InitData(string data)
    {
        base.InitData(data);

        GetComponent<Text>().text = data;
    }
}
