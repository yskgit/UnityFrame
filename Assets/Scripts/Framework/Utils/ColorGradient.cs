using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorGradient : MonoBehaviour
{
    //这两个颜色在编辑器中选择了后，_image图片会被“隐藏掉”，UGUI的bug!!!
    public Color From = Color.white;
    public Color To = Color.gray;

    public float Speed = 10;
    private Image _image;

    private float timer;
    private bool _forward;
    
    void Awake()
    {
        _image = GetComponent<Image>();
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (timer<=0f)
	    {
	        _forward = true;
	    }

	    if (timer>=1f)
	    {
	        _forward = false;
        }

	    if (_forward)
	    {
	        timer += Time.deltaTime * Speed;
        }
	    else
	    {
	        timer -= Time.deltaTime * Speed;
        }

        //_image.color = Color.Lerp(From, To, timer);
        _image.color = Color.Lerp(Color.white, Color.gray, timer);
    }
}
