/**
 * 红点系统管理类;
 * RedDotManager.cs
 * 
 * Created by Onelei 12/11/2017 10:21:47 AM. All rights reserved.
**/

using System.Collections.Generic;

namespace RedDot
{
    public class RedDotManager
    {
        private static RedDotManager _instance;
        public static RedDotManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new RedDotManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 红点数据;
        /// </summary>
        Dictionary<RedDotType, RedDotBase> RedDotConditionDict = new Dictionary<RedDotType, RedDotBase>();
        /// <summary>
        /// 红点物体;
        /// </summary>
        Dictionary<RedDotType, List<RedDotItem>> RedDotObjDict = new Dictionary<RedDotType, List<RedDotItem>>();
        /// <summary>
        /// 初始化红点系统(注意只需要初始化一次);
        /// </summary>
        public void Initilize()
        {
           RedDotConditionDict.Clear();

            // 添加红点数据判断;
            RedDotConditionDict.Add(RedDotType.Email_UnReadSystem, new RedDot_EmailUnReadSystem());
            RedDotConditionDict.Add(RedDotType.Email_UnReadPlayer, new RedDot_EmailUnReadPlayer());

        }
         
        /// <summary>
        /// 注册红点;
        /// </summary>
        /// <param name="redDotType"></param>
        /// <param name="item"></param>
        public void RegisterRedDot(List<RedDotType> redDotTypes, RedDotItem item)
        {
            for (int i = 0; i < redDotTypes.Count; i++)
            {
                RegisterRedDot(redDotTypes[i], item);
            }
        }
        /// <summary>
        /// 取消注册红点;
        /// </summary>
        /// <param name="item"></param>
        public void UnRegisterRedDot(RedDotItem item)
        {
            Dictionary<RedDotType, List<RedDotItem>>.Enumerator itor = RedDotObjDict.GetEnumerator();
            while (itor.MoveNext())
            {
                List<RedDotItem> redDotItems = itor.Current.Value;
                if (redDotItems.Contains(item))
                {
                    redDotItems.Remove(item);
                    break;
                }
            }
        }

        /// <summary>
        /// 检查红点提示;
        /// </summary>
        /// <param name="redDotType"></param>
        /// <returns></returns>
        public bool Check(List<RedDotType> redDotTypes, object[] objs = null)
        {
            for (int i = 0; i < redDotTypes.Count; i++)
            {
                //只要有一个需要点亮,就显示;
                if (Check(redDotTypes[i], objs))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 更新该类型的所有红点组件;
        /// </summary>
        /// <param name="redDotType"></param>
        /// <returns></returns>
        public void NotifyAll(RedDotType redDotType, object[] objs = null)
        {
            if (RedDotObjDict.ContainsKey(redDotType))
            {
                for (int i = 0; i < RedDotObjDict[redDotType].Count; i++)
                {
                    RedDotItem item = RedDotObjDict[redDotType][i];
                    if (item != null)
                    {
                        List<RedDotType> redDotTypes = item.GetRedDotTypes();
                        bool bCheck = Check(redDotTypes, objs);
                        item.SetData(bCheck);
                    }
                }
            }
        }
        #region private
        /// <summary>
        /// 添加红点界面;
        /// </summary>
        /// <param name="redDotType"></param>
        /// <param name="item"></param>
        private void RegisterRedDot(RedDotType redDotType, RedDotItem item)
        {
            if (RedDotObjDict.ContainsKey(redDotType))
            {
                RedDotObjDict[redDotType].Add(item);
            }
            else
            {
                List<RedDotItem> items = new List<RedDotItem>();
                items.Add(item);
                RedDotObjDict.Add(redDotType, items);
            }
        }
        /// <summary>
        /// 检查红点提示,内部调用;
        /// </summary>
        /// <param name="redDotType"></param>
        /// <returns></returns>
        private bool Check(RedDotType redDotType, object[] objs)
        {
            if (RedDotConditionDict.ContainsKey(redDotType))
            {
                return RedDotConditionDict[redDotType].ShowRedDot(objs);
            }
            return false;
        } 
        #endregion
    }
}
