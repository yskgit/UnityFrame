using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum E_Direct
{
    Horizontal,
    Vertical
}

public enum E_ClickKey
{
    Up,
    Down,
    Left,
    Right
}

public class PageData<T>
{
    public Transform Trans;
    public List<T> Datas;
    public int Page;
}

public class ScrollList<T> : MonoBehaviour
{
    public E_Direct Direction = E_Direct.Horizontal;

    public Transform PageTrans;

    public Ease EaseType = Ease.Flash;

    public GameObject UpSelectedObj;

    public GameObject DownSelectedObj;

    public GameObject LeftSelectedObj;

    public GameObject RightSelectedObj;

    /// <summary>
    /// 离开列表时选中的物体
    /// </summary>
    public GameObject TempGameObject
    {
        get;
        private set;
    }

    /// <summary>
    /// 列表为空的时候选中的物体
    /// </summary>
    public GameObject EmptyListSelectedObj;

    /// <summary>
    /// 显示pageItem显示时的位置
    /// </summary>
    public Vector3 CenterPosition = Vector3.zero;

    public Action<ListItemBase<T>> HandleSelected;
    public Action<ListItemBase<T>> HandleDeselected;
    public Action<T> HandleItemClick;

    /// <summary>
    /// 列表拉倒底部的回调
    /// </summary>
    public Action HandleBottom;

    /// <summary>
    /// 列表开始移动的回调
    /// </summary>
    public Action HandleMove;

    /// <summary>
    /// 列表移动完成的回调
    /// </summary>
    public Action HandleMoveComplete;

    /// <summary>
    /// 页码变化
    /// </summary>
    public Action<int> HandlePageChange;

    public int MaxPage;

    //一个list就只需要两个PageItem，在Awake的时候获取到这俩item
    private List<Transform> _pageTransforms = new List<Transform>();
    //_items的数量，也就是用于无限循环列表的item数量
    private const int ITEM_NUM = 2;

    /// <summary>
    /// 所有的数据
    /// </summary>
    protected List<T> _datas = new List<T>();

    /// <summary>
    /// 当前选中的物体
    /// </summary>
    protected ListItemBase<T> _currentSelectedItemBase;

    /// <summary>
    /// 是否正在翻页移动
    /// </summary>
    private bool _isMoving;

    /// <summary>
    /// 所有的页码数据
    /// </summary>
    List<PageData<T>> _pageDatas = new List<PageData<T>>();

    /// <summary>
    /// 当前页码的数据
    /// </summary>
    private PageData<T> _currentPageData = new PageData<T>();

    /// <summary>
    /// PageTrans 的子物体数量
    /// </summary>
    private int _pageChildCount;

    private bool _init;

    /// <summary>
    /// 绑定到页码下的子物体item的脚本
    /// </summary>
    protected virtual System.Type ItemType
    {
        get
        {
            return typeof(MonoBehaviour);
        }
    }

    protected virtual string TransName
    {
        get { return string.Empty; }
    }

    private void Awake()
    {

    }

    private void InitUi()
    {
        if (!PageTrans)
        {
            PageTrans = (ObjectCache.instance.GetCacheAsset(TransName) as GameObject).transform;
        }

        for (int i = 0; i < ITEM_NUM; i++)
        {
            _pageTransforms.Add(InstantiateItem(PageTrans.gameObject, transform));
        }

        _pageChildCount = PageTrans.childCount;
        if (_pageChildCount <= 0)
        {
            Debug.LogWarning("PageTrans 下必须有子物体!!!");
        }
    }

    private void OnEnable()
    {
        KeyEventManager.instance.HandleUpKeyDown += OnUpKeyDown;
        KeyEventManager.instance.HandleDownKeyDown += OnDownKeyDown;
        KeyEventManager.instance.HandleLeftKeyDown += OnLeftKeyDown;
        KeyEventManager.instance.HandleRightKeyDown += OnRightKeyDown;
    }

    private void OnDisable()
    {
        KeyEventManager.instance.HandleUpKeyDown -= OnUpKeyDown;
        KeyEventManager.instance.HandleDownKeyDown -= OnDownKeyDown;
        KeyEventManager.instance.HandleLeftKeyDown -= OnLeftKeyDown;
        KeyEventManager.instance.HandleRightKeyDown -= OnRightKeyDown;
    }

    private void OnDestroy()
    {
        HandleSelected = null;
        HandleDeselected = null;
        HandleItemClick = null;
        HandleBottom = null;
        HandleMove = null;
        HandleMoveComplete = null;
    }

    private Transform InstantiateItem(GameObject item, Transform parent)
    {
        Transform trans = Instantiate(item).transform;
        trans.SetParent(parent);
        trans.localPosition = CenterPosition;
        trans.localScale = Vector3.one;
        trans.name = "item_" + parent.childCount;
        SetNavigationOfThePageItem(trans);
        return trans;
    }

    private void SetNavigationOfThePageItem(Transform trans)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            var mySelectable = trans.GetChild(i).GetComponent<MySelectable>();
            if (Direction == E_Direct.Horizontal)
            {
                MySelectable left = null;
                MySelectable right = null;
                if (i > 0)
                {
                    left = trans.GetChild(i - 1).GetComponent<MySelectable>();
                }
                if (i < trans.childCount - 1)
                {
                    right = trans.GetChild(i + 1).GetComponent<MySelectable>();
                }
                Util.SetNavigation(mySelectable, null, null, left, right);
            }
            else
            {
                MySelectable up = null;
                MySelectable down = null;
                if (i > 0)
                {
                    up = trans.GetChild(i - 1).GetComponent<MySelectable>();
                }
                if (i < trans.childCount - 1)
                {
                    down = trans.GetChild(i + 1).GetComponent<MySelectable>();
                }
                Util.SetNavigation(mySelectable, up, down, null, null);
            }
        }
    }

    /// <summary>
    /// 根据索引值拿到数据
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private List<T> GetPageItemDatasByIndex(int index)
    {
        int page = GetPage(index);
        List<T> pageDatas = new List<T>();
        for (int i = page * _pageChildCount; i < (page + 1) * _pageChildCount; i++)
        {
            if (i >= _datas.Count)
            {
                break;
            }
            pageDatas.Add(_datas[i]);
        }
        return pageDatas;
    }

    private Transform GetTheOtherPageTrans(Transform trans)
    {
        int index = _pageTransforms.IndexOf(trans);
        var theOtherTrans = index == 0 ? _pageTransforms[1] : _pageTransforms[0];
        return theOtherTrans;
    }

    /// <summary>
    /// 根据索引获取page
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Transform GetPageTransByIndex(int index)
    {
        int page = GetPage(index);
        var trans = _pageTransforms[page % _pageTransforms.Count];
        return trans;
    }

    private int GetPage(int index)
    {
        int page = index / _pageChildCount;
        return page;
    }

    private int GetPage(T data)
    {
        return GetPage(_datas.IndexOf(data));
    }

    private int GetMaxPage()
    {
        if (_datas.Count <= 0)
        {
            return GetPage(0);
        }
        return GetPage(_datas.Count - 1);
    }

    private void SetCurrentPageData(PageData<T> pageData)
    {
        _currentPageData = pageData;
        InitPageItemsData(_currentPageData);
        if (_currentPageData != null)
        {
            DoPageChange(_currentPageData.Page);
        }
    }

    private void DoPageChange(int page)
    {
        if (HandlePageChange != null)
        {
            HandlePageChange(page);
        }
    }

    /// <summary>
    /// 翻页时设置当前pagedata信息，ahead为true为顺序翻页
    /// </summary>
    /// <param name="ahead"></param>
    private void SetCurrentPageData(bool ahead)
    {
        //顺序翻页
        if (ahead)
        {
            if (_currentPageData.Page < _pageDatas.Count - 1)
            {
                _currentPageData = _pageDatas[_currentPageData.Page + 1];
                InitPageItemsData(_currentPageData);
                DoPageChange(_currentPageData.Page);
            }
            else
            {
                Debug.Log(string.Format("SetCurrentPageData error!!!page {0}是最后一页，不能顺序执行翻页"));
            }
        }
        //逆序翻页
        else
        {
            if (_currentPageData.Page > 0)
            {
                _currentPageData = _pageDatas[_currentPageData.Page - 1];
                InitPageItemsData(_currentPageData);
                DoPageChange(_currentPageData.Page);
            }
            else
            {
                Debug.Log(string.Format("SetCurrentPageData error!!!page {0}是第一页，不能逆序执行翻页"));
            }
        }
    }

    /// <summary>
    /// 更新整个列表，index为更新后，选中的物体index，selected为是否选中第index的物体
    /// </summary>
    /// <param name="datas"></param>
    /// <param name="index"></param>
    /// <param name="selected"></param>
    public void UpdateList(List<T> datas, int index = 0, bool selected = true)
    {
        if (datas == null)
        {
            Debug.Log("UpdateList error,datas should not be null!!!");
            return;
        }

        if (!_init)
        {
            _init = true;
            InitUi();
        }

        _datas = datas;
        MaxPage = GetMaxPage();
        InitPageDatas(datas);
        SetCurrentPageData(GetPageData(index));
        ResetPositionByIndex(index);

        if (datas.Count <= 0)
        {
            TempGameObject = null;
            if (HandleSelected != null)
            {
                HandleSelected(null);
            }
            if (EmptyListSelectedObj)
            {
                UIController.instance.CurrentSelectedObj = EmptyListSelectedObj;
            }
        }
        else if (selected)
        {
            SetCurrentSelectedItem(index);
        }
        else
        {
            TempGameObject = _currentPageData.Trans.GetChild(0).gameObject;
        }
    }

    private void InitPageDatas(List<T> datas)
    {
        int index = 0;
        _pageDatas.Clear();
        while (index < datas.Count)
        {
            PageData<T> pageData = new PageData<T>();
            pageData.Trans = GetPageTransByIndex(index);
            pageData.Datas = GetPageItemDatasByIndex(index);
            pageData.Page = GetPage(index);
            _pageDatas.Add(pageData);
            index += _pageChildCount;
        }
    }

    private PageData<T> GetPageData(T data)
    {
        int index = _datas.IndexOf(data);
        return GetPageData(index);
    }

    private PageData<T> GetPageData(int index)
    {
        int page = GetPage(index);
        PageData<T> pageData = null;
        _pageDatas.ForEach(data =>
        {
            if (data.Page == page)
            {
                pageData = data;
            }
        });
        return pageData;
    }

    private void ResetPositionByIndex(int index)
    {
        var trans = GetPageTransByIndex(index);
        trans.localPosition = CenterPosition;
        var otherTrans = GetTheOtherPageTrans(trans);
        otherTrans.localPosition = new Vector3(8888f, 8888f);
    }

    private void SetCurrentSelectedItem(int index)
    {
        int selectedIndex = index % _pageChildCount;
        SetCurrentSelectedItem(GetPageTransByIndex(index).GetChild(selectedIndex).gameObject);
    }

    public void ReturnBack()
    {
        if (TempGameObject)
        {
            SetCurrentSelectedItem(TempGameObject);
        }
    }

    private void SetCurrentSelectedItem(GameObject obj)
    {
        _currentSelectedItemBase = obj.GetComponent<ListItemBase<T>>();
        UIController.instance.CurrentSelectedObj = obj;
    }

    private void InitPageItemsData(PageData<T> pageData)
    {
        //_datas为空的情况
        if (pageData == null)
        {
            for (int i = 0; i < _pageTransforms.Count; i++)
            {
                for (int j = 0; j < _pageTransforms[i].childCount; j++)
                {
                    _pageTransforms[i].GetChild(j).gameObject.SetActive(false);
                }
            }
            return;
        }
        for (int i = 0; i < pageData.Trans.childCount; i++)
        {
            pageData.Trans.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < pageData.Datas.Count; i++)
        {
            pageData.Trans.GetChild(i).gameObject.SetActive(true);
            InitItemData(pageData.Datas[i], pageData.Trans.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 初始化item数据，子类可重写这个方法以获取到item的GameObject，从而获取到ListItemBase组件自定义其他事件
    /// </summary>
    /// <param name="val"></param>
    /// <param name="obj"></param>
    protected virtual void InitItemData(T val, GameObject obj)
    {
        ListItemBase<T> itemType = obj.GetComponent(ItemType) as ListItemBase<T>;
        if (!itemType)
        {
            itemType = obj.gameObject.AddComponent(ItemType) as ListItemBase<T>;
        }
        if (!itemType)
        {
            return;
        }
        itemType.InitData(val);

        var mySelectable = itemType.GetComponent<MySelectable>();
        mySelectable.HandleSelected += (s) =>
        {
            StartCoroutine(SetCurrentSelectedItemBaseAfterSelecting(itemType));
            //            CheckBottom(val);
        };
        mySelectable.HandleDeselected += (s) =>
        {
            TempGameObject = itemType.gameObject;
            if (HandleDeselected != null)
            {
                HandleDeselected(itemType);
            }
            if (itemType.HandleDeselect != null)
            {
                itemType.HandleDeselect.Invoke();
            }
        };
        mySelectable.HandleClick += (s) =>
        {
            if (!_isMoving && HandleItemClick != null)
            {
                HandleItemClick(val);
            }
            if (itemType.HandleClick != null)
            {
                itemType.HandleClick.Invoke();
            }
        };
    }

    IEnumerator SetCurrentSelectedItemBaseAfterSelecting(ListItemBase<T> item)
    {
        yield return new WaitForFixedUpdate();
        _currentSelectedItemBase = item;
        if (HandleSelected != null)
        {
            HandleSelected(item);
            if (item.HandleSelect != null)
            {
                item.HandleSelect.Invoke();
            }
        }
    }

    private void CheckBottom(T val)
    {
        if (_datas.IndexOf(val) == _datas.Count - 1)
        {
            Debug.Log("到达底部！");
            if (HandleBottom != null)
            {
                HandleBottom();
            }
        }
    }

    /// <summary>
    /// 更新单个item的数据
    /// </summary>
    /// <param name="data"></param>
    public void UpdateData(T data)
    {
        //判断是否更新UI。同页更新UI，不同页则不只更新数据
        int page = GetPage(data);
        if (page != _currentPageData.Page || page == -1)
        {
            return;
        }
        int rank = _datas.IndexOf(data) % _pageChildCount;
        InitItemData(data, _currentPageData.Trans.GetChild(rank).gameObject);
    }

    /// <summary>
    /// 检查选中的PageItem下的子item是否是第0个子物体
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool CheckIsTheFirstItemOfThePage(ListItemBase<T> item)
    {
        var datas = _currentPageData.Datas;
        return item.Data.Equals(datas[0]);
    }

    /// <summary>
    /// 检查选中的PageItem下的子item是否是List的第0个数据
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool CheckIsTheFirstItemOfTheList(ListItemBase<T> item)
    {
        var datas = _currentPageData.Datas;
        return item.Data.Equals(datas[0]);
    }

    /// <summary>
    /// 检查选中的PageItem下的子item是否是List的最后一个数据
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool CheckIsTheLastItemOfTheList(ListItemBase<T> item)
    {
        var datas = _currentPageData.Datas;
        return item.Data.Equals(datas[datas.Count - 1]);
    }

    /// <summary>
    /// 检查选中的PageItem下的子item是否是最后一个子物体
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool CheckIsTheLastItemOfThePage(ListItemBase<T> item)
    {
        var datas = _currentPageData.Datas;
        return item.Data.Equals(datas[datas.Count - 1]);
    }

    private void OnUpKeyDown()
    {
        OnKeyDown(E_ClickKey.Up);
    }

    private void OnDownKeyDown()
    {
        OnKeyDown(E_ClickKey.Down);
    }

    private void OnLeftKeyDown()
    {
        OnKeyDown(E_ClickKey.Left);
    }

    private void OnRightKeyDown()
    {
        OnKeyDown(E_ClickKey.Right);
    }

    private void OnKeyDown(E_ClickKey clickKey)
    {
        //焦点出了列表
        if (!_currentSelectedItemBase || _currentSelectedItemBase.gameObject != UIController.instance.CurrentSelectedObj)
        {
            return;
        }

        //如果正在移动，return!!!
        if (_isMoving)
        {
            return;
        }
        //1、检查是否是执行导航操作
        if (CheckNavigation(clickKey))
        {
            DoNavigation(clickKey);
            return;
        }
        //2、检查是否可以移动
        if (CheckMove(clickKey))
        {
            //移动界面
            DoPageTransMove(clickKey);
        }
    }

    /// <summary>
    /// 执行导航
    /// </summary>
    /// <param name="clickKey"></param>
    private void DoNavigation(E_ClickKey clickKey)
    {
        switch (clickKey)
        {
            case E_ClickKey.Up:
                if (UpSelectedObj)
                {
                    UIController.instance.CurrentSelectedObj = UpSelectedObj;
                }
                break;
            case E_ClickKey.Down:
                if (DownSelectedObj)
                {
                    UIController.instance.CurrentSelectedObj = DownSelectedObj;
                }
                break;
            case E_ClickKey.Left:
                if (LeftSelectedObj)
                {
                    UIController.instance.CurrentSelectedObj = LeftSelectedObj;
                }
                break;
            case E_ClickKey.Right:
                if (RightSelectedObj)
                {
                    UIController.instance.CurrentSelectedObj = RightSelectedObj;
                }
                break;
        }
    }

    private void LoadNewPageData(E_ClickKey clickKey)
    {
        //逆序翻页
        if (clickKey == E_ClickKey.Up || clickKey == E_ClickKey.Left)
        {
            SetCurrentPageData(false);
        }
        //顺序翻页
        else if (clickKey == E_ClickKey.Down || clickKey == E_ClickKey.Right)
        {
            SetCurrentPageData(true);
        }
    }

    /// <summary>
    /// 向下或者向右翻页。适用于外部主动调用
    /// </summary>
    public void DoPageTransMoveForward()
    {
        if (CheckMove(Direction == E_Direct.Horizontal ? E_ClickKey.Right : E_ClickKey.Down))
        {
            //移动界面
            DoPageTransMove(Direction == E_Direct.Horizontal ? E_ClickKey.Right : E_ClickKey.Down);
        }
    }

    /// <summary>
    /// 向上或者向左翻页。适用于外部主动调用
    /// </summary>
    public void DoPageTransMoveBack()
    {
        if (CheckMove(Direction == E_Direct.Horizontal ? E_ClickKey.Left : E_ClickKey.Up))
        {
            //移动界面
            DoPageTransMove(Direction == E_Direct.Horizontal ? E_ClickKey.Left : E_ClickKey.Up);
        }
    }

    /// <summary>
    /// 执行翻页动画
    /// </summary>
    /// <param name="clickKey"></param>
    private void DoPageTransMove(E_ClickKey clickKey)
    {
        //加载新界面数据
        LoadNewPageData(clickKey);

        Transform currentPageItem = GetTheOtherPageTrans(_currentPageData.Trans);
        Transform theOtherPageItem = _currentPageData.Trans;

        Vector3 theOtherStartPos = default(Vector3);
        Vector3 theOtherTargetPos = CenterPosition;

        Vector3 curStartPos = CenterPosition;
        Vector3 curTargetPos = default(Vector3);

        Vector2 pageSize = PageTrans.GetComponent<RectTransform>().sizeDelta;
        switch (clickKey)
        {
            case E_ClickKey.Up:
                theOtherStartPos = new Vector3(0f, pageSize.y);
                curTargetPos = new Vector3(0f, -pageSize.y);
                break;
            case E_ClickKey.Down:
                theOtherStartPos = new Vector3(0f, -pageSize.y);
                curTargetPos = new Vector3(0f, pageSize.y);
                break;
            case E_ClickKey.Left:
                theOtherStartPos = new Vector3(-pageSize.x, 0f);
                curTargetPos = new Vector3(pageSize.x, 0f);
                break;
            case E_ClickKey.Right:
                theOtherStartPos = new Vector3(pageSize.x, 0f);
                curTargetPos = new Vector3(-pageSize.x, 0f);
                break;
        }

        currentPageItem.localPosition = curStartPos;
        theOtherPageItem.localPosition = theOtherStartPos;

        _isMoving = true;
        if (HandleMove != null)
        {
            HandleMove();
        }
        EnablePageItemBtn(currentPageItem, false);
        //        Debug.Log(currentPageItem.localPosition + "  " + curTargetPos);
        //        Debug.Log(theOtherPageItem.localPosition + "  " + theOtherTargetPos);
        DoTweenHelper.DoLocalMove(currentPageItem, curTargetPos, 0.5f, EaseType, () =>
        {
            EnablePageItemBtn(currentPageItem, true);
        });
        DoTweenHelper.DoLocalMove(theOtherPageItem, theOtherTargetPos, 0.5f, EaseType, () =>
        {
            _isMoving = false;
            if (HandleMoveComplete != null)
            {
                HandleMoveComplete();
            }
            if (clickKey == E_ClickKey.Up || clickKey == E_ClickKey.Left)
            {
                for (int i = theOtherPageItem.childCount - 1; i >= 0; i--)
                {
                    var item = theOtherPageItem.GetChild(i).gameObject;
                    if (item.activeSelf)
                    {
                        UIController.instance.CurrentSelectedObj = item;
                        break;
                    }
                }
            }
            else if (clickKey == E_ClickKey.Down || clickKey == E_ClickKey.Right)
            {
                UIController.instance.CurrentSelectedObj = theOtherPageItem.GetChild(0).gameObject;
            }
        });
    }

    private void EnablePageItemBtn(Transform trans, bool isTrue)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            var mySelectable = trans.GetChild(i).GetComponent<MySelectable>();
            if (mySelectable)
            {
                mySelectable.enabled = isTrue;
            }
        }
    }

    /// <summary>
    /// 检查是否执行导航
    /// </summary>
    /// <param name="clickKey"></param>
    /// <returns></returns>
    private bool CheckNavigation(E_ClickKey clickKey)
    {
        if (_isMoving)
        {
            return false;
        }
        int firstPage = 0;
        if (clickKey == E_ClickKey.Up)
        {
            if (Direction == E_Direct.Vertical && UpSelectedObj && _currentPageData.Page == firstPage && CheckIsTheFirstItemOfTheList(_currentSelectedItemBase))
            {
                return true;
            }

            if (Direction == E_Direct.Horizontal && UpSelectedObj)
            {
                return true;
            }
        }
        else if (clickKey == E_ClickKey.Down)
        {
            if (DownSelectedObj && Direction == E_Direct.Vertical && _currentPageData.Page == MaxPage && CheckIsTheLastItemOfTheList(_currentSelectedItemBase))
            {
                return true;
            }

            if (DownSelectedObj && Direction == E_Direct.Horizontal)
            {
                return true;
            }
        }
        else if (clickKey == E_ClickKey.Left)
        {
            if (LeftSelectedObj && Direction == E_Direct.Horizontal && _currentPageData.Page == firstPage && CheckIsTheFirstItemOfTheList(_currentSelectedItemBase))
            {
                return true;
            }

            if (LeftSelectedObj && Direction == E_Direct.Vertical)
            {
                return true;
            }
        }
        else if (clickKey == E_ClickKey.Right)
        {
            if (RightSelectedObj && Direction == E_Direct.Horizontal && _currentPageData.Page == MaxPage && CheckIsTheLastItemOfTheList(_currentSelectedItemBase))
            {
                return true;
            }

            if (RightSelectedObj && Direction == E_Direct.Vertical)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查按下按键后，列表是否可以移动
    /// </summary>
    /// <param name="clickKey"></param>
    /// <returns></returns>
    private bool CheckMove(E_ClickKey clickKey)
    {
        //        Debug.Log("clickKey = " + clickKey);
        if (_isMoving)
        {
            return false;
        }

        //1、是否到达底部
        CheckBottom(clickKey);

        //2、移动方向是否正确
        //点击上键或者左键
        if ((clickKey == E_ClickKey.Up || clickKey == E_ClickKey.Down) && Direction == E_Direct.Horizontal)
        {
            //如果方向不对，则不能移动
            return false;
        }
        //点击下键或者右键
        if ((clickKey == E_ClickKey.Left || clickKey == E_ClickKey.Right) && Direction == E_Direct.Vertical)
        {
            return false;
        }

        //3、是否满足当页可翻页条件
        if (clickKey == E_ClickKey.Up || clickKey == E_ClickKey.Left)
        {
            //1、如果当前的page已经是最上的一页，则不能移动
            if (_currentPageData.Page == 0)
            {
                return false;
            }
            //2、如果选中的item不是page下的第一个item，则不能移动
            if (!CheckIsTheFirstItemOfThePage(_currentSelectedItemBase))
            {
                return false;
            }
        }
        else if (clickKey == E_ClickKey.Down || clickKey == E_ClickKey.Right)
        {
            bool isLastItem = CheckIsTheLastItemOfThePage(_currentSelectedItemBase);
            //1、如果当前的page已经是最上的一页，则不能移动
            if (_currentPageData.Page == MaxPage)
            {
                return false;
            }
            //2、如果选中的item不是page下的最后一个item，则不能移动
            if (!isLastItem)
            {
                return false;
            }
        }

        return true;
    }

    private void CheckBottom(E_ClickKey clickKey)
    {
        if (clickKey == E_ClickKey.Down || clickKey == E_ClickKey.Right)
        {
            bool isLastItem = CheckIsTheLastItemOfThePage(_currentSelectedItemBase);
            int currentPage = _currentPageData.Page;
            if (currentPage == MaxPage && isLastItem)
            {
                Debug.Log("到达底部！");
                if (HandleBottom != null)
                {
                    HandleBottom();
                }
            }
        }
    }
}