//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using UnityEngine.UI;
//using System.Text;
//public class ImporterSpriteTP:ImporterSpriteBase
//{
//	// 导入选中的UI图片
//	public static void ImportSelectUISPrite(Object obj)
//	{
//		DealSelectedObj (obj, ImportSingleUISPrite);
//		Debug.Log ("ImportSelectAtlas Complete！");
//	}

//	#region 内部工具接口

//    // 导入单张图片--UI
//	private static void ImportSingleUISPrite(string strFile)
//	{
//		string strPath = Path.GetDirectoryName (strFile);

//		TextureImporter importer = TextureImporter.GetAtPath (strFile) as TextureImporter;
//		if(importer==null)
//		{
//			Debug.LogErrorFormat ("Import atlas:{0}, is not a texture!", strFile);
//			return;
//		}
//		importer.textureType = TextureImporterType.Sprite;
//		importer.mipmapEnabled = false;
//        importer.alphaIsTransparency = true;
//		importer.wrapMode = TextureWrapMode.Clamp;
//		if(importer.spriteImportMode==SpriteImportMode.None)
//		{
//			importer.spriteImportMode = SpriteImportMode.Single;
//		}
		
//		// SetSpriteFullRect(importer);

//		importer.SaveAndReimport ();
//	}

//    #endregion
//}
