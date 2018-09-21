using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableRec : IResTable
{
    /// <summary>
    /// 第一列的名字
    /// </summary>
    private string _firstColName;
    /// <summary>
    /// 第一列的信息，不包括列名
    /// </summary>
    private List<string> _firstColList;
    /// <summary>
    /// 列名对应的列数据
    /// </summary>
    private Dictionary<string, List<string>> _colsDic;
    /// <summary>
    /// 第一列的信息
    /// </summary>
    private Dictionary<string, int> _firstColDic;
    /// <summary>
    /// 表名
    /// </summary>
    private string _tableName;

    public TableRec(string tableName, ResFile resFile)
    {
        _colsDic = new Dictionary<string, List<string>>();
        _firstColDic = new Dictionary<string, int>();
        _firstColList = new List<string>();
        _tableName = tableName;

        for (int j = 0; j < resFile.items.Count; ++j)
        {
            _colsDic.Add(resFile.items[j].key, resFile.items[j].value);
        }

        _firstColName = resFile.items[0].key;
        SetPrimaryColInfo();
    }

    /// <summary>
    /// 设置第一列的信息
    /// </summary>
    private void SetPrimaryColInfo()
    {
        _firstColDic.Clear();
        var tmp = _colsDic[_firstColName];
        bool duplicated = false;
        for (int i = 0; i < tmp.Count; ++i)
        {
            if (_firstColDic.ContainsKey(tmp[i]))
            {
                duplicated = true;
                break;
            }
            _firstColDic.Add(tmp[i], i);
        }

        if (duplicated)
        {
            _firstColDic.Clear();
#if UNITY_EDITOR
            if (_tableName != "dirtyWords")
                Debug.LogError(string.Format("column'{0}' of table'{1}' has duplicated values, can't be used as the index column.",
                    _firstColName, _tableName));
#endif
        }

        _firstColList = tmp;
        _colsDic.Remove(_firstColName);
    }

    public void Clear()
    {
        _colsDic.Clear();
        _firstColDic.Clear();
        _firstColList.Clear();
        _firstColName = null;
    }

    /// <summary>
    /// 获取列数据
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public string[] GetColData(string columnName)
    {
        List<string> colList;
        if (!_colsDic.TryGetValue(columnName, out colList))
        {
            if (columnName != _firstColName)
            {
#if UNITY_EDITOR
                Debug.LogError("columnName " + columnName + " is not existed.");
#endif
                return null;
            }

            colList = _firstColList;
        }

        return colList.FindAll(x => x.Length > 0).ToArray();
    }

    /// <summary>
    /// 获取行数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string[] GetRowData(string key)
    {
        int rowId;
        if (!_firstColDic.TryGetValue(key, out rowId))
        {
#if UNITY_EDITOR && DEBUG_LOG
				Debug.LogWarning("key " + key + " is not existed.");
#endif
            return null;
        }

        var ans = new List<string>(_colsDic.Count);
        foreach (var pair in _colsDic)
        {
            if (pair.Key != key)
            {
                ans.Add(pair.Value[rowId]);
            }
        }

        //AOTSafe.Foreach<KeyValuePair<string, List<string>>>(col, pair => {
        //    if (pair.Key != key)
        //        ans.Add(pair.Value[rowId]);
        //});

        return ans.ToArray();
    }

    /// <summary>
    /// 获取行数据，拿到对应的列值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="row"></param>
    /// <param name="colNameOfRow"></param>
    /// <returns></returns>
    public bool GetRowData(string key, List<string> row, List<string> colNameOfRow)
    {
        int rowId;
        if (!_firstColDic.TryGetValue(key, out rowId))
        {
#if UNITY_EDITOR
            Debug.LogWarning("key " + key + " is not exited.");
#endif
            return false;
        }

        row.Clear();
        colNameOfRow.Clear();
        foreach (var pair in _colsDic)
        {
            if (pair.Key != key)
            {
                row.Add(pair.Value[rowId]);
                colNameOfRow.Add(pair.Key);
            }
        }
        return true;
    }

    /// <summary>
    /// 获取指定列名的行数据
    /// </summary>
    /// <param name="key"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public string[] GetRowData(string key, params string[] columnName)
    {
        int rowId;
        if (!_firstColDic.TryGetValue(key, out rowId))
        {
#if UNITY_EDITOR && DEBUG_LOG
				Debug.LogWarning("key " + key + " is not existed.");
#endif
            return null;
        }

        var ans = new List<string>(columnName.Length);
        List<string> selCol;
        for (int i = 0; i < columnName.Length; ++i)
        {
            if (_colsDic.TryGetValue(columnName[i], out selCol))
                ans.Add(selCol[rowId]);
            else
            {
                Debug.LogError("columnName " + columnName[i] + " is not existed.");
                ans.Add("");
            }
        }

        return ans.ToArray();
    }

    public bool HasKey(string key)
    {
        if (key == null)
        {
            Debug.LogError("key is null.");
            return false;
        }

        return _firstColDic.ContainsKey(key);
    }

    /// <summary>
    /// 根据行列获取单个值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public string GetItemData(string key, string columnName)
    {
        if (key == null)
        {
#if UNITY_EDITOR
            Debug.LogError("key is null. while columnName: " + columnName);
#endif
            return null;
        }

        int rowId;
        if (!_firstColDic.TryGetValue(key, out rowId))
        {
#if UNITY_EDITOR
            Debug.LogWarning("key " + key + " is not existed.");
#endif
            return null;
        }

        List<string> list;
        if (!_colsDic.TryGetValue(columnName, out list))
        {
#if UNITY_EDITOR
            Debug.LogError("columnName " + columnName + " is not existed.");
#endif
            return null;
        }

        return list[rowId];
    }

    public string[] GetAllData()
    {
        List<string> values = new List<string>();
        foreach (var item in _colsDic)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                values.Add(item.Value[i]);
            }
        }
        return values.ToArray();
    }

    public string TableName
    {
        get
        {
            return _tableName;
        }
    }
}
