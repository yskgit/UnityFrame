using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadUIManager : UIManager
{
    private Text _percentageText;
    private Text _fileCountText;
    private Text _noticeText;

    private Image _percentageImage;

    private MyButton _retryBtn;

    private Dictionary<string, SingleBundleInfo> _downloadDic;

    /// <summary>
    /// 下载完成后，调用此回调
    /// </summary>
    private Action _onFinished;

    public override void InitUI()
    {
        base.InitUI();

        _percentageText = _texts[0];
        _fileCountText = _texts[1];
        _noticeText = _texts[2];

        _percentageImage = _images[0];

        _retryBtn = _btns[0];
        _retryBtn.onClick.AddListener(() =>
        {
            DownloadUpdate(_downloadDic);
        });
        ShowRetryBtn(false);
    }

    public override void InitData(object[] args)
    {
        base.InitData(args);

        _onFinished = args[0] as Action;

        _noticeText.text = "正在检查资源更新，请稍候...";

        LoadingWebWindow.instance.Show();
        AssetBundleManager.instance.CheckUpdateAsync((downloadDic) =>
        {
            LoadingWebWindow.instance.Close();

            _downloadDic = downloadDic;

            //没有资源更新
            if (_downloadDic == null || _downloadDic.Count == 0)
            {
                _noticeText.text = "您的游戏已是最新版本！";
                ReturnBack();
            }
            else
            {
                float size = 0f;
                foreach (var bundleInfo in downloadDic)
                {
                    size += bundleInfo.Value.size;
                }

                if (size < 0.1f)
                {
                    size = 0.1f;
                }
                string sizeStr = size.ToString("0.##");

                TipsWindow.instance.Show($"有资源需要更新，大小为：{sizeStr}M", () =>
                {
                    DownloadUpdate(_downloadDic);
                }, GameManager.instance.QuitGame, "确定", "退出游戏");
            }
        });
    }

    private void DownloadUpdate(Dictionary<string, SingleBundleInfo> dic)
    {
        if (dic != null && dic.Count > 0)
        {
            AssetBundleManager.instance.DownloadUpdateAsync(dic, (abName, fileIndex, percentage) =>
            {
                _percentageText.text = (int)(percentage * 100) + "%";
                _percentageImage.fillAmount = percentage;
                _fileCountText.text = fileIndex + "/" + dic.Count;
                _noticeText.text = $"正在下载:{abName}";
            }, (success) =>
            {
                if (success)
                {
                    _noticeText.text = "下载完成";
                    ReturnBack();
                }
                else
                {
                    _noticeText.text = "下载失败";
                    ShowRetryBtn(true);
                }
            });
        }
    }

    private void ShowRetryBtn(bool isTrue)
    {
        _retryBtn.gameObject.SetActive(isTrue);
    }

    private void ReturnBack()
    {
        base.ReturnBack(1, _onFinished);
    }
}
