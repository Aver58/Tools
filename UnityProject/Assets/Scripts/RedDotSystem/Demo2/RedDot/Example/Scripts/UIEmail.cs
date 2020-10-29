using UnityEngine;
using UnityEngine.UI;

public class UIEmail : MonoBehaviour
{
    /// <summary>
    /// 系统邮件按钮;
    /// </summary>
    [SerializeField]
    Button Button_EmailSystem;
    /// <summary>
    /// 玩家邮件按钮;
    /// </summary>
    [SerializeField]
    Button Button_EmailPlayer;

    void Awake()
    {
        // 注意这个函数只需要初始化一次;
        RedDot.RedDotManager.Instance.Initilize();
    }

    // Use this for initialization
    void Start()
    {
        // 设置按钮回调;
        Button_EmailSystem.onClick.AddListener(OnClickEmailSystem);
        Button_EmailPlayer.onClick.AddListener(OnClickEmailPlayer);

    }

    /// <summary>
    /// 系统按钮的回调;
    /// </summary>
    void OnClickEmailSystem()
    {
        // 点击之后表示邮件读取过了,设置邮件为已读状态;
        // (注意这里不能够设置红点的显隐,因为是有数据控制的,所以要控制数据那边的红点逻辑);
        // RedDot.RedDotManager.Instance.SetData(RedDot.RedDotType.Email_UnRead,false);

        RedDot.MailManager.Instance.SetSystemRedDot(false);
        RedDot.RedDotManager.Instance.NotifyAll(RedDot.RedDotType.Email_UnReadSystem);
    }

    /// <summary>
    /// 玩家按钮的回调;
    /// </summary>
    void OnClickEmailPlayer()
    {
        // 点击之后表示邮件读取过了,设置邮件为已读状态;
        // (注意这里不能够设置红点的显隐,因为是有数据控制的,所以要控制数据那边的红点逻辑);
        // RedDot.RedDotManager.Instance.SetData(RedDot.RedDotType.Email_UnRead,false);

        RedDot.MailManager.Instance.SetPlayerRedDot(false);
        RedDot.RedDotManager.Instance.NotifyAll(RedDot.RedDotType.Email_UnReadPlayer);
    }
}
