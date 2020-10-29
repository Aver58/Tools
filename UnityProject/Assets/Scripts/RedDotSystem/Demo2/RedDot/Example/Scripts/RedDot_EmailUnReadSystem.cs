/**
 * 未读邮件红点判断逻辑-系统红点;
 * RedDot_EmailUnRead.cs
 * 
 * Created by Onelei 12/11/2017 10:25:00 AM. All rights reserved.
**/
using RedDot;

public class RedDot_EmailUnReadSystem : RedDotBase
{
    public override bool ShowRedDot(object[] objs)
    {
        return MailManager.Instance.IsSystemRedDot();
    }
}
