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
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MyNamespace
{
    public class Graph<Location>
    {
        // NameValueCollection would be a reasonable alternative here, if
        // you're always using string location types
        public Dictionary<Location, Location[]> edges = new Dictionary<Location, Location[]>();

        public Location[] Neighbors(Location id)
        {
            return edges[id];
        }
    };

    public class PathFinder
    {
        private static List<GridItem> openSet;
        private static HashSet<GridItem> closeSet;
        static Stopwatch sw = new Stopwatch();

        static void Search(Graph<string> graph, string start, string end)
        {
            var frontier = new Queue<string>();
            //首先将根节点放入队列中。
            frontier.Enqueue(start);
            var reached = new HashSet<string>();
            reached.Add(start);

            while(frontier.Count > 0)
            {
                //从队列中取出第一个节点，并检验它是否为目标。
                var current = frontier.Dequeue();
                //如果找到目标，则结束搜寻并回传结果。
                if(current == end)
                {
                    Debug.Log("BFS Find!");
                    break;
                }
                //否则将它所有尚未检验过的直接子节点（邻节点）加入队列中。
                foreach(var next in graph.Neighbors(current))
                {
                    if(!reached.Contains(next))
                    {
                        frontier.Enqueue(next);
                        reached.Add(next);
                    }
                }
            }
            //若队列为空，表示整张图都检查过了——亦即图中没有欲搜寻的目标。结束搜寻并回传“找不到目标”。
            Debug.Log("BFS Can't Find!");
        }

        static void Main()
        {
            Graph<string> g = new Graph<string>();
            g.edges = new Dictionary<string, string[]>
            {
                { "A", new [] { "B" } },
                { "B", new [] { "A", "C", "D" } },
                { "C", new [] { "A" } },
                { "D", new [] { "E", "A" } },
                { "E", new [] { "B" } }
            };

            Search(g, "A","B");
        }

        #region Episode 01 - pseudocode 伪代码
        /*
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
              // 如果到这个邻居的路径更短或者这个邻居不在开发列表
              if new path to neighbour is shorter OR neighbour is not in OPEN
                      set f_cost of neighbour
                      set parent of neighbour to current
                      if neighbour is not in OPEN
                         add neighbour to OPEN
        */
        #endregion
        /// <summary>
        /// 输入起始点和结束点，输出路径列表
        /// A星算法核心公式就是F值的计算：
        /// F = G + H
        /// F - 方块的总移动代价
        /// G - 开始点到当前方块的移动代价
        /// H - 当前方块到结束点的预估移动代价
        /// </summary>
        public static List<GridItem> FindPath_AStar(PathFindManager mgr, GridItem start, GridItem end)
        {
            sw.Start();

            bool pathSuccess = false;
            openSet = new List<GridItem>();
            closeSet = new HashSet<GridItem>();

            openSet.Add(start);

            while(openSet.Count > 0)
            {
                var current = openSet[0];
                //找出开放列表中F代价最小的节点 todo 优化==》最优队列、或者最小堆
                foreach(var item in openSet)
                {
                    if(item.fCost < current.fCost)
                    {
                        current = item;
                    }
                }

                openSet.Remove(current);
                closeSet.Add(current);

                if(current == end)
                {
                    sw.Stop();
                    Debug.Log("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                var neighbors = mgr.GetNeighbors(current);
                foreach(var neighbor in neighbors)
                {
                    // 剔除封闭列表元素
                    if(closeSet.Contains(neighbor) || !neighbor.walkable)
                        continue;

                    // 逐个计算邻居的gCost
                    int newCostToNeighbour = current.gCost + GetDistance(current, neighbor);
                    if(newCostToNeighbour < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCostToNeighbour;
                        neighbor.hCost = GetDistance(end, neighbor);
                        neighbor.parent = current;//做成一个链表，最后用来做结果
                        neighbor.SetText(string.Format("f:{0}\ng:{1}\nh:{2}", neighbor.fCost, neighbor.gCost,neighbor.hCost));
                        if(openSet.Contains(neighbor))
                        {
                            //openSet
                        }
                        else
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            if(pathSuccess)
            {
                var results = RetracePath(start, end);
                pathSuccess = results.Count > 0;
                return results;
            }
            return null;
        }

        private static List<GridItem> RetracePath(GridItem start, GridItem end)
        {
            List<GridItem> path = new List<GridItem>();
            GridItem current = end.parent;

            while(current != start)
            {
                path.Add(current);
                current = current.parent;
            }

            path.Reverse();
            return path;
        }

        private static int GetDistance(GridItem itemA, GridItem itemB)
        {
            int dstX = Mathf.Abs(itemB.gridX - itemA.gridX);
            int dstY = Mathf.Abs(itemB.gridY - itemA.gridY);
            //return D * (deltaX + deltaY); D是一个系数
            // 一、对角线距离 有下面这3种表示方法
            //①return D * (dx + dy) + (D2 - 2 * D) * min(dx, dy)
            //②return D * max(dx, dy) +(D2 - D) * min(dx, dy)
            //③Patrick Lester if(dx > dy) (D * (dx - dy) + D2 * dy) else (D * (dy - dx) + D2 * dx)
            // 二、 曼哈顿距离
            if(dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}