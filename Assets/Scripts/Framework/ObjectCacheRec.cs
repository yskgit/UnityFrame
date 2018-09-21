using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

internal class ObjectCacheRec
{
    public const float DatedTime = 300f;

    private bool _isPermanent;
    private Object _cachedObject;
    private float _lastAccessTime;
    private readonly List<string> _groupNames;

    public ObjectCacheRec()
    {
        _groupNames = new List<string>();
    }

    /// <summary>
    /// 缓存的物体
    /// </summary>
	public Object CachedObject
    {
        get
        {
            return _cachedObject;
        }
        set
        {
            _cachedObject = value;
        }
    }

    /// <summary>
    /// 上一次访问的事件
    /// </summary>
	public float LastAccessTime
    {
        get
        {
            return _lastAccessTime;
        }
        set
        {
            _lastAccessTime = value;
        }
    }

    /// <summary>
    /// 当前资源是否永久存在于缓存
    /// </summary>
    public bool IsPermanent
    {
        get
        {
            return _isPermanent;
        }
        set
        {
            _isPermanent = value;
        }
    }

    /// <summary>
    /// 是否过期了
    /// </summary>
	public bool IsDated
    {
        get
        {
            return Time.realtimeSinceStartup - _lastAccessTime > DatedTime;
        }
    }

    /// <summary>
    /// 是否缓存了该分组
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
	public bool IsCachedForGroup(string groupName)
    {
        return _groupNames.Contains(groupName);
    }

    /// <summary>
    /// 是否有分组
    /// </summary>
    /// <returns></returns>
	public bool HasGroupName()
    {
        return _groupNames.Count > 0;
    }

    /// <summary>
    /// 移除分组
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
	public bool RemoveGroupName(string groupName)
    {
        return _groupNames.Remove(groupName);
    }

    /// <summary>
    /// 添加分组
    /// </summary>
    /// <param name="groupName"></param>
	public void AddGroupName(string groupName)
    {
        if (!_groupNames.Contains(groupName))
        {
            _groupNames.Add(groupName);
        }
    }
}
