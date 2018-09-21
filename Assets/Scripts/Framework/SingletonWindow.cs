using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonWindow<T> : SingletonBehaviour<T> where T : SingletonBehaviour<T>
{
    /// <summary>
    /// 界面是否已经显示
    /// </summary>
    /// <returns></returns>
    public bool IsShow()
    {
        return gameObject.activeInHierarchy;
    }

    /// <summary>
    /// 显示界面与否
    /// </summary>
    /// <param name="isTrue"></param>
    private void ShowSelf(bool isTrue)
    {
        gameObject.SetActive(isTrue);
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    public virtual void Show()
    {
        ShowSelf(true);
        Util.SetMaximumSortingOrder(gameObject);
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public virtual void Close()
    {
        ShowSelf(false);
        if (UIController.instance.CurrentUIManager)
        {
            UIController.instance.CurrentUIManager.ReturnBackToThisWindow(false);
        }
    }
}
