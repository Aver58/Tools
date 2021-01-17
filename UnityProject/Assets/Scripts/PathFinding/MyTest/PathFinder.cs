#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    PathFinder.cs
 Author:      Zeng Zhiwei
 Time:        2021\1\17 星期日 15:54:03
=====================================================
*/
#endregion

using System.Collections.Generic;

namespace MyNamespace
{
    public class PathFinder
    {
        #region A星教程

        //从开始点，每走一步都选择代价最小的格子走，直到达到结束点。
        //A星算法核心公式就是F值的计算：
        //F = G + H
        //F - 方块的总移动代价
        //G - 开始点到当前方块的移动代价
        //H - 当前方块到结束点的预估移动代价

        /*
         Episode 01 - pseudocode
            OPEN //开放列表
            CLOSED //封闭列表
            把开始节点加入开放列表
             
            loop
                //找出开放接列表F代价最小的节点
                current = node in OPEN with the lowest f_cost 
                remove current from OPEN
                add current to CLOSED
             
                if current is the target node //path has been found
                        return
                
                // 遍历所有邻居
                foreach neighbour of the current node
                   // 如果邻居已经遍历过了，或者邻居已经在封闭列表就跳过
                   if neighbour is not traversable or neighbour is in CLOSED
                           skip to the next neighbour
                   // 如果到这个邻居的新的路径更短或者这个
                   if new path to neighbour is shorter OR neighbour is not in OPEN
                           set f_cost of neighbour
                           set parent of neighbour to current
                           if neighbour is not in OPEN
                              add neighbour to OPEN
             */


        #endregion
        /// <summary>
        /// 输入起始点和结束点，输出路径列表
        /// </summary>
        public static List<GridItem> FindPath_AStar(PathFindManager mgr, GridItem start, GridItem end)
        {
            List<GridItem> result = new List<GridItem>();
            return result;
        }
    }
}