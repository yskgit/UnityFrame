using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 公告窗口，跑马灯
/// </summary>
public class AnnouncementWindow : SingletonWindow<AnnouncementWindow>
{
    /// <summary>
    /// 公告窗口显示的公告信息
    /// </summary>
    private struct TipsInfo
    {
        public string Tips;
        public int LoopTimes;

        public TipsInfo(string tips, int loopTimes)
        {
            Tips = tips;
            LoopTimes = loopTimes;
        }
    }

    /// <summary>
    /// AnnouncementWindow 的提示信息
    /// </summary>
    private Text _tips;

    /// <summary>
    /// _tips上的mask长度。用于计算_tips移动距离。
    /// </summary>
    private float _maskTransformLength;

    /// <summary>
    /// _timer 时间跑完 _maskTransformLength 的距离。用于计算_tips框移动的时间。为了使用DOTween实现匀速移动。
    /// </summary>
    private float _timer = 2f;

    /// <summary>
    /// _tips框的原始位置。用于跑马灯效果完成后，重置_tips框的位置
    /// </summary>
    private Vector3 _originPos;

    /// <summary>
    /// 公告队列
    /// </summary>
    private Queue<TipsInfo> _tipsQueue;

    /// <summary>
    /// 移动tips框的协程，用于退出界面的时候停止协程。特别是外界强制调用Close关闭公告界面的时候需要停止掉此协程。
    /// </summary>
    private Coroutine _moveCoroutine;

    protected override void Awake()
    {
        base.Awake();

        var texts = GetComponent<TextContainer>().Texts;
        var objs = GetComponent<GameObjectContainer>().Objs;

        _tips = texts[0];

        _maskTransformLength = objs[0].GetComponent<RectTransform>().sizeDelta.x;

        _originPos = _tips.transform.localPosition;

        _tipsQueue = new Queue<TipsInfo>();
    }

    /// <summary>
    /// 显示公告界面。tips和loopTimes添加到公告队列，按照添加顺序显示公告。
    /// </summary>
    /// <param name="tips">显示的内容</param>
    /// <param name="loopTimes">循环次数</param>
    public void Show(string tips, int loopTimes = 1)
    {
        _tipsQueue.Enqueue(new TipsInfo(tips, loopTimes));

        base.Show();
    }

    /// <summary>
    /// 每次界面显示的时候去检测_tipsQueue，有公告则显示
    /// </summary>
    private void OnEnable()
    {
        //因为Text框里输入文字后，数据和界面更新都是在Update之后，
        //所以需要启用协程里，等待FixedUpdate时执行后续移动操作，否则 Text框的大小未更新，跑马灯效果移动会出现错误。
        _moveCoroutine = StartCoroutine(MoveTips());
    }

    IEnumerator MoveTips()
    {
        if (_tipsQueue.Count <= 0)
        {
            base.Close();
            yield break;
        }

        TipsInfo tipsInfo = _tipsQueue.Dequeue();
      
        _tips.text = tipsInfo.Tips;
        yield return new WaitForFixedUpdate();
        //_tips框移动的最终距离
        float endValueX = _tips.transform.localPosition .x - (_tips.GetComponent<RectTransform>().sizeDelta.x + _maskTransformLength);
        //_tips框移动的时间
        float duration = Mathf.Abs(endValueX / _maskTransformLength * _timer);
        DoTweenHelper.DoLocalMoveX(_tips.transform, endValueX, duration, Ease.Flash, () =>
        {
            ResetTipsInfo();
            _moveCoroutine = StartCoroutine(MoveTips());
        }).SetLoops(tipsInfo.LoopTimes, LoopType.Restart);
    }

    /// <summary>
    /// 重置Tips框的信息。位置、内容
    /// </summary>
    private void ResetTipsInfo()
    {
        _tips.text = string.Empty;
        _tips.transform.localPosition = _originPos;
    }

    /// <summary>
    /// 外界强制关闭公告界面，停止掉tips框移动协程，清空tips框内容，清空消息队列
    /// </summary>
    public override void Close()
    {
        _tipsQueue.Clear();
        StopCoroutine(_moveCoroutine);
        ResetTipsInfo();
        base.Close();
    }
}
