using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedPointNode
{
    public string nodeName;// 节点名称
    public int pointNum = 0;// 红点数量
    public RedPointNode parent = null;// 父节点
    public RedPointSystem.OnPointNumChange numChangeFunc; // 发生变化的回调函数

    // 子节点
    public Dictionary<string, RedPointNode> dicChilds = new Dictionary<string, RedPointNode>();

    /// <summary>
    /// 设置当前节点的红点数量
    /// </summary>
    /// <param name="rpNum"></param>
    public void SetRedPointNum(int rpNum)
    {
        if(dicChilds.Count > 0) // 红点数量只能设置叶子节点
        {
            Debug.LogError("Only Can Set Leaf Node!");
            return;
        }
        pointNum = rpNum;

        NotifyPointNumChange();
        if (parent != null)
        {
            parent.ChangePredPointNum();
        }
    }

    /// <summary>
    /// 计算当前红点数量
    /// </summary>
    public void ChangePredPointNum()
    {
        int num = 0;
        foreach(var node in dicChilds.Values)
        {
            num += node.pointNum;
        }
        if(num != pointNum) // 红点有变化
        {
            pointNum = num;
            NotifyPointNumChange();
        }
    }

    /// <summary>
    /// 通知红点数量变化
    /// </summary>
    public void NotifyPointNumChange()
    {
        if(numChangeFunc != null)
        {
            numChangeFunc.Invoke(this);
        }
    }

}

public class RedPointConst
{
    public const string main = "Main";// 主界面
    public const string mail  =  "Main.Mail";// 主界面邮件按钮
    public const string mailSystem = "Main.Mail.System";// 邮件系统界面
    public const string mailTeam = "Main.Mail.Team";// 邮件队伍界面
    public const string mailAlliance = "Main.Mail.Alliance";// 邮件公会界面

    public const string task = "Main.Task";// 主界面任务按钮

    public const string alliance = "Main.Alliance";// 主界面公会按钮
}


public class RedPointSystem 
{
    public delegate void OnPointNumChange(RedPointNode node);// 红点变化通知
    RedPointNode mRootNode; // 红点树Root节点

    static List<string> lstRedPointTreeList = new List<string> // 初始化红点树
    {
        RedPointConst.main,
        RedPointConst.mail,
        RedPointConst.mailSystem,
        RedPointConst.mailTeam ,
        RedPointConst.mailAlliance ,

        RedPointConst.task,

        RedPointConst.alliance,
    };

    public void InitRedPointTreeNode()
    {
        mRootNode = new RedPointNode();
        mRootNode.nodeName = RedPointConst.main;

        foreach (var s in lstRedPointTreeList)
        {
            var node = mRootNode;
            var treeNodeAy = s.Split('.');
            if(treeNodeAy[0] != mRootNode.nodeName)
            {
                Debug.Log("RedPointTree Root Node Error:" + treeNodeAy[0]);
                continue;
            }
            if(treeNodeAy.Length > 1)
            {
                for(int i = 1; i < treeNodeAy.Length; i++)
                {
                    if(!node.dicChilds.ContainsKey(treeNodeAy[i]))
                    {
                        node.dicChilds.Add(treeNodeAy[i],new RedPointNode());
                    }
                    node.dicChilds[treeNodeAy[i]].nodeName = treeNodeAy[i];
                    node.dicChilds[treeNodeAy[i]].parent = node;

                    node = node.dicChilds[treeNodeAy[i]];
                }
            }
        }
    }

    public void SetRedPointNodeCallBack(string strNode, RedPointSystem.OnPointNumChange callBack)
    {
        var nodeList = strNode.Split('.');// 分析树节点
        if(nodeList.Length == 1)
        {
            if(nodeList[0] != RedPointConst.main)
            {
                Debug.Log("Get Wrong Root Node! current is " + nodeList[0]);
                return;
            }
        }

        var node = mRootNode;
        for (int i =1; i < nodeList.Length; i++ )
        {
            if (!node.dicChilds.ContainsKey(nodeList[i]))
            {
                Debug.Log("Does Not Contains Child Node :" + nodeList[i]);
                return;
            }
            node = node.dicChilds[nodeList[i]];

            if (i == nodeList.Length -1) // 最后一个节点了
            {
                node.numChangeFunc = callBack;
                return;
            }
        }
    }
  
    /// <summary>
    /// 激发数据变化
    /// </summary>
    /// <param name="nodeName"></param>
    /// <param name="rpNum"></param>
    public void SetInvoke(string strNode, int rpNum)
    {
        var nodeList = strNode.Split('.');// 分析树节点
        if (nodeList.Length == 1)
        {
            if (nodeList[0] != RedPointConst.main)
            {
                Debug.Log("Get Wrong Root Node! current is " + nodeList[0]);
                return;
            }
        }

        var node = mRootNode;
        for (int i = 1; i < nodeList.Length; i++)
        {
            if (!node.dicChilds.ContainsKey(nodeList[i]))
            {
                Debug.Log("Does Not Contains Child Node :" + nodeList[i]);
                return;
            }
            node = node.dicChilds[nodeList[i]];

            if (i == nodeList.Length - 1) // 最后一个节点了
            {
                node.SetRedPointNum(rpNum); // 设置节点的红点数量
            }
        }
    }



}
