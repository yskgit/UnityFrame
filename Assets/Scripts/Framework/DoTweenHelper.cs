using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

public static class DoTweenHelper
{
    private static bool _isPause;//防止在PauseAll方法执行后的dotween执行

    /// <summary>
    /// 运行动画时检测是否暂停所有dotween
    /// </summary>
    private static void CheckPause()
    {
        if (_isPause)
        {
            PauseAll();
        }
    }

    /// <summary>
    /// 暂停动画。立即执行
    /// </summary>
    public static void PauseAll()
    {
        _isPause = true;
        DOTween.PauseAll();
    }

    /// <summary>
    /// 恢复播放动画
    /// </summary>
    public static void UnPauseAll()
    {
        _isPause = false;
        DOTween.PlayAll();
    }

    /// <summary>
    /// 定时执行事件。
    /// </summary>
    /// <param name="tweener">需要定时执行事件的 Tweener</param>
    /// <param name="stepTime">tweener开始执行后的第stepTime秒执行事件</param>
    /// <param name="onStepComplete">定时执行的事件</param>
    private static void DoStep(Tweener tweener, float stepTime, Action onStepComplete)
    {
        //stepTime小于0不存在执行的意义
        if (stepTime < 0f)
        {
            return;
        }

        float timer = 0f;
        bool timing = true;
        tweener.onUpdate = () =>
        {
            if (timing)
            {
                timer += Time.deltaTime;
                if (timer >= stepTime)
                {
                    timing = false;
                    if (onStepComplete != null)
                    {
                        onStepComplete();
                    }
                }
            }
        };
    }

    public static Tweener DoMove(Transform tras, Vector3 targetPos, float duration, Ease easeType, Action onComplete, float stepTime = 1f, Action onStepComplete = null)
    {
        Tweener tweener = tras.DOMove(targetPos, duration).SetEase(easeType);
        if (onStepComplete != null)
        {
            DoStep(tweener, stepTime, onStepComplete);
        }
        tweener.onComplete = () =>
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };

        CheckPause();
        return tweener;
    }

    public static Tweener DoLocalMove(Transform tras, Vector3 targetPos, float duration, Ease easeType, Action onComplete, float stepTime = 1f, Action onStepComplete = null)
    {
        Tweener tweener = tras.DOLocalMove(targetPos, duration).SetEase(easeType);
        if (onStepComplete != null)
        {
            DoStep(tweener, stepTime, onStepComplete);
        }
        tweener.onComplete = () =>
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };

        CheckPause();
        return tweener;
    }

    public static Tweener DoLocalMoveX(Transform tras, float endValue, float duration, Ease easeType, Action onComplete, float stepTime = 1f, Action onStepComplete = null)
    {
        Tweener tweener = tras.DOLocalMoveX(endValue, duration).SetEase(easeType);
        if (onStepComplete != null)
        {
            DoStep(tweener, stepTime, onStepComplete);
        }
        tweener.onComplete = () =>
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };

        CheckPause();
        return tweener;
    }

    public static Tweener DoLocalMoveY(Transform tras, float endValue, float duration, Ease easeType, Action onComplete, float stepTime = 1f, Action onStepComplete = null)
    {
        Tweener tweener = tras.DOLocalMoveY(endValue, duration).SetEase(easeType);
        if (onStepComplete != null)
        {
            DoStep(tweener, stepTime, onStepComplete);
        }
        tweener.onComplete = () =>
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };

        CheckPause();
        return tweener;
    }

    public static Tweener DoLocalMoveZ(Transform tras, float endValue, float duration, Ease easeType, Action onComplete, float stepTime = 1f, Action onStepComplete = null)
    {
        Tweener tweener = tras.DOLocalMoveZ(endValue, duration).SetEase(easeType);
        if (onStepComplete != null)
        {
            DoStep(tweener, stepTime, onStepComplete);
        }
        tweener.onComplete = () =>
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };

        CheckPause();
        return tweener;
    }

    public static Tweener DoScale(Transform tras, Vector3 targetPos, float duration, Ease easeType, Action onComplete, float stepTime = 1f, Action onStepComplete = null)
    {
        Tweener tweener = tras.DOScale(targetPos, duration).SetEase(easeType);
        if (onStepComplete != null)
        {
            DoStep(tweener, stepTime, onStepComplete);
        }
        tweener.onComplete = () =>
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };

        CheckPause();
        return tweener;
    }

    public static Tweener DoFade(MaskableGraphic tras, float endValue, float duration, Ease easeType, Action onComplete, float stepTime = 1f, Action onStepComplete = null)
    {
        Tweener tweener = tras.DOFade(endValue, duration).SetEase(easeType);
        if (onStepComplete != null)
        {
            DoStep(tweener, stepTime, onStepComplete);
        }
        tweener.onComplete = () =>
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };

        CheckPause();
        return tweener;
    }

    public static Tweener DoRotate(Transform trans, Vector3 targetPos, float duration, Ease easeType, Action onComplete, float stepTime = 1f, Action onStepComplete = null)
    {
        Tweener tweener = trans.DORotate(targetPos, duration).SetEase(easeType);
        if (onStepComplete != null)
        {
            DoStep(tweener, stepTime, onStepComplete);
        }
        tweener.onComplete = () =>
        {
            if (onComplete != null)
            {
                onComplete();
            }
        };

        CheckPause();
        return tweener;
    }
}
