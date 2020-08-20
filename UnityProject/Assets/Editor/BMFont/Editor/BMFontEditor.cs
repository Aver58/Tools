using UnityEngine;
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
				int width = bmInfo.width;
				int height = bmInfo.height;
				int texWidth = bmFont.texWidth;
				int texHeight = bmFont.texHeight;

				float uvX = 1f * bmInfo.x / texWidth;
				float uvY = 1 - (1f * bmInfo.y / texHeight);//UV的坐标轴是以左上为0点
				float uvWidth = 1f * width / texWidth;
				float uvHeight = -1f * height / texHeight;

				info.uvBottomLeft = new Vector2(uvX, uvY);
				info.uvBottomRight = new Vector2(uvX + uvWidth, uvY);
				info.uvTopLeft = new Vector2(uvX, uvY + uvHeight);
				info.uvTopRight = new Vector2(uvX + uvWidth, uvY + uvHeight);

				info.minX = bmInfo.offsetX;
				info.minY = bmInfo.offsetY + height / 2;
				info.maxX = bmInfo.offsetX + width;//todo test
				info.maxY = bmInfo.offsetY + height;
				info.glyphWidth = width;
				info.glyphHeight = -height; // 不知道为什么要用负的，可能跟unity纹理uv有关  
				info.advance = bmInfo.advance;
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
