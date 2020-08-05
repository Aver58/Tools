#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    TextEx.cs
 Author:      Zeng Zhiwei
 Time:        2020/8/4 22:32:07
=====================================================
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public partial class TextEx
{
    #region 静态方法及常量
    const char EMJSPACE = '\u2001';

    static bool IsIn(int codePoint, int min, int max)
    {
        return codePoint >= min && codePoint <= max;
    }

    static bool isEmojiCharacter(int codePoint)
    {
        return (codePoint >= 0x2600 && codePoint <= 0x27BF) // 杂项符号与符号字体
                || codePoint == 0x303D  //中日韩统一符号(CJK)
                || codePoint == 0x2049  //通用符号
                || codePoint == 0x203C

                //添加
                || codePoint == 0x00A9 || codePoint == 0x00AE   //拉丁符号
                || IsIn(codePoint, 0x2194, 0x2199)              //箭头
                || IsIn(codePoint, 0x21A9, 0x21AA)              //箭头
                || IsIn(codePoint, 0x25AA, 0x25AB)              //几何图形
                || IsIn(codePoint, 0x25FB, 0x25FE)              //几何图形
                || codePoint == 0x25B6 || codePoint == 0x25C0   //几何图形
                || codePoint == 0x3030   //中日韩统一符号(CJK)
                                         //End

                || IsIn(codePoint, 0x2000, 0x200F)
                || IsIn(codePoint, 0x2028, 0x202F)
                || codePoint == 0x205F
                || IsIn(codePoint, 0x2065, 0x206F)
                || IsIn(codePoint, 0x2100, 0x214F)              // 字母符号
                || IsIn(codePoint, 0x2300, 0x23FF)              //各种技术符号
                || IsIn(codePoint, 0x2B00, 0x2BFF)              // 箭头A
                || IsIn(codePoint, 0x2900, 0x297F)              // 箭头B
                || IsIn(codePoint, 0x3200, 0x32FF)              // 中文符号
                || IsIn(codePoint, 0xD800, 0xDFFF)              // 高低位替代符保留区域
                || IsIn(codePoint, 0xE000, 0xF8FF)              // 私有保留区域
                || IsIn(codePoint, 0xFE00, 0xFE0F)              // 变异选择器
                || codePoint >= 0x10000; // Plane在第二平面以上的，char都不可以存，全部都转
                                         //|| codePoint == 0x3297 || codePoint == 0x3299   //中日韩统一字母(CJK)
    }
    #endregion

    #region 初始化数据
    static Dictionary<string, string> EmjRectDic = new Dictionary<string, string>();

    public static void AddEmoji(string emj)
    {
        EmjRectDic[GetConvertedString(emj)] = emj;
    }
    public static void ClearEmojiDic()
    {
        EmjRectDic.Clear();
    }

    static string GetConvertedString(string inputString)
    {
        string[] converted = inputString.Split('-');
        for(int j = 0; j < converted.Length; j++)
        {
            converted[j] = char.ConvertFromUtf32(Convert.ToInt32(converted[j], 16));
        }
        return string.Join(string.Empty, converted);
    }
    #endregion

    struct EmojiData
    {
        public int pos;
        public string emj;

        public EmojiData(int p, string s)
        {
            pos = p;
            emj = s;
        }
    }
    
    List<EmojiData> m_emjDataLst = new List<EmojiData>();
    // 测试emoji：😀、😁
    string EmojiParse(string inputStr)
    {
        m_emjDataLst.Clear();
        bool isEmoji = false;
        for(int x = 0; x < inputStr.Length; x++)
        {
            if(isEmojiCharacter(inputStr[x]))
            {
                isEmoji = true;
                break;
            }
        }

        if(!isEmoji)
            return inputStr;

        StringBuilder sb = new StringBuilder();
        int i = 0;
        while(i < inputStr.Length)
        {
            string singleChar = inputStr.Substring(i, 1);
            string doubleChar = inputStr.Length - 1 <= i ? string.Empty : inputStr.Substring(i, 2);
            string fourChar = inputStr.Length - 3 <= i ? string.Empty : inputStr.Substring(i, 4);

            if(EmjRectDic.ContainsKey(fourChar))
            {
                sb.Append(EMJSPACE);
                m_emjDataLst.Add(new EmojiData(sb.Length - 1, EmjRectDic[fourChar]));
                //RefreshHyperlinkIndex(i, 3);
                i += 4;
            }
            else if(EmjRectDic.ContainsKey(doubleChar))
            {
                sb.Append(EMJSPACE);
                m_emjDataLst.Add(new EmojiData(sb.Length - 1, EmjRectDic[doubleChar]));
                //RefreshHyperlinkIndex(i, 1);
                i += 2;
            }
            else if(EmjRectDic.ContainsKey(singleChar))
            {
                sb.Append(EMJSPACE);
                m_emjDataLst.Add(new EmojiData(sb.Length - 1, EmjRectDic[singleChar]));
                i++;
            }
            else
            {
                if(!isEmojiCharacter(inputStr[i]))
                {
                    sb.Append(inputStr[i]);
                }
                i++;
            }
        }
        if(m_emjDataLst.Count > 0)
        {
            CreateEmoji();
        }
        return sb.ToString();
    }

    #region image管理
    List<ImageEx> m_emjImgLst = new List<ImageEx>();

    void ClearEmoji()
    {
        for(int i = 0; i < m_emjImgLst.Count; i++)
        {
            m_emjImgLst[i].gameObject.SetActive(false);
        }
    }

    void CreateEmoji()
    {
        int count = m_emjDataLst.Count;
        int min = Mathf.Min(count, m_emjImgLst.Count);
        for(int i = 0; i < min; i++)
        {
            m_emjImgLst[i].gameObject.SetActive(true);
            m_emjImgLst[i].SetEmojiName(m_emjDataLst[i].emj);
        }
        while(m_emjImgLst.Count < count)
        {
            GameObject obj = new GameObject("emoji");
            obj.transform.SetParent(transform);
            obj.transform.localScale = Vector3.one;
            ImageEx img = obj.AddComponent<ImageEx>();
            img.raycastTarget = false;
            img.rectTransform.anchorMin = Vector2.up;
            img.rectTransform.anchorMax = Vector2.up;
            img.rectTransform.pivot = Vector2.up * 0.16f;
            img.rectTransform.sizeDelta = Vector2.one * fontSize;
            img.SetEmojiName(m_emjDataLst[m_emjImgLst.Count].emj);
            m_emjImgLst.Add(img);
            obj.SetActive(true);
            obj.hideFlags = HideFlags.DontSaveInEditor;
        }
    }

    void DrawEmoji(VertexHelper toFill, bool isEllipsis = false, int ellipsisIndex = 0)
    {
        int count = m_emjDataLst.Count;
        if(count <= 0)
            return;

        UIVertex vert = new UIVertex();
        UIVertex vert2 = new UIVertex();
        for(int i = 0; i < count; i++)
        {
            if(m_emjImgLst.Count <= i)
                break;
            int emojiIndex = m_emjDataLst[i].pos;
            int vertsIndex = emojiIndex * 4 - 1;
            if(vertsIndex >= toFill.currentVertCount - 4)// || (isEllipsis && ellipsisIndex == emojiIndex)
            {
                m_emjImgLst[i].rectTransform.position = Vector3.up * 100000;
            }
            else
            {
                toFill.PopulateUIVertex(ref vert, vertsIndex + 1);
                toFill.PopulateUIVertex(ref vert2, vertsIndex + 2);
                m_emjImgLst[i].rectTransform.localPosition = (vert.position + vert2.position) * 0.5f;
            }
        }
    }
    #endregion
}