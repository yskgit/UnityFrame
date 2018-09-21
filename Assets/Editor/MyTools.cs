using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// 自定义某些简单编辑器功能
/// </summary>
public sealed class MyTools
{
    [MenuItem("MyTools/Delete All Local prefs", false, 0)]
    public static void DeleteAllLocalprefs()
    {
        MemoryHelper.DeleteAll();
    }

}
