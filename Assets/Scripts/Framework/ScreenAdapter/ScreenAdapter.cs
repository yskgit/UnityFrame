using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_Pos
{
    Top_Left,//上左
    Top_Center,//上中
    Top_Right,//上右
    Center_Left,//中左
    Center_Center,//中
    Center_Right,//中右
    Bottom_Left,//下左
    Bottom_Center,//下中
    Bottom_Right,//下右
}

public class ScreenAdapter : MonoBehaviour
{
    public E_Pos Pos = E_Pos.Center_Center;

    private bool _haveSetPos;

    // Use this for initialization
    void Start()
    {

    }

    private void OnEnable()
    {
        //放在“OnEnable”里调用时为了防止当前物体在初始化出来时active为false不触发Awake函数的问题
        SetPos();
    }

    private void SetPos()
    {
        if (_haveSetPos)
        {
            return;
        }

        //肯定是正数
        float deltaWidth = (GameManager.instance.GetScreenWidth() - GameManager.instance.ScreenWidth) / 2f;
        float deltaHeight = (GameManager.instance.GetScreenHeight() - GameManager.instance.ScreenHeight) / 2f;
        //当前物体原始的位置
        float originX = transform.localPosition.x;
        float originY = transform.localPosition.y;

        //最终得到的当前物体的坐标位置
        float x = originX, y = originY;
        switch (Pos)
        {
            case E_Pos.Top_Left:
                x = originX - deltaWidth;
                y = originY + deltaHeight;
                break;
            case E_Pos.Top_Center:
                y = originY + deltaHeight;
                break;
            case E_Pos.Top_Right:
                x = originX + deltaWidth;
                y = originY + deltaHeight;
                break;
            case E_Pos.Center_Left:
                x = originX - deltaWidth;
                break;
            case E_Pos.Center_Center:
                break;
            case E_Pos.Center_Right:
                x = originX + deltaWidth;
                break;
            case E_Pos.Bottom_Left:
                x = originX - deltaWidth;
                y = originY - deltaHeight;
                break;
            case E_Pos.Bottom_Center:
                y = originY - deltaHeight;
                break;
            case E_Pos.Bottom_Right:
                x = originX + deltaWidth;
                y = originY - deltaHeight;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        transform.localPosition = new Vector3(x, y, 0f);

        _haveSetPos = true;
    }
}
