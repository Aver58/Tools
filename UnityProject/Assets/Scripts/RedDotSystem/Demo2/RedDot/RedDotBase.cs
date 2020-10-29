/**
 * 红点数据类-基类;
 * RedDotBase.cs
 * 
 * Created by Onelei 12/11/2017 10:23:47 AM. All rights reserved.
**/

namespace RedDot
{
    public abstract class RedDotBase
    {
        /// <summary>
        /// 是否显示红点(true表示显示,false表示不显示;)
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public virtual bool ShowRedDot(object[] objs)
        {
            return false;
        }
    }
}
