using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiseSkillTbl
{
    private const string TBL_NAME = "character";
    public const string LV = "LV";
    public const string AGI = "AGI";
    public const string STR = "STR";
    public const string DEX = "DEX";
    public const string NAME = "NAME";
    public const string INT_HAVE_BUG = "INT_Have_Bug";

    public static string GetItemStr(string key,string colName)
    {
        string value = ResTableContainer.GetItemData(TBL_NAME, key, colName);
        if (string.IsNullOrEmpty(value))
        {
            Debug.Log(string.Format("character excel configuration error,GetItemStr key = {0}", key));
            return "";
        }

        var tbl = ResTableContainer.GetTable(TBL_NAME);
        var values = tbl.GetAllData();
        for (int i = 0; i < values.Length; i++)
        {
            Debug.Log(values[i]);
        }

        return value;
    }
}

public class TestReadExcel : MonoBehaviour {


    // Use this for initialization
    void Start ()
    {
        var aig = WiseSkillTbl.GetItemStr("5", WiseSkillTbl.AGI);
        Debug.Log("aig = " + aig);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
