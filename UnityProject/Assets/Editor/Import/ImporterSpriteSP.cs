//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using UnityEngine.UI;
//using System.Text;
//public class ImporterSpriteSP:ImporterSpriteBase
//{
//	// 重置所有图集
//	public static void Import()
//	{
//		DealSubPath ("Assets/Data/ui/atlas/", ImportSingleUISPriteCompress, new string[] { "common32"});
//		DealSubPath ("Assets/Art/ui/atlas/", ImportSingleUISPriteCompress);
//		// 真彩目录
//		DealPath ("Assets/Data/ui/atlas/common32", ImportSingleUISPriteTrueColor);
//	}
//	// 重置所有UI图集名称
//	public static void ResetAtlasName()
//	{
//		// UI
//		DealPath ("Assets/Data/ui/atlas/", SetSingleAtlasName);
//		DealPath ("Assets/Art/ui/atlas/", SetSingleAtlasName);
//	}

//	// 导入选中的UI图片
//	public static void ImportSelectUISPrite(Object obj)
//	{
//		DealSelectedObj (obj, ImportSingleUISPriteCompress);
//		Debug.Log ("ImportSelectAtlas Complete！");
//	}
//	// 导入选中的UI真彩图片
//	public static void ImportSelectUISPriteTrueColor(Object obj)
//	{
//		DealSelectedObj (obj, ImportSingleUISPriteTrueColor);
//		Debug.Log ("ImportSelectAtlas Complete！");
//	}
//	// 导入选中的场景图片
//	public static void ImportSelectSceneSprite(Object obj)
//	{
//		DealSelectedObj (obj, ImportSingleSceneSprite);
//		Debug.Log ("ImportSelectAtlas Complete！");
//	}

//	// 重置选中图片的图集名字
//	public static void ResetSelectAtlasName(Object obj)
//	{
//		DealSelectedObj (obj, SetSingleAtlasName);
//	}
//	//清除图集设置
//	public static void ResetAtlasSetting(Object obj)
//	{
//		DealSelectedObj (obj, ClearSingleAtlas);
//		Debug.Log ("Reset Atlas Setting Complete！");
//	}

//	public static void CloseMipmapOfTerrain()
//	{
//		DealPath ("Assets/Art/scene/terrain", CloseTerrainSpriteMipmaps);
//	}

//    public static void DebugLogAllAtlas(object obj)
//    {
//        if (obj.GetType() == typeof(GameObject))
//        {
//            List<string> atlas = new List<string>();
//            List<string> noneAlas = new List<string>();
//            GameObject go = obj as GameObject;
//            var trans = go.GetComponentsInChildren<Transform>();
//            foreach (var item in trans)
//            {
//                Sprite tex = null;
//                var image = item.GetComponent<Image>();

//                if (image != null)
//                {
//                    tex = image.sprite;
//                }
//                else
//                {
//                    var rt = item.GetComponent<SpriteRenderer>();
//                    if (rt != null)
//                    {
//                        tex = rt.sprite;
//                    }
//                }

//                if (tex)
//                {
//                    string path = AssetDatabase.GetAssetPath(tex);
//                    TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
//                    if (ti != null)
//                    {
//                        if (!atlas.Contains(ti.spritePackingTag))
//                        {
//                            atlas.Add(ti.spritePackingTag);
//                        }
//                    }
//                    else
//                    {
//                        noneAlas.Add(item.name + ": " + path);
//                    }
//                }
//            }

//            StringBuilder sb = new StringBuilder(go.name).Append("\n\n-----使用的图集：  \n");
//            foreach (var item in atlas)
//            {
//                sb.Append(item).Append("\n");
//            }

//            sb.Append("\n\n-----未使用图集对象：  \n");
//            foreach (var item in noneAlas)
//            {
//                sb.Append(item).Append("\n");
//            }

//            Debug.Log(sb.ToString());
//        }
//    }

//	#region 内部工具接口

//	// 清除图集设置
//	private static void ClearSingleAtlas(string strFile)
//	{
//		TextureImporter importer = TextureImporter.GetAtPath(strFile) as TextureImporter;
//		if(importer==null)
//		{
//			Debug.LogErrorFormat("Reset atlas:{0}, is not a texture!", strFile);
//			return;
//		}
//		importer.spritePackingTag = "";
//		importer.SaveAndReimport ();
//	}

//	// 导入单张图片--UI ETC
//	private static void ImportSingleUISPriteCompress(string strFile)
//	{
//		string strPath = Path.GetDirectoryName (strFile);
//		string strSPName = GetSpriteNameFromPath (strPath);

//		TextureImporter importer = TextureImporter.GetAtPath (strFile) as TextureImporter;
//		if(importer==null)
//		{
//			Debug.LogErrorFormat ("Import atlas:{0}, is not a texture!", strFile);
//			return;
//		}
//		Debug.LogFormat ("TextureImport:{0}-->spname:{1}", strFile, strSPName);
//		importer.textureType = TextureImporterType.Sprite;
//        if (importer.spriteImportMode == SpriteImportMode.Multiple)
//            importer.spritePackingTag = string.Empty;
//        else
//            importer.spritePackingTag = strSPName;
//		importer.mipmapEnabled = false;
//        importer.alphaIsTransparency = true;
//		importer.wrapMode = TextureWrapMode.Clamp;
//		if(importer.spriteImportMode==SpriteImportMode.None)
//		{
//			importer.spriteImportMode = SpriteImportMode.Single;
//		}
		
//		SetSpriteFullRect(importer);

//		importer.ClearPlatformTextureSettings ("iPhone");
//		// importer.ClearPlatformTextureSettings ("Android");
//		SetAndroidPlatformSetting(importer);

//		importer.SaveAndReimport ();
//	}
//	private static void SetAndroidPlatformSetting(TextureImporter importer)
//	{
//		TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
//		platformSettings.name = "Android";
//		platformSettings.allowsAlphaSplitting = true;
//		platformSettings.textureCompression =  TextureImporterCompression.Compressed;
//		platformSettings.format = TextureImporterFormat.ETC_RGB4;
//		platformSettings.maxTextureSize = 2048;
//		platformSettings.overridden = true;
//		importer.SetPlatformTextureSettings(platformSettings);
//	}

//	// 导入单张图片 -- UI真彩
//	private static void ImportSingleUISPriteTrueColor(string strFile)
//	{
//		string strPath = Path.GetDirectoryName (strFile);
//		string strSPName = GetSpriteNameFromPath (strPath);

//		TextureImporter importer = TextureImporter.GetAtPath (strFile) as TextureImporter;
//		if(importer==null)
//		{
//			Debug.LogErrorFormat ("Import atlas:{0}, is not a texture!", strFile);
//			return;
//		}
//		Debug.LogFormat ("TextureImport:{0}-->spname:{1}", strFile, strSPName);
//		importer.textureType = TextureImporterType.Sprite;
//		importer.spritePackingTag = strSPName;
//		importer.mipmapEnabled = false;
//        importer.alphaIsTransparency = true;
//        importer.wrapMode = TextureWrapMode.Clamp;
//		importer.spriteImportMode = SpriteImportMode.Single;

//		SetSpriteFullRect(importer);
		
//		SetTrueColorPlatformSetting(importer,"Android");
//		SetTrueColorPlatformSetting(importer,"iPhone");

//		importer.SaveAndReimport ();
//	}

//	private static void SetTrueColorPlatformSetting(TextureImporter importer, string platform)
//	{
//		TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
//		platformSettings.name = platform;
//		platformSettings.allowsAlphaSplitting = false;
//		platformSettings.textureCompression =  TextureImporterCompression.Uncompressed;
//		platformSettings.format = TextureImporterFormat.RGBA32;
//		platformSettings.maxTextureSize = 1024;
//		platformSettings.overridden = true;
//		importer.SetPlatformTextureSettings(platformSettings);
//	}

//	// 导入单张图片-- 场景图片
//	private static void ImportSingleSceneSprite(string strFile)
//	{
//		string strPath = Path.GetDirectoryName (strFile);
//		string strSPName = GetSpriteNameFromPath (strPath);

//		TextureImporter importer = TextureImporter.GetAtPath (strFile) as TextureImporter;
//		if(importer==null)
//		{
//			Debug.LogErrorFormat ("Import atlas:{0}, is not a texture!", strFile);
//			return;
//		}
//		Debug.LogFormat ("TextureImport:{0}-->spname:{1}", strFile, strSPName);
//		importer.textureType = TextureImporterType.Sprite;
//		importer.spritePackingTag = strSPName;
//		importer.mipmapEnabled = false;
//		importer.wrapMode = TextureWrapMode.Clamp;
//		importer.spriteImportMode = SpriteImportMode.Single;
//		importer.spritePixelsPerUnit = 100;

//		TextureImporterSettings tSetting = new TextureImporterSettings();
//		importer.ReadTextureSettings(tSetting);
//		importer.SetTextureSettings(tSetting);
		
//		SetSpriteFullRect(importer);

//		importer.ClearPlatformTextureSettings ("iPhone");
//		// importer.ClearPlatformTextureSettings ("Android");
//		SetAndroidPlatformSetting(importer);

//		importer.SaveAndReimport ();
//	}

//	private static string GetSpriteNameFromPath(string strSpritePath)
//	{
//		strSpritePath = strSpritePath.Replace ("/", "_");
//		string strAtlasName = strSpritePath.Replace ("Assets_Art_", "").Replace ("Assets_Data_", "").ToLower();
//		return strAtlasName;
//	}

//	// 设置单张图片的图集名称
//	private static void SetSingleAtlasName(string strFile)
//	{
//        strFile = strFile.Replace('\\', '/');
//        string strDiretoryName = FileHelper.GetLastDirectoryFromFile(strFile);
//        Debug.LogFormat("SetSingleAtlasName, file:{0}, directory:{1}", strFile, strDiretoryName);
//        // bg贴图的图集不自动设置
//        if(strDiretoryName.IndexOf("background")>=0 || strDiretoryName.IndexOf("hd_word")>=0)
//        {
//            return;
//        }
//		string strPath = Path.GetDirectoryName (strFile);
//		string strSPName = GetSpriteNameFromPath (strPath);

//		TextureImporter importer = TextureImporter.GetAtPath (strFile) as TextureImporter;
//		if(importer==null)
//		{
//			Debug.LogErrorFormat ("Import atlas:{0}, is not a texture!", strFile);
//			return;
//		}
//		Debug.LogFormat ("TextureImport:{0}-->spname:{1}", strFile, strSPName);
//		if(importer.textureType != TextureImporterType.Sprite)
//		{
//			// ImportSingleUISPriteCompress (strFile);
//			return;
//		}

//		if(importer.spritePackingTag!=strSPName)
//		{
//			importer.spritePackingTag = strSPName;
//			importer.SaveAndReimport ();
//		}
//	}

//	// 关闭场景贴图的mipmap选项
//	private static void CloseTerrainSpriteMipmaps(string strFile)
//	{
//		TextureImporter importer = TextureImporter.GetAtPath (strFile) as TextureImporter;
//		if(importer==null)
//		{
//			Debug.LogErrorFormat ("Import atlas:{0}, is not a texture!", strFile);
//			return;
//		}
//		if (importer.mipmapEnabled)
//		{
//			importer.mipmapEnabled = false;
//			importer.SaveAndReimport ();
//		}
//	}

//    #endregion

//    public static void SetClipBlank()
//    {
//        Object[] textures = GetSelected<Texture2D>();
//        foreach(Texture2D texture in textures)
//        {
//            string path = AssetDatabase.GetAssetPath(texture);
//            ImportSingleUISPriteCompress(path);

//            Texture2D oldTexture = LoadTexture(path);
//            float oldWidth = oldTexture.width;
//            float oldHeight = oldTexture.height;
//            //Debug.Log("oldWidth "+ oldWidth+ " oldHeight "+ oldHeight);

//            Vector2 newPivot = GetNewPivot(oldTexture);
//            TextureImporter importer = GetImporter<TextureImporter>(path);

//            TextureImporterSettings tis = new TextureImporterSettings();
//            importer.ReadTextureSettings(tis);
//            tis.spriteAlignment = (int)SpriteAlignment.Custom;
//            tis.spritePivot = newPivot;
//            importer.SetTextureSettings(tis);

//            importer.SaveAndReimport();

//            // 裁切空白部分 
//            oldTexture = CutClipBlank(oldTexture);
//            byte[] btr = oldTexture.EncodeToPNG();
//            File.WriteAllBytes(path, btr);
//            AssetDatabase.Refresh();
//        }
//        Debug.Log("SetClipBlank Complete！");
//    }

//    //锚点是左下角坐标系
//    public static Vector2 GetNewPivot(Texture2D texture)
//    {
//        //计算旧锚点位置在新图中的位置
//        float basePivotPosX = texture.width / 2;
//        float basePivotPosY = texture.height / 2;

//        Vector4 border = GetBorder(texture);
//        var left = border.x;
//        var bottom = border.y;
//        var right = border.z;
//        var top = border.w;

//        //Debug.Log("left：" + left + " bottom：" + bottom + " right：" + right + " top： " + top);

//        // 用左下角做坐标轴（扣去偏移，得到：旧锚点在新坐标轴中的位置）
//        float newPivotPosX = basePivotPosX - left;
//        float newPivotPosY = basePivotPosY - top;
//        float newWidth = right - left;
//        float newHeight = bottom - top;
//        //Debug.Log(" newPivot " + newPivotPosX + "," + newPivotPosY + " newWidth " + newWidth + " newHeight " + newHeight);

//        float newPivotX = newPivotPosX > newWidth ? 1 : newPivotPosX / newWidth;
//        float newPivotY = newPivotPosY > newHeight ? 1 : newPivotPosY / newHeight;
//        Vector2 newPivot = new Vector2(newPivotX, newPivotY);
//        //Debug.Log("newPivot " + newPivot.x + " , " + newPivot.y);
//        return newPivot;
//    }

//    /// <summary>
//    /// 获取选择的对象
//    /// </summary>
//    /// <returns></returns>
//    public static Object[] GetSelected<T>()
//    {
//        return Selection.GetFiltered(typeof(T), SelectionMode.DeepAssets);
//    }

//    /// <summary>
//    /// 获取importer
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    /// <param name="path"></param>
//    /// <returns></returns>
//    public static T GetImporter<T>(string path)where T: AssetImporter
//    {
//        return AssetImporter.GetAtPath(path) as T;
//    }

//    public static Texture2D LoadTexture(string FullPath)
//    {
//        // 创建文件读取流
//        FileStream fileStream = new FileStream(FullPath, FileMode.Open, FileAccess.Read);
//        fileStream.Seek(0, SeekOrigin.Begin);
//        //创建文件长度缓冲区
//        byte[] bytes = new byte[fileStream.Length];
//        //将png文件读成字节数组
//        fileStream.Read(bytes, 0, (int)fileStream.Length);
//        //释放文件读取流
//        fileStream.Close();
//        fileStream.Dispose();
//        fileStream = null;

//        //创建Texture,不用在意这个的大小，这个的尺寸会被bytes覆盖
//        Texture2D texture = new Texture2D(1024, 1024);
//        //将图片字节流转为Unity支持的Texture2D格式
//        texture.LoadImage(bytes);

//        return texture;
//    }

//    public static Vector4 GetBorder(Texture2D orgin)
//    {
//        float left = 0, top = 0, right = 0, bottom = 0;

//        var find = false;
//        // 左侧
//        for(var i = 0; i < orgin.width; i++)
//        {
//            find = false;
//            for(var j = 0; j < orgin.height; j++)
//            {
//                var color = orgin.GetPixel(i, j);
//                //alpha大于0；
//                if(System.Math.Abs(color.a) > 1e-6)
//                {
//                    find = true;
//                    break;
//                }
//            }
//            if(find)
//            {
//                left = i;
//                break;
//            }
//        }

//        // 右侧
//        for(var i = orgin.width - 1; i >= 0; i--)
//        {
//            find = false;
//            for(var j = 0; j < orgin.height; j++)
//            {
//                var color = orgin.GetPixel(i, j);
//                if(System.Math.Abs(color.a) > 1e-6)
//                {
//                    find = true;
//                    break;
//                }
//            }
//            if(find)
//            {
//                right = i;
//                break;
//            }
//        }

//        // 上侧
//        for(var j = 0; j < orgin.height; j++)
//        {
//            find = false;
//            for(var i = 0; i < orgin.width; i++)
//            {
//                var color = orgin.GetPixel(i, j);
//                if(System.Math.Abs(color.a) > 1e-6)
//                {
//                    find = true;
//                    break;
//                }
//            }
//            if(find)
//            {
//                top = j;
//                break;
//            }
//        }

//        // 下侧
//        for(var j = orgin.height - 1; j >= 0; j--)
//        {
//            find = false;
//            for(var i = 0; i < orgin.width; i++)
//            {
//                var color = orgin.GetPixel(i, j);
//                if(System.Math.Abs(color.a) > 1e-6)
//                {
//                    find = true;
//                    break;
//                }
//            }
//            if(find)
//            {
//                bottom = j;
//                break;
//            }
//        }

//        //X = left, Y = bottom, Z = right, W = top.
//        return new Vector4(left, bottom,right, top);
//    }

//    public static Texture2D CutClipBlank(Texture2D origin)
//    {
//        try
//        {
//            int left = 0, top = 0, right = 0, bottom = 0;

//            Vector4 border = GetBorder(origin);
//            //X = left, Y = bottom, Z = right, W = top.
//            left = (int)border.x;
//            bottom = (int)border.y;
//            right = (int)border.z;
//            top = (int)border.w;

//            // 创建新纹理
//            var width = right - left;
//            var height = bottom - top;

//            var result = new Texture2D(width, height, TextureFormat.ARGB32, false)
//            {
//                alphaIsTransparency = true
//            };

//            // 复制有效颜色区块
//            //获取以x,y 为起始点，大小为width,height的一个区块，
//            //返回的是一个数组，数组内颜色的点顺序为从左至右，从下至上
//            var colors = origin.GetPixels(left, top, width, height);
//            //result.SetPixels(colors);
//            result.SetPixels(0, 0, width, height, colors);

//            result.Apply();
//            return result;
//        }
//        catch(System.Exception e)
//        {
//            Debug.Log(e.ToString());
//            return null;
//        }

//        //构建裁剪的矩形时，以左上角为原点，向右为X正轴，向下为Y正轴
//    }
//}
