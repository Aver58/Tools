/**
 * 未读邮件红点判断逻辑-玩家红点;
 * RedDot_EmailUnReadPlayer.cs
 * 
 * Created by Onelei 12/11/2017 10:25:00 AM. All rights reserved.
**/
using RedDot;

public class RedDot_EmailUnReadPlayer : RedDotBase
{
    public override bool ShowRedDot(object[] objs)
    {
        return MailManager.Instance.IsPlayerRedDot();
    }
}
