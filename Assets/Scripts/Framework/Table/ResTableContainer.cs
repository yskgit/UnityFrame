using UnityEngine;
using System.Collections.Generic;

public static class ResTableContainer
{
    //    public const char splitMark = ';';

    private const string RES_ROOT_NAME = "resRoot";

//    private static bool _loaded;
//    private static ResRoot _resRoot;
    private static readonly Dictionary<string, TableRec> _tableDic = new Dictionary<string, TableRec>();

    public static IResTable GetTable(string tableName)
    {
        TableRec ans;
        if (!_tableDic.TryGetValue(tableName, out ans))
        {
            var resRoot = LoadResRoot();
            for (int i = 0; i < resRoot.fileNames.Count; i++)
            {
                if (resRoot.fileNames[i].Equals(tableName))
                {
                    ans = new TableRec(resRoot.fileNames[i], resRoot.files[i]);
                    _tableDic.Add(resRoot.fileNames[i], ans);
                }
            }
        }
        return ans;
        //if (!_loaded)
        //{
        //    LoadTables();
        //}

        //TableRec ans;
        //_tableDic.TryGetValue(tableName, out ans);
        //if (ans == null)
        //{
        //    Debug.LogError("no table named by " + tableName);
        //}

        //return ans;
    }

    //public static void LoadTables()
    //{
    //    if (!_loaded)
    //    {
    //        _loaded = true;
    //        _tableDic.Clear();
    //        LoadRoot();
    //    }
    //}

    //static void LoadRoot()
    //{
    //    if (_resRoot != null) return;

    //    _resRoot = LoadResTable<ResRoot>(RES_ROOT_NAME, false);
    //    for (int i = 0; i < _resRoot.fileNames.Count; ++i)
    //    {
    //        var tableRec = new TableRec(_resRoot.fileNames[i], _resRoot.files[i]);
    //        _tableDic.Add(tableRec.TableName, tableRec);
    //    }
    //}

    private static ResRoot LoadResRoot()
    {
        return LoadResTable<ResRoot>(RES_ROOT_NAME, false);
    }

    private static T LoadResTable<T>(string resRootName, bool decode)
    {
        PathManager instance = SingletonScriptable<PathManager>.instance;
        resRootName += instance.ResTableExtension;
        return FileHelper.ReadPbFromFile<T>(instance.LocalResourcePath + instance.SubDir(ResourceType.Resource_ResTable) + resRootName, decode);
    }

    public static void UnloadAll()
    {
        //        _loaded = false;
        //        _resRoot = null;
        _tableDic.Clear();
    }

    public static string GetItemData(string tblName, string key, string colName)
    {
        if (string.IsNullOrEmpty(key)) return "";
        IResTable tbl = GetTable(tblName);
        return tbl.GetItemData(key, colName);
    }
}
