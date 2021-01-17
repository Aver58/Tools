using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// 红点树视图
/// </summary>
public class ReddotTreeView : TreeView
{
    private ReddotTreeViewItem m_Root;

    private int m_Id;

    public ReddotTreeView(TreeViewState state) : base(state)
    {
        Reload();

        useScrollView = true;

        ReddotMananger.Instance.NodeNumChangeCallback += Reload;
        ReddotMananger.Instance.NodeValueChangeCallback += Repaint;
    }

    protected override TreeViewItem BuildRoot()
    {
        m_Id = 0;

        m_Root = PreOrder(ReddotMananger.Instance.Root);
        m_Root.depth = -1;

        SetupDepthsFromParentsAndChildren(m_Root);

        return m_Root;
    }

    private ReddotTreeViewItem PreOrder(TreeNode root)
    {
        if (root == null)
        {
            return null;
        }

        ReddotTreeViewItem item = new ReddotTreeViewItem(m_Id++, root);

        if (root.ChildrenCount > 0)
        {
            foreach (TreeNode child in root.Children)
            {
                item.AddChild(PreOrder(child));
            }
        }

        return item;
    }

    private void Repaint(TreeNode node, int value)
    {
        Repaint();
    }

    public void OnDestory()
    {
        ReddotMananger.Instance.NodeNumChangeCallback -= Reload;
        ReddotMananger.Instance.NodeValueChangeCallback -= Repaint;
    }


}
