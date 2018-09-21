using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRotation : MonoBehaviour {
    public float Speed = -30f;

    private static Vector3 _localEulerAngles;

    private void OnEnable() {
        transform.localEulerAngles = _localEulerAngles;
    }

    private void OnDisable()
    {
        _localEulerAngles = transform.localEulerAngles;
    }

	// Update is called once per frame
	void Update ()
	{
    }

    void FixedUpdate()
    {
        transform.Rotate(Vector3.forward * Time.fixedDeltaTime * Speed);
    }
}
