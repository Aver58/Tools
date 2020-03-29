using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PdrAtlasMaker
{
    static string PATH_ATLAS = PathDef.SourceAtlasPath;
    static string PATH_TP_TEMP = PathDef.TempTPPath;
    static string PATH_ATLAS_TP = PathDef.AtlasTPPath;

    #region 对外接口
    public static void MakeAtlas(Object obj, bool isCompress, bool isSplitChannel)
    {
        string sorPath = AssetDatabase.GetAssetPath(obj);
        string name = Path.GetFileNameWithoutExtension(sorPath);
        ExportAtlas(sorPath, name, isCompress, isSplitChannel);
    }
    //public static void MakeOnlyTP(Object obj)
    //{
    //    string sorPath = AssetDatabase.GetAssetPath(obj);
    //    string name = Path.GetFileNameWithoutExtension(sorPath);
    //    ExportOnlyTP(sorPath, name);
    //}
    //private static void ExportOnlyTP(string path, string name)
    //{

    //    int totalCount = 1;
    //    int current = 0;

    //    bool trim = true;


    //    AtlasEditorHelper.PlayProgressBar(name, "MAKE_ATLAS", (++current) / totalCount);
    //    AtlasEditorHelper.BuildMakeAtlas(name, path, path, trim);
    //    AtlasEditorHelper.ClearProgressBar();

    //}

    //拆分通道
    public static void BuildSplitChannel(string path)
    {
        if (File.Exists(path))
        {
            int index = path.LastIndexOf("/");
            string name = Path.GetFileNameWithoutExtension(path);
            string p = Path.GetDirectoryName(path);
            AtlasEditorHelper.BuildSplitChannel(name,p,p,false);
        }
        else
        {
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            foreach (FileInfo info in files)
            {
                string name = info.Name;
                name = name.Replace(".png", "");
                AtlasEditorHelper.BuildSplitChannel(name,path,path,false);
            }
        }
        Debug.Log("SplitPngChannel Completed");
    }
    //public static void MakeSpineSplitSprite(string path)
    //{
    //    OptimizingSpineResource(path);
    //}
    // 打所有图集
    //public static void MakeAllAtlas()
    //{
    //    DirectoryInfo root = new DirectoryInfo(PATH_ATLAS);
    //    int count = root.GetDirectories().Length;
    //    int index = 0;
    //    foreach (DirectoryInfo atlas in root.GetDirectories())
    //    {
    //        index++;
    //        ExportAtlasWithoutProgress(PATH_ATLAS + "/" + atlas.Name, atlas.Name);
    //        EditorUtility.DisplayProgressBar(index + "/" + count, atlas.Name, (float)index / (float)count);
    //    }
    //    EditorUtility.ClearProgressBar();
    //    Debug.Log("Build All Atlas Completed");
    //}
    #endregion


    public static void ExportAtlas(string path, string name, bool isCompress = true, bool isSplitChannel = true)
    {
        bool trim = true;

        int totalCount = 5;
        int current = 0;

        EditorUtility.DisplayProgressBar(name, "MAKE_ATLAS", (++current) / totalCount);
        AtlasEditorHelper.BuildMakeAtlas(name, path, PATH_TP_TEMP,trim);

        EditorUtility.DisplayProgressBar(name, "SPLIT_CHANNEL", (++current) / totalCount);
        string pathSplitDst = string.Format("{0}/{1}", PATH_ATLAS_TP, name);
        AtlasEditorHelper.BuildSplitChannel(name, PATH_TP_TEMP, pathSplitDst, isCompress, isSplitChannel);

        EditorUtility.DisplayProgressBar(name, "CREATE_MATERIAL", (++current) / totalCount);
        AtlasEditorHelper.BuildCreateMaterial(name, PATH_ATLAS_TP, isSplitChannel);

        EditorUtility.DisplayProgressBar(name, "IMPORT_SPRITE", (++current) / totalCount);
        AtlasEditorHelper.BuildImportSprite(name, path, PATH_TP_TEMP, PATH_ATLAS_TP, isCompress);

        EditorUtility.DisplayProgressBar(name, "CREATE_PREFAB", (++current) / totalCount);
        AtlasEditorHelper.BuildCreateSpritePrefab(name, PATH_TP_TEMP, PATH_ATLAS_TP);

        EditorUtility.ClearProgressBar();
    }
    //private static void ExportAtlasWithoutProgress(string path, string name, bool isCompress = true, bool isSplitChannel = true)
    //{

    //    bool trim = true;

    //    AtlasEditorHelper.BuildMakeAtlas(name, path, PATH_TP_TEMP,trim);
    //    string pathSplitDst = string.Format("{0}/{1}", PATH_ATLAS_TP, name);

    //    AtlasEditorHelper.BuildSplitChannel(name, PATH_TP_TEMP, pathSplitDst, isCompress, isSplitChannel);

    //    AtlasEditorHelper.BuildCreateMaterial(name, PATH_ATLAS_TP, isSplitChannel);

    //    AtlasEditorHelper.BuildImportSprite(name, path, PATH_TP_TEMP, PATH_ATLAS_TP, isCompress);

    //    AtlasEditorHelper.BuildCreateSpritePrefab(name, PATH_TP_TEMP, PATH_ATLAS_TP);
    //}


    //private static void OptimizingSpineResource(string path)
    //{

    //    List<string> files = new List<string>();
    //    PdrFileUtil.GetAllChildFiles(path, "mat", files);
    //    foreach (string file in files)
    //    {
    //        Material mat = AssetDatabase.LoadAssetAtPath<Material>(file);
    //        if (!mat.shader.name.Contains("Spine/"))
    //        {
    //            continue;
    //        }
    //        Texture texture = mat.mainTexture;
    //        if (texture == null)
    //        {
    //            continue;
    //        }

    //        string pathTextureFull = AssetDatabase.GetAssetPath(texture);
    //        string pathTexture = pathTextureFull;
    //        string name = Path.GetFileNameWithoutExtension(pathTextureFull);
    //        pathTexture = Path.GetDirectoryName(pathTexture);

    //        string pathC = string.Format("{0}/{1}_c.png", pathTexture, name);
    //        string pathA = string.Format("{0}/{1}_a.png", pathTexture, name);
    //        //判断是否拆过通道 如果拆过只是设置一下贴图就了事
    //        if(!FileHelper.IsFileExist(pathA))
    //        {
    //            AtlasEditorHelper.BuildSplitChannel(name, pathTexture, pathTexture);
    //            FileHelper.DeleteFile(pathTextureFull);
    //            File.Move(pathC, pathTextureFull);
    //        }

    //        Texture texC = AssetDatabase.LoadAssetAtPath<Texture>(pathTextureFull);
    //        Texture texA = AssetDatabase.LoadAssetAtPath<Texture>(pathA);

    //        mat.shader = Shader.Find("Spine/SkeletonGraphic (Premultiply Alpha)");
    //        mat.mainTexture = texC;
    //        mat.SetTexture("_MainTex", texC);
    //        mat.SetTexture("_AlphaTex", texA);
    //        EditorUtility.SetDirty(mat);
		  //  AssetDatabase.SaveAssets();
    //        AssetDatabase.Refresh();
    //    }
    //}
}

