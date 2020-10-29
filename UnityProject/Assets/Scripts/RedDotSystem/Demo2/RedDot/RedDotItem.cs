/**
 * 红点物体;
 * RedDotItem.cs
 * 
 * Created by Onelei 12/11/2017 10:21:47 AM. All rights reserved.
**/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace RedDot
{
    public class RedDotItem : MonoBehaviour
    {  
        /// <summary>
        /// 红点物体;
        /// </summary>
        public GameObject Go_RedDot;
        /// <summary>
        /// 红点类型;
        /// </summary>
        public List<RedDotType> redDotTypes;

        private bool bCachedRedDot = false;

        void Start()
        {
            //注册红点;
            RedDotManager.Instance.RegisterRedDot(redDotTypes, this);
            //设置红点;
            bool bRedDot = RedDotManager.Instance.Check(redDotTypes);
            SetData(bRedDot, true);
        }

        void OnDestroy()
        {
            //取消注册红点;
            RedDotManager.Instance.UnRegisterRedDot(this);
        }

        /// <summary>
        /// 设置红点显示;
        /// </summary>
        /// <param name="bRedDot"></param>
        public void SetData(bool bRedDot, bool bForceRefresh = false)
        {
            if (bForceRefresh)
            {
                Go_RedDot.SetActive(bRedDot);
                bCachedRedDot = bRedDot;
                return;
            }

            if (bCachedRedDot != bRedDot)
            {
                Go_RedDot.SetActive(bRedDot);
                bCachedRedDot = bRedDot;
            }
        }
        /// <summary>
        /// 获取当前物体挂载的所有红点;
        /// </summary>
        /// <returns></returns>
        public List<RedDotType> GetRedDotTypes()
        {
            return this.redDotTypes;
        }

    }
}
