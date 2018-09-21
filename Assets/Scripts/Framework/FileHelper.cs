using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ProtoBuf;

public static class FileHelper
{
    private static readonly byte[] _key;
    private static Dictionary<string, string> _configDic = new Dictionary<string, string>();

    static FileHelper()
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        _key = md5.ComputeHash(Encoding.UTF8.GetBytes("LKADSZ34t57x$"));
    }

    public static string[] GetXingArray()
    {
        return GetWordArray("name_xing");
    }

    public static string[] GetMaleMingArray()
    {
        return GetWordArray("name_ming_nan");
    }

    public static string[] GetFemaleMingArray()
    {
        return GetWordArray("name_ming_nv");
    }

    private static string[] GetWordArray(string url)
    {
        url = SingletonScriptable<PathManager>.instance.LocalResourcePath +
              SingletonScriptable<PathManager>.instance.SubDir(ResourceType.Resource_Nickname) + url + ".txt";
        string words = ReadStrFromFile(url, false);
        string[] wordsArr = words.Split(' ');
        return wordsArr;

        //TextAsset wordText = Resources.Load<TextAsset>(url);
        //if (!wordText)
        //{
        //    Debug.LogWarning(string.Format("未找到名字为{0}的资源", url));
        //    return null;
        //}
        //string words = wordText.text;
        //string[] wordsArr = words.Split(' ');
        //return wordsArr;
    }

    public static void Decode(byte[] bytes)
    {
    }

    public static void Encode(byte[] bytes)
    {
    }

    public static void SaveBytesToFile(byte[] bytes, string path, bool encode)
    {
        if (encode)
        {
            Encode(bytes);
        }
        FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        stream.SetLength(0);
        stream.Write(bytes, 0, bytes.Length);
        stream.Dispose();
    }

    public static void SaveStrToFile(string str, string path, bool encode)
    {
        SaveBytesToFile(Encoding.UTF8.GetBytes(str), path, encode);
    }

    public static byte[] ReadBytesFromFile(string path, bool decode)
    {
        WWW www = LoadFile(path);
        if (www == null)
        {
            return null;
        }

        return www.bytes;

        //以下代码在unity editor中可用，但是在安卓不适用，留作参考
        //if (File.Exists(path))
        //{
        //    FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //    byte[] bytes = new byte[stream.Length];
        //    stream.Read(bytes, 0, (int)stream.Length);
        //    stream.Dispose();
        //    if (decode)
        //    {
        //        Decode(bytes);
        //    }
        //    return bytes;
        //}
        //return new byte[0];
    }

    public static string ReadStrFromFile(string path, bool decode)
    {
        WWW www = LoadFile(path);
        if (www == null)
        {
            return null;
        }

        return www.text;

        //byte[] bytes = ReadBytesFromFile(path, decode);
        //return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }

    private static WWW LoadFile(string url)
    {
        WWW www = new WWW(url);
        while (!www.isDone)
        {
            //Debug.Log(string.Format("LoadFile :{0},www.progress = {1}", url, www.progress));
        }
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log(string.Format("LoadFile error:{0},no file path :{1}", www.error, url));
            return null;
        }
        return www;
    }

    public static string ReadConfig(string key)
    {
        if (_configDic.Count == 0)
        {
            string configPath = SingletonScriptable<PathManager>.instance.LocalResourcePath +
                                SingletonScriptable<PathManager>.instance.SubDir(ResourceType.Resource_Config) +
                                "config.json";

            WWW www = LoadFile(configPath);
            if (www == null)
            {
                return default(string);
            }
            string json = www.text;

            _configDic = Util.FromJson<Dictionary<string, string>>(json);
        }

        string value;
        if (!_configDic.TryGetValue(key,out value))
        {
            Debug.Log($"config文件没有{key}键值对");
        }

        return value;
    }

    private static void DoWriteToFile<T>(Stream stream, T obj)
    {
        Serializer.Serialize(stream, obj);
    }

    private static T DoReadFromFile<T>(Stream stream)
    {
        return Serializer.Deserialize<T>(stream);
    }

    public static void WritePbToFile<T>(T pb, string path, bool encode)
    {
        if (encode)
        {
            byte[] bytes = null;
            using (var ms = new MemoryStream())
            {
                DoWriteToFile(ms, pb);
                ms.Flush();
                bytes = ms.ToArray();
                //				Debug.Log("bytes (before encrypt) = " + bytes.Length + "," + path);
                bytes = Xxtea.XXTEA.Encrypt(bytes, _key);
                using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    stream.SetLength(0);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    //					Debug.Log("write bytes (after encrypt) = " + bytes.Length + "," + path);
                }
            }
        }
        else
        {
            using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.SetLength(0);
                DoWriteToFile(stream, pb);
                stream.Flush();
                //				Debug.Log("write bytes (without encrypt) = " + stream.Length + "," + path);
            }
        }
    }

    public static T ReadPbFromFile<T>(string path, bool decode)
    {
        WWW www = LoadFile(path);
        if (www == null)
        {
            return default(T);
        }
        using (var ms = new MemoryStream(www.bytes))
        {
            return DoReadFromFile<T>(ms);
        }

        //if (File.Exists(path))
        //{
        //    using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        //    {
        //        if (decode)
        //        {
        //            byte[] bytes = new byte[fs.Length];
        //            fs.Read(bytes, 0, (int)fs.Length);
        //            if (bytes != null)
        //            {
        //                bytes = Xxtea.XXTEA.Decrypt(bytes, _key);
        //                //						Debug.Log("load bytes = " + bytes.Length + "," + path);
        //                using (var ms = new MemoryStream(bytes))
        //                {
        //                    return DoReadFromFile<T>(ms);
        //                }
        //            }

        //            return default(T);
        //        }
        //        else
        //        {
        //            //					Debug.Log("load bytes = " + fs.Length + "," + path);
        //            return DoReadFromFile<T>(fs);
        //        }
        //    }
        //}
        //else
        //{
        //    return default(T);
        //}
    }
}
