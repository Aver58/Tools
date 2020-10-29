/**
 * 邮件系统管理类;
 * MailManager.cs
 * 
 * Created by Onelei 12/11/2017 11:25:28 AM. All rights reserved.
**/
using UnityEngine;
using UnityEngine.UI;

namespace RedDot
{
    public class MailManager 
    {
        private static MailManager _instance;
        public static MailManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new MailManager();
                }
                return _instance;
            }
        }

        #region 系统红点; 
        /// <summary>
        /// 是否显示红点;
        /// </summary>
        private bool bSystemRedDot = true;

        /// <summary>
        /// 返回是否显示红点;
        /// </summary>
        /// <returns></returns>
        public bool IsSystemRedDot()
        {
            return bSystemRedDot;
        }

        /// <summary>
        /// 设置红点;
        /// </summary>
        /// <param name="bRedDot"></param>
        public void SetSystemRedDot(bool bRedDot)
        {
            this.bSystemRedDot = bRedDot;
        }
        #endregion


        #region 玩家红点;
        /// <summary>
        /// 是否显示玩家红点;
        /// </summary>
        private bool bPlayerRedDot = true;

        /// <summary>
        /// 返回是否显示玩家红点;
        /// </summary>
        /// <returns></returns>
        public bool IsPlayerRedDot()
        {
            return bPlayerRedDot;
        }

        /// <summary>
        /// 设置玩家红点;
        /// </summary>
        /// <param name="bRedDot"></param>
        public void SetPlayerRedDot(bool bRedDot)
        {
            this.bPlayerRedDot = bRedDot;
        }
        #endregion
    }
}
