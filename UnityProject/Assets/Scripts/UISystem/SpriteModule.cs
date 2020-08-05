#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    SpriteModule.cs
 Author:      Zeng Zhiwei
 Time:        2020/8/4 23:17:58
=====================================================
*/
#endregion

using System.Collections.Generic;
using UnityEngine;

public class SpriteModule: Singleton<SpriteModule>
{
    private Material m_emojiMaterial;
    private Dictionary<string, Sprite> m_dicEmojiSprite = new Dictionary<string, Sprite>();

    public delegate void LoadedEmjCallback(object data, Material mat);
    public void LoadEmojiByName(string strName, LoadedEmjCallback callback)
    {
        if(callback != null)
            callback(m_dicEmojiSprite[strName], m_emojiMaterial);
    }

    public void InitEmoji()
    {
        string strPath = "Assets/Emoji/emoji/emoji_c.png";
        Object[] sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(strPath);
        for(int i = 0; i < sprites.Length; i++)
        {
            Sprite sp = sprites[i] as Sprite;
            if(sp != null)
            {
                TextEx.AddEmoji(sp.name);
                if(!m_dicEmojiSprite.ContainsKey(sp.name))
                    m_dicEmojiSprite.Add(sp.name, sp);
                continue;
            }
        }
        strPath = "Assets/Emoji/emoji/emoji_mat.mat";
        Object mat = UnityEditor.AssetDatabase.LoadAssetAtPath(strPath, typeof(Material));
        m_emojiMaterial = mat as Material;
    }
}