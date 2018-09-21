using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class AssetBundleManager : SingletonBehaviour<AssetBundleManager>
{

    /// <summary>
    /// 服务器检测AssetBundle更新的地址
    /// </summary>
    private string _baseURL;

    /// <summary>
    /// 从服务器下载的ABConfig文件内容
    /// </summary>
    private string _serverABConfig;

    /// <summary>
    /// AssetBundleManifest记录所有AssetBundle的信息（依赖、hash值等），加载AssetBundle时需要用到
    /// </summary>
    private AssetBundleManifest _assetBundleManifest;

    /// <summary>
    /// 所有AssetBundle加载的路径。从缓存（persistentDataPath）还是本地（StreamingAssetDataPath）加载。下载完成后，记录所有资源的加载路径。
    /// </summary>
    private Dictionary<string, LoadBundleInfo> _bundleInfoDic;

    /// <summary>
    /// 缓存起来的AssetBundle
    /// </summary>
    private Dictionary<string, MyCachedAssetBundle> _cachedAssetBundleDic;

    /// <summary>
    /// 所缓存起来的Asset资源。asset是AssetBundle里的资源。暂时未用到。因为资源暂时未缓存。现在缓存的最小单位是AssetBundle。
    /// </summary>
    private Dictionary<string, Object> _cachedAssetDic;

    protected override void Awake()
    {
        base.Awake();

        _cachedAssetBundleDic = new Dictionary<string, MyCachedAssetBundle>();
        _bundleInfoDic = new Dictionary<string, LoadBundleInfo>();
        _cachedAssetDic = new Dictionary<string, Object>();

        _baseURL = FileHelper.ReadConfig("AssetBundleBaseUrl");
    }

    private void SetAssetBundleManifest(AssetBundleManifest abm)
    {
        _assetBundleManifest = abm;
    }

    public AssetBundleManifest GetAssetBundleManifest()
    {
        //_assetBundleManifest 必须在下载更新完成时赋值，
        //不能提前获取本地 AssetBundleManifest 给_assetBundleManifest赋值，数据不准确。
        if (_assetBundleManifest == null)
        {
            LogUtil.LogError("AssetBundleManifest 为空，请先检查更新，并下载更新！");
        }
        return _assetBundleManifest;
    }

    /// <summary>
    /// 检查是否有更新。回调方法返回更新列表，更新列表数量大于0说明有更新，否则无更新。
    /// Dictionary<string, SingleBundleInfo>为更新的列表
    /// </summary>
    /// <param name="onFinished"></param>
    public void CheckUpdateAsync(Action<Dictionary<string, SingleBundleInfo>> onFinished)
    {
        if (HasCheckUpdate())
        {
            LogUtil.LogWarning("已经检查过更新，无需再次检查更新，返回null");
            onFinished?.Invoke(null);
            return;
        }

        StartCoroutine(DoCheckUpdate(onFinished));
    }

    /// <summary>
    /// 下载服务器的ABConfig.json与本地ABConfig.json对比差异，有差异则下载更新
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoCheckUpdate(Action<Dictionary<string, SingleBundleInfo>> onFinished)
    {
        string abConfigUrl = _baseURL + "ABConfig.json";

        UnityWebRequest request = UnityWebRequest.Get(abConfigUrl);
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            LogUtil.LogError($"下载AssetBundle版本文件时发生错误，url:{abConfigUrl},error:{request.error}");
            yield break;
        }

        _serverABConfig = request.downloadHandler.text;
        request.Dispose();

        Debug.Log("下载AssetBundle的版本文件:\n" + _serverABConfig);
        var serverAllBundleInfo = JsonConvert.DeserializeObject<AllBundleInfo>(_serverABConfig);

        if (serverAllBundleInfo == null)
        {
            LogUtil.LogError("下载AssetBundle版本文件解析出来是空值！");
            yield break;
        }

        string streamingAssetsABconfigPath = Application.streamingAssetsPath + "/ABconfig.json";//本地路径
        if (!File.Exists(streamingAssetsABconfigPath))
        {
            LogUtil.LogError("streamingAssetsPath文件夹没有ABconfig.json文件！");
            yield break;
        }

        //缓存文件夹没有 ABconfig.json ，则从 streamingAssetsPath 文件下拷贝过去
        string persistentABconfigPath = Application.persistentDataPath + "/ABconfig.json";//缓存路径
        if (!File.Exists(persistentABconfigPath))
        {
            FileInfo fileInfo = new FileInfo(streamingAssetsABconfigPath);
            fileInfo.CopyTo(persistentABconfigPath);
            Debug.Log("成功从“streamingAssetsPath”拷贝ABconfig.json到“persistentDataPath”！");
        }
        else
        {
            Debug.Log("“persistentDataPath”路径下存在ABconfig.json文件！");
        }

        //打开缓存的 ABconfig.json，比较差别，检测是否需要更新
        string abConfig = FileHelper.ReadStrFromFile(persistentABconfigPath, false);
        var localAllBundleInfo = JsonConvert.DeserializeObject<AllBundleInfo>(abConfig);

        if (localAllBundleInfo == null)
        {
            LogUtil.LogError("缓存文件夹下ABconfig.json文件格式错误！");
            yield break;
        }

        Debug.Log("本地AssetBundle版本文件：\n" + JsonConvert.SerializeObject(localAllBundleInfo, Formatting.Indented));

        //获取需要更新的assetbundle。
        Dictionary<string, SingleBundleInfo> changedDic = GetChangedBundleInfo(serverAllBundleInfo.BundleInfoList, localAllBundleInfo.BundleInfoList);

        foreach (var item in changedDic)
        {
            List<Hash128> lis = new List<Hash128>();
            Caching.GetCachedVersions(item.Value.bundleName, lis);
            foreach (var hash128 in lis)
            {
                Debug.Log(string.Format("AssetBundle:{0},hash128:{1}", item.Value.bundleName, hash128));
            }
        }

        Debug.Log(string.Format("需要更新的资源有{0}个，列表如下:", changedDic.Count));
        foreach (var item in changedDic)
        {
            Debug.Log(item.Value.bundleName);
        }

        //没有资源更新的情况
        if (changedDic.Count == 0)
        {
            InitBasicData();
        }

        onFinished?.Invoke(changedDic);
    }

    /// <summary>
    /// 是否已经检测更新了。
    /// </summary>
    /// <returns></returns>
    private bool HasCheckUpdate()
    {
        //_serverABConfig会在检查更新的时候从服务器下载并赋值
        return !string.IsNullOrEmpty(_serverABConfig);
    }

    /// <summary>
    /// 异步下载资源
    /// </summary>
    /// <param name="changedDic">需要下载的资源</param>
    /// <param name="progress">下载进度，int为正在下载文件是第几个，float是当前文件下载的进度</param>
    /// <param name="onFinished">下载完成或者失败的回调。失败请退出游戏</param>
    public void DownloadUpdateAsync(Dictionary<string, SingleBundleInfo> changedDic, Action<string, int, float> progress, Action<bool> onFinished)
    {
        if (!HasCheckUpdate())
        {
            LogUtil.LogError("请先检查更新");
            return;
        }

        StartCoroutine(DoDownloadUpdate(changedDic, progress, onFinished));
    }

    private IEnumerator DoDownloadUpdate(Dictionary<string, SingleBundleInfo> changedDic, Action<string, int, float> progress, Action<bool> onFinished)
    {
        //如果缓存系统还没准备好，等待
        while (!Caching.ready)
            yield return null;

        string persistentABconfigPath = Application.persistentDataPath + "/ABconfig.json";//缓存路径

        string abConfig = FileHelper.ReadStrFromFile(persistentABconfigPath, false);
        var localAllBundleInfo = JsonConvert.DeserializeObject<AllBundleInfo>(abConfig);
        var serverAllBundleInfo = JsonConvert.DeserializeObject<AllBundleInfo>(_serverABConfig);

        int index = 1;
        foreach (var singleBundleInfo in changedDic)
        {
            Debug.Log(string.Format("下载AssetBundle的信息：{0}", singleBundleInfo));
            //mainmanifest没有hash值，当使用hash和crc一起请求服务器时，会返回错误：CRC Mismatch. Provided 5c70b10e, calculated a4189913 from data. 
            //“5c70b10e”是我向服务器请求的crc值，“a4189913”是在缓存的crc值。
            //所以推断：GetAssetBundle(string uri, Hash128 hash, uint crc)这个API是先从本地查找有无当前hash值得缓存
            //1、有，则 hash值配合crc获取到本地资源，但是mainmanifest hash值相对，crc不能对应，并报错“CRC Mismatch”。
            //2、无，则向服务器请求更新。
            //而 “CRC”用于判断从服务器下载的AssetBundle的完整性，所以，尽量使用 GetAssetBundle(string uri, Hash128 hash, uint crc) 这个API
            //所以：检测“mainmanifest”的时候需要特殊判断
            UnityWebRequest request;
            if (CheckIsMainManifest(singleBundleInfo.Value))
            {
                request = UnityWebRequest.GetAssetBundle(_baseURL + singleBundleInfo.Key, Hash128.Parse(singleBundleInfo.Value.bundleHash128), 0);
            }
            else
            {
                request = UnityWebRequest.GetAssetBundle(_baseURL + singleBundleInfo.Key,
                    Hash128.Parse(singleBundleInfo.Value.bundleHash128), singleBundleInfo.Value.bundleCRC);
            }

            request.SendWebRequest();
            while (!request.isDone)
            {
                progress?.Invoke(singleBundleInfo.Value.bundleName, index, request.downloadProgress);
                yield return null;
            }

            //下载失败
            if (request.error != null)
            {
                Debug.LogError(string.Format("下载AssetBundle失败，url:{0},error:{1}", _baseURL + singleBundleInfo.Key, request.error));
                request.Dispose();
                onFinished?.Invoke(false);
                yield break;
            }

            request.Dispose();

            Debug.Log(string.Format("下载AssetBundle成功，url:{0}", _baseURL + singleBundleInfo.Value.bundleName));
            //下载成功。
            //下载成功的AssetBundle会自动缓存到“persistentDataPath”路径，需要使用的时候就可以从缓存路径直接加载
            //所以，只需要更新ABConfig.json，记录已下载的更新。

            //服务器下载的AssetBundle如果在本地有相同名字的AssetBundle，则替换掉本地的AssetBundle
            var bundleInfo = serverAllBundleInfo.BundleInfoList[singleBundleInfo.Key];
            if (localAllBundleInfo.BundleInfoList.ContainsKey(singleBundleInfo.Value.bundleName))
            {
                Debug.Log(string.Format("资源：{0} 属于更新资源！", bundleInfo.bundleName));
                localAllBundleInfo.BundleInfoList[bundleInfo.bundleName] = bundleInfo;
            }
            //如果在本地没有相同名字的AssetBundle，则增加到本地
            else
            {
                Debug.Log(string.Format("资源：{0} 属于下载资源！", bundleInfo.bundleName));
                localAllBundleInfo.BundleInfoList.Add(bundleInfo.bundleName, bundleInfo);
            }

            //每下载一个bundle就更新一次本地AbConfig.json文件，
            //这样即使玩家在下载途中退出游戏，再次进入游戏时，也可以继续更新
            string tempAbConfig = JsonConvert.SerializeObject(localAllBundleInfo, Formatting.Indented);
            using (StreamWriter writer = new StreamWriter(persistentABconfigPath, false))
            {
                writer.Write(tempAbConfig);
                writer.Flush();
            }
        }

        InitBasicData();

        onFinished?.Invoke(true);

        ////获取AssetBundleManifest。
        ////1、获取StreamingAsset下的AssetBundleManifest，AssetBundleManifest可能过时，所以不采用。
        ////2、获取缓存的AssetBundleManifest。所有采用“UnityWebRequest.GetAssetBundle”获取本地缓存或者服务器请求。
        //var mainManifestBundleInfo = GetMainManifestBundleInfo(serverAllBundleInfo);
        //Debug.Log("是否缓存Mainmanifest:" + Caching.IsVersionCached(mainManifestBundleInfo.bundleName, Hash128.Parse(mainManifestBundleInfo.bundleHash128)));
        ////清理掉AssetBundleManifest的缓存，因为AssetBundleManifest的hash值一直为“00000000000000000000000000000000”，
        ////导致缓存里的AssetBundleManifest数据不能更新，所以每次需要清除掉AssetBundleManifest缓存，重新下载。
        //Caching.ClearCachedVersion(mainManifestBundleInfo.bundleName,
        //    Hash128.Parse(mainManifestBundleInfo.bundleHash128));

        ////注意：以下两个方法不会再本地生成缓存文件
        ////request = UnityWebRequest.GetAssetBundle(_baseURL + mainManifestBundleInfo.bundleInfo);
        ////request = UnityWebRequest.GetAssetBundle(_baseURL + mainManifestBundleInfo.bundleInfo, mainManifestBundleInfo.bundleCRC);
        ////以下方法会在本地生成缓存文件
        //UnityWebRequest request = UnityWebRequest.GetAssetBundle(_baseURL + mainManifestBundleInfo.bundleName, Hash128.Parse(mainManifestBundleInfo.bundleHash128), 0);
        //yield return request.SendWebRequest();
        //if (request.error != null)
        //{
        //    Debug.LogError(string.Format("下载AssetBundle失败，url:{0},error:{1}", _baseURL + mainManifestBundleInfo.bundleName, request.error));
        //    request.Dispose();
        //    yield break;
        //}
        //AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(request);
        //request.Dispose();
        ////获取AssetBundless.manifest的manifest文件
        //AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        //assetBundle.Unload(false);
        //SetAssetBundleManifest(manifest);

        //assetBundle.mainAsset


        //Dictionary<string, AssetBundle> cacheAssetBundles = new Dictionary<string, AssetBundle>();
        ////加载 manifest 中所有的AssetBundle到内存。（TODO：暂时做测试使用）
        //foreach (var bundle in manifest.GetAllAssetBundles())
        //{
        //    LoadBundleInfo loadBundleInfo;
        //    if (!_bundleInfoDic.TryGetValue(bundle, out loadBundleInfo))
        //    {
        //        continue;
        //    }

        //    Debug.Log("loadBundleInfo = " + loadBundleInfo);

        //    //request = UnityWebRequest.GetAssetBundle(loadBundleInfo.LoadUrl,Hash128.Parse(loadBundleInfo.BundleInfo.bundleHash128), loadBundleInfo.BundleInfo.bundleCRC);
        //    //从streamingAssetsPath加载使用“AssetBundle.LoadFromFile”方法，更快。
        //    if (loadBundleInfo.LoadUrl.Contains(Application.streamingAssetsPath))
        //    {
        //        AssetBundle streamAssetBundle = AssetBundle.LoadFromFile(loadBundleInfo.LoadUrl);

        //        if (streamAssetBundle)
        //        {
        //            cacheAssetBundles.Add(streamAssetBundle.name, streamAssetBundle);
        //            streamAssetBundle.Unload(false);
        //        }
        //        else
        //        {
        //            Debug.LogError("LoadFromFile失败!");
        //        }
        //    }
        //    //否则加载使用“UnityWebRequest.GetAssetBundle”方法，此方法会自动从缓存加载数据，
        //    //若缓存下没有数据，则会从服务器下载数据，并缓存到本地。
        //    else
        //    {
        //        request = UnityWebRequest.GetAssetBundle(loadBundleInfo.LoadUrl, Hash128.Parse(loadBundleInfo.BundleInfo.bundleHash128), 0);
        //        yield return request.SendWebRequest();
        //        if (request.error != null)
        //        {
        //            Debug.LogError(string.Format("下载AssetBundle失败，url:{0},error:{1}", _baseURL + bundle, request.error));
        //            request.Dispose();
        //            continue;
        //        }
        //        AssetBundle tempBundle = DownloadHandlerAssetBundle.GetContent(request);
        //        request.Dispose();
        //        cacheAssetBundles.Add(tempBundle.name, tempBundle);
        //        tempBundle.Unload(false);
        //    }
        //}
    }

    /// <summary>
    /// 初始化核心的数据
    /// </summary>
    private void InitBasicData()
    {
        //下载完成后，记录所有资源的加载路径
        _bundleInfoDic = GetAllLoadBundleInfo();

        var serverAllBundleInfo = JsonConvert.DeserializeObject<AllBundleInfo>(_serverABConfig);
        //加载 AssetBundleManifest 
        SingleBundleInfo mainManifestBundleInfo = GetMainManifestBundleInfo(serverAllBundleInfo);

        LoadBundleInfo manifestInfo;
        if (_bundleInfoDic.TryGetValue(mainManifestBundleInfo.bundleName, out manifestInfo))
        {
            AssetBundleManifest manifest = LoadAsset<AssetBundleManifest>(mainManifestBundleInfo.bundleName, "AssetBundleManifest");
            SetAssetBundleManifest(manifest);
        }
    }

    /// <summary>
    /// 异步加载AssetBundle里的资源集合。AssetBundle和assetName是一对多的关系。
    /// </summary>
    /// <param name="assetBundleName">AssetBundle名字</param>
    /// <param name="assetNames">加载的资源名字集合</param>
    /// <param name="onFinished">加载完成回调</param>
    public void LoadAssetAsync(string assetBundleName, string[] assetNames, Action<Dictionary<string, Object>> onFinished)
    {
        StartCoroutine(DoLoadAssetAsync(assetBundleName, assetNames, onFinished));
    }

    private IEnumerator DoLoadAssetAsync(string assetBundleName, string[] assetNames, Action<Dictionary<string, Object>> onFinished)
    {
        yield return new WaitForFixedUpdate();
        Dictionary<string, Object> abDic = LoadAsset(assetBundleName, assetNames);
        onFinished?.Invoke(abDic);
    }

    /// <summary>
    /// 同步加载AssetBundle里的资源集合。AssetBundle和assetName是一对多的关系。
    /// </summary>
    /// <param name="assetBundleName">AssetBundle名字</param>
    /// <param name="assetNames">加载的资源名字集合</param>
    public Dictionary<string, Object> LoadAsset(string assetBundleName, string[] assetNames)
    {
        if (string.IsNullOrEmpty(assetBundleName) || assetNames == null || assetNames.Length == 0)
        {
            LogUtil.LogWarning("加载资源失败，参数不正确");
            return null;
        }

        Dictionary<string, AssetBundle> abDic = LoadAssetBundle(new[] { assetBundleName });
        Dictionary<string, Object> assets = new Dictionary<string, Object>();
        AssetBundle ab;
        if (abDic.TryGetValue(assetBundleName, out ab))
        {
            foreach (var assetName in assetNames)
            {
                Object asset = ab.LoadAsset(assetName);
                if (!asset)
                {
                    LogUtil.LogError($"加载资源失败，AssetBundle:{assetBundleName},资源:{assetName}");
                }
                else
                {
                    assets.Add(assetName, asset);
                }
            }
            if (!_cachedAssetBundleDic.ContainsKey(assetBundleName))
            {
                ab.Unload(false);
            }
        }
        else
        {
            LogUtil.LogError($"加载资源{assetBundleName}失败");
        }

        return assets;
    }

    /// <summary>
    /// 异步加载AssetBundle里的单个资源。AssetBundle和assetName是一对多的关系。
    /// </summary>
    /// <param name="assetBundleName">AssetBundle名字</param>
    /// <param name="assetName">加载的资源名字集合</param>
    /// <param name="onFinished">加载完成回调</param>
    public void LoadAssetAsync(string assetBundleName, string assetName, Action<Dictionary<string, Object>> onFinished)
    {
        LoadAssetAsync(assetBundleName, new[] { assetName }, onFinished);
    }

    /// <summary>
    /// 同步加载AssetBundle里的单个资源。AssetBundle和assetName是一对多的关系。比较费时，推荐异步加载
    /// </summary>
    /// <param name="assetBundleName">AssetBundle名字</param>
    /// <param name="assetName">加载的资源名字集合</param>
    /// <param name="onFinished">加载完成回调</param>
    public Object LoadAsset(string assetBundleName, string assetName)
    {
        Dictionary<string, Object> dic = LoadAsset(assetBundleName, new[] { assetName });
        return dic[assetBundleName];
    }

    /// <summary>
    /// 同步加载AssetBundle里的单个资源。AssetBundle和assetName是一对多的关系。比较费时，推荐异步加载
    /// </summary>
    /// <param name="assetBundleName">AssetBundle名字</param>
    /// <param name="assetName">加载的资源名字集合</param>
    public T LoadAsset<T>(string assetBundleName, string assetName) where T : Object
    {
        Dictionary<string, Object> dic = LoadAsset(assetBundleName, new[] { assetName });
        return dic[assetName] as T;
    }

    /// <summary>
    /// 同步加载AssetBundle
    /// </summary>
    /// <param name="bundleNames">需要加载的bundle集合</param>
    /// <param name="groupName">缓存AssetBundle的场景。groupName不为空，则缓存AssetBundle。用于离开场景时销毁AssetBundle</param>
    /// <returns></returns>
    public Dictionary<string, AssetBundle> LoadAssetBundle(string[] bundleNames, string groupName = "")
    {
        //本次加载的AssetBundle集合
        Dictionary<string, AssetBundle> loadedAssetBundleDic = new Dictionary<string, AssetBundle>();

        foreach (var bundleName in bundleNames)
        {
            MyCachedAssetBundle cab = LoadSingle(bundleName, groupName);

            if (cab == null)
            {
                continue;
            }

            loadedAssetBundleDic.Add(bundleName, cab.AssetBundle);
        }
        return loadedAssetBundleDic;
    }

    /// <summary>
    /// 异步加载AssetBundle。groupName不为空则缓存场景，缓存后读取更快速。
    /// </summary>
    /// <param name="bundleNames">需要加载的bundle集合</param>
    /// <param name="onFinished">加载完成回调</param>
    /// <param name="groupName">缓存AssetBundle的场景。groupName不为空，则缓存AssetBundle。用于离开场景时销毁AssetBundle</param>
    public void LoadAssetBundleAsync(string[] bundleNames, Action<Dictionary<string, AssetBundle>> onFinished, string groupName = "")
    {
        StartCoroutine(DoLoadAssetBundleAsync(bundleNames, onFinished, groupName));
    }

    private IEnumerator DoLoadAssetBundleAsync(string[] bundleNames, Action<Dictionary<string, AssetBundle>> onFinished, string groupName)
    {
        //协程会同步执行“yield return”之前的代码，执行到“yield return”时，“开启异步模式”（）并非真正的异步。
        //所以“yield return”后的代码可以用同步方法执行了。
        yield return new WaitForFixedUpdate();

        //本次加载的AssetBundle集合
        Dictionary<string, AssetBundle> loadedAssetBundleDic = LoadAssetBundle(bundleNames, groupName);

        onFinished?.Invoke(loadedAssetBundleDic);
    }

    /// <summary>
    /// 同步加载单个AssetBundle
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="groupName"></param>
    /// <returns></returns>
    private MyCachedAssetBundle LoadSingle(string bundleName, string groupName)
    {
        LoadBundleInfo loadBundleInfo;
        if (!_bundleInfoDic.TryGetValue(bundleName, out loadBundleInfo))
        {
            LogUtil.LogError($"加载AssetBundle失败，AssetBundle“{bundleName}”不存在");
            return null;
        }

        MyCachedAssetBundle temp;
        if (_cachedAssetBundleDic.TryGetValue(bundleName, out temp))
        {
            if (string.IsNullOrEmpty(groupName))
            {
                temp.AddGroup(groupName);
            }
            return temp;
        }

        Debug.Log("loadBundleInfo = " + loadBundleInfo);

        LoadDependecies(bundleName, groupName);

        AssetBundle ab;
        //从streamingAssetsPath加载使用“AssetBundle.LoadFromFile”方法，同步加载更快。
        if (loadBundleInfo.LoadUrl.Contains(Application.streamingAssetsPath))
        {
            ab = AssetBundle.LoadFromFile(loadBundleInfo.LoadUrl);
        }
        //否则加载使用“UnityWebRequest.GetAssetBundle”方法，此方法会自动从缓存加载数据，
        //若缓存下没有数据，则会从服务器下载数据，并缓存到本地。
        else
        {
            UnityWebRequest request = UnityWebRequest.GetAssetBundle(loadBundleInfo.LoadUrl, Hash128.Parse(loadBundleInfo.BundleInfo.bundleHash128), 0);
            request.SendWebRequest();

            while (!request.isDone)
            {
                Debug.Log(string.Format("Load AssetBundle :{0},progress = {1}", loadBundleInfo.BundleInfo.bundleName, request.downloadProgress));
            }

            if (request.error != null)
            {
                Debug.LogError($"加载AssetBundle失败，url:{loadBundleInfo.LoadUrl},error:{request.error}");
                request.Dispose();
                return null;
            }

            ab = DownloadHandlerAssetBundle.GetContent(request);
            request.Dispose();
        }

        MyCachedAssetBundle cab = new MyCachedAssetBundle(ab);
        if (!string.IsNullOrEmpty(groupName))
        {
            cab.AddGroup(groupName);
            _cachedAssetBundleDic.Add(bundleName, cab);
        }
        return cab;
    }

    private void LoadDependecies(string abName, string groupName)
    {
        if (_assetBundleManifest == null)
        {
            Debug.Log($"_assetBundleManifest还未初始化，未获取到{abName}的依赖信息");
            return;
        }
        string[] dependencys = _assetBundleManifest.GetDirectDependencies(abName);//得到需要加载的资源的依赖关系
        Debug.Log(string.Format("资源{0}的依赖数量为:{1}", abName, dependencys.Length));
        foreach (var item in dependencys) //加载所有的依赖
        {
            Debug.Log($"资源{abName}的依赖有:{item}");
        }
        foreach (var dependency in dependencys) //加载所有的依赖
        {
            LoadSingle(dependency, groupName);
        }
    }

    /// <summary>
    /// 清除除了allBundleInfo中其他版本号的资源
    /// </summary>
    /// <param name="allBundleInfo"></param>
    private void ClearOtherCachedVersions(AllBundleInfo allBundleInfo)
    {
        foreach (var singleBundleInfo in allBundleInfo.BundleInfoList)
        {
            bool success = Caching.ClearOtherCachedVersions(singleBundleInfo.Value.bundleName, Hash128.Parse(singleBundleInfo.Value.bundleHash128));

            string bundleName = singleBundleInfo.Value.bundleName;
            Hash128 hash128 = Hash128.Parse(singleBundleInfo.Value.bundleHash128);
            Debug.Log(string.Format("缓存bundleName:{0}，清理版本号hash128:{1}之外的资源,success:{2}", bundleName, hash128, success));
        }
    }

    /// <summary>
    /// 检测是否是主Manifest
    /// </summary>
    /// <param name="bundleInfo"></param>
    /// <returns></returns>
    private bool CheckIsMainManifest(SingleBundleInfo bundleInfo)
    {
        return bundleInfo.bundleHash128.Equals("00000000000000000000000000000000");
    }

    private SingleBundleInfo GetMainManifestBundleInfo(AllBundleInfo allBundleInfo)
    {
        foreach (var bundleInfo in allBundleInfo.BundleInfoList)
        {
            if (CheckIsMainManifest(bundleInfo.Value))
            {
                return bundleInfo.Value;
            }
        }
        return null;
    }

    /// <summary>
    /// 确定所有AssetBundle的加载路径。
    /// 1、persistent有streaming没有，则加载路径为persistent
    /// 2、persistent有streaming都有，但是信息不一致，则加载路径为persistent
    /// 3、persistent有streaming都有，且信息一致，则加载路径为streamingAssets
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, LoadBundleInfo> GetAllLoadBundleInfo()
    {
        Dictionary<string, LoadBundleInfo> loadBundleInfos = new Dictionary<string, LoadBundleInfo>();

        string streamingAssetsABconfigPath = Application.streamingAssetsPath + "/ABconfig.json";//本地路径
        string persistentABconfigPath = Application.persistentDataPath + "/ABconfig.json";//缓存路径

        //persistentAbConfig是最新的bundle信息
        string persistentAbConfig = FileHelper.ReadStrFromFile(persistentABconfigPath, false);
        var persistentAllBundleInfo = JsonConvert.DeserializeObject<AllBundleInfo>(persistentAbConfig);

        //streamingAbConfig里是下载包的原始bundle信息
        string streamingAbConfig = FileHelper.ReadStrFromFile(streamingAssetsABconfigPath, false);
        var streamingAllBundleInfo = JsonConvert.DeserializeObject<AllBundleInfo>(streamingAbConfig);

        foreach (var persistentBundleInfo in persistentAllBundleInfo.BundleInfoList)
        {
            SingleBundleInfo tempBundleInfo;
            streamingAllBundleInfo.BundleInfoList.TryGetValue(persistentBundleInfo.Value.bundleName, out tempBundleInfo);

            string loadPath;
            if (tempBundleInfo == null || !persistentBundleInfo.Value.Equals(tempBundleInfo))
            {
                loadPath = _baseURL + persistentBundleInfo.Value.bundleName;
            }
            else
            {
                loadPath = Application.streamingAssetsPath + "/" + persistentBundleInfo.Value.bundleName;
            }
            loadBundleInfos.Add(persistentBundleInfo.Value.bundleName, new LoadBundleInfo(persistentBundleInfo.Value, loadPath));
        }

        return loadBundleInfos;
    }

    /// <summary>
    /// 特殊的获取集合不同元素。
    /// 1、sourceBundleInfos中有，targetBundleInfos中没有的，添加到返回集合
    /// 2、sourceBundleInfos中有，targetBundleInfos中对应名字的SingleBundleInfo但是hash值或者crc不一样的，添加到返回集合
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, SingleBundleInfo> GetChangedBundleInfo(Dictionary<string, SingleBundleInfo> sourceBundleInfos, Dictionary<string, SingleBundleInfo> targetBundleInfos)
    {
        Dictionary<string, SingleBundleInfo> resultBundleInfos = new Dictionary<string, SingleBundleInfo>();
        foreach (var sourceBundleInfo in sourceBundleInfos)
        {
            SingleBundleInfo tempBundleInfo;
            if (targetBundleInfos.TryGetValue(sourceBundleInfo.Value.bundleName, out tempBundleInfo))
            {
                if (!sourceBundleInfo.Value.Equals(tempBundleInfo))
                {
                    resultBundleInfos.Add(sourceBundleInfo.Value.bundleName, sourceBundleInfo.Value);
                }
            }
            else
            {
                resultBundleInfos.Add(sourceBundleInfo.Value.bundleName, sourceBundleInfo.Value);
            }
        }

        return resultBundleInfos;
    }
}
