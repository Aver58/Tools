using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BootStart : MonoBehaviour
{
    public Image imgMail;
    public Image imgMailSystem;
    public Image imgMailTeam;

    public Text txtMail;
    public Text txtMailSystem;
    public Text txtMailTeam;

    private void Start()
    {
        RedPointSystem rps = new RedPointSystem();
        rps.InitRedPointTreeNode();

        // 设置 红点节点的处理函数
        rps.SetRedPointNodeCallBack(RedPointConst.mail, MailCallBack);
        rps.SetRedPointNodeCallBack(RedPointConst.mailSystem, MailSystemCallBack);
        rps.SetRedPointNodeCallBack(RedPointConst.mailTeam, MailTeamCallBack);

        // 激发节点变化
        rps.SetInvoke(RedPointConst.mailSystem,3);
        rps.SetInvoke(RedPointConst.mailTeam, 2);
    }

    void MailCallBack(RedPointNode node)
    {
        txtMail.text = node.pointNum.ToString();
        imgMail.gameObject.SetActive(node.pointNum > 0);

        Debug.Log(node.nodeName + " rp Num changed!  num = " + node.pointNum);
    }

    void MailSystemCallBack(RedPointNode node)
    {
        txtMailSystem.text = node.pointNum.ToString();
        imgMailSystem.gameObject.SetActive(node.pointNum > 0);
        Debug.Log(node.nodeName + " rp Num changed! num = " + node.pointNum);
    }

    void MailTeamCallBack(RedPointNode node)
    {
        txtMailTeam.text = node.pointNum.ToString();
        imgMailTeam.gameObject.SetActive(node.pointNum > 0);
        Debug.Log(node.nodeName + " rp Num changed! num = " + node.pointNum);
    }

}
