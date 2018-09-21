using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public static class AtlasHelper
{
    private const string ATLAS_DIR = "Atlas/";

    private static readonly Dictionary<string, SpriteAtlas> _atlas = new Dictionary<string, SpriteAtlas>();

    public static void LoadSprite(Image image, string atlasName, string spriteName, bool isSetNativeSize = false)
    {
        SpriteAtlas atlas;
        if (!_atlas.TryGetValue(atlasName, out atlas))
        {
            atlas = ObjectCache.instance.LoadResource<SpriteAtlas>(ATLAS_DIR + atlasName);
            if (atlas == null)
            {
                Debug.LogWarning("LoadSprite error,No atlas:" + ATLAS_DIR + atlasName);
                return;
            }
        }

        var sprite = atlas.GetSprite(spriteName);
        if (!sprite)
        {
            Debug.LogWarning(string.Format("LoadSprite error!!!atlas {0} do not have spriteName {1}", atlasName, spriteName));
//            return;
        }
        image.sprite = sprite;
        if (isSetNativeSize)
        {
            image.SetNativeSize();
        }
    }

    /// <summary>
    /// 加载商品图片.包括房卡，银币等商品
    /// </summary>
    /// <param name="image"></param>
    /// <param name="spriteName"></param>
    public static void LoadGoodsSprite(Image image, string spriteName)
    {
        string atlasName = "atlas_goods";
        LoadSprite(image, atlasName, spriteName, true);
    }

    public static void LoadRoomCardSprite(Image image, int num)
    {
        if (num <= 0)
        {
            Debug.Log(string.Format("LoadRoomCardImg error,image :{0},wrong num :{1},", image.name, num));
            return;
        }

        string spriteName;
        if (1 <= num && num <= 10)
        {
            spriteName = "roomcard_0";
        }
        else if (11 <= num && num <= 35)
        {
            spriteName = "roomcard_1";
        }
        else if (36 <= num && num <= 65)
        {
            spriteName = "roomcard_2";
        }
        else
        {
            spriteName = "roomcard_3";
        }
        LoadGoodsSprite(image, spriteName);
    }

    public static void LoadHeadSprite(bool isBoy, int portrait, Image image, bool isSetNativeSize = false)
    {
        var spriteName = isBoy ? GetBoyHeadName(portrait) : GetGirlHeadName(portrait);
        LoadHeadSprite(image, spriteName, isSetNativeSize);
    }

    private static void LoadHeadSprite(Image image, string spriteName, bool isSetNativeSize)
    {
        string atlasName = "atlas_head";
        LoadSprite(image, atlasName, spriteName, isSetNativeSize);
    }

    private static string GetBoyHeadName(int portrait)
    {
        switch (portrait)
        {
            case 1:
                return "boy1";
            case 2:
                return "boy2";
            case 3:
                return "boy3";
            case 4:
                return "boy4";
            case 5:
                return "boy5";
            case 6:
                return "boy6";
            case 7:
                return "boy7";
            case 8:
                return "boy8";
            case 9:
                return "boy9";
            case 10:
                return "boy10";
            case 11:
                return "boy11";
            case 12:
                return "boy12";
            default:
                Debug.LogWarning(string.Format("error Boy portrait : {0}", portrait));
                return string.Empty;
        }
    }

    private static string GetGirlHeadName(int portrait)
    {
        switch (portrait)
        {
            case 1:
                return "girl1";
            case 2:
                return "girl2";
            case 3:
                return "girl3";
            case 4:
                return "girl4";
            case 5:
                return "girl5";
            case 6:
                return "girl6";
            case 7:
                return "girl7";
            case 8:
                return "girl8";
            case 9:
                return "girl9";
            case 10:
                return "girl10";
            case 11:
                return "girl11";
            case 12:
                return "girl12";
            default:
                Debug.LogWarning(string.Format("error Girl portrait : {0}", portrait));
                return string.Empty;
        }
    }
}
