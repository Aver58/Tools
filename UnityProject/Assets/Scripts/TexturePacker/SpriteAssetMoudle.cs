//using UnityEngine;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using Debuger = LuaInterface.Debugger;

//public class SpriteAssetMoudle : ModuleBase
//{
//    private Dictionary<string, Vector4> m_spritePaddingMap = new Dictionary<string, Vector4>();
//    private Dictionary<string, string> m_spriteAtlasMap = new Dictionary<string, string>();

//    Dictionary<string, AtlasData> m_atlas = new Dictionary<string, AtlasData>();

//    public SpriteAssetMoudle()
//    {
//    }


//    static SpriteAssetMoudle ms_instance;
//    public static SpriteAssetMoudle Instance
//    {
//        get
//        {
//            if (ms_instance == null)
//            {
//                ms_instance = ModuleManager.Instance.Get<SpriteAssetMoudle>();
//            }
//            return ms_instance;
//        }
//    }

//    public override void Init()
//    {
//        if (Config.Instance.UseTexturePacker)
//        {
//            LoadModule.Instance.LoadAssetFromBundle(BaseDef.AtlasTpPaddingPath, BaseDef.AtlasTpPadding, typeof(PaddingData), (data) =>
//            {
//                PaddingData pd = data as PaddingData;
//                InitDataFromPaddingData(pd);
//                m_bInit = true;
//            }, false);
//        }
//        else
//        {
//            m_bInit = true;
//        }
//    }

//    public bool HasSprite(string spriteName)
//    {
//        return m_spriteAtlasMap.ContainsKey(spriteName);
//    }

//    public void GetAtlasData(string spriteName, LoadedCallback onLoaded)
//    {
//        string atlasName = GetAtlasName(spriteName);
//        if (string.IsNullOrEmpty(atlasName))
//        {
//            onLoaded(null);
//        }
//        LoadSpriteData(atlasName, onLoaded);
//    }

//    public string GetAtlasName(string spriteName)
//    {
//        string atlasName;
//        if (!m_spriteAtlasMap.TryGetValue(spriteName, out atlasName))
//        {
//            Debuger.LogError(spriteName);
//            return "";
//        }
//        return atlasName;
//    }

//    void InitDataFromPaddingData(PaddingData pd)
//    {
//        if (pd == null)
//        {
//            return;
//        }

//        List<PaddingData.SpriteInfo> sprites;
//        PaddingData.SpriteInfo info;
//        for (int i = 0; i < pd.atlas.Count; ++i)
//        {
//            PaddingData.AtlasInfo atlas = pd.atlas[i];
//            sprites = atlas.sprites;
//            bool isPoly = atlas.isPoly;
//            for (int j = 0; j < sprites.Count; ++j)
//            {
//                info = sprites[j];
//                string name = info.name;
//                AddSpriteAtlasInfo(name, pd.atlas[i].name);
//                AddPaddingInfo(name, info.padding);
//            }
//        }
//        //设置SDImage Padding
//        SDImage.SetPaddingConfig(m_spritePaddingMap);
//    }

//    private void AddSpriteAtlasInfo(string spriteName, string atlasName)
//    {
//        if (m_spriteAtlasMap.ContainsKey(spriteName))
//        {
//            m_spriteAtlasMap[spriteName] = atlasName;
//        }
//        else
//        {
//            m_spriteAtlasMap.Add(spriteName, atlasName);
//        }
//    }

//    private void AddPaddingInfo(string k, Vector4 v)
//    {
//        if (m_spritePaddingMap.ContainsKey(k))
//        {
//            m_spritePaddingMap[k] = v;
//        }
//        else
//        {
//            m_spritePaddingMap.Add(k, v);
//        }
//    }

//    public void LoadSpriteData(string atlas, LoadedCallback onLoaded)
//    {
//        // 先从缓存中取
//        AtlasData atlasdata = FindSpriteData(atlas);
//        if (atlasdata != null)
//        {
//            if (onLoaded != null)
//            {
//                onLoaded(atlasdata);
//            }
//            return;
//        }

//        string strPath = BaseDef.AtlasTPHead + atlas;
//        LoadModule.Instance.LoadAssetFromBundle(strPath, atlas, typeof(GameObject), (data) =>
//        {
//            GameObject go = data as GameObject;
//            atlasdata = AddSprites(atlas, go);
//            if (onLoaded != null)
//            {
//                onLoaded(atlasdata);
//            }
//        }, true);
//    }
//    public AtlasData AddSprites(string atlas, GameObject go)
//    {
//        if (go == null)
//        {
//            return null;
//        }

//        if (m_atlas.ContainsKey(atlas))
//        {
//            return m_atlas[atlas];
//        }

//        AtlasHierarchy hierarchy = go.GetComponent<AtlasHierarchy>();
//        if (hierarchy == null)
//        {
//            return null;
//        }

//        AtlasData atlasdata = hierarchy.atlas;
//        atlasdata.InitMap();
//        m_atlas.Add(atlas, atlasdata);
//        return atlasdata;
//    }

//    AtlasData FindSpriteData(string atlas)
//    {
//        if (m_atlas.ContainsKey(atlas))
//            return m_atlas[atlas];
//        return null;
//    }
//}