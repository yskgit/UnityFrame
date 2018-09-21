using System;
//using UnityEditor;
using UnityEngine;

public class PathManager : SingletonScriptable<PathManager>
{
    public const string PrefabPath = "art/";
    public string DependencyFile = "dependency.txt";
    //	public string codeFile = "code.txt";
    public string DownloadPath;
    public string LocalResourcePath;
    public string DownloadBackPath;
    public string Root;

    public string LanguageExtension
    {
        get
        {
            return ".txt";
        }
    }
    public string BundleExtension
    {
        get
        {
            return ".bundle";
        }
    }
    public string ResTableExtension
    {
        get
        {
            return ".res";
        }
    }

    //	public string LightmapExtension
    //	{
    //		get
    //		{
    //			return ".lm";
    //		}
    //	}

    protected override void OnDestroy()
    {
    }
    protected override void AfterCreate()
    {
        this.Root = Application.persistentDataPath;
        Debug.Log("Root path = " + this.Root);
        this.DownloadPath = this.Root + "/art";
        this.DownloadBackPath = this.Root + "/back";
        this.LocalResourcePath = Application.streamingAssetsPath + "/LocalResource.bundle";
        Debug.Log("LocalResourcePath  = " + this.LocalResourcePath);
    }
    //public string PlatformDir(BuildTarget platform)
    //{
    //    if ((int)platform == 9)
    //    {
    //        return "/ios";
    //    }
    //    if ((int)platform == 13)
    //    {
    //        return "/android";
    //    }
    //    if ((int)platform != 26)
    //    {
    //        return "/mac";
    //    }
    //    return "/wp";
    //}
    public string SubDir(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Resource_General:
                return "/";
            case ResourceType.Resource_Bundle:
                return "/Bundles/";
            case ResourceType.Resource_Language:
                return "/Languages/";
            case ResourceType.Resource_ResTable:
                return "/ResTable/";
            case ResourceType.Resource_Code:
                return "/code/";
            case ResourceType.Resource_Config:
            case ResourceType.Resource_Lightmap:
                return "/config/";
            case ResourceType.Resource_Nickname:
                return "/Nickname/";
            default:
                return "/";
        }
    }
}
