using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

/*IResTable为字符串表(表格中每个元素均为字符串).若要使用整形或浮点型数据请自行转化.

IResTable从Execl转化到Json,再转化成加密pb文件(见TableConverter类).

其每一列有列表名,对应excel的表头.根据列名可以查询对应列的所有元素.
默认excel表中从左往右第一列为索引列. 下列函数中的参数key为索引列中某一行元素.
可通过key查询其所在行的所有元素（不包括key）,以及key所在行对应某一列的表格元素.*/
public interface IResTable
{
    string[] GetColData(string columnName);

    string GetItemData(string key, string columnName);
    string[] GetRowData(string key, params string[] columnName);
    string[] GetRowData(string key);
    bool GetRowData(string key, List<string> row, List<string> colNameOfRow);
    bool HasKey(string key);
    string[] GetAllData();
}

/// <summary>
/// 所有表的数据
/// </summary>
[ProtoContract]
public class ResRoot
{
    /// <summary>
    /// 表名
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    public List<string> fileNames;
    /// <summary>
    /// 所有的表数据
    /// </summary>
    [ProtoMember(2, IsRequired = true)]
    public List<ResFile> files;

    public ResRoot()
    {
        fileNames = new List<string>();
        files = new List<ResFile>();
    }
}

/// <summary>
/// 单个表的数据
/// </summary>
[ProtoContract]
public class ResFile
{
    /// <summary>
    /// 所有的列的数据
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    public List<ResItem> items;

    public ResFile()
    {
        items = new List<ResItem>();
    }
}

/// <summary>
/// 每一列的数据
/// </summary>
[ProtoContract]
public class ResItem
{
    /// <summary>
    /// 列名
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    public string key;
    /// <summary>
    /// 一列的所有值
    /// </summary>
    [ProtoMember(2, IsRequired = true)]
    public List<string> value;

    public ResItem()
    {
        value = new List<string>();
    }
}
