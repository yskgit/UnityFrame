using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

public class LogUtil : MonoBehaviour
{
    public enum E_LogLevel : byte
    {
        None = 0,//不输出任何日志
        Exception = 1,//输出异常日志
        Error = 2,//输出错误日志
        Warning = 3,//输出警告日志
        Info = 4,//输出所有日志
    }

    private static E_LogLevel _logLevel = E_LogLevel.Info;

    public static void SetLogLevel(string level)
    {
        switch (level)
        {
            case "None":
                _logLevel = E_LogLevel.None;
                break;
            case "Exception":
                _logLevel = E_LogLevel.Exception;
                break;
            case "Error":
                _logLevel = E_LogLevel.Error;
                break;
            case "Warning":
                _logLevel = E_LogLevel.Warning;
                break;
            case "Info":
                _logLevel = E_LogLevel.Info;
                break;
        }

        Debug.Log("设置_logLevel = " + _logLevel);
    }

#if !UNITY_EDITOR
    private static string _infoColor = "#909090";
    private static string _warningColor = "orange";
    private static string _errorColor = "red";
#endif

    /// <summary>
    /// 暂停掉editor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="sender"></param>
    public static void LogBreak(object message, UnityEngine.Object sender = null)
    {
        Log(message, sender);
        Debug.Break();
    }

    public static void LogFormat(string format, UnityEngine.Object sender, params object[] message)
    {
        if (_logLevel >= E_LogLevel.Info)
        {
            LogLevelFormat(E_LogLevel.Info, string.Format(format, message), sender);
        }
    }

    public static void LogFormat(string format, params object[] message)
    {
        if (_logLevel >= E_LogLevel.Info)
        {
            LogLevelFormat(E_LogLevel.Info, string.Format(format, message), null);
        }
    }

    public static void Log(object message, UnityEngine.Object sender = null)
    {
        if (_logLevel >= E_LogLevel.Info)
        {
            LogLevelFormat(E_LogLevel.Info, message, sender);
        }
    }

    public static void LogWarning(object message, UnityEngine.Object sender = null)
    {
        if (_logLevel >= E_LogLevel.Warning)
        {
            LogLevelFormat(E_LogLevel.Warning, message, sender);
        }
    }

    public static void LogError(object message, UnityEngine.Object sender = null)
    {
        if (_logLevel >= E_LogLevel.Error)
        {
            LogLevelFormat(E_LogLevel.Error, message, sender);
        }
    }

    public static void LogException(Exception exption, UnityEngine.Object sender = null)
    {
        if (_logLevel >= E_LogLevel.Exception)
        {
            LogLevelFormat(E_LogLevel.Exception, exption, sender);
        }
    }

    private static void LogLevelFormat(E_LogLevel level, object message, UnityEngine.Object sender)
    {
        switch (level)
        {
            case E_LogLevel.None:
                break;
            case E_LogLevel.Exception:
                break;
            case E_LogLevel.Error:
                Debug.LogError(message);
                break;
            case E_LogLevel.Warning:
                Debug.LogWarning(message);
                break;
            case E_LogLevel.Info:
                Debug.Log(message);
                break;
        }


        //#if UNITY_EDITOR
        //        //        _logStackFrameList.Add(stackFrame);

        //        switch (level)
        //        {
        //            case E_LogLevel.None:
        //                break;
        //            case E_LogLevel.Exception:
        //                break;
        //            case E_LogLevel.Error:
        //                Debug.LogError(message);
        //                break;
        //            case E_LogLevel.Warning:
        //                Debug.LogWarning(message);
        //                break;
        //            case E_LogLevel.Info:
        //                Debug.Log(message);
        //                break;
        //        }

        //#else
        //        StackTrace stackTrace = new StackTrace(true);
        //        var stackFrame = stackTrace.GetFrame(2);
        //        string stackMessageFormat = Path.GetFileName(stackFrame.GetFileName()) + ":" + stackFrame.GetMethod().Name + "():at line " + stackFrame.GetFileLineNumber();
        //        string timeFormat = "Frame:" + Time.frameCount + "," + DateTime.Now.Millisecond + "ms";
        //        string objectName = string.Empty;
        //        string colorFormat = _infoColor;
        //        if (level == E_LogLevel.Warning)
        //            colorFormat = _warningColor;
        //        else if (level == E_LogLevel.Error)
        //            colorFormat = _errorColor;

        //        StringBuilder sb = new StringBuilder();
        //        string levelFormat = level.ToString().ToUpper();
        //        sb.AppendFormat("<color={3}>[{0}][{4}][{1}]{2}</color>", levelFormat, timeFormat, message, colorFormat, stackMessageFormat);
        //        LogUtil.Log(sb, sender);
        //#endif
    }

#if UNITY_EDITOR
    private static int _instanceId;
    //    private static int _line = 104;
    //    private static List<StackFrame> _logStackFrameList = new List<StackFrame>();
    //ConsoleWindow  
    //        private static object s_ConsoleWindow;
    private static object _logListView;
    //    private static FieldInfo _logListViewTotalRows;
    private static FieldInfo _logListViewCurrentRow;
    //LogEntry  
    private static MethodInfo _logEntriesGetEntry;
    private static object _logEntry;
    //instanceId 非UnityEngine.Object的运行时 InstanceID 为零所以只能用 LogEntry.Condition 判断  
    //private static FieldInfo _logEntryInstanceId;
    //private static FieldInfo _logEntryLine;
    private static FieldInfo _logEntryCondition;
    static LogUtil()
    {
        _instanceId = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Framework/Utils/LogUtil.cs").GetInstanceID();

        //        _logStackFrameList.Clear();

        GetConsoleWindowListView();
    }

    private static void GetConsoleWindowListView()
    {
        if (_logListView == null)
        {
            Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            Type consoleWindowType = unityEditorAssembly.GetType("UnityEditor.ConsoleWindow");
            FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindow = fieldInfo.GetValue(null);
            FieldInfo listViewFieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
            _logListView = listViewFieldInfo.GetValue(consoleWindow);
            //            _logListViewTotalRows = listViewFieldInfo.FieldType.GetField("totalRows", BindingFlags.Instance | BindingFlags.Public);
            _logListViewCurrentRow = listViewFieldInfo.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);

            //LogEntries  
            Assembly sceneAssembly = Assembly.GetAssembly(typeof(SceneView));
            Type logEntriesType = sceneAssembly.GetType("UnityEditor.LogEntries");
            //Type logEntriesType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntries");
            _logEntriesGetEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
            //Type logEntryType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntry");
            Type logEntryType = sceneAssembly.GetType("UnityEditor.LogEntry");
            _logEntry = Activator.CreateInstance(logEntryType);
            //_logEntryInstanceId = logEntryType.GetField("instanceID", BindingFlags.Instance | BindingFlags.Public);
            //_logEntryLine = logEntryType.GetField("line", BindingFlags.Instance | BindingFlags.Public);
            _logEntryCondition = logEntryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public);
        }
    }

    //    private static StackFrame GetListViewRowCount()
    //    {
    //        GetConsoleWindowListView();
    //        if (_logListView == null)
    //        {
    //            Log("GetListViewRowCount error,_logListView can not be null!!!");
    //            return null;
    //        }
    //
    //        int totalRows = (int)_logListViewTotalRows.GetValue(_logListView);
    //        int row = (int)_logListViewCurrentRow.GetValue(_logListView);
    //        int logByThisClassCount = 0;
    //
    //        for (int i = totalRows - 1; i >= row; i--)
    //        {
    //            _logEntriesGetEntry.Invoke(null, new object[] { i, _logEntry });
    //            string condition = _logEntryCondition.GetValue(_logEntry) as string;
    //            //判断是否是由LoggerUtility打印的日志  
    //            if (condition.Contains("LogUtility"))
    //                logByThisClassCount++;
    //        }
    //
    //        //同步日志列表，ConsoleWindow 点击Clear 会清理  
    //        while (_logStackFrameList.Count > totalRows)
    //            _logStackFrameList.RemoveAt(0);
    //        if (_logStackFrameList.Count >= logByThisClassCount)
    //            return _logStackFrameList[_logStackFrameList.Count - logByThisClassCount];
    //        return null;
    //    }


    /// <summary>
    /// 返回true意思是其他调用此方法的地方不用再打开"instanceID"脚本资源了，也就是在此方法已经处理（打开或者未打开asset）了
    /// </summary>
    /// <param name="instanceId">需要打开的脚本资源的instanceID</param>
    /// <param name="line">需要定位到的脚本的行号</param>
    /// <returns></returns>
    [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        //打开调用LogUtility输出日志的脚本资源
        if (instanceId == _instanceId)
        {
            int row = (int)_logListViewCurrentRow.GetValue(_logListView);
            _logEntriesGetEntry.Invoke(null, new object[] { row, _logEntry });
            string condition = _logEntryCondition.GetValue(_logEntry) as string;
            List<string> logs = condition.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!logs.Contains("UnityEngine.Debug:Log(Object)") && !logs.Contains("UnityEngine.Debug:LogWarning(Object)") && !logs.Contains("UnityEngine.Debug:LogError(Object)"))
            {
                LogUtil.Log("It is not debug msg,do not open Asset!!!");
                return true;
            }

            string targetDebugStr = string.Empty;
            for (int i = 0; i < logs.Count; i++)
            {
                if (logs[i].Equals("UnityEngine.Debug:Log(Object)") || logs[i].Equals("UnityEngine.Debug:LogWarning(Object)") || logs[i].Equals("UnityEngine.Debug:LogError(Object)"))
                {
                    if (i + 3 >= logs.Count)
                    {
                        LogUtil.Log("It is a debug or error of LogUtility,do not open Asset!!!");
                        return true;
                    }
                    targetDebugStr = logs[i + 3];
                    break;
                }
            }

            string tailPath = targetDebugStr.Substring(targetDebugStr.IndexOf("Assets/"));
            if (tailPath.Equals(targetDebugStr))
            {
                LogUtil.Log(string.Format("targetDebugStr:{0}  do not have Assets path,do not open Asset!!!", targetDebugStr));
                return true;
            }
            string fileAssetPath = tailPath.Split(':')[0];
            int assetline = int.Parse(tailPath.Split(':')[1].Replace(")", ""));

            if (fileAssetPath.Equals(typeof(LogUtil).FullName))
            {
                return true;
            }
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(fileAssetPath), assetline);
            return true;

            //var stackFrame = GetListViewRowCount();
            //if (stackFrame != null)
            //{
            //    string fileName = stackFrame.GetFileName();
            //    string fileAssetPath = fileName.Substring(fileName.IndexOf("Assets"));
            //    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(fileAssetPath), stackFrame.GetFileLineNumber());
            //    return true;
            //}
        }

        return false;
    }
#endif
}
