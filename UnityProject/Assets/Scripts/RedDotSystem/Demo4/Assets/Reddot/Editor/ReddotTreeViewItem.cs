
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

/// <summary>
/// 红点树视图条目
/// </summary>
public class ReddotTreeViewItem : TreeViewItem
{
    private TreeNode m_node;

    /// <summary>
    /// 节点路径
    /// </summary>
    public string Path
    {
        get;
        private set;
    }

    /// <summary>
    /// 节点值
    /// </summary>
    public int Value
    {
        get;
        private set;
    }


    public ReddotTreeViewItem(int id, TreeNode node)
    {
        base.id = id;

        m_node = node;
        Path = node.FullPath;
        Value = node.Value;
    }

    public override string displayName
    {
        get
        {
            return $"{m_node.Name} - 节点值: {m_node.Value} - 子节点数: {m_node.ChildrenCount}";
        }


    }


}
