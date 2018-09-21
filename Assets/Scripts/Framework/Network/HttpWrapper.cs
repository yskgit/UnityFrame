#if Network
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class HttpWrapper : SingletonBehaviour<HttpWrapper>
{
    public void HttpRequest<TReq, TResp>(string url, TReq req, Action<TResp> onFinished)
    {
        StartCoroutine(DoHttpRequest(url, req, onFinished));
    }

    IEnumerator DoHttpRequest<TReq, TResp>(string url, TReq req, Action<TResp> onFinished)
    {
        MemoryStream ms = new MemoryStream();
        Serializer.Serialize(ms, req);
        byte[] data = ms.ToArray();
        ms.Close();

//        MemoryStream stream = new MemoryStream(data);
//        TReq reqt = Serializer.Deserialize<TReq>(stream);
//
//        stream.Close();

        //WWW的三个参数: url, postData, headers    
        string requestUrl = "http://127.0.0.1:8080/ServletWrapper";

        var headers = new Dictionary<string, string>();
        //headers["Session"] = cookie;
        headers["Accept"] = "application/x-protobuf";
        headers["Content-Type"] = "application/x-protobuf";
        headers["URL"] = url;
        //headers["IncludeOther"] = includeOther.ToString();
        //headers["Retry"] = isRetrying.ToString();
        //if (!isRetrying)
        //    timeStamp = Time.realtimeSinceStartup.ToString();
        headers["Timestamp"] = "111";
        //headers["mainVersion"] = LocalVerison.instance.LocalMainVersion.ToString();
        //headers["subVersion"] = LocalVerison.instance.LocalSubVersion.ToString();
        //headers["macroVersion"] = LocalVerison.instance.LocalMacroVersion.ToString();
        //headers["zoneTag"] = zoneTag;

        //发送请求    
        //WWW www_instance = new WWW(requestUrl, data);

        LogUtil.Log("消息发送成功!");
        WWW www_instance = new WWW(requestUrl, data, headers);

        //web服务器返回    
        yield return www_instance;
        LogUtil.Log("消息返回成功!");
        if (www_instance.error != null)
        {
            LogUtil.Log(www_instance.error);
        }
        else
        {   //显示返回数据  
            byte[] respData = www_instance.bytes;
            MemoryStream memStream = new MemoryStream(respData);
            TResp response = Serializer.Deserialize<TResp>(memStream);
            onFinished(response);
            memStream.Close();
        }
    }
}
#endif