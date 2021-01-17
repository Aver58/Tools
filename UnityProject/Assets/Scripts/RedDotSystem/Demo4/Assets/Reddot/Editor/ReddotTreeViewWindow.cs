using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

/// <summary>
/// 红点树视图窗口
/// </summary>
public class ReddotTreeViewWindow : EditorWindow
{
    private static ReddotTreeViewWindow s_Window;

    private ReddotTreeView m_TreeView;

    private SearchField m_SearchField;

    [MenuItem("Window/红点树视图窗口")]
    private static void OpenWindow()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("警告", "运行后才能打开红点树视图窗口", "了解");
            return;
        }

        s_Window = GetWindow<ReddotTreeViewWindow>();
        s_Window.titleContent = new GUIContent("红点树视图窗口");
        s_Window.Show();
    }


    private void OnEnable()
    {
        m_TreeView = new ReddotTreeView(new TreeViewState());

        m_SearchField = new SearchField();
        m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
    }

    private void OnPlayModeStateChange(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredEditMode:
                break;
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                break;
            case PlayModeStateChange.ExitingPlayMode:
                s_Window.Close();
                break;
        }
    }

    private void OnDestroy()
    {
        m_TreeView.OnDestory();
        EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
    }

    private void OnGUI()
    {
        UpToolbar();

        TreeView();
       
        BottomToolBar();
    }

    private void UpToolbar()
    {
        m_TreeView.searchString = m_SearchField.OnGUI(new Rect(0, 0, position.width - 40f, 20f), m_TreeView.searchString);
    }

    private void TreeView()
    {
        m_TreeView.OnGUI(new Rect(0, 20f, position.width, position.height - 40f));
    }

    private void BottomToolBar()
    {
        GUILayout.BeginArea(new Rect(20f, position.height - 18f, position.width - 40f, 16f));

        using (new EditorGUILayout.HorizontalScope())
        {

            string style = "miniButton";
            if (GUILayout.Button("展开全部节点", style))
            {
                m_TreeView.ExpandAll();
            }

            if (GUILayout.Button("收起全部节点", style))
            {
                m_TreeView.CollapseAll();
            }

           
        }

        GUILayout.EndArea();
    }
}
