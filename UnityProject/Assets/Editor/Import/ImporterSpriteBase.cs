//using UnityEngine;
//using UnityEditor;
//using System.IO;
//public class ImporterSpriteBase
//{
//    protected static void SetSpriteFullRect(TextureImporter importer)
//	{
//		TextureImporterSettings settings = new TextureImporterSettings();
//		importer.ReadTextureSettings(settings);
//		settings.spriteMeshType = SpriteMeshType.FullRect;
//		settings.spriteExtrude = 0;
//		importer.SetTextureSettings(settings);
//	}
//    protected static void DealSelectedObj(Object obj, System.Action<string> dealFunc)
//	{
//		if (obj == null)
//			return;

//		string path = AssetDatabase.GetAssetPath (obj);
//		Debug.LogFormat ("Import Select:{0}", path);

//		System.Type typeObj = obj.GetType ();
//		if(typeObj == typeof(Texture2D))
//		{
//			dealFunc (path);
//		}
//		else if (typeObj==typeof(DefaultAsset))
//		{
//			DealPath (path, dealFunc);
//		}
//	}

//	protected static void DealSubPath(string strPath, System.Action<string> dealFunc, string[] excludes=null)
//	{
//		string[] arrSubPath = Directory.GetDirectories(strPath);
//		for(int i=0;i<arrSubPath.Length; i++)
//		{
//			string subPath = arrSubPath[i];
//            subPath = subPath.Replace('\\', '/');
//            string strPathName = subPath.Substring(subPath.LastIndexOf('/') + 1);
//			if (EditorHelper.CheckStringInArray(excludes, strPathName))
//			{
//				Debug.LogFormat("----跳过目录:{0}", subPath);
//				continue;
//			}
//			DealPath(subPath, dealFunc);
//		}
//	}

//    protected static void DealPath(string strPath, System.Action<string> dealFunc)
//	{
//		string[] arrFiles = FileHelper.GetAllChildFiles (strPath, ".png", SearchOption.AllDirectories);
//		int len = arrFiles.Length;
//		for(int i=0; i<len; i++)
//		{
//			string strFile = arrFiles [i];
//			dealFunc (strFile);
//			EditorUtility.DisplayProgressBar(strPath, strFile, ((float)i+1)/len);
//		}
//		EditorUtility.ClearProgressBar();
//	}
//}