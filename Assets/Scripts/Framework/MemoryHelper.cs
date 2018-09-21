using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MemoryHelper
{
    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void SetValue<T>(string key, T value)
    {
        if (value is string)
        {
            PlayerPrefs.SetString(key, value.ToString());
        }
        else if (value is int)
        {
            PlayerPrefs.SetInt(key, Convert.ToInt32(value));
        }
        else if (value is float)
        {
            PlayerPrefs.SetFloat(key, Convert.ToSingle(value));
        }
        else if (value is bool)
        {
            PlayerPrefs.SetString(key, value.ToString());
        }
        else
        {
            LogUtil.Log("Set memory value error,invalid value type : " + typeof(T).Name);
        }
    }

    public static T GetValue<T>(string key)
    {
        if (!HasKey(key))
        {
            //Debug.Log("Get memory value error,do not have key : " + key);
            return default(T);
        }
        Type t = typeof(T);
        object obj = null;
        if (t == typeof(string))
        {
            obj = PlayerPrefs.GetString(key);
        }
        else if (t == typeof(int))
        {
            obj = PlayerPrefs.GetInt(key);
        }
        else if (t == typeof(float))
        {
            obj = PlayerPrefs.GetFloat(key);
        }
        else if (t == typeof(bool))
        {
            obj = PlayerPrefs.GetString(key).Equals("True");//True False
        }
        else
        {
            LogUtil.Log("Get memory value error,invalid T type : " + typeof(T).Name);
            return default(T);
        }
        return (T)obj;
    }

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public static void DeleteKey(string key)
    {
        if (!HasKey(key))
        {
            //Debug.Log("Get memory value error,do not have key : " + key);
            return;
        }
        PlayerPrefs.DeleteKey(key);
    }

    public static void SetScore(int score)
    {
        int currentScore = GetScore();
        currentScore += score;
        //if (currentScore < 0)
        //{
        //    currentScore = 0;
        //}
        SetValue("score", currentScore);
    }

    public static int GetScore()
    {
        return GetValue<int>("score");
    }

    public static bool HasKeyUsername()
    {
        return HasKey("username");
    }

    public static string GetUserName()
    {
        return GetValue<string>("username");
    }

    public static void SetUserName(string username)
    {
        SetValue("username", username);
    }

    public static void SetUserSex(string sex)
    {
        SetValue("isMale", sex);
    }

    public static string SetPortrait()
    {
        return GetValue<string>("portrait");
    }

    public static string GetPortrait()
    {
        return GetValue<string>("portrait");
    }

    public static void SetBgMusicState(bool bgMusicState)
    {
        SetValue("bgMusicState", bgMusicState);
    }

    public static bool GetBgMusicState()
    {
        return GetValue<bool>("bgMusicState");
    }

    public static void SetEffectMusicState(bool effectMusicState)
    {
        SetValue("effectMusicState", effectMusicState);
    }

    public static bool GetEffectMusicState()
    {
        return GetValue<bool>("effectMusicState");
    }
}
