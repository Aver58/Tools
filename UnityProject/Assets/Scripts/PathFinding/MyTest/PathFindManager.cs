#region Copyright © 2020 Aver. All rights reserved.
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
    }

    public class PathFindManager : MonoBehaviour
    {
        public int Rows;
        public int Cols;
        public int itemSize = 20;
        public GameObject GridPrefab;

        public GridItem[] GridItems { get; private set; }

        private GridItem startItem;
        private GridItem endItem;
        private const int DefaultWeight = 1;
        private const int ExpensiveWeight = 50;
        private const int InfinityWeight = int.MaxValue;
        private Color DefaultColor = new Color(0.86f, 0.83f, 0.83f);
        private Color ExpensiveColor = new Color(0.19f, 0.65f, 0.43f);
        private Color InfinityColor = new Color(0.37f, 0.37f, 0.37f);
        private Color StartColor = Color.green;
        private Color EndColor = Color.red;
        private Color PathColor = new Color(0.73f, 0.0f, 1.0f);
        private Color VisitedColor = new Color(0.75f, 0.55f, 0.38f);
        private Color FrontierColor = new Color(0.4f, 0.53f, 0.8f);
        private CommandType curCommand = CommandType.None;
        private IEnumerator _pathRoutine;

        void Awake()
        {
            GridItems = new GridItem[Rows * Cols];
            var parent = transform.Find("Grids");
            for(int r = 0; r < Rows; r++)
            {
                for(int c = 0; c < Cols; c++)
                {
                    GridItem item = new GridItem(r, c, DefaultWeight);
                    item.InitGameObject(parent, GridPrefab,itemSize);

                    int index = GetTileIndex(r, c);
                    GridItems[index] = item;
                }
            }

            SetStart(0, 0);
            SetEnd(10, 10);
            SetExpensive(1,1);

            ResetGrids();
        }

        void OnGUI()
        {
            if(GUILayout.Button("设置起点"))
                curCommand = CommandType.SetStart;
            if(GUILayout.Button("设置终点"))
                curCommand = CommandType.SetEnd;
            if(GUILayout.Button("设置障碍"))
                curCommand = CommandType.SetExpensive;

            if(GUILayout.Button("开始寻路"))
                StartPathFinding();

            if(Input.GetMouseButtonDown(0))
            {
                Debug.Log(Input.mousePosition);
                int row = (int)(Input.mousePosition.x / itemSize);
                int col = (int)(Input.mousePosition.y / itemSize);

                switch(curCommand)
                {
                    case CommandType.None:
                        break;
                    case CommandType.SetStart:
                        SetStart(row,col);
                        break;
                    case CommandType.SetEnd:
                        SetEnd(row, col);
                        break;
                    case CommandType.SetExpensive:
                        SetExpensive(row, col);
                        break;
                    default:
                        break;
                }
            }

            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                StopPathCoroutine();
                _pathRoutine = FindPath(startItem, endItem, PathFinder.FindPath_AStar);
                StartCoroutine(_pathRoutine);
            }
        }

        private int GetTileIndex(int row, int col)
        {
            return row * Cols + col;
        }

        private IEnumerator FindPath(GridItem start, GridItem end, Func<PathFindManager, GridItem, GridItem, List<GridItem>> pathFindingFunc)
        {
            ResetGrids();

            pathFindingFunc(this, start, end);

            //foreach(var step in steps)
            //{
            //    step.Execute();
            //    yield return new WaitForFixedUpdate();
            //}
            yield return null;
        }

        void SetStart(int row,int col)
        {
            int index = GetTileIndex(row, col);
            var gridItem = GridItems[index];
            startItem = gridItem;
            ResetGrid(gridItem);
        }

        void SetEnd(int row, int col)
        {
            int index = GetTileIndex(row, col);
            var gridItem = GridItems[index];
            endItem = gridItem;
            ResetGrid(gridItem);
        }

        void SetExpensive(int row, int col)
        {
            int index = GetTileIndex(row, col);
            var gridItem = GridItems[index];
            gridItem.Weight = ExpensiveWeight;
            ResetGrid(gridItem);
        }

        private void StopPathCoroutine()
        {
            if(_pathRoutine != null)
            {
                StopCoroutine(_pathRoutine);
                _pathRoutine = null;
            }
        }

        void StartPathFinding()
        {

        }

        private void ResetGrid(GridItem gridItem)
        {
            gridItem.Cost = 0;
            //gridItem.PrevTile = null;
            gridItem.SetText("");

            switch(gridItem.Weight)
            {
                case DefaultWeight:
                    gridItem.SetColor(DefaultColor);
                    break;
                case ExpensiveWeight:
                    gridItem.SetColor(ExpensiveColor);
                    break;
                case InfinityWeight:
                    gridItem.SetColor(InfinityColor);
                    break;
            }
        }

        private void ResetGrids()
        {
            foreach(GridItem gridItem in GridItems)
            {
                ResetGrid(gridItem);
            }
        }
    }
}
