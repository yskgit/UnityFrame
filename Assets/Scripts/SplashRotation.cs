using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashRotation : MonoBehaviour
{
    public Vector3 StartRotation;
    public Vector3 TargetRotation;
    public float RotateTime;
    private float _timer;

    public void StartRotate()
    {
        //StartRotation = Vector3.zero;
        //TargetRotation = new Vector3(0f, 0f, 360f);
        transform.localEulerAngles = StartRotation;
        _timer = 0;
    }

    //Use this for initialization

    void Start()
    {
    }

    void FixedUpdate()
    {
        if (_timer <= 1.0f)
        {
            _timer += Time.fixedDeltaTime / RotateTime;
            transform.localEulerAngles = Vector3.Lerp(StartRotation, TargetRotation, _timer);
        }
    }
}
