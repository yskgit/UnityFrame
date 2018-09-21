using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Event, AllowMultiple = false)]
public class ResponseEventAttribute : System.Attribute
{
    public int ProtoId;
    public ResponseEventAttribute(int protoId)
    {
        ProtoId = protoId;
    }

}
