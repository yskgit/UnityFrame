﻿using System;
        //if (showstack)
        GUILayout.Label(stack);










/// <summary>

    List<logdata> logDatas = new List<logdata>();//log链表
    List<logdata> errorDatas = new List<logdata>();//错误和异常链表
    List<logdata> warningDatas = new List<logdata>();//警告链表

    static List<string> mWriteTxt = new List<string>();
        //Application.persistentDataPath Unity中只有这个路径是既可以读也可以写的。
        //Debug.Log(Application.persistentDataPath);
        outpath = Application.persistentDataPath + "/outLog.txt";
        //每次启动客户端删除之前保存的Log
        if (System.IO.File.Exists(outpath))
        //转换场景不删除
        DontDestroyOnLoad(gameObject);
        //注册log监听
        Application.logMessageReceived += HangleLog;

    void OnDisable()
        // Remove callback when object goes out of scope
        //当对象超出范围，删除回调。
        Application.logMessageReceived -= HangleLog;
        //因为写入文件的操作必须在主线程中完成，所以在Update中才给你写入文件。
        if (errorDatas.Count > 0)