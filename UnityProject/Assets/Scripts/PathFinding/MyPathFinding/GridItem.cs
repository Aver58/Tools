#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    GridItem.cs
 Author:      Zeng Zhiwei
 Time:        2021\1\17 星期日 14:43:28
=====================================================
*/
#endregion

using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public enum GridType
    {
        Normal,
        Start,
        End,
        Obstacle,
        Path,
        Frontier,
        Visited
    }

    /// <summary>
    /// 一个格子实例
    /// </summary>
    public class GridItem
    {
        // todo heap优化
        //数据
        public int gridX;
        public int gridY;
        public bool walkable { get { return gridType != GridType.Obstacle; } }
        public GridType gridType;
        //格子A*三属性f-g-h
        public int fCost{ get { return gCost + hCost; } }
        public int hCost;
        public int gCost;
        public int weight { get { return (gridType == GridType.Obstacle) ? 50 : 1; } }
        public GridItem parent;

        // view
        private Image _image;
        private Text _textComponent;
        private GameObject _gameObject;

        public GridItem(int row, int col, GridType gridType)
        {
            this.gridX = row;
            this.gridY = col;
            this.gridType = gridType;
        }

        public void InitGameObject(Transform parent, GameObject prefab, int factor)
        {
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.name = $"Grid({gridX}, {gridY})";
            _gameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(factor,factor);
            var transform = _gameObject.transform;
            transform.SetParent(parent);
            transform.localPosition = new Vector3(gridY * factor, gridX * factor, 0.0f);
            transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);// 做条痕出来
            _image = _gameObject.GetComponent<Image>();
            _textComponent = _gameObject.GetComponentInChildren<Text>();
        }

        public void SetColor(Color color)
        {
            _image.color = color;
        }

        public void SetText(string text)
        {
            _textComponent.text = text;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", gridX, gridY);
        }
    }
}
