using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 添加屏幕适配物体选项
/// </summary>
public class ScreenAdaption {

    [MenuItem("GameObject/Create Screen-Adaption Object/All", false, -1)]
    public static void CreatePosObj()
    {
        CreateTopLeftPosObj();
        CreateTopCenterPosObj();
        CreateTopRightPosObj();
        CreateCenterLeftPosObj();
        CreateCenterPosObj();
        CreateCenterRightPosObj();
        CreateBottomLeftPosObj();
        CreateBottomCenterPosObj();
        CreateBottomRightPosObj();
    }

    private static void InitPosObj(string posName, Vector3 localPos)
    {
        GameObject obj = new GameObject();
        obj.AddComponent<RectTransform>();
        obj.name = posName;
        obj.transform.SetParent(Selection.activeGameObject.transform);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = localPos;
        obj.layer = LayerMask.NameToLayer("UI");
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Top Left", false, 0)]
    private static void CreateTopLeftPosObj()
    {
        InitPosObj("top_left", new Vector3(-640f, 360f, 0));
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Top Center", false, 1)]
    private static void CreateTopCenterPosObj()
    {
        InitPosObj("top_center", new Vector3(0f, 360f, 0));
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Top Right", false, 2)]
    private static void CreateTopRightPosObj()
    {
        InitPosObj("top_right", new Vector3(640f, 360f, 0));
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Center Left", false, 3)]
    private static void CreateCenterLeftPosObj()
    {
        InitPosObj("center_left", new Vector3(-640f, 0f, 0));
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Center", false, 4)]
    private static void CreateCenterPosObj()
    {
        InitPosObj("center", new Vector3(0f, 0f, 0));
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Center Right", false, 5)]
    private static void CreateCenterRightPosObj()
    {
        InitPosObj("center_right", new Vector3(640f, 0f, 0));
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Bottom Left", false, 6)]
    private static void CreateBottomLeftPosObj()
    {
        InitPosObj("bottom_left", new Vector3(-640f, -360f, 0));
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Bottom Center", false, 7)]
    private static void CreateBottomCenterPosObj()
    {
        InitPosObj("bottom_center", new Vector3(0f, -360f, 0));
    }

    [MenuItem("GameObject/Create Screen-Adaption Object/Bottom Right", false, 8)]
    private static void CreateBottomRightPosObj()
    {
        InitPosObj("bottom_right", new Vector3(640f, -360f, 0));
    }

}
