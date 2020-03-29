using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AtlasHierarchy : MonoBehaviour
{
	public AtlasData atlas;

    public void SetAtlasData(List<Sprite> sprites, Material mat)
    {
        atlas = new AtlasData();
        atlas.sprites = new List<Sprite>();
        for (int i = 0; i < sprites.Count; i++)
        {
            atlas.sprites.Add(sprites[i]);
        }
        atlas.material = mat;
    }

    public List<string> GetSpriteDataName()
    {
        List<string> names = new List<string>();
        for (int i = 0; i < atlas.sprites.Count; i++)
        {
            names.Add(atlas.sprites[i].name);
        }
        return names;
    }
}


// ------------------------------------------------------------------------------------------------
[System.Serializable]
public class AtlasData
{
	
	public Material material;                   // 材质
	public List<Sprite> sprites;            // Sprite列表

	Dictionary<string, Sprite> spriteMap;   // Sprite列表

	public void InitMap()
	{
		if (spriteMap != null)
		{
			// 已经初始化过了
			return;
		}

		spriteMap = new Dictionary<string, Sprite>();
		if (sprites != null)
		{
			Sprite data = null;
			for (int i = 0; i < sprites.Count; ++i)
			{
				data = sprites[i];
				spriteMap.Add(data.name, data);
			}
		}
	}


	public void Dispose()
	{
		spriteMap = null;
	}

	public int SpriteCount
	{
		get
		{
			InitMap();
			return spriteMap.Count;
		}
	}

	public string[] SpriteNames
	{
		get
		{
			InitMap();

			string[] names = new string[spriteMap.Count];
			spriteMap.Keys.CopyTo(names, 0);
			return names;
		}
	}

	int _referenceCount;
	public int ReferenceCount
	{
		get
		{
			return _referenceCount;
		}
		set
		{
			_referenceCount = value;
		}
	}

	public string Name  // 图集名字
	{
		get; set;
	}


	Sprite GetSpriteData(string name)
	{
		InitMap();

		Sprite data = null;
		spriteMap.TryGetValue(name, out data);

		return data;
	}

	// 根据名称查找Sprite
	public Sprite GetSprite(string name)
	{
		Sprite data = GetSpriteData(name);
		if (data == null)
		{
			return null;
		}

		++ReferenceCount;
		return data;
	}

	// 查找并设置给Image(包括Sprite和材质)
	public void SetToImage(ImageEx image, string name)
	{
		Sprite data = GetSpriteData(name);
		if (data != null)
		{
			SetImage(image, this, data);
		}
		else {
			image.sprite = null;
			image.material = null;
		}
	}

	public static void SetImage(ImageEx image, AtlasData ad, Sprite sprite)
	{
		image.sprite = sprite;
		//image.SpriteName = sprite.name;
		if (ad.material != null || !(image.material != null && string.Equals(image.material.shader.name, "UI/Default(grey)")))
		{
			image.material = ad.material;
		}
		else
		{
			image.material = null;
		}

		AtlasReference reference = image.GetAtlasReference();
		if (reference != null)
		{
			reference.AddRef(image, ad);
		}
	}
}
