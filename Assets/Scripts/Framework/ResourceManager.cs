using System;
using System.Collections;
using System.Collections.Generic;
//using Pathfinding.Serialization.JsonFx;
using UnityEngine;

public enum ResourceType
{
    Resource_General,
    Resource_Bundle,
    Resource_Language,
    Resource_ResTable,
    Resource_Code,
    Resource_Config,
    Resource_Lightmap,
    Resource_Nickname,
}

public class ResourceManager : SingletonBehaviour<ResourceManager>
{
//    private Dictionary<string, object> configDict;
    //private BundleCache cache;
    //private Dictionary<string, bool> files = new Dictionary<string, bool>();
    //private HashSet<string> isLoadingCache = new HashSet<string>();
    //protected override void Awake()
    //{
    //    base.Awake();
    //    //this.cache = new BundleCache();
    //    //this.cacheFileNames();
    //}

    //public void GarbageCollect(bool force = false)
    //{
    //    //this.cache.GarbageCollect(force);
    //}

    //public static T FromJson<T>(string json)
    //{
    //    return JsonReader.Deserialize<T>(json);
    //}

    //public void LoadBundleByPrefabName(string prefabName, Action<AssetBundle> bundleLoaded)
    //{
    //    //if (string.IsNullOrEmpty(prefabName))
    //    //{
    //    //    bundleLoaded.Invoke(null);
    //    //}
    //    //else
    //    //{
    //    //    base.StartCoroutine(this.LoadBundle(prefabName, bundleLoaded));
    //    //}
    //}
}
