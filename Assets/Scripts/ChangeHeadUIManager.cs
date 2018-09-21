using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeHeadUIManager : UIManager
{
    private GameObject _headsObj;

    private MyButton _closeBtn;

    public override string[] CacheAssets
    {
        get { return new[] { "ui_item_head" }; }
    }

    public override void InitUI()
    {
        base.InitUI();

        _closeBtn = _btns[0];
        _closeBtn.onClick.AddListener(OnCloseBtnClick);

        _headsObj = _objs[0];
    }

    public override void InitData(object[] args)
    {
        base.InitData(args);

        InitHeadsInfo();

        if (_headsObj.transform.childCount > 0)
        {
            CurrentSelectedObj = _headsObj.transform.GetChild(0).gameObject;
        }
        else
        {
            Debug.LogWarning("_headsObj childCount should not be zero!!!");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        KeyEventManager.instance.HandleSelectObj += OnSelectObj;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        KeyEventManager.instance.HandleSelectObj -= OnSelectObj;
    }

    private void OnSelectObj(GameObject obj)
    {
        if (obj != _closeBtn.gameObject)
        {
            Util.SetDownNavigation(_closeBtn, obj.GetComponent<MyButton>());
            Util.SetLeftNavigation(_closeBtn, obj.GetComponent<MyButton>());
        }
    }

    public override void OnEscapeKeyDown(BaseEventData eventData)
    {
        base.OnEscapeKeyDown(eventData);
        ReturnBack();
    }

    private void InitHeadsInfo()
    {
        for (int i = 1; i <= 12; i++)
        {

            var item = ObjectCache.instance.GetCacheAsset("ui_item_head") as GameObject;
            if (!item)
            {
                continue;
            }
            item.transform.SetParent(_headsObj.transform);
            item.transform.localScale = Vector3.one;

            int portrait = i;
            GameObject use = item.transform.Find("Image_use").gameObject;
            use.SetActive(portrait == int.Parse(MemoryHelper.GetPortrait()));
            Image head = item.GetComponent<Image>();
            AtlasHelper.LoadHeadSprite(true, portrait, head);

            item.GetComponent<MyButton>().onClick.AddListener(() =>
            {
                DoChangeHeadImage(portrait);
            });
        }
    }

    private void DoChangeHeadImage(int portrait)
    {
        Debug.Log("DoChangeHeadImage!!! portrait = " + portrait);

        UIController.instance.GetUIManager<HallUIManager>().UpdateInfo(E_Update.Head);

        ReturnBack();
    }

    private void OnCloseBtnClick()
    {
        ReturnBack();
    }
}
