using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EAndioType
{
    Bg, //背景音乐
    Effect,//效果音乐
    Btn,//按钮音乐
    Extra//备用的AudioSource
}

public enum E_SettingAndioType
{
    Bg, //背景音乐
    Effect,//效果音乐。音效是除了背景音乐外的所有音乐
}

public class AudioManager : SingletonBehaviour<AudioManager>
{
    public bool IsCanPlayBtnAudio = true; //uicontroller 设置第一次进入界面时不能播放选中声音

    private const string AUDIO_DIR = "Sound/";

    private readonly Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    /// <summary>
    /// 背景音乐是否开启
    /// </summary>
    private bool _isBgMusicOpen
    {
        get { return MemoryHelper.GetBgMusicState(); }
        set { MemoryHelper.SetBgMusicState(value); }
    }

    /// <summary>
    /// 音效是否开启。音效是除了背景音乐外的所有音乐
    /// </summary>
    private bool _isEffectMusicOpen
    {
        get { return MemoryHelper.GetEffectMusicState(); }
        set { MemoryHelper.SetEffectMusicState(value); }
    }

    private AudioSource _bgAudioSource;
    private AudioSource _btnAudioSource;
    private AudioSource _effectAudioSource;
    private AudioSource _extraAudioSource;//备用的AudioSource

    private Coroutine _bgCoroutine;//音乐间隔时间播放协程

    protected override void Awake()
    {
        base.Awake();
        _bgAudioSource = gameObject.AddComponent<AudioSource>();
        _bgAudioSource.playOnAwake = false;
        _btnAudioSource = gameObject.AddComponent<AudioSource>();
        _btnAudioSource.playOnAwake = false;
        _effectAudioSource = gameObject.AddComponent<AudioSource>();
        _effectAudioSource.playOnAwake = false;
        _extraAudioSource = gameObject.AddComponent<AudioSource>();
        _extraAudioSource.playOnAwake = false;
    }

    /// <summary>
    /// 设置音乐的开关状态
    /// </summary>
    /// <param name="settingType"></param>
    /// <param name="isTrue"></param>
    public void SetMusicOpen(E_SettingAndioType settingType, bool isTrue)
    {
        if (settingType == E_SettingAndioType.Bg)
        {
            _isBgMusicOpen = isTrue;
            if (_isBgMusicOpen)
            {
                PlayAudio(EAndioType.Bg, "bg_hall");
            }
            else
            {
                StopMusic(E_SettingAndioType.Bg);
            }
        }
        else
        {
            _isEffectMusicOpen = isTrue;
            if (_isEffectMusicOpen)
            {
                StopMusic(E_SettingAndioType.Effect);
            }
        }
    }

    /// <summary>
    /// 获取音乐开关状态
    /// </summary>
    /// <param name="settingType"></param>
    /// <returns></returns>
    public bool GetMusicOpen(E_SettingAndioType settingType)
    {
        if (settingType == E_SettingAndioType.Bg)
        {
            return _isBgMusicOpen;
        }
        return _isEffectMusicOpen;
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="audioType"></param>
    /// <param name="audioName"></param>
    public void PlayAudio(EAndioType audioType, string audioName)
    {
        E_SettingAndioType settingType = GetSettingType(audioType);
        if (settingType == E_SettingAndioType.Bg && !_isBgMusicOpen)
        {
            return;
        }
        if (settingType == E_SettingAndioType.Effect && !_isEffectMusicOpen)
        {
            return;
        }

        switch (audioType)
        {
            case EAndioType.Bg:
                PlayBgAudio(audioName);
                break;
            case EAndioType.Btn:
                PlayBtnAudio(audioName);
                break;
            case EAndioType.Effect:
                PlayEffectAudio(audioName);
                break;
            case EAndioType.Extra:
                PlayExtraAudio(audioName);
                break;
        }
    }

    /// <summary>
    /// 播放效果声音，如：失败、胜利等。可被下一个效果声音打断
    /// </summary>
    /// <param name="audioName"></param>
    private void PlayEffectAudio(string audioName)
    {
        if (_effectAudioSource)
        {
            _effectAudioSource.Stop();
        }

        AudioClip audioClip = GetAudioClip(audioName);
        if (!audioClip)
        {
            Debug.LogWarning("PlayEffectAudio error!!!");
            return;
        }

        _effectAudioSource.clip = audioClip;
        _effectAudioSource.Play();
    }

    /// <summary>
    /// 播放按钮声音，可被下一个按钮播放声音打断
    /// </summary>
    /// <param name="audioName"></param>
    private void PlayBtnAudio(string audioName)
    {
        if (!IsCanPlayBtnAudio)
        {
            IsCanPlayBtnAudio = true;
            return;
        }

        if (_btnAudioSource)
        {
            _btnAudioSource.Stop();
        }

        AudioClip audioClip = GetAudioClip(audioName);
        if (!audioClip)
        {
            Debug.LogWarning("PlayBtnAudio error!!!");
            return;
        }

        _btnAudioSource.clip = audioClip;
        _btnAudioSource.Play();
    }

    /// <summary>
    /// 播放背景音乐，可被下一个背景音乐打断
    /// </summary>
    /// <param name="audioName"></param>
    private void PlayBgAudio(string audioName)
    {
        if (!_bgAudioSource)
        {
            Debug.LogWarning("_bgAudioSource 不应该为null");
            return;
        }
        _bgAudioSource.Stop();

        AudioClip audioClip = GetAudioClip(audioName);
        if (!audioClip)
        {
            Debug.LogWarning("PlayBgAudio error!!!");
            return;
        }

        _bgAudioSource.clip = audioClip;
        _bgAudioSource.Play();

        if (_bgCoroutine != null)
        {
            StopCoroutine(_bgCoroutine);
        }
        _bgCoroutine = StartCoroutine(IEPlayBgAudio(_bgAudioSource.clip.length));
    }

    IEnumerator IEPlayBgAudio(float clipLength)
    {
        //Debug.Log("IEPlayBgAudio time :" + Time.realtimeSinceStartup + " clipLength:" + clipLength);
        yield return new WaitForSeconds(clipLength + 30f);
        StopCoroutine(_bgCoroutine);
        _bgAudioSource.Play();
        _bgCoroutine = StartCoroutine(IEPlayBgAudio(clipLength));
    }

    /// <summary>
    /// 播放备用音频，可被下一个播放声音打断
    /// </summary>
    /// <param name="audioName"></param>
    private void PlayExtraAudio(string audioName)
    {
        if (_extraAudioSource)
        {
            _extraAudioSource.Stop();
        }

        AudioClip audioClip = GetAudioClip(audioName);
        if (!audioClip)
        {
            Debug.LogWarning("PlayExtraAudio error!!!");
            return;
        }

        _extraAudioSource.clip = audioClip;
        _extraAudioSource.Play();
    }

    private AudioClip GetAudioClip(string audioName)
    {
        AudioClip audioClip;
        if (!_audioClips.TryGetValue(audioName, out audioClip))
        {
            audioClip = ObjectCache.instance.LoadResource<AudioClip>(AUDIO_DIR + audioName);
            if (!audioClip)
            {
                Debug.LogWarning(string.Format("Play Audio error!!!no audioName:{0}", AUDIO_DIR + audioName));
            }
            _audioClips.Add(audioName, audioClip);
        }
        return audioClip;
    }

    /// <summary>
    /// 暂停音乐
    /// </summary>
    /// <param name="audioType"></param>
    public void PauseMusic(EAndioType audioType)
    {
        if (!_isBgMusicOpen)
        {
            return;
        }
        switch (audioType)
        {
            case EAndioType.Bg:
                PauseBgMusic();
                break;
            case EAndioType.Btn:
                PauseBtnMusic();
                break;
            case EAndioType.Effect:
                PauseEffectMusic();
                break;
            case EAndioType.Extra:
                PauseExtraMusic();
                break;
        }
    }

    private void PauseBgMusic()
    {
        if (_bgAudioSource && _bgAudioSource.isPlaying)
        {
            _bgAudioSource.Pause();
        }
    }

    private void PauseEffectMusic()
    {
        if (_effectAudioSource && _effectAudioSource.isPlaying)
        {
            _effectAudioSource.Pause();
        }
    }

    private void PauseBtnMusic()
    {
        if (_btnAudioSource && _btnAudioSource.isPlaying)
        {
            _btnAudioSource.Pause();
        }
    }

    private void PauseExtraMusic()
    {
        if (_extraAudioSource && _extraAudioSource.isPlaying)
        {
            _extraAudioSource.Pause();
        }
    }

    /// <summary>
    /// 解除暂停音乐
    /// </summary>
    /// <param name="audioType"></param>
    public void UnPauseMusic(EAndioType audioType)
    {
        E_SettingAndioType settingType = GetSettingType(audioType);
        if (settingType == E_SettingAndioType.Bg && !_isBgMusicOpen)
        {
            return;
        }
        if (settingType != E_SettingAndioType.Effect && !_isEffectMusicOpen)
        {
            return;
        }

        switch (audioType)
        {
            case EAndioType.Bg:
                UnPauseBgMusic();
                break;
            case EAndioType.Btn:
                UnPauseBtnMusic();
                break;
            case EAndioType.Effect:
                UnPauseEffectMusic();
                break;
            case EAndioType.Extra:
                UnPauseExtraMusic();
                break;
        }
    }

    private E_SettingAndioType GetSettingType(EAndioType audioType)
    {
        if (audioType == EAndioType.Bg)
        {
            return E_SettingAndioType.Bg;
        }
        return E_SettingAndioType.Effect;
    }

    private void UnPauseBgMusic()
    {
        if (_bgAudioSource && _bgAudioSource.isPlaying)
        {
            _bgAudioSource.Pause();
        }
    }

    private void UnPauseEffectMusic()
    {
        if (_effectAudioSource && _effectAudioSource.isPlaying)
        {
            _effectAudioSource.Pause();
        }
    }

    private void UnPauseBtnMusic()
    {
        if (_btnAudioSource && _btnAudioSource.isPlaying)
        {
            _btnAudioSource.Pause();
        }
    }

    private void UnPauseExtraMusic()
    {
        if (_extraAudioSource && _extraAudioSource.isPlaying)
        {
            _extraAudioSource.Pause();
        }
    }

    /// <summary>
    /// 停止播放音乐
    /// </summary>
    /// <param name="settingType"></param>
    public void StopMusic(E_SettingAndioType settingType)
    {
        if (settingType == E_SettingAndioType.Bg)
        {
            _bgAudioSource.Stop();
            if (_bgCoroutine != null)
            {
                StopCoroutine(_bgCoroutine);
            }
        }
        else
        {
            _btnAudioSource.Stop();
            _effectAudioSource.Stop();
            _extraAudioSource.Stop();
        }
    }

    /// <summary>
    /// 切换音乐开关状态
    /// </summary>
    /// <param name="settingType"></param>
    public void SwitchMusicState(E_SettingAndioType settingType)
    {
        bool musicOpen = GetMusicOpen(settingType);
        SetMusicOpen(settingType, !musicOpen);
    }

    /// <summary>
    /// 调整音乐声音大小，volum最大值100，最小值0.
    /// </summary>
    /// <param name="audioType"></param>
    /// <param name="volum"></param>
    public void ChangeMusicVolum(EAndioType audioType, int volum)
    {
        E_SettingAndioType settingType = GetSettingType(audioType);
        if (settingType == E_SettingAndioType.Bg && !_isBgMusicOpen)
        {
            return;
        }
        if (settingType != E_SettingAndioType.Effect && !_isEffectMusicOpen)
        {
            return;
        }

        switch (audioType)
        {
            case EAndioType.Bg:
                ChangeAudioSourceVolum(_bgAudioSource,volum);
                break;
            case EAndioType.Btn:
                ChangeAudioSourceVolum(_btnAudioSource, volum);
                break;
            case EAndioType.Effect:
                ChangeAudioSourceVolum(_effectAudioSource, volum);
                break;
            case EAndioType.Extra:
                ChangeAudioSourceVolum(_extraAudioSource, volum);
                break;
        }
    }

    private void ChangeAudioSourceVolum(AudioSource audioSource, int volum)
    {
        audioSource.volume = volum / 100f;
    }
}
