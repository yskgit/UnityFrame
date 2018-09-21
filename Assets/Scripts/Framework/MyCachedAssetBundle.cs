using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 缓存的AssetBundle信息
/// </summary>
public class MyCachedAssetBundle
{
    /// <summary>
    /// 缓存的AssetBundle
    /// </summary>
    public AssetBundle _assetBundle;
    /// <summary>
    /// 生存时间长度。
    /// </summary>
    public const float Lifetime = 300f;
    /// <summary>
    /// 是否常驻（游戏运行期间不销毁）内存。暂时备用。
    /// </summary>
    public bool Permanent;
    /// <summary>
    /// AssetBundle缓存的场景，如大厅、商城等。用于清理对应场景的缓存。
    /// </summary>
    private readonly List<string> _group;
    /// <summary>
    /// AssetBundle上一次访问的时间。每次访问的时候赋值 Time.realtimeSinceStartup
    /// </summary>
    private float _lastAccessTime;

    public MyCachedAssetBundle(AssetBundle ab)
    {
        _group = new List<string>();
        AssetBundle = ab;
    }

    /// <summary>
    /// 缓存的AssetBundle
    /// </summary>
    public AssetBundle AssetBundle
    {
        get
        {
            _lastAccessTime = Time.realtimeSinceStartup;
            return _assetBundle;
        }
        private set
        {
            _lastAccessTime = Time.realtimeSinceStartup;
            _assetBundle = value;
        }
    }

    /// <summary>
    /// 添加AssetBundle缓存的场景，如大厅、商城等。用于清理对应场景的缓存。
    /// </summary>
    public MyCachedAssetBundle AddGroup(string groupName)
    {
        _group.Add(groupName);
        return this;
    }

    /// <summary>
    /// 获取AssetBundle缓存的场景，如大厅、商城等。用于清理对应场景的缓存。
    /// </summary>
    public List<string> GetGroup()
    {
        return _group;
    }

    /// <summary>
    /// AssetBundle上一次访问的时间。每次访问的时候赋值 Time.realtimeSinceStartup
    /// </summary>
    public float LastAccessTime
    {
        get { return _lastAccessTime; }
    }

    /// <summary>
    /// 缓存的AssetBundle是否过期了
    /// </summary>
    public bool IsDated()
    {
        return Time.realtimeSinceStartup - _lastAccessTime > 300f;
    }

    /// <summary>
    /// 获取已生存时间。上次访问到当前的间隔。
    /// </summary>
    /// <returns></returns>
    public float GetLastTime()
    {
        return Time.realtimeSinceStartup - _lastAccessTime;
    }

    /// <summary>
    /// AssetBundle是否存在于目标场景
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public bool IsCachedForGroup(string groupName)
    {
        return _group.Contains(groupName);
    }

    /// <summary>
    /// 移除AssetBundle缓存的场景名字
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public bool RemoveGroupName(string groupName)
    {
        return _group.Remove(groupName);
    }

    /// <summary>
    /// 是否有缓存场景
    /// </summary>
    /// <returns></returns>
    public bool HasGroupName()
    {
        return _group.Count > 0;
    }
}

/// <summary>
/// AssetBundle信息和加载路径。
/// </summary>
public class LoadBundleInfo
{
    public SingleBundleInfo BundleInfo;
    /// <summary>
    /// 加载路径。缓存（persistentDataPath）还是本地（StreamingAssetDataPath）。
    /// </summary>
    public string LoadUrl;

    public LoadBundleInfo(SingleBundleInfo bundleInfo, string loadUrl)
    {
        BundleInfo = bundleInfo;
        LoadUrl = loadUrl;
    }

    public override string ToString()
    {
        return BundleInfo + "," + "LoadUrl = " + LoadUrl;
    }
}

[System.Serializable]
public class AllBundleInfo
{

    public Dictionary<string, SingleBundleInfo> BundleInfoList;
    //public List<SingleBundleInfo> BundleInfoList;


    public AllBundleInfo()
    {
        //BundleInfoList = new List<SingleBundleInfo>();
        BundleInfoList = new Dictionary<string, SingleBundleInfo>();
    }
}

/// <summary>
/// AssetBundle信息
/// </summary>
[System.Serializable]
public class SingleBundleInfo
{
    public string bundleName;
    /// <summary>
    /// AssetBundle的CRC
    /// </summary>
    public uint bundleCRC;
    /// <summary>
    /// AssetBundle的hash值
    /// </summary>
    public string bundleHash128;
    /// <summary>
    /// AssetBundle的大小
    /// </summary>
    public float size;

    public override string ToString()
    {
        return $"bundleName = {bundleName},bundleCRC = {bundleCRC},bundleHash128 = {bundleHash128},size = {size}";
    }

    /// <summary>
    /// 比较AssetBundle的名字、crc、hash值是否相同
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
        if (!(obj is SingleBundleInfo))
        {
            return false;
        }
        SingleBundleInfo info = (SingleBundleInfo)obj;
        return bundleName.Equals(info.bundleName) && bundleCRC == info.bundleCRC && bundleHash128.Equals(info.bundleHash128);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}