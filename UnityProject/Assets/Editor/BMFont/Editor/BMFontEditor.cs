﻿using UnityEngine;
using UnityEditor;

public class BMFontEditor : EditorWindow
{
	[MenuItem("Tools/BMFont Maker")]
	static public void OpenBMFontMaker()
	{
		EditorWindow.GetWindow<BMFontEditor>(false, "BMFont Maker", true).Show();
	}

	[SerializeField]
	private Font targetFont;
	[SerializeField]
	private TextAsset fntData;
	[SerializeField]
	private Material fontMaterial;
	[SerializeField]
	private Texture2D fontTexture;

	private BMFont bmFont = new BMFont();

	public BMFontEditor()
	{
	}

	void OnGUI()
	{
        //改成拖动文件夹
		targetFont = EditorGUILayout.ObjectField("Target Font", targetFont, typeof(Font), false) as Font;
		fntData = EditorGUILayout.ObjectField("Fnt Data", fntData, typeof(TextAsset), false) as TextAsset;
		fontMaterial = EditorGUILayout.ObjectField("Font Material", fontMaterial, typeof(Material), false) as Material;
		fontTexture = EditorGUILayout.ObjectField("Font Texture", fontTexture, typeof(Texture2D), false) as Texture2D;

		if (GUILayout.Button("Create BMFont"))
		{
			BMFontReader.Load(bmFont, fntData.name, fntData.bytes); // 借用NGUI封装的读取类
			CharacterInfo[] characterInfo = new CharacterInfo[bmFont.glyphs.Count];
			for (int i = 0; i < bmFont.glyphs.Count; i++)
			{
				BMGlyph bmInfo = bmFont.glyphs[i];
				CharacterInfo info = new CharacterInfo();
				info.index = bmInfo.index;
				info.uv.x = (float)bmInfo.x / (float)bmFont.texWidth;
				info.uv.y = 1 - (float)bmInfo.y / (float)bmFont.texHeight;
				info.uv.width = (float)bmInfo.width / (float)bmFont.texWidth;
				info.uv.height = -1f * (float)bmInfo.height / (float)bmFont.texHeight;
				info.vert.x = 0;
				info.vert.y = (-(float)bmInfo.height)/2; //居中
				info.vert.width = (float)bmInfo.width;
				info.vert.height = (float)bmInfo.height;
				info.width = (float)bmInfo.advance;
				characterInfo[i] = info;
			}
			targetFont.characterInfo = characterInfo;
			if (fontMaterial)
			{
				fontMaterial.mainTexture = fontTexture;
			}
			targetFont.material = fontMaterial;
			fontMaterial.shader = Shader.Find("UI/Default");

			Debug.Log("create font <" + targetFont.name + "> success");
			Close();
		}
	}
}
