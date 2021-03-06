﻿#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    PathFindManager.cs
 Author:      Zeng Zhiwei
 Time:        2021\1\17 星期日 14:44:32
=====================================================
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public enum CommandType
    {
        None,
        SetStart,
        SetEnd,
        SetExpensive,
        SetNormal,
    }

    public class PathFindManager : MonoBehaviour
    {
        public int Rows = 15;
        public int Cols = 15;
        private int itemSize = 40;
        public GameObject GridPrefab;

        public GridItem[] GridItems { get; private set; }

        private GridItem startItem;
        private GridItem endItem;
        private Color DefaultColor = new Color(0.86f, 0.83f, 0.83f);
        private Color ExpensiveColor = new Color(0.19f, 0.65f, 0.43f);
        //private Color InfinityColor = new Color(0.37f, 0.37f, 0.37f);
        private Color StartColor = Color.green;
        private Color EndColor = Color.red;
        private Color PathColor = new Color(0.73f, 0.0f, 1.0f);
        private Color VisitedColor = new Color(0.75f, 0.55f, 0.38f);
        private Color FrontierColor = new Color(0.4f, 0.53f, 0.8f);
        private CommandType curCommand = CommandType.None;
        private IEnumerator _pathRoutine;
        private List<GridItem> neighbors;
        private List<GridItem> tempPath;

        private int ExpensiveWeight = 50;

        void Awake()
        {
            neighbors = new List<GridItem>();
            GridItems = new GridItem[Rows * Cols];
            var parent = transform.Find("Grids");
            for(int row = 0; row < Rows; row++)
            {
                for(int col = 0; col < Cols; col++)
                {
                    GridItem item = new GridItem(row, col, GridType.Normal);
                    item.InitGameObject(parent, GridPrefab, itemSize);

                    int index = GetTileIndex(row, col);
                    GridItems[index] = item;
                }
            }

            ClearGrids();
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
                ClearGrids();

            if(Input.GetMouseButtonDown(0))
            {
                int row = (int)(Input.mousePosition.y / itemSize);
                int col = (int)(Input.mousePosition.x / itemSize);
                if(IsValid(row, col))
                {
                    switch(curCommand)
                    {
                        case CommandType.None:
                            break;
                        case CommandType.SetStart:
                            SetStart(row, col);
                            break;
                        case CommandType.SetEnd:
                            SetEnd(row, col);
                            break;
                        case CommandType.SetExpensive:
                            SetExpensive(row, col);
                            break;
                        case CommandType.SetNormal:
                            SetNormal(row, col);
                            break;
                        default:
                            break;
                    }
                }
            }

            // 开始算法
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                StopPathCoroutine();
                _pathRoutine = FindPath(startItem, endItem, PathFinder.FindPath_BFS);
                StartCoroutine(_pathRoutine);
            }
            if(Input.GetKeyDown(KeyCode.Alpha2))
            {
                StopPathCoroutine();
                _pathRoutine = FindPath(startItem, endItem, PathFinder.FindPath_Dijkstra);
                StartCoroutine(_pathRoutine);
            }
            if(Input.GetKeyDown(KeyCode.Alpha3))
            {
                StopPathCoroutine();
                _pathRoutine = FindPath(startItem, endItem, PathFinder.FindPath_GreedyBestFirstSearch);
                StartCoroutine(_pathRoutine);
            }
            if(Input.GetKeyDown(KeyCode.Alpha4))
            {
                StopPathCoroutine();
                _pathRoutine = FindPath(startItem, endItem, PathFinder.FindPath_AStar);
                StartCoroutine(_pathRoutine);
            }
        }

        void OnGUI()
        {
            //在一帧中会调用多次
            if(GUILayout.Button("设置起点"))
                curCommand = CommandType.SetStart;
            if(GUILayout.Button("设置终点"))
                curCommand = CommandType.SetEnd;
            if(GUILayout.Button("设置障碍"))
                curCommand = CommandType.SetExpensive;
            if(GUILayout.Button("设置普通"))
                curCommand = CommandType.SetNormal;

            //if(GUILayout.Button("开始寻路"))
            //    StartPathFinding();

            //if(GUILayout.Button("下一步"))
            //    StartPathFindingOneStep();

            //if(GUILayout.Button("上一步"))
            //  StartPathFinding();
            //OnGUI
        }

        private int GetTileIndex(int row, int col)
        {
            //Debug.LogFormat("row:{0} col:{1} index:{2}", row,col, row * Cols + col);
            return row * Cols + col;
        }

        public bool IsValid(int x,int y)
        {
            if(x >= 0 && x < Cols && y >= 0 && y < Rows)
            {
                return true;
            }
            return false;
        }

        private List<int[]> neighborMap = new List<int[]>
        {
            {new int[2]{-1, 0}},
            {new int[2]{ 1, 0}},
            {new int[2]{ 0,-1}},
            {new int[2]{ 0, 1 }},
        };

        public IEnumerable<GridItem> GetNeighbors(GridItem tile)
        {
            neighbors.Clear();
            // 九宫格
            //for(int x = -1; x <= 1; x++)
            //{
            //    for(int y = -1; y <= 1; y++)
            //    {
            //        // 排除自己
            //        if(x == 0 && y == 0)
            //            continue;

            //        int gridX = source.gridX + x;
            //        int gridY = source.gridY + y;
            //        if(IsValid(gridX, gridY))
            //        {
            //            var item = GetGridItem(gridX, gridY);
            //            neighbors.Add(item);
            //        }
            //    }
            //}
            // 四格
            //for(int i = 0; i < neighborMap.Count; i++)
            //{
            //    var array = neighborMap[i];
            //    int gridX = tile.gridX + array[0];
            //    int gridY = tile.gridY + array[1];
            //    if(IsValid(gridX, gridY))
            //    {
            //        var item = GetGridItem(gridX, gridY);
            //        neighbors.Add(item);
            //    }
            //}

            //return neighbors;

            var right = GetGridItem(tile.gridX, tile.gridY + 1);
            if(right != null)
            {
                yield return right;
            }

            var up = GetGridItem(tile.gridX - 1, tile.gridY);
            if(up != null)
            {
                yield return up;
            }

            var left = GetGridItem(tile.gridX, tile.gridY - 1);
            if(left != null)
            {
                yield return left;
            }

            var down = GetGridItem(tile.gridX + 1, tile.gridY);
            if(down != null)
            {
                yield return down;
            }
        }

        private GridItem GetGridItem(int row, int col)
        {
            if(!IsValid(row, col))
                return null;

            int index = GetTileIndex(row, col);
            var item = GridItems[index];
            if(item != null)
                return item;

            return null;
        }

        private IEnumerator FindPath(GridItem start, GridItem end, Func<PathFindManager, GridItem, GridItem, List<IVisualStep>> pathFindingFunc)
        {
            ResetGrids();

            List<IVisualStep> steps = pathFindingFunc(this, start, end);
            if(steps != null)
            {
                foreach(var step in steps)
                {
                    step.Execute();
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        private void SetStart(int row,int col)
        {
            if(IsValid(row, col))
            {
                if(startItem != null)
                {
                    startItem.gridType = GridType.Normal;
                    ResetGrid(startItem);
                }

                var item = GetGridItem(row, col);
                item.gridType = GridType.Start;
                startItem = item;
                ResetGrid(item);
            }
        }

        private void SetEnd(int row, int col)
        {
            if(IsValid(row, col))
            {
                if(endItem != null)
                {
                    endItem.gridType = GridType.Normal;
                    ResetGrid(endItem);
                }

                var item = GetGridItem(row, col);
                item.gridType = GridType.End;
                endItem = item;
                ResetGrid(item);
            }
        }

        private void SetNormal(int row, int col)
        {
            if(IsValid(row, col))
            {
                var item = GetGridItem(row, col);
                item.gridType = GridType.Normal;
                ResetGrid(item);
            }
        }

        private void SetExpensive(int row, int col)
        {
            if(IsValid(row, col))
            {
                var item = GetGridItem(row, col);
                item.gridType = GridType.Obstacle;
                item.gCost = ExpensiveWeight;
                ResetGrid(item);
            }
        }

        private void CreateExpensiveArea(int row, int col, int width, int height)
        {
            for(int r = row; r < row + height; r++)
            {
                for(int c = col; c < col + width; c++)
                {
                    SetExpensive(r, c);
                }
            }
        }

        private void StopPathCoroutine()
        {
            if(_pathRoutine != null)
            {
                StopCoroutine(_pathRoutine);
                _pathRoutine = null;
            }
        }

        private void StartPathFinding()
        {
            ResetGrids();
            //tempPath = PathFinder.FindPath_AStar(this, startItem, endItem);
        }

        private void StartPathFindingOneStep()
        {
            if(tempPath != null)
            {
                //tempPath
            }
        }

        private void ClearGrids()
        {
            foreach(GridItem gridItem in GridItems)
            {
                gridItem.SetText("");
                gridItem.gridType = GridType.Normal;
            }

            SetStart(2, 0);
            SetEnd(12, 14);

            CreateExpensiveArea(2, 2, 10, 1);
            CreateExpensiveArea(2, 12, 1, 10);
            CreateExpensiveArea(12, 2, 11, 1);

            foreach(GridItem gridItem in GridItems)
            {
                ResetGrid(gridItem);
            }
        }

        private void ResetGrids()
        {
            foreach(GridItem gridItem in GridItems)
            {
                ResetGrid(gridItem);
            }
        }

        public void ResetGrid(GridItem gridItem)
        {
            gridItem.gCost = 0;
            //gridItem.SetText("");

            switch(gridItem.gridType)
            {
                case GridType.Normal:
                    gridItem.SetColor(DefaultColor);
                    break;
                case GridType.Obstacle:
                    gridItem.SetColor(ExpensiveColor);
                    break;
                case GridType.Start:
                    gridItem.SetColor(StartColor);
                    break;
                case GridType.End:
                    gridItem.SetColor(EndColor);
                    break;
                case GridType.Path:
                    gridItem.SetColor(PathColor);
                    break;
                case GridType.Frontier:
                    gridItem.SetColor(FrontierColor);
                    break;
                case GridType.Visited:
                    gridItem.SetColor(VisitedColor);
                    break;
                default:
                    break;
            }
        }
    }
}
