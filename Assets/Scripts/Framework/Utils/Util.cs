using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public static class Util
{
    /// <summary>
    /// 场景内触发为所有Canvas重新排序的sortingOrder值。用于控制层级最大值，防止层级一直往上递增。
    /// </summary>
    private static int _sortingOrderThreshold = 4;

    /// <summary>
    /// 场景内触发为所有Canvas重新排序后 SORTING_ORDER_THRESHHOLD 需要递增的值
    /// </summary>
    private const int SORTING_ORDER_ADD = 20;

    /// <summary>
    /// 场景内SortingOrder的最大层级
    /// </summary>
    private const int SORTING_ORDER_MAX = 100;

    /// <summary>
    /// json字符串转换为对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// 激活或者禁用窗体下所有交互组件的交互性。如 isTrue 为 false 时，window 下所有的 mybutton 都不能触发点击事件
    /// </summary>
    /// <param name="window"></param>
    /// <param name="isTrue"></param>
    public static void EnableWindowSelectable(GameObject window, bool isTrue)
    {
        var mySelectables = window.GetComponentsInChildren<MySelectable>();
        for (int i = 0; i < mySelectables.Length; i++)
        {
            mySelectables[i].enabled = isTrue;
        }
    }

    /// <summary>
    /// 设置导航模式
    /// </summary>
    /// <param name="mySelectable"></param>
    /// <param name="mode"></param>
    public static void SetNavigationMode(MySelectable mySelectable, Navigation.Mode mode)
    {
        if (!mySelectable)
        {
            return;
        }
        var navigation = mySelectable.navigation;
        navigation.mode = mode;
        mySelectable.navigation = navigation;
    }

    /// <summary>
    /// 设置导航
    /// </summary>
    /// <param name="mySelectable"></param>
    /// <param name="up"></param>
    /// <param name="down"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public static void SetNavigation(MySelectable mySelectable, MySelectable up, MySelectable down, MySelectable left, MySelectable right)
    {
        if (!mySelectable)
        {
            return;
        }
        var navigation = mySelectable.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = up;
        navigation.selectOnDown = down;
        navigation.selectOnLeft = left;
        navigation.selectOnRight = right;
        mySelectable.navigation = navigation;
    }

    /// <summary>
    /// 设置 up 键导航
    /// </summary>
    /// <param name="mySelectable"></param>
    /// <param name="up"></param>
    public static void SetUpNavigation(MySelectable mySelectable, MySelectable up)
    {
        if (!mySelectable)
        {
            return;
        }
        var navigation = mySelectable.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = up;
        mySelectable.navigation = navigation;
    }

    /// <summary>
    /// 设置 down 键导航
    /// </summary>
    /// <param name="mySelectable"></param>
    /// <param name="down"></param>
    public static void SetDownNavigation(MySelectable mySelectable, MySelectable down)
    {
        if (!mySelectable)
        {
            return;
        }
        var navigation = mySelectable.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnDown = down;
        mySelectable.navigation = navigation;
    }

    /// <summary>
    /// 设置 left 键导航
    /// </summary>
    /// <param name="mySelectable"></param>
    /// <param name="left"></param>
    public static void SetLeftNavigation(MySelectable mySelectable, MySelectable left)
    {
        if (!mySelectable)
        {
            return;
        }
        var navigation = mySelectable.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnLeft = left;
        mySelectable.navigation = navigation;
    }

    /// <summary>
    /// 设置 right 键导航
    /// </summary>
    /// <param name="mySelectable"></param>
    /// <param name="right"></param>
    public static void SetRightNavigation(MySelectable mySelectable, MySelectable right)
    {
        if (!mySelectable)
        {
            return;
        }
        var navigation = mySelectable.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnRight = right;
        mySelectable.navigation = navigation;
    }

    /// <summary>
    /// 获取字符串的 MD5 加密数据
    /// </summary>
    /// <param name="encryptStr"></param>
    /// <returns></returns>
    public static string GetMd5EncryptStr(string encryptStr)
    {
        byte[] result = Encoding.Default.GetBytes(encryptStr);
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] output = md5.ComputeHash(result);
        string resultStr = BitConverter.ToString(output).Replace("-", "");
        return resultStr;
    }

    /// <summary>
    /// 初始化 obj 的三维信息
    /// </summary>
    /// <param name="obj"></param>
    public static void InitTrans(GameObject obj)
    {
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 打乱列表顺序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lis"></param>
    /// <returns></returns>
    public static List<T> DisorgnizeListOrder<T>(List<T> lis)
    {
        if (lis == null)
        {
            return null;
        }
        List<T> resultList = new List<T>();

        for (int i = lis.Count - 1; i >= 0; i--)
        {
            T value = lis[UnityEngine.Random.Range(0, lis.Count)];
            resultList.Add(value);
            lis.Remove(value);
        }

        return resultList;
    }

    /// <summary>
    /// 向上遍父物体，得到第一个 canvas 的 SortingOrder。获取失败返回-1.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static int GetFirstCanvasParentSortingOrder(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        if (!parent)
        {
            return -1;
        }
        var canvas = parent.GetComponent<Canvas>();
        while (canvas == null)
        {
            parent = parent.parent;
            if (parent == null)
            {
                break;
            }
            canvas = parent.GetComponent<Canvas>();
            if (canvas != null)
            {
                break;
            }
        }
        if (canvas == null)
        {
            return -1;
        }

        return canvas.sortingOrder;
    }

    /// <summary>
    /// 根据 RGB 值获取 Color
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Color GetColorByRGB(float r, float g, float b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }

    /// <summary>
    /// 获取当前场景最大的 SortingOrder。如果忽略常驻窗口，则遍历到常驻窗口时，不比较常驻窗口的层级。也就是说计算出来的最大层级与常驻窗口无关
    /// </summary>
    /// <returns></returns>
    /// <param name="isIgnoreSingletonWindow">是否忽略常驻窗口</param>
    private static int GetMaximumSortingOrder(bool isIgnoreSingletonWindow = true)
    {
        Canvas[] canvas = Resources.FindObjectsOfTypeAll<Canvas>();
        int maximumSortOrder = int.MinValue;
        for (int i = 0; i < canvas.Length; i++)
        {
            //如果忽略常驻窗口，则遍历到常驻窗口时，不比较常驻窗口的层级
            if (isIgnoreSingletonWindow && CheckIsSingletonWindow(canvas[i].gameObject))
            {
                continue;
            }
            if (canvas[i].sortingOrder > maximumSortOrder)
            {
                maximumSortOrder = canvas[i].sortingOrder;
            }
        }

        return maximumSortOrder;
    }

    /// <summary>
    /// 检测 obj 是否是场景常驻窗口。即是否挂载继承自SingletonWindow的组件。
    /// </summary>
    /// <returns></returns>
    private static bool CheckIsSingletonWindow(GameObject obj)
    {
        return UIController.instance.GetSingletonWindows().Contains(obj);
    }

    /// <summary>
    /// 设置 window 显示层级。有两种情况：
    /// 1、window是驻窗口，则设置层级 = 所有窗口的层级中最大层级 + order
    /// 2、window不是常驻窗口，则设置层级 = 所有窗口（除开所有常驻窗口）的层级中最大层级 + order
    /// </summary>
    /// <param name="window"></param>
    /// <param name="order"></param>
    public static void SetMaximumSortingOrder(GameObject window, int order = 1)
    {
        if (!window)
        {
            LogUtil.LogWarning("SetMaximumSortingOrder 失败，window 不能为 null！！！");
            return;
        }
        if (!window.GetComponent<Canvas>())
        {
            window.AddComponent<Canvas>();
            LogUtil.LogWarning(string.Format("window:{0}没有Canvas组件，动态添加Canvas组件！！！", window.name));
        }
        window.GetComponent<Canvas>().overrideSorting = true;
        //1、window是驻窗口，则设置层级 = 所有窗口的层级中最大层级 + order
        //2、window不是常驻窗口，则设置层级 = 所有窗口（除开所有常驻窗口）的层级中最大层级 + order
        int targetOrder = GetMaximumSortingOrder(!CheckIsSingletonWindow(window)) + order;
        if (targetOrder >= SORTING_ORDER_MAX)
        {
            //这里也可以做其他处理
            LogUtil.LogWarning(string.Format("达到最大层级:{0}了，需要减少Canvas的使用！", SORTING_ORDER_MAX));
        }
        if (targetOrder >= _sortingOrderThreshold)
        {
            _sortingOrderThreshold += SORTING_ORDER_ADD;
            ResetAllCanvasSortingOrder();
            LogUtil.Log(string.Format("重新设置_sortingOrderThreshold:{0}", _sortingOrderThreshold));
            targetOrder = GetMaximumSortingOrder(!CheckIsSingletonWindow(window)) + order;
        }
        window.GetComponent<Canvas>().sortingOrder = targetOrder;
    }

    /// <summary>
    /// 重新设置所有Canvas层级
    /// </summary>
    public static void ResetAllCanvasSortingOrder()
    {
        LogUtil.Log("重新设置所有Canvas层级！");
        Canvas[] canvas = Resources.FindObjectsOfTypeAll<Canvas>();
        //排序并设置层级
        for (int i = 0; i < canvas.Length; i++)
        {
            for (int j = i; j < canvas.Length; j++)
            {
                if (canvas[i].sortingOrder > canvas[j].sortingOrder)
                {
                    Canvas temp = canvas[i];
                    canvas[i] = canvas[j];
                    canvas[j] = temp;
                }
            }

            if (i == 0)
            {
                canvas[i].sortingOrder = 0;
            }
            else
            {
                canvas[i].sortingOrder = canvas[i - 1].sortingOrder + 1;
            }

            LogUtil.Log(string.Format("gameobject:{0},sortingorder:{1}", canvas[i].gameObject.name, canvas[i].sortingOrder));
        }
    }

    /// <summary>
    /// 创建空物体，物体名字修改为componentName，并挂载T类型的组件。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="componentName"></param>
    public static T CreateGameObject<T>(string componentName = "") where T : MonoBehaviour
    {
        componentName = string.IsNullOrEmpty(componentName) ? typeof(T).Name : componentName;
        var co = new GameObject(componentName).AddComponent<T>();
        if (!co)
        {
            LogUtil.LogError(string.Format("Do not have component :{0}!!!", componentName));
        }

        return co;
    }

    /// <summary>
    /// 在root根节点下创建单例窗口，物体名字修改为componentName，并挂载T类型的组件。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prefabName"></param>
    public static T CreateSingletonWindow<T>(string prefabName) where T : SingletonWindow<T>
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            LogUtil.LogError("prefabName can not be empty or null!!!");
        }
        var obj = ObjectCache.instance.LoadResource<GameObject>(prefabName);
        if (!obj)
        {
            LogUtil.LogError(string.Format("Do not have resource :{0}!!!", prefabName));
        }

        obj = UnityEngine.Object.Instantiate(obj);
        var component = obj.AddComponent<T>();
        obj.name = prefabName;
        obj.transform.SetParent(GameManager.instance.RootTrans);
        obj.transform.localScale = Vector3.one;
        obj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        obj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        obj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        obj.SetActive(false);

        return component;
    }
}
