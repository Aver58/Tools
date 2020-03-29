
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

class TexturePacker
{
	public static List<SpriteMetaData> ProcessToSprites(string text)
	{
		Hashtable hashtable = text.hashtableFromJson();
		TexturePacker.MetaData metaData = new TexturePacker.MetaData((Hashtable)hashtable["meta"]);
		List<TexturePacker.PackedFrame> list = new List<TexturePacker.PackedFrame>();
		Hashtable hashtable2 = (Hashtable)hashtable["frames"];
		foreach(object obj in hashtable2)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
			list.Add(new TexturePacker.PackedFrame((string)dictionaryEntry.Key, metaData.size, (Hashtable)dictionaryEntry.Value));
		}
		TexturePacker.SortFrames(list);
		List<SpriteMetaData> list2 = new List<SpriteMetaData>();
		for(int i = 0; i < list.Count; i++)
		{
			SpriteMetaData spriteMetaData = list[i].BuildBasicSprite(0.01f, new Color32(128, 128, 128, 128));
			bool flag = !spriteMetaData.name.Equals("IGNORE_SPRITE");
			if(flag)
			{
				list2.Add(spriteMetaData);
			}
		}
		return list2;
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00004424 File Offset: 0x00002624
	public static List<TexturePacker.PackedFrame> ProcessToFrames(string text)
	{
		Hashtable hashtable = text.hashtableFromJson();
		TexturePacker.MetaData metaData = new TexturePacker.MetaData((Hashtable)hashtable["meta"]);
		List<TexturePacker.PackedFrame> list = new List<TexturePacker.PackedFrame>();
		Hashtable hashtable2 = (Hashtable)hashtable["frames"];
		foreach(object obj in hashtable2)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
			list.Add(new TexturePacker.PackedFrame((string)dictionaryEntry.Key, metaData.size, (Hashtable)dictionaryEntry.Value));
		}
		TexturePacker.SortFrames(list);
		return list;
	}

	// Token: 0x06000045 RID: 69 RVA: 0x000044EC File Offset: 0x000026EC
	private static List<TexturePacker.PackedFrame> SortFrames(List<TexturePacker.PackedFrame> frames)
	{
		for(int i = frames.Count - 1; i > 0; i--)
		{
			for(int j = 0; j < i; j++)
			{
				bool flag = string.Compare(frames[j + 1].name, frames[j].name) < 0;
				if(flag)
				{
					TexturePacker.PackedFrame value = frames[j + 1];
					frames[j + 1] = frames[j];
					frames[j] = value;
				}
			}
		}
		return frames;
	}
	public class PackedFrame
	{
		// Token: 0x0600004A RID: 74 RVA: 0x000046EC File Offset: 0x000028EC
		public PackedFrame(string name, Vector2 atlasSize, Hashtable table)
		{
			this.name = name;
			this.atlasSize = atlasSize;
			this.frame = ((Hashtable)table["frame"]).TPHashtableToRect();
			this.spriteSourceSize = ((Hashtable)table["spriteSourceSize"]).TPHashtableToRect();
			this.sourceSize = ((Hashtable)table["sourceSize"]).TPHashtableToVector2();
			this.rotated = (bool)table["rotated"];
			this.trimmed = (bool)table["trimmed"];
		}

		// Token: 0x0600004B RID: 75 RVA: 0x0000478C File Offset: 0x0000298C
		public Mesh BuildBasicMesh(float scale, Color32 defaultColor)
		{
			return this.BuildBasicMesh(scale, defaultColor, Quaternion.identity);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000047AC File Offset: 0x000029AC
		public Mesh BuildBasicMesh(float scale, Color32 defaultColor, Quaternion rotation)
		{
			Mesh mesh = new Mesh();
			Vector3[] array = new Vector3[4];
			Vector2[] array2 = new Vector2[4];
			Color32[] array3 = new Color32[4];
			bool flag = !this.rotated;
			if(flag)
			{
				array[0] = new Vector3(this.frame.x, this.frame.y, 0f);
				array[1] = new Vector3(this.frame.x, this.frame.y + this.frame.height, 0f);
				array[2] = new Vector3(this.frame.x + this.frame.width, this.frame.y + this.frame.height, 0f);
				array[3] = new Vector3(this.frame.x + this.frame.width, this.frame.y, 0f);
			}
			else
			{
				array[0] = new Vector3(this.frame.x, this.frame.y, 0f);
				array[1] = new Vector3(this.frame.x, this.frame.y + this.frame.width, 0f);
				array[2] = new Vector3(this.frame.x + this.frame.height, this.frame.y + this.frame.width, 0f);
				array[3] = new Vector3(this.frame.x + this.frame.height, this.frame.y, 0f);
			}
			array2[0] = array[0].TPVector3toVector2();
			array2[1] = array[1].TPVector3toVector2();
			array2[2] = array[2].TPVector3toVector2();
			array2[3] = array[3].TPVector3toVector2();
			for(int i = 0; i < array2.Length; i++)
			{
				Vector2[] array4 = array2;
				int num = i;
				array4[num].x = array4[num].x / this.atlasSize.x;
				Vector2[] array5 = array2;
				int num2 = i;
				array5[num2].y = array5[num2].y / this.atlasSize.y;
				array2[i].y = 1f - array2[i].y;
			}
			bool flag2 = this.rotated;
			if(flag2)
			{
				array[3] = new Vector3(this.frame.x, this.frame.y, 0f);
				array[0] = new Vector3(this.frame.x, this.frame.y + this.frame.height, 0f);
				array[1] = new Vector3(this.frame.x + this.frame.width, this.frame.y + this.frame.height, 0f);
				array[2] = new Vector3(this.frame.x + this.frame.width, this.frame.y, 0f);
			}
			for(int j = 0; j < array.Length; j++)
			{
				array[j].y = this.atlasSize.y - array[j].y;
			}
			for(int k = 0; k < array.Length; k++)
			{
				Vector3[] array6 = array;
				int num3 = k;
				array6[num3].x = array6[num3].x - (this.frame.x - this.spriteSourceSize.x + this.sourceSize.x / 2f);
				Vector3[] array7 = array;
				int num4 = k;
				array7[num4].y = array7[num4].y - (this.atlasSize.y - this.frame.y - (this.sourceSize.y - this.spriteSourceSize.y) + this.sourceSize.y / 2f);
			}
			for(int l = 0; l < array.Length; l++)
			{
				array[l] *= scale;
			}
			bool flag3 = rotation != Quaternion.identity;
			if(flag3)
			{
				for(int m = 0; m < array.Length; m++)
				{
					array[m] = rotation * array[m];
				}
			}
			for(int n = 0; n < array3.Length; n++)
			{
				array3[n] = defaultColor;
			}
			mesh.vertices = array;
			mesh.uv = array2;
			mesh.colors32 = array3;
			mesh.triangles = new int[]
			{
					0,
					3,
					1,
					1,
					3,
					2
			};
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.name = this.name;
			return mesh;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004CFC File Offset: 0x00002EFC
		public SpriteMetaData BuildBasicSprite(float scale, Color32 defaultColor)
		{
			SpriteMetaData spriteMetaData = default(SpriteMetaData);
			bool flag = !this.rotated;
			Rect rect;
			if(flag)
			{
				rect = this.frame;
			}
			else
			{
				rect = new Rect(this.frame.x, this.frame.y, this.frame.height, this.frame.width);
			}
			bool flag2 = this.frame.x + this.frame.width > this.atlasSize.x || this.frame.y + this.frame.height > this.atlasSize.y || this.frame.x < 0f || this.frame.y < 0f;
			SpriteMetaData result;
			if(flag2)
			{
				Debug.Log(this.name + " is outside from texture! Sprite is ignored!");
				spriteMetaData.name = "IGNORE_SPRITE";
				result = spriteMetaData;
			}
			else
			{
				rect.y = this.atlasSize.y - this.frame.y - rect.height;
				spriteMetaData.name = Path.GetFileNameWithoutExtension(this.name);
				spriteMetaData.rect = rect;
				bool flag3 = !this.trimmed;
				if(flag3)
				{
					spriteMetaData.alignment = 0;
					spriteMetaData.pivot = this.frame.center;
				}
				else
				{
					spriteMetaData.alignment = 9;
					spriteMetaData.pivot = new Vector2((this.sourceSize.x / 2f - this.spriteSourceSize.x) / this.spriteSourceSize.width, 1f - (this.sourceSize.y / 2f - this.spriteSourceSize.y) / this.spriteSourceSize.height);
				}
				result = spriteMetaData;
			}
			return result;
		}

		// Token: 0x0400001F RID: 31
		public string name;

		// Token: 0x04000020 RID: 32
		public Rect frame;

		// Token: 0x04000021 RID: 33
		public Rect spriteSourceSize;

		// Token: 0x04000022 RID: 34
		public Vector2 sourceSize;

		// Token: 0x04000023 RID: 35
		public bool rotated;

		// Token: 0x04000024 RID: 36
		public bool trimmed;

		// Token: 0x04000025 RID: 37
		private Vector2 atlasSize;
	}
	// Token: 0x0200000C RID: 12
	public class MetaData
	{
		// Token: 0x0600004E RID: 78 RVA: 0x00004EE4 File Offset: 0x000030E4
		public MetaData(Hashtable table)
		{
			this.image = (string)table["image"];
			this.format = (string)table["format"];
			this.size = ((Hashtable)table["size"]).TPHashtableToVector2();
			this.scale = float.Parse(table["scale"].ToString());
			this.smartUpdate = (string)table["smartUpdate"];
		}

		// Token: 0x04000026 RID: 38
		public string image;

		// Token: 0x04000027 RID: 39
		public string format;

		// Token: 0x04000028 RID: 40
		public Vector2 size;

		// Token: 0x04000029 RID: 41
		public float scale;

		// Token: 0x0400002A RID: 42
		public string smartUpdate;
	}
}
