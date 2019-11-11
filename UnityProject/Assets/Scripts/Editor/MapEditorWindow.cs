#region Copyright © 2018 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    MapEditorWindow.cs
 Author:      Zeng Zhiwei
 Time:        2019/10/23 20:43:17
=====================================================
*/
#endregion

using UnityEditor;
using UnityEngine;

public class MapEditorWindow : EditorWindow
{
    private int m_maxCommandCount;
    private int selUndoGridInt = 0;
    private string[] UndoStringList;

    private Vector2 m_scroll1 = Vector2.zero;
    private UndoRedoManager undoRedoManager;
    private static MapEditorWindow window;

    [MenuItem("Window/打开MapEditorWindow", false, 1)]
    public static void OpenMapEditorWindow()
    {
        ShowWindow();
    }

    public static void ShowWindow()
    {
        window = GetWindow<MapEditorWindow>();
        window.Show();
    }

    void Awake()
    {
        Init();
    }

    void OnDestroy()
    {
        EditorPrefs.SetInt("MaxCommandCount",m_maxCommandCount);
        SceneView.onSceneGUIDelegate -= ShowMenu;
    }

    void Init()
    {
        Debug.Log("Init MapEditorWindow");
        SceneView.onSceneGUIDelegate += ShowMenu;

        // 撤销
        m_maxCommandCount = EditorPrefs.GetInt("MaxCommandCount",30);
        undoRedoManager = new UndoRedoManager(m_maxCommandCount);
        selUndoGridInt = undoRedoManager.TotalCommandCount;
    }

    // 自定义SceneView ---- 主逻辑
    void ShowMenu(SceneView sceneView)
    {
        Event e = Event.current;
        OnRightMouseDown(e);
    }

    // 删除数据
    void DeleteOneGrid(int gridX, int gridY)
    {
        DeleteObjCommand deleteCmd = new DeleteObjCommand(gridX, gridY);
        deleteCmd.Excute();
        selUndoGridInt = undoRedoManager.AddCommand(deleteCmd);
    }

    // 添加数据
    void AddOneGrid(int gridX, int gridY)
    {
        AddObjCommand addCmd = new AddObjCommand(gridX, gridY);
        addCmd.Excute();
        selUndoGridInt = undoRedoManager.AddCommand(addCmd);
    }

    void OnRightMouseDown(Event e)
    {
        if (e != null && e.button == 1 && e.type == EventType.mouseDown)
        {
            //右键单击啦，在这里显示菜单
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("添加"), false, OnMenuClickAdd, "menu_1");
            menu.AddItem(new GUIContent("删除"), false, OnMenuClickDelete, "menu_2");
            menu.ShowAsContext();
        }
    }


    void OnMenuClickAdd(object userData)
    {
        int x = Random.Range(0, 10);
        int y = Random.Range(0, 10);
        AddOneGrid(x, y);
    }

    void OnMenuClickDelete(object userData)
    {
        int x = Random.Range(0, 10);
        int y = Random.Range(0, 10);
        DeleteOneGrid(x, y);
    }

    private void SaveData()
    {
        if (undoRedoManager != null)
        {
            // 关闭界面的时候，保存操作指令到本地
            undoRedoManager.Serialization();
        }
    }
    private void DrawUndoPanel()
    {
        //Undo.PerformUndo
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("操作记录：最多保存条数：");
        m_maxCommandCount = EditorGUILayout.IntField(m_maxCommandCount);
        if (GUILayout.Button("设置"))
        {
            window.Close();
        }
        EditorGUILayout.EndHorizontal();

        m_scroll1 = EditorGUILayout.BeginScrollView(m_scroll1);

        int totalCmdCount = undoRedoManager.TotalCommandCount;
        if (totalCmdCount > 0)
        {
            UndoStringList = undoRedoManager.GetUndoCommandStringList();

            // 回滚到指定步骤
            while (selUndoGridInt != undoRedoManager.SelectIndex)
            {
                if (selUndoGridInt < undoRedoManager.SelectIndex)
                {
                    undoRedoManager.Undo();
                }
                else if (selUndoGridInt > undoRedoManager.SelectIndex)
                {
                    undoRedoManager.Redo();
                }
            }

            selUndoGridInt = GUILayout.SelectionGrid(selUndoGridInt, UndoStringList, 1);
        }

        EditorGUILayout.EndScrollView();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical("Box");

        if (undoRedoManager != null)
        {
            DrawUndoPanel();
        }

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("保存", GUILayout.Height(40)))
        {
            SaveData();
        }
    }
}
