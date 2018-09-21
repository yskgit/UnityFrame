using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

public sealed class ObjectCache : SingletonBehaviour<ObjectCache>
{
    /// <summary>
    /// 是否正在缓存物体
    /// </summary>
    public bool IsCaching;
    /// <summary>
    /// 缓存的所有资源
    /// </summary>
    private Dictionary<string, ObjectCacheRec> _assets;
    /// <summary>
    /// 所有的对象池
    /// </summary>
    private Dictionary<string, List<GameObject>> _gameObjectPools;
    /// <summary>
    /// 上一次回收销毁物体的时间
    /// </summary>
    private float _lastCacheTime;
    /// <summary>
    /// 所有缓存池里的物体的父物体。只是用来存放使用，无实际意义
    /// </summary>
    private Transform _poolParent;

    protected override void Awake()
    {
        base.Awake();
        _assets = new Dictionary<string, ObjectCacheRec>();
        _gameObjectPools = new Dictionary<string, List<GameObject>>();

        _poolParent = new GameObject("GameObjectPoolParent").transform;
        _poolParent.SetParent(transform);
    }

    private void DoGC()
    {
        float realtimeSinceStartup = Time.realtimeSinceStartup;
        if (realtimeSinceStartup - _lastCacheTime > 1f)
        {
            _lastCacheTime = realtimeSinceStartup;
            //SingletonBehaviour<ResourceManager>.instance.GarbageCollect(true);
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }

    /// <summary>
    /// 直接从磁盘加载物体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public T LoadResource<T>(string assetName) where T : Object
    {
        return Resources.Load<T>(assetName);
    }

    /// <summary>
    /// 缓存物体
    /// </summary>
    /// <param name="isPermanent">是否永久缓存</param>
    /// <param name="groupName">缓存组名，用于标识资源所在分组</param>
    /// <param name="assetNames">需要缓存的所有的资源名字</param>
    /// <param name="onCaching">缓存的进度</param>
    /// <param name="onFinished">缓存结束回调</param>
    public void CacheAssetsAsync(bool isPermanent, string groupName, string[] assetNames, Action<float> onCaching, Action onFinished)
    {
        IsCaching = true;
        if (assetNames == null || assetNames.Length <= 0)
        {
            Debug.Log("assetNames error!!!");
            return;
        }
        StartCoroutine(StartCacheAssets(isPermanent, groupName, assetNames, onCaching, () =>
        {
            IsCaching = false;
            if (onFinished != null)
            {
                onFinished.Invoke();
            }
        }));
    }

    private IEnumerator StartCacheAssets(bool isPermanent, string groupName, string[] assetNames, Action<float> onCaching, Action onFinished)
    {
        for (int i = 0; i < assetNames.Length; i++)
        {
            ObjectCacheRec objectCacheRec;
            if (_assets.ContainsKey(assetNames[i]))
            {
                objectCacheRec = _assets[assetNames[i]];
                objectCacheRec.IsPermanent = (objectCacheRec.IsPermanent || isPermanent);
            }
            else
            {
                var request = Resources.LoadAsync(assetNames[i]);
                yield return request;
                objectCacheRec = new ObjectCacheRec();
                objectCacheRec.CachedObject = request.asset;
                _assets.Add(assetNames[i], objectCacheRec);
                objectCacheRec.IsPermanent = isPermanent;
            }
            objectCacheRec.AddGroupName(groupName);
            objectCacheRec.LastAccessTime = Time.realtimeSinceStartup;

            float pencentage = (float)(i + 1) / assetNames.Length;
            Debug.Log(string.Format("资源缓存进度:{0}", pencentage));
            if (onCaching != null)
            {
                onCaching.Invoke(pencentage);
            }
        }
        if (onFinished != null)
        {
            onFinished.Invoke();
        }
    }

    /// <summary>
    /// 生成物体对象池。assetName对应的必须要是已经缓存的物体
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="count"></param>
    public void CreateGameObjectPool(string assetName, int count)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            LogUtil.LogWarning("创建对象池失败，assetName不能为空!");
            return;
        }

        //从预先缓存的资源里获取到资源
        ObjectCacheRec objectCacheRec;
        if (!_assets.TryGetValue(assetName, out objectCacheRec))
        {
            Debug.LogWarning(string.Format("创建对象池失败，没有预先缓存资源:{0}", assetName));
            return;
        }

        if (!objectCacheRec.CachedObject)
        {
            Debug.LogWarning(string.Format("创建对象池失败，预先缓存资源“{0}”是空值!", assetName));
            return;
        }

        if (!(objectCacheRec.CachedObject is GameObject))
        {
            Debug.LogWarning(string.Format("创建对象池失败，缓存资源“{0}”类型不是GameObject!", assetName));
            return;
        }

        if (_gameObjectPools.ContainsKey(assetName))
        {
            LogUtil.LogWarning(string.Format("创建对象池失败，已经存在名字为“{0}”的对象池，" +
                                             "请使用“GetObjectToPool”方法获取对象池物体！", assetName));
            return;
        }

        List<GameObject> list = new List<GameObject>();
        _gameObjectPools.Add(assetName, list);

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(objectCacheRec.CachedObject) as GameObject;
            if (!obj)
            {
                Debug.LogWarning(string.Format("创建对象池失败，obj是空值，assetName:{0}", assetName));
                continue;
            }
            obj.name = assetName;
            obj.transform.SetParent(_poolParent);
            obj.SetActive(false);
            list.Add(obj);
        }
    }

    /// <summary>
    /// 检查是否存在某个对象池
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public bool CheckIsExistGameObjectPool(string assetName)
    {
        return _gameObjectPools.ContainsKey(assetName);
    }

    /// <summary>
    /// 检查对象池是否已经完全使用
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public bool CheckIsGameObjectPoolFullUse(string assetName)
    {
        if (!CheckIsExistGameObjectPool(assetName))
        {
            LogUtil.LogWarning("添加物体到对象池失败，assetName不能为空!");
            return false;
        }
        return _gameObjectPools.ContainsKey(assetName);
    }

    /// <summary>
    /// 添加物体对象池。assetName对应的必须要是已经缓存的物体
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="count"></param>
    public void AddGameObjectToPool(string assetName, int count = 1)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            LogUtil.LogWarning("添加物体到对象池失败，assetName不能为空!");
            return;
        }

        //获取对象池列表
        List<GameObject> list;
        if (!_gameObjectPools.TryGetValue(assetName, out list))
        {
            LogUtil.LogWarning(string.Format("添加物体到对象池失败，没有“{0}”的对象池!", assetName));
            return;
        }

        //从预先缓存的资源里获取到资源
        ObjectCacheRec objectCacheRec;
        if (!_assets.TryGetValue(assetName, out objectCacheRec))
        {
            Debug.LogWarning(string.Format("添加物体到对象池失败，没有预先缓存资源:{0}", assetName));
            return;
        }

        if (!objectCacheRec.CachedObject)
        {
            Debug.LogWarning(string.Format("添加物体到对象池失败，预先缓存资源“{0}”是空值!", assetName));
            return;
        }

        if (!(objectCacheRec.CachedObject is GameObject))
        {
            Debug.LogWarning(string.Format("添加物体到对象池失败，缓存资源“{0}”类型不是GameObject!", assetName));
            return;
        }

        //添加物体到对象池
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(objectCacheRec.CachedObject) as GameObject;
            if (!obj)
            {
                Debug.LogWarning(string.Format("添加物体到对象池失败，obj是空值，assetName:{0}", assetName));
                continue;
            }
            obj.name = assetName;
            obj.SetActive(false);
            list.Add(obj);
        }
    }

    /// <summary>
    /// 从物体对象池获取物体
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="active">获取到的物体初始active状态</param>
    /// <param name="autoAdd">当对象池物体被取光时，是否自动添加一个新物体到对象池</param>
    /// <returns></returns>
    public GameObject GetGameObjectFromPool(string assetName, bool active = true, bool autoAdd = true)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            LogUtil.LogWarning("获取对象池物体失败，assetName不能为空!");
            return null;
        }

        //获取对象池列表
        List<GameObject> list;
        if (!_gameObjectPools.TryGetValue(assetName, out list))
        {
            LogUtil.LogWarning(string.Format("获取对象池物体失败，没有“{0}”的对象池!", assetName));
            return null;
        }

        //检测添加新物体到对象池
        if (list.Count <= 0)
        {
            if (!autoAdd)
            {
                LogUtil.LogWarning(string.Format("获取对象池物体失败，对象池“{0}”中的对象已经被获取光!", assetName));
                return null;
            }
            AddGameObjectToPool(assetName);
        }

        //获取对象
        GameObject obj = list[list.Count - 1];
        //每次取出物体时，从列表移除物体
        list.Remove(obj);

        return obj;
    }

    /// <summary>
    /// 从物体对象池获取物体
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="count">获取物体的个数</param>
    /// <param name="active">获取到的物体初始active状态</param>
    /// <param name="autoAdd">当对象池物体被取光时，是否自动添加一个新物体到对象池</param>
    /// <returns></returns>
    public List<GameObject> GetGameObjectFromPool(string assetName, int count, bool active = true, bool autoAdd = true)
    {
        List<GameObject> objs = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            var obj = GetGameObjectFromPool(assetName, active, autoAdd);
            if (!obj)
            {
                return null;
            }
            objs.Add(obj);
        }
        return objs;
    }

    /// <summary>
    /// 拿到缓存的物体，拿到后必须自己再实例化一份，不能直接拿内存中的使用！
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public Object GetCacheAsset(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            LogUtil.LogWarning("获取缓存资源失败，assetName不能为空!");
            return null;
        }

        //先从缓存里拿去物体
        ObjectCacheRec objectCacheRec;
        if (!_assets.TryGetValue(assetName, out objectCacheRec))
        {
            Debug.LogWarning(string.Format("获取缓存资源失败，没有名字为{0}的缓存资源", assetName));
            return null;
        }

        if (!objectCacheRec.CachedObject)
        {
            Debug.LogWarning(string.Format("获取缓存资源失败，预先缓存资源“{0}”是空值!", assetName));
            return null;
        }

        objectCacheRec.LastAccessTime = Time.realtimeSinceStartup;

        return objectCacheRec.CachedObject;
    }

    /// <summary>
    /// 释放物体。Destroy掉。
    /// </summary>
    /// <param name="obj"></param>
    private void Free(Object obj)
    {
        if (obj == null)
        {
            return;
        }
        GameObject tempObj = obj as GameObject;
        if (tempObj != null && tempObj.activeSelf)
        {
            tempObj.SetActive(false);
        }
        Destroy(obj);
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    /// <param name="obj"></param>
    public void Recycle(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        List<GameObject> list;
        if (!_gameObjectPools.TryGetValue(obj.name, out list))
        {
            LogUtil.LogWarning(string.Format("回收对象失败，没有“{0}”的对象池!", obj.name));
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(_poolParent);

        list.Add(obj);
    }

    /// <summary>
    /// 清理掉指定缓存，包括Permanent的缓存
    /// </summary>
    public void Clear(params string[] assetNames)
    {
        for (int i = 0; i < assetNames.Length; i++)
        {
            ObjectCacheRec objectCacheRec;
            if (_assets.TryGetValue(assetNames[i], out objectCacheRec))
            {
                if (!objectCacheRec.IsPermanent)
                {
                    _assets.Remove(assetNames[i]);
                    Debug.Log(string.Format("移除资源{0}", assetNames[i]));
                }
                else
                {
                    Debug.LogWarning(string.Format("移除资源失败，不能移除isPermanent资源{0}", assetNames[i]));
                }
            }
            else
            {
                Debug.LogWarning(string.Format("移除资源失败，未找到缓存资源{0}", assetNames[i]));
            }

            List<GameObject> list;
            if (_gameObjectPools.TryGetValue(assetNames[i], out list))
            {
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    Free(list[j]);
                }
                _gameObjectPools.Remove(assetNames[i]);
            }
        }
        DoGC();
    }

    /// <summary>
    /// 根据条件清理缓存
    /// </summary>
    /// <param name="func"></param>
    private void Clear(Func<KeyValuePair<string, ObjectCacheRec>, bool> func)
    {
        List<string> toClear = new List<string>();
        AOTSafe.Foreach(_assets, delegate (KeyValuePair<string, ObjectCacheRec> pair)
        {
            if (func(pair))
            {
                toClear.Add(pair.Key);
            }
        });
        Clear(toClear.ToArray());
    }

    /// <summary>
    /// 清理掉对应组的缓存
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="withPermanent">是否移除“永久”资源</param>
    public void ClearGroup(string groupName, bool withPermanent = false)
    {
        Clear(delegate (KeyValuePair<string, ObjectCacheRec> pair)
        {
            if (!pair.Value.HasGroupName())
            {
                return false;
            }
            if (!pair.Value.IsCachedForGroup(groupName))
            {
                return false;
            }

            if (withPermanent)
            {
                pair.Value.RemoveGroupName(groupName);
                return true;
            }

            if (pair.Value.IsPermanent)
            {
                return false;
            }

            pair.Value.RemoveGroupName(groupName);
            return true;
        });
    }

    /// <summary>
    /// 清理掉非永久的缓存，暂时不用，返回界面的时候清理缓存
    /// </summary>
    private void ClearImpermanent()
    {
        Clear(pair => !pair.Value.IsPermanent);
    }

    /// <summary>
    /// 清理超时且非Permanent的缓存，暂时不用，返回界面的时候清理缓存
    /// </summary>
    private void ClearTimeOut()
    {
        Clear(pair => pair.Value.IsDated && !pair.Value.IsPermanent);
    }

    /// <summary>
    /// 清理掉所有的缓存,退出游戏的时候才能调用
    /// </summary>
    private void ClearAll()
    {
        _assets.Clear();
        DoGC();
    }
}
