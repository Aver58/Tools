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
    /// <summary>
    /// 一个格子实例
    /// </summary>
    public class GridItem : MonoBehaviour
    {
        //数据
        private int Row;
        private int Col;

        //格子A*三属性f-g-h
        public int f;
        public int g;
        public int h;

        public int Weight { get { return Weight; } set { Weight = value; } }
        public int Cost { get { return Cost; } set { Cost = value; } }

        // view
        private GameObject _gameObject;
        private SpriteRenderer _spriteRenderer;
        private Text _textComponent;

        public GridItem(int row, int col,int weight)
        {
            this.Row = row;
            this.Col = col;
            this.Weight = weight;
        }

        public void InitGameObject(Transform parent, GameObject prefab, int factor)
        {
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.name = $"Grid({Row}, {Col})";
            _gameObject.transform.parent = parent;
            _gameObject.transform.localPosition = new Vector3(Col * factor, Row * factor, 0.0f);
            _gameObject.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);// 做条痕出来
            _spriteRenderer = _gameObject.GetComponent<SpriteRenderer>();
            _textComponent = _gameObject.GetComponentInChildren<Text>();
        }

        public void SetColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        public void SetText(string text)
        {
            _textComponent.text = text;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(Col, Row);
        }
    }
}
