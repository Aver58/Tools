using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class PdrMenuOptions
{
    //批量选中Assets目录下的对象的处理
    public static void ExportSelection(string name, System.Action<UnityEngine.Object> onAction, SelectionMode mode = SelectionMode.DeepAssets)
    {
        UnityEngine.Object[] items = Selection.GetFiltered(typeof(UnityEngine.Object), mode);
        for(int i = 0; i < items.Length; ++i)
        {
            if(onAction != null)
            {
                onAction(items[i]);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log(string.Format("{0} Completed", name));
    }

    public static void ClearConsole()
    {
        // 5.x的方法
        // var logEntries = Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        // // debug下检查括号内条件, 若条件为false。则会显示一个消息框，其中会显示调用堆栈
        // System.Diagnostics.Debug.Assert(logEntries != null, "logEntries != null");
        // var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        // clearMethod.Invoke(null, null);

        // 2017+的方法
        Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
        Type logEntries = assembly.GetType("UnityEditor.LogEntries");
        System.Diagnostics.Debug.Assert(logEntries != null, "logEntries != null");
        MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
        clearConsoleMethod.Invoke(new object(), null);
    }

    public static void ExcuteAssetOperat(System.Action<UnityEngine.Object> action, SelectionMode Mode = SelectionMode.Deep)
    {
        //ClearConsole();
        UnityEngine.Object[] items = Selection.GetFiltered(typeof(UnityEngine.Object), Mode);
        foreach(var item in items)
        {
            Debug.Log(item);
            action(item);
        }
        AssetDatabase.Refresh();
        Debug.Log("----------------Finish----------------");
    }

    //[MenuItem("Assets/导入设置UI碎图", false, 100)]
    //public static void AssetsImportUISprite()
    //{
    //    ExportSelection("Import a sprite", ImporterSpriteTP.ImportSelectUISPrite);
    //}
    [MenuItem("Assets/打压缩图集", false, 100)]
    static void ExportCompressAtlas()
    {
        ExcuteAssetOperat(obj =>
        {
            bool isCompress = true;
            bool isSplitChannel = true;
            PdrAtlasMaker.MakeAtlas(obj, isCompress, isSplitChannel);
        }, SelectionMode.Assets);
    }
    //[MenuItem("Assets/打真彩图集", false, 100)]
    //static void ExportTrueColorAtlas()
    //{
    //    PdrEditorHelper.ExcuteAssetOperat(obj =>
    //    {
    //        bool isCompress = false;
    //        bool isSplitChannel = false;
    //        PdrAtlasMaker.MakeAtlas(obj, isCompress, isSplitChannel);
    //    }, SelectionMode.Assets);
    //}
    //[MenuItem("Assets/只打TP图集", false, 101)]
    //static void ExportNoSplitAtlas()
    //{
    //    PdrEditorHelper.ExcuteAssetOperat(obj =>
    //    {
    //        PdrAtlasMaker.MakeOnlyTP(obj);
    //    }, SelectionMode.Assets);
    //}
    
    //[MenuItem("Assets/剥离透明通道 %p", false, 103)]
    //public static void SplitChannel()
    //{
    //    var activeObjs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
    //    for (int i = 0; i < activeObjs.Length; i++)
    //    {
    //        string path = AssetDatabase.GetAssetPath(activeObjs[i]);
    //        PdrAtlasMaker.BuildSplitChannel(path);
    //    }
    //}
}