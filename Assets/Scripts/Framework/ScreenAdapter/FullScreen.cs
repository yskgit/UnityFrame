using System.Collections;
using UnityEngine;

public class FullScreen : MonoBehaviour
{
    private void OnEnable()
    {
        if (GameManager.instance)
        {
            SetFullScreen();
        }
        else
        {
            StartCoroutine(SetFullScreenAsync());
        }
    }

    private void SetFullScreen()
    {
        transform.localScale = Vector3.one * GameManager.instance.GetBgScale();
    }

    private IEnumerator SetFullScreenAsync()
    {
        while (!GameManager.instance)
        {
            yield return null;
        }
        transform.localScale = Vector3.one * GameManager.instance.GetBgScale();
    }
}
