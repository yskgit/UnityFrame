using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class UGUISpriteAnimation : MonoBehaviour
{
    public float FPS = 5;
    public List<Sprite> SpriteFrames;
    public bool DoOnstart = false;
    public bool DoOnEnable = false;
    public bool Loop = false;
    public bool IsNativeSize = false;
    public bool Foward = true;  //图片切换顺序    Foward为true：SpriteFrames[0]到SpriteFrames[SpriteFrames.Count]  反之则倒过来
    public bool HideOnComplete = false;

    public Action OnComplete;//loop为false时，动画播放完执行
    public Action OnStepComplete;//loop为true时，每一轮结束调用一次

    private Image ImageSource;
    private int mCurFrame = 0;
    private float mDelta = 0;
    private bool _isPlaying = false;

    public int FrameCount {
        get {
            return SpriteFrames.Count;
        }
    }

    void Awake()
    {
        ImageSource = GetComponent<Image>();
    }

    void Start()
    {
        if (!ImageSource)
        {
            ImageSource = GetComponent<Image>();
        }

        if (DoOnstart)
        {
            PlayFromZeroFrame();
        }
    }

    private void OnEnable()
    {
        if (!ImageSource)
        {
            ImageSource = GetComponent<Image>();
        }

        if (DoOnEnable)
        {
            PlayFromZeroFrame();
        }
    }

    public void PlayFromZeroFrame()
    {
        if (!ImageSource)
        {
            ImageSource = GetComponent<Image>();
        }

        _isPlaying = true;
        mCurFrame = 0;
        SetSprite(mCurFrame);
    }

    private void SetSprite(int idx)
    {
        if (SpriteFrames == null || SpriteFrames.Count == 0)
        {
            return;
        }

        ImageSource.sprite = SpriteFrames[idx];
        if (IsNativeSize)
        {
            ImageSource.SetNativeSize();
        }
    }

    void Update()
    {
        if (!_isPlaying || 0 == FrameCount)
        {
            return;
        }

        mDelta += Time.deltaTime;
        if (mDelta > 1 / FPS)
        {
            mDelta = 0;
            if (Foward)
            {
                mCurFrame++;
            }
            else
            {
                mCurFrame--;
            }

            if (mCurFrame >= FrameCount)
            {
                if (Loop)
                {
                    mCurFrame = 0;
                    if (OnStepComplete != null)
                    {
                        OnStepComplete();
                    }
                }
                else
                {
                    _isPlaying = false;
                    if (OnComplete != null)
                    {
                        OnComplete();
                    }
                    if (HideOnComplete)
                    {
                        gameObject.SetActive(false);
                    }
                    return;
                }
            }
            else if (mCurFrame < 0)
            {
                if (Loop)
                {
                    mCurFrame = FrameCount - 1;
                    if (OnStepComplete != null)
                    {
                        OnStepComplete();
                    }
                }
                else
                {
                    _isPlaying = false;
                    if (OnComplete != null)
                    {
                        OnComplete();
                    }
                    if (HideOnComplete)
                    {
                        gameObject.SetActive(false);
                    }
                    return;
                }
            }

            SetSprite(mCurFrame);
        }
    }

    /// <summary>
    /// 从之前帧开始动画
    /// </summary>
    public void PlayFromOldFrame()
    {
        _isPlaying = true;
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public bool CheckIsPlaying()
    {
        return _isPlaying;
    }
}