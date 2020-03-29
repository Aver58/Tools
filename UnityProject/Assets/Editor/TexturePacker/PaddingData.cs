using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PaddingData : ScriptableObject
{
    [System.Serializable]
    public class SpriteInfo
    {
        public string name;
        public bool trim = false;       // 是否切白边
        public Vector4 padding;

        public SpriteInfo() { }
        public SpriteInfo(string _name, Vector4 _padding, bool _trim = false) 
        { 
            name = _name;
            padding = _padding;
            trim = _trim;
        }
    }

    [System.Serializable]
    public class AtlasInfo
    {
        public string name;
        public bool isPoly;
        public List<SpriteInfo> sprites;
    }

    public List<AtlasInfo> atlas;

    public void AddAtlas(string name, bool ispoly, List<SpriteInfo> sprites)
    {
        if (atlas == null)
        {
            atlas = new List<AtlasInfo>();
        }
        bool found = false;
        for (int i = 0; i < atlas.Count; ++i)
        {
            if (string.Equals(atlas[i].name, name))
            {
                atlas[i].sprites = sprites;
                found = true;
                break;
            }
        }

        if (!found)
        {
            AtlasInfo info = new AtlasInfo();
            info.name = name;
            info.sprites = sprites;
            info.isPoly = ispoly;

            atlas.Add(info);
        }
    }

    public void AddAtlasEx(string name, bool isPoly, List<string> spriteNames, List<Vector4> paddings, List<bool> _trims)
    {
        List<PaddingData.SpriteInfo> SpriteInfoList = new List<PaddingData.SpriteInfo>();
        for (int i = 0; i < spriteNames.Count; i++)
        {
            PaddingData.SpriteInfo sprite = new PaddingData.SpriteInfo(spriteNames[i], paddings[i], _trims[i]);
            SpriteInfoList.Add(sprite);
        }
        AddAtlas(name, isPoly, SpriteInfoList);
    }

    public void RemoveAtlasItem(string name)
    {
        if (atlas != null)
        {
            for (int i = atlas.Count - 1; i >= 0; i--)
            {
                if (atlas[i].name == name)
                {
                    atlas.RemoveAt(i);
                }
            }
        }
    }
}
