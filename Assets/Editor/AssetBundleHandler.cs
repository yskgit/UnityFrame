using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AssetBundleBrowser;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

//[System.Serializable]
//public class AllBundleInfo
//{

//    public Dictionary<string, SingleBundleInfo> BundleInfoList;
//    //public List<SingleBundleInfo> BundleInfoList;


//    public AllBundleInfo()
//    {
//        //BundleInfoList = new List<SingleBundleInfo>();
//        BundleInfoList = new Dictionary<string, SingleBundleInfo>();
//    }
//}


//[System.Serializable]
//public class SingleBundleInfo
//{

//    public string bundleName;

//    public uint bundleCRC;

//    public string bundleHash128;

//    public float size;

//    public override string ToString()
//    {
//        return $"bundleName = {bundleName},bundleCRC = {bundleCRC},bundleHash128 = {bundleHash128},size = {size}";
//    }

//    public override bool Equals(object obj)
//    {
//        if (!(obj is SingleBundleInfo))
//        {
//            return false;
//        }
//        SingleBundleInfo info = (SingleBundleInfo)obj;
//        return bundleName.Equals(info.bundleName) && bundleCRC == info.bundleCRC && bundleHash128.Equals(info.bundleHash128);
//    }

//    public override int GetHashCode()
//    {
//        return base.GetHashCode();
//    }
//}

//注意：主Manifest名字与打包输出目录同名！！！
//注意：生成版本控制文件需要主Manifest！！！
//注意：生成版本控制文件必须只能遍历“StreamingAsset”文件夹下的AssetBundle！！！因为“AssetImporter”类只能识别“Assets”文件夹下的资源。

//找到AssetBundle打包路径。
//因为AssetBundle browser 打包输出目录会根据不同的选择输出到不同目录下，如“\UnityFrame\AssetBundles\StandaloneWindows”文件夹
//所以需要找到缓存的输出路径，才能得到主Manifest的名字
//因为我打包的时候，勾选了复制AssetBundle文件到“StreamingAsset”文件夹
//所以我如果要拿到“StreamingAsset”文件夹的主Manifest文件，则必须先拿到AssetBundle browser打包下的主Manifest文件名字

/// <summary>
/// 处理文件AssetBundle名的添加与删除
/// </summary>
public class AssetBundleHandler
{
    /// <summary>
    /// 给所有选中的文件、文件夹（不包括其下的文件）设置AssetBundleName
    /// </summary>
    [MenuItem("MyTools/Set Selected File AssetBundleName", false, 11)]
    private static void SetSelectedAssetBundleName()
    {
        Debug.Log("开始设置选中物体的 AssetBundle Name");
        var guids = Selection.assetGUIDs;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            SetAssetBundleName(path);
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    /// <summary>
    /// 设置单个文件、文件夹的AssetBundleName
    /// </summary>
    /// <param name="pathname"></param>
    static void SetAssetBundleName(string pathname)
    {
        AssetImporter asset = AssetImporter.GetAtPath(pathname);
        if (asset != null)
        {
            //asset.SetAssetBundleNameAndVariant(pathname.Replace("Assets/",""), "bytes");
            pathname = pathname.Replace("Assets/", string.Empty);
            if (pathname.Contains("."))
            {
                pathname = pathname.Substring(0, pathname.LastIndexOf('.'));
            }

            if (pathname.Equals(asset.assetBundleName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning("没有修改，原名和设置名一样:" + pathname);
            }
            else if (!string.IsNullOrEmpty(asset.assetBundleName))
            {
                Debug.Log(string.Format("成功修改assetbundle name，{0}=>{1}", asset.assetBundleName, pathname));
                asset.assetBundleName = pathname;
            }
            else
            {
                asset.assetBundleName = pathname;
                Debug.Log("成功设置assetbundle name:" + pathname);
            }
        }
        else
        {
            Debug.LogError(string.Format("设置AssetBundle失败，路径{0}不存在", pathname));
        }
    }

    /// <summary>
    /// 删除所有选中的文件、文件夹（不包括其下的文件）的AssetBundleName
    /// </summary>
    [MenuItem("MyTools/Delete Selected File AssetBundleName", false, 12)]
    private static void DeleteSelectedAssetBundleName()
    {
        Debug.Log("开始删除选中物体的 AssetBundle Name");
        var guids = Selection.assetGUIDs;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            DeleteAssetBundleName(path);
        }

        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    /// <summary>
    /// 删除单个文件、文件夹的AssetBundleName
    /// </summary>
    /// <param name="pathname"></param>
    static void DeleteAssetBundleName(string pathname)
    {
        AssetImporter asset = AssetImporter.GetAtPath(pathname);
        if (asset != null)
        {
            if (string.IsNullOrEmpty(asset.assetBundleName))
            {
                Debug.LogWarning(string.Format("删除AssetBundle失败失败，文件{0}没有设置AssetBundleName", pathname));
            }
            else
            {
                asset.assetBundleName = string.Empty;
                Debug.Log("成功删除了assetbundle name:" + pathname);
            }
        }
        else
        {
            Debug.LogError(string.Format("删除AssetBundle失败，路径{0}不存在", pathname));
        }
    }

    /// <summary>
    /// 删除所有未引用的AssetBundleName
    /// </summary>
    [MenuItem("MyTools/Delete All Unused AssetBundleName", false, 100)]
    private static void DeleteAllUnusedAssetBundleName()
    {
        Debug.Log("开始删除所有未引用的 AssetBundle Name");
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log("删除所有未引用的 AssetBundle Name 成功！");
    }

    /// <summary>
    /// 删除所有AssetBundleName，包括正在使用的
    /// </summary>
    [MenuItem("MyTools/Delete All AssetBundleName", false, 101)]
    private static void DeleteAllAssetBundleName()
    {
        Debug.Log("开始删除所有的 AssetBundle Name");
        string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (var item in allAssetBundleNames)
        {
            AssetDatabase.RemoveAssetBundleName(item, true);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log("删除所有的 AssetBundle Name 成功！");
    }

    /// <summary>
    /// 生成AssetBundle版本文件。所有AssetBundle的AssetBundleName、crc、hash值。
    /// </summary>
    [MenuItem("MyTools/Generate AssetBundle Version File", false, 23)]
    private static void GenerateAssetBundleVersionFile()
    {
        string mainManifestName = GetMainManifestName();
        if (!string.IsNullOrEmpty(mainManifestName))
        {
            GenerateABConfig(mainManifestName);
        }
    }

    /// <summary>
    /// 获取主manifest名字
    /// </summary>
    /// <returns></returns>
    private static string GetMainManifestName()
    {
        string outputPath = GetOutputPath();
        return outputPath.Split('/')[outputPath.Split('/').Length - 1];
    }

    /// <summary>
    /// 获取AssetBundle browser的输出路径
    /// </summary>
    /// <returns></returns>
    private static string GetOutputPath()
    {
        string outputPath = string.Empty;

        //LoadData...
        var dataPath = System.IO.Path.GetFullPath(".");
        dataPath = dataPath.Replace("\\", "/");
        dataPath += "/Library/AssetBundleBrowserBuild.dat";//dataPath存储了AssetBundle browser 打包时所有的设置信息
        if (File.Exists(dataPath))
        {
            using (FileStream file = File.Open(dataPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                var data = bf.Deserialize(file) as AssetBundleBuildTab.BuildTabData;
                if (data != null)
                {
                    outputPath = data.m_OutputPath;
                }
                else
                {
                    Debug.LogError("反序列化失败!");
                }
            }
        }
        else
        {
            Debug.LogError(string.Format("文件 {0} 不存在，先使用AssetBundle Browser打包!", dataPath));
        }

        return outputPath;
    }

    /// <summary>
    /// 生成 ABConfig.json 文件。通过streamingAssetsPath拼接主manifest名字获取到主manifest文件，从而得到所有assetbundle信息。
    /// </summary>
    /// <param name="mainManifestName"></param>
    private static void GenerateABConfig(string mainManifestName)
    {
        if (string.IsNullOrEmpty(mainManifestName))
        {
            Debug.LogError("mainManifestName 不能为空值");
        }

        AllBundleInfo allBundleInfo = new AllBundleInfo();
        //添加主manifest的AssetBundle信息
        allBundleInfo.BundleInfoList.Add(mainManifestName, ComputeManifestHashAndCRC(GetAssetBundleDirectory() + "/" + mainManifestName));
        //加载主manifest的AssetBundle
        AssetBundle assetBundle = AssetBundle.LoadFromFile(GetAssetBundleDirectory() + "/" + mainManifestName);
        if (!assetBundle)
        {
            Debug.LogError(string.Format("生成AssetBundle版本文件失败，主Manifest文件未正常加载！path:{0}", GetAssetBundleDirectory() + "/" + mainManifestName));
            return;
        }
        AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        //必须要卸载AssetBundle，否则编辑器未重启的情况下LoadFromFile就会出错，因为不允许重复加载AssetBundle！
        assetBundle.Unload(false);
        //获取打包的所以AssetBundle名字
        var bundleNames = manifest.GetAllAssetBundles();
        if (bundleNames.Length <= 0)
        {
            Debug.LogError("生成AssetBundle版本文件失败，没有AssetBundle文件，先打包AssetBundle");
            return;
        }

        foreach (var bundleName in bundleNames)
        {
            //AssetImporter.GetAtPath 的参数必须是是以“Assets/”开头的相对路径，包括后缀名。
            var bundleFile = bundleName;
            //变体需要截断后面的变体后缀，只传文件名字
            if (bundleFile.Contains("."))
            {
                bundleFile = bundleFile.Split('.')[0];
            }

            string assetName = bundleName.Substring(bundleName.LastIndexOf('/') + 1);
            Debug.Log("assetName = " + assetName);

            //获取到AssetBundle相对路径下的完全路径
            //如果是以文件夹为单位打包的AssetBundle，fullPath 即为 "Assets/" + bundleName
            string fullPath;
            if (AssetDatabase.IsValidFolder("Assets/" + bundleName))
            {
                fullPath = "Assets/" + bundleName;
            }
            //如果是以单个文件为单位打包的AssetBundle，fullPath 即为 "Assets/" + bundleName + 后缀名
            else
            {
                string[] fullPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
                fullPath = fullPaths.Length > 0 ? fullPaths[0] : bundleFile;
            }

            AssetImporter asset = AssetImporter.GetAtPath(fullPath);
            if (asset != null)
            {
                allBundleInfo.BundleInfoList.Add(bundleName, ComputeHashAndCRC(asset));
            }
            else
            {
                Debug.LogError(string.Format("生成AssetBundle版本文件错误，没有文件:{0}", "Assets/" + bundleName));
            }
        }

        string json = JsonConvert.SerializeObject(allBundleInfo, Formatting.Indented);
        WriteManifestJsonConfig(json);
    }

    /// <summary>
    /// 计算AssetBundle的hash和crc值。
    /// 参数AssetImporter类型asset变量为导入unity的资源信息，包含了很多需要的信息。
    /// 如：资源的assetBundleName、assetBundleVariant、assetPath等。
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    //[Obsolete("这个方法不能得到变体的")]
    private static SingleBundleInfo ComputeHashAndCRC(AssetImporter asset)
    {
        if (asset == null)
        {
            Debug.LogError("asset 不能为空值");
            return null;
        }

        SingleBundleInfo info = new SingleBundleInfo();
        //assetBundleName有变种需要接上变种的后缀。
        string fileName = GetAssetBundleDirectory() + "/" + asset.assetBundleName + (string.IsNullOrEmpty(asset.assetBundleVariant) ? "" : "." + asset.assetBundleVariant);
        //BuildPipeline.GetHashForAssetBundle 和 BuildPipeline.GetCRCForAssetBundle 的第一个参数都必须是完全路径
        Hash128 hash128;
        if (BuildPipeline.GetHashForAssetBundle(fileName, out hash128))
        {
            info.bundleHash128 = hash128.ToString();
        }
        else
        {
            Debug.LogError(string.Format("未获取到{0}的hash值", fileName));
        }
        uint crc;
        if (BuildPipeline.GetCRCForAssetBundle(fileName, out crc))
        {
            info.bundleCRC = crc;
        }
        else
        {
            Debug.LogError(string.Format("未获取到{0}的crc值", fileName));
        }
        info.bundleName = asset.assetBundleName + (string.IsNullOrEmpty(asset.assetBundleVariant) ? "" : "." + asset.assetBundleVariant);
        using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            info.size = (float)stream.Length / 1024 / 1024;
        }

        Debug.Log(string.Format("成功获取AssetBundle信息，{0}", info));
        return info;
    }

    /// <summary>
    /// 计算主Manifest的hash和crc值
    /// </summary>
    /// <param name="manifestName"></param>
    /// <returns></returns>
    private static SingleBundleInfo ComputeManifestHashAndCRC(string manifestName)
    {
        if (string.IsNullOrEmpty(manifestName))
        {
            Debug.LogError("manifestName不能为空值");
            return null;
        }

        SingleBundleInfo info = new SingleBundleInfo();
        Hash128 hash128;
        //BuildPipeline.GetHashForAssetBundle 和 BuildPipeline.GetCRCForAssetBundle 的第一个参数都必须是完全路径
        if (BuildPipeline.GetHashForAssetBundle(manifestName, out hash128))
        {
            info.bundleHash128 = hash128.ToString();
        }
        else
        {
            Debug.LogError(string.Format("未获取到{0}的hash值", manifestName));
        }
        uint crc;
        if (BuildPipeline.GetCRCForAssetBundle(manifestName, out crc))
        {
            info.bundleCRC = crc;
        }
        else
        {
            Debug.LogError(string.Format("未获取到{0}的crc值", manifestName));
        }

        info.bundleName = manifestName.Replace(GetAssetBundleDirectory() + "/", string.Empty);
        Debug.Log(string.Format("成功获取主Manifest信息，{0}", info));
        return info;
    }

    /// <summary>
    /// 获取打包AssetBundle的路径
    /// </summary>
    /// <returns></returns>
    private static string GetAssetBundleDirectory()
    {
        string path = Application.streamingAssetsPath;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// 将AssetBundle版本信息写入 StreamingAsset/ABconfig.json文件
    /// </summary>
    /// <param name="json"></param>
    private static void WriteManifestJsonConfig(string json)
    {
        string path = GetAssetBundleDirectory() + "/ABconfig.json";
        using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        using (StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(json);
        }

        string outputPath = GetOutputPath();
        File.Copy(path, outputPath + "/ABconfig.json", true);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
