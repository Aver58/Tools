//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Debugger = LuaInterface.Debugger;

//public class ImporterAssetBundle
//{
//	static Dictionary<string, string> ms_uiBundles = new Dictionary<string, string>();
//	public static void Reimport(bool buildLua = false)
//	{
//		ClearBundleName ();

//		// -- 设置图集名字
//		// ImporterSprite.ResetAtlasName();

//		ms_uiBundles.Clear ();

//		ImportAll ();

//		// 因为ImportAll里面会生成BundleDefine.lua，所以lua得在此之后build，build完了之后再设置lua的abname
//		if(buildLua)
//		{
//			LuaBuilder.Build ();
//			// 导入Lua
//			ImportLua();
//		}

//		AssetDatabase.RemoveUnusedAssetBundleNames ();
//		AssetDatabase.Refresh ();

//		Debugger.Log ("AssetBundle Reimport Completed");
//	}

//	// 清除AssetBundle设置
//	public static void ClearBundleName()
//	{
//		var names = AssetDatabase.GetAllAssetBundleNames();
//		foreach (string name in names)
//		{
//			AssetDatabase.RemoveAssetBundleName(name, true);
//		}
//	}

//	private static void ImportAll()
//	{
//        // 导入场景
//        ImportScenes();
//        // 导入UI
//        ImportUI();
//        // 导入特效
//		ImportSingleFile("Assets/Art/effect/common","art/effect/common", false); //scene和ui的公共ab
//		ImportSubPath("Assets/Art/effect/scene","effect/scene/", false); //scene和ui模块分开放
//		ImportSubPath("Assets/Art/effect/ui", "effect/", false);
//		ImportSubPath("Assets/Data/effect", "effect/", false);

//        // UNITY内置资源
//        ImportSingleFile("Assets/Art/builtin", "art/unity_builtin", false);

//        // Shader
//        ImportSingleFile("Assets/Shader", "shader",false);

//        // 导入Art目录 一般是一些common目录
//        ImportSingleFile("Assets/Art/character/common", "art/character/common",false);
//		ImportSingleFile("Assets/Art/character/animator", "art/character/animator",false);
//		ImportSingleFile("Assets/Art/character/children/Materials", "art/character/children/Materials",false);
//		ImportSubPath("Assets/Art/character/other", "art/character/other/",false);
//        ImportSubPath("Assets/Art/spine/other", "art/spine/other/", false);
//		ImportSubPath("Assets/Art/scene/city2d", "art/scene/city2d/", false);

//        // Data目录
//		ImportSingleFile("Assets/Data/ui/font/static_font", "ui/font/static_font", false);
//        ImportFilesInPath("Assets/Data/character/hunt", "character/hunt/", true);
//        ImportFilesInPath("Assets/Data/character/children", "character/children/", true);
//		ImportFilesInPath("Assets/Data/character/other", "character/other/", true);
//        // sound
//		ImportSingleFile("Assets/Data/sound/new", "sound/new", false);
//		ImportSubPath("Assets/Data/sound/voice/npc", "sound/voice/npc/", false);
//		ImportSubPath("Assets/Data/sound/voice/cg", "sound/voice/cg/", false);
//		ImportSubPath("Assets/Data/sound/voice/page", "sound/voice/page/", false);
//		ImportFilesInPath("Assets/Data/sound/music", "sound/music/", false);
//        // scene
//        ImportFilesInPath("Assets/Data/scene/children", "scene/children/", true);
//		ImportFilesInPath("Assets/Data/scene/dance", "scene/dance/", true);
//        // spine
//        ImportFilesInPath("Assets/Data/spine/hero", "spine/hero/", true);
//        ImportFilesInPath("Assets/Data/spine/lord", "spine/lord/", true);
//        ImportFilesInPath("Assets/Data/spine/marriage", "spine/marriage/", true);
//        ImportFilesInPath("Assets/Data/spine/npc", "spine/npc/", true);
//        ImportFilesInPath("Assets/Data/spine/children", "spine/children/", true);
//        ImportFilesInPath("Assets/Data/spine/other", "spine/other/", true);
//        ImportFilesInPath("Assets/Data/spine/allianceDungeon", "spine/allianceDungeon/", true);

//        SaveBundleList(ms_uiBundles, "BundleDefine");
//    }

//    public static void ExportEffectPrefabDefine()
//	{
//		GenUIPrefabDefine("Assets/Data/effect/", "effect", "EffectDefine");
//		Debug.Log("ExportEffectPrefabDefine Complete!!");
//	}

//    private static void ImportUI()
//    {
//        ImportSingleFile("Assets/Data/ui/animation", "ui/animation", false);
//		//-----------UIAtlas---------------
//		//-- 设置AB
//        // string[] arrArtExcludes = new string[] { "SpriteNeedDeleteFolder", "background"};
//		// SetBundleOfUIAtlasPath("Assets/Art/ui/atlas", false,arrArtExcludes);
//        // string[] arrDataExcludes = new string[] { "SpriteNeedDeleteFolder", "castle_sss", "dressup_castle"};
//		// SetBundleOfUIAtlasPath("Assets/Data/ui/atlas", true, arrDataExcludes);
//        ReimportAtlasTP();
//        ImportSubPath("Assets/Data/ui/atlas_tp", "ui/atlas_tp/",false);

//		//---------UIPanel---------------
//        // 本数组中的目录是一整个目录打一个bundle
//        // 其中common目录存放需要常驻内存的公共组件
//        string[] arrDirExcludes = new string[] {};//"common", "commonitems"
//		SetUIPrefabPath("Assets/Data/ui/panel/", "ui/panel/", arrDirExcludes);

//		string[] arrDirExcludesLR = new string[] {"common", "commonitems"};
//		SetUIPrefabPath("Assets/Data/ui/panel_lr/", "ui/panel_lr/", arrDirExcludesLR);
//    }

//    // 导入TP图集
//    static void ReimportAtlasTP()
//    {
//        string path = PathDef.UI_ASSETS_PATH + "/atlas_tp/";
//        if (!Directory.Exists(path))
//        {
//            return;
//        }

//        ImportSingleFile(PathDef.UI_ASSETS_PATH + "/atlas_tp_padding.asset", BaseDef.AtlasTpPaddingPath,false);
//        ImportSubPath("Assets/Data/ui/atlas_tp", "ui/atlas_tp/",false);
//	}

//	private static void SetUIPrefabPath(string strUIPath, string abHead, string[] arrDirExcludes)
//	{
//		if (!Directory.Exists(strUIPath))
//		{
//			Debugger.LogError("[未找到panel路径] path:{0}", strUIPath);
//			return;
//		}
//        //arrDirExcludes 列表中的目录 一个目录打一个bundle
//		for(int i=0; i<arrDirExcludes.Length; i++)
//		{
//			string strFile = arrDirExcludes[i];
//			ImportSingleFile(strUIPath+strFile, abHead+strFile, false);
//		}

//        // 其他目录，每个目录下的第一级prefab单独打包，第二级目录 一整个目录打一个包
//        string[] allDirs = Directory.GetDirectories(strUIPath);
//        for(int i=0; i< allDirs.Length; i++)
//        {
//            string strPath = allDirs[i];
//            strPath = strPath.Replace('\\', '/');
//            string strPathName = strPath.Substring(strPath.LastIndexOf('/') + 1);
//            if (EditorHelper.CheckStringInArray(arrDirExcludes, strPathName))
//            {
//                continue;
//            }
//            string strSubAbHead = abHead + strPathName + "/";
//            ImportFilesInPath(strPath, strSubAbHead, false, ".prefab");

//			ImportSubPath(strPath, strSubAbHead, false); // 预制关联的方式挂在预制上，不会需要直接加载
//        }
//	}

//	private static void SetBundleOfUIAtlasPath(string strSpritePath, bool includebackground, string[] arrDirExcludes)
//	{
//		string[] allSpDirs = Directory.GetDirectories(strSpritePath);
//		Debug.LogFormat ("import ui atlas, strSpritePath:{0}, allSpDirs:{1}", strSpritePath, allSpDirs.Length);
//		for(int i=0; i<allSpDirs.Length; i++)
//		{
//			string strPath = allSpDirs[i];
//			strPath = strPath.Replace('\\', '/');
//			string strPathName = strPath.Substring (strPath.LastIndexOf ('/') + 1);
//			// if (strPathName=="SpriteNeedDeleteFolder" || (!includebackground && strPathName=="background"))
//			// {
//			// 	continue;
//			// }
//            if (EditorHelper.CheckStringInArray(arrDirExcludes, strPathName))
//            {
//                continue;
//            }
//			string strABName = "ui/atlas/"+strPathName;
//			ImportSingleFile (strPath, strABName, false); // sprite 不加入BundleDefine
//		}
//        // art下的background 要特殊设置，底下有子文件夹
//        if(!includebackground)
//        {
//            string strBackgroundPath = strSpritePath + "/background";
//            ImportFilesInPath(strBackgroundPath, "ui/atlas/background/", false);
//            ImportSubPath(strBackgroundPath, "ui/atlas/background/", false);
//        }
//	}

//    private static void ImportScenes()
//    {
//        string[] arrSceneExcludes = new string[] { "GameMain", "Login", "Idle", "PathTest" };
//		ImportFilesInPath("Assets/Scenes", "scenes/", true, ".unity", arrSceneExcludes, SearchOption.AllDirectories);
//    }

//    private static void ImportLua()
//    {
//        //lua 编译后位置
//        for (int arch = 0; arch < 3; arch++)
//        {
//            string tmpPath = string.Format("Assets/{0}/{1}{2}/{3}", EditorDef.TEMP_LUA_PATH, BaseDef.LUAJIT_DIR, arch, BaseDef.TOLUA_PACKAGE);
//            if (Directory.Exists(tmpPath))
//            {
//				ImportSingleFile(tmpPath, BaseDef.TOLUA_PACKAGE + arch, false);
//            }

//            tmpPath = string.Format("Assets/{0}/{1}{2}/{3}", EditorDef.TEMP_LUA_PATH, BaseDef.LUAJIT_DIR, arch, BaseDef.LUA_PACKAGE);
//            if (Directory.Exists(tmpPath))
//            {
//				ImportSingleFile(tmpPath, BaseDef.LUA_PACKAGE + arch, false);
//            }  
//        }
//    }

//    #region 导入功能函数
//	// 将路径下的子路径，按目录设置Bundle
//	private static void ImportSubPath(string strPath, string abHead, bool bAddDefine, string[] excludes=null)
//	{
//		if (!Directory.Exists(strPath))
//		{
//			return;
//		}

//		string[] allChilds = Directory.GetDirectories(strPath);
//        for (int i=0; i<allChilds.Length ;i++)
//		{
//			string childPath = allChilds[i];
//			if(excludes!=null && EditorHelper.CheckStringInArray(excludes, childPath))
//			{
//				continue;
//			}
//			string strPathName = childPath.Replace('\\', '/');
//            strPathName = strPathName.Substring(strPathName.LastIndexOf('/') + 1);
//			string strAbName = abHead + strPathName;
//            Debugger.Log("ImportSubPath:{0}-->{1}", childPath, strAbName);

//			ImportSingleFile (childPath, strAbName, bAddDefine);
//		}
//	}

//	// 将路径下的每个文件导出成独立的Bundle 只处理一级文件
//	private static void ImportFilesInPath(string path, string abHead, bool bAddDefine, string suffix="", string[] excludes=null, SearchOption option=SearchOption.TopDirectoryOnly)
//	{
//		if (!Directory.Exists(path))
//		{
//			return;
//		}

//		string[] allFiles = FileHelper.GetAllChildFiles(path, suffix, option);
//        for (int i=0; i<allFiles.Length ;i++)
//		{
//			string file = allFiles[i];
//			if(file.EndsWith(".meta") || file.EndsWith("txt"))
//			{
//				continue;
//			}

//			string strFileName = Path.GetFileNameWithoutExtension (file);
//			if(excludes!=null && EditorHelper.CheckStringInArray(excludes, strFileName))
//			{
//				continue;
//			}

//			string abName = abHead + strFileName;
//            // Debugger.Log("ImportFileInPath:{0}-->{1}", file, abName);
            
//			ImportSingleFile (file, abName, bAddDefine);
//		}
//	}

//	// 设置单个文件（或目录）的ABName
//	private static void ImportSingleFile(string Path, string abName, bool bAddDefine)
//	{
//		AssetImporter importer = AssetImporter.GetAtPath(Path);
//		if(bAddDefine)
//		{
//			AddBundleDefine(Path, abName);
//		}

//		if (importer == null)
//		{
//			Debugger.LogError("[路径错误] path:{0}", Path);
//			return;
//		}
//		abName = abName.Replace ('\\', '_').Replace ('/','_');
//		importer.SetAssetBundleNameAndVariant(abName, BaseDef.AB_SUFFIX);
//	}
	
//    #endregion

//	private static void AddBundleDefine(string strPath, string strABName)
//	{
//		//如果是一个路径 才需要添加到字典
//		if(Directory.Exists(strPath))
//		{
//			string[] files = Directory.GetFiles(strPath,"*", SearchOption.AllDirectories);
//			for(int i=0; i<files.Length; i++)
//			{
//				string filename = files[i];
//				if(filename.EndsWith(".meta"))
//				{
//					continue;
//				}

//				// string strFileName = Path.GetFileNameWithoutExtension(filename);
//				AddToBundleDic(filename, strABName);
//			}
//		}
//		else if(File.Exists(strPath))
//		{
//			AddToBundleDic(strPath, strABName);
//		}
//	}
//	private static void AddToBundleDic(string strFileName, string strABName)
//	{
//		string strExtension = Path.GetExtension(strFileName);
//		string fullName = strFileName.Replace("Assets/Data/","").Replace(strExtension,"").Replace("\\", "/");

//        if (ms_uiBundles.ContainsKey(fullName))
//		{
//			Debug.LogWarningFormat("文件名字重复:{0}, strABName old:{1}, new:{2}", fullName, ms_uiBundles[fullName], strABName);
//		}

//        ms_uiBundles[fullName] = strABName;
//    }

//	static void SaveBundleList(Dictionary<string, string> dicAB, string strFileName)
//	{
//		string strSaveFile = string.Format("Assets/Lua/game/define/{0}.lua",strFileName);
//		// 保存到文件
//		StringBuilder builder = new StringBuilder ();
//		builder.AppendLine ("-- Create by Tool, don`t modify");
//		builder.AppendLine ();
		
//		string strHeadLine = strFileName + " = {";
//		builder.AppendLine (strHeadLine);
//		{
//			var iter = dicAB.GetEnumerator ();
//			while (iter.MoveNext ()) {
//				string prefabName = iter.Current.Key.Replace ("\\", "/").ToLower();
//				string abName = iter.Current.Value.Replace ("\\", "/").ToLower();
//				builder.AppendFormat ("\t[\"{0}\"] = \"{1}\",\n", prefabName, abName);
//			}
//		}

//		builder.AppendLine ("}");
//		builder.AppendLine ();
//		builder.AppendLine ("--EOF");

//		FileHelper.SaveTextToFile (builder.ToString (), strSaveFile);
//		EditorHelper.EditorSaveFile(strSaveFile);

//		ms_uiBundles.Clear ();
//	}

//	public static void ExportUIPrefabDefine()
//	{
//		Reimport();
//        // ImportUI(); 之前设置过ab的，移动文件夹后，如果没有重新清理ab name，则后续收集会出错，所以还是reimport
//		GenUIPrefabDefine("Assets/Data/ui/panel/", "ui/panel", "UIPanelDefine");
//		GenUIPrefabDefine("Assets/Data/ui/panel_lr/", "ui/panel_lr", "UIPanelLRDefine");
//		Debug.Log("ExportUIPrefabDefine Complete!!");
//	}

//	public static void GenUIPrefabDefine(string strPath, string strABHead, string strFileName)
//	{
//		if(!Directory.Exists(strPath))
//		{
//			Debugger.LogError("[未找到panel路径] path:{0}", strPath);
//			return;
//		}
//		Dictionary<string, string> dicPanelAB = new Dictionary<string,string>();
//		DealSubPathUIPrefab(strPath, null, strABHead, dicPanelAB);
//		SaveBundleList(dicPanelAB, strFileName);
//	}
//	private static void DealSubPathUIPrefab(string strPath, string strPathABName, string strABHead, Dictionary<string,string> dicAB)
//	{
//		if(!strPath.Contains("component"))
//		{
//			string[] arrAllFiles = FileHelper.GetAllChildFiles(strPath, ".prefab", SearchOption.TopDirectoryOnly);
//			for(int i=0; i<arrAllFiles.Length; i++)
//			{
//				string strFile = arrAllFiles[i];
//				string strFileName = Path.GetFileNameWithoutExtension(strFile);
//				if(dicAB.ContainsKey(strFileName))
//				{
//					Debug.LogErrorFormat("Panel名字重复，不允许:{0}", strFileName);
//					continue;
//				}
//				AssetImporter importer = AssetImporter.GetAtPath(strFile);
//				string strFileABPath = strABHead;
//				if(!string.IsNullOrEmpty(importer.assetBundleName)) //拆除smallpanel后，因为预制里面之前已经设置bundlename了，所以这里导致直接就设置了smallpanel/xxx.prefab，就错了
//				{
//					strFileABPath += "/" + strFileName;
//				}
//				dicAB.Add(strFileName, strFileABPath);
//			}
//		}
		

//		string[] allChilds = Directory.GetDirectories(strPath);
//        for (int i=0; i<allChilds.Length ;i++)
//		{
//			string strSubPath = allChilds[i];
//			AssetImporter importer = AssetImporter.GetAtPath(strSubPath);
//			string strSubABName = importer.assetBundleName;
//			string strSubABPath = strABHead;
//			string strPathName = strSubPath.Replace('\\', '/');
//			strPathName = strPathName.Substring(strPathName.LastIndexOf('/') + 1);
//			strSubABPath +=  "/" + strPathName;
//			DealSubPathUIPrefab(strSubPath, strSubABName, strSubABPath, dicAB);
//		}
//	}

//}