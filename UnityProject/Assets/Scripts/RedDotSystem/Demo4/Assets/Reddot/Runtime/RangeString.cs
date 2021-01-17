using System;

/// <summary>
/// 范围字符串
/// 表示在Source字符串中，从StartIndex到EndIndex范围的字符构成的字符串
/// </summary>
public struct RangeString : IEquatable<RangeString>
{
    /// <summary>
    /// 源字符串
    /// </summary>
    private string m_Source;

    /// <summary>
    /// 开始索引
    /// </summary>
    private int m_StartIndex;

    /// <summary>
    /// 结束范围
    /// </summary>
    private int m_EndIndex;

    /// <summary>
    /// 长度
    /// </summary>
    private int m_Length;

    /// <summary>
    /// 源字符串是否为Null或Empty
    /// </summary>
    private bool m_IsSourceNullOrEmpty;

    /// <summary>
    /// 哈希码
    /// </summary>
    private int m_HashCode;



    public RangeString(string source, int startIndex, int endIndex)
    {
        m_Source = source;
        m_StartIndex = startIndex;
        m_EndIndex = endIndex;
        m_Length = endIndex - startIndex + 1;
        m_IsSourceNullOrEmpty = string.IsNullOrEmpty(source);
        m_HashCode = 0;
    }

    public bool Equals(RangeString other)
    {

        bool isOtherNullOrEmpty = string.IsNullOrEmpty(other.m_Source);

        if (m_IsSourceNullOrEmpty && isOtherNullOrEmpty)
        {
            return true;
        }

        if (m_IsSourceNullOrEmpty || isOtherNullOrEmpty)
        {
            return false;
        }

        if (m_Length != other.m_Length)
        {
            return false;
        }

        for (int i = m_StartIndex, j = other.m_StartIndex; i <= m_EndIndex; i++, j++)
        {
            if (m_Source[i] != other.m_Source[j])
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        if (m_HashCode == 0 && !m_IsSourceNullOrEmpty)
        {
            for (int i = m_StartIndex; i <= m_EndIndex; i++)
            {
                m_HashCode = 31 * m_HashCode + m_Source[i];
            }
        }

        return m_HashCode;
    }

    public override string ToString()
    {
        ReddotMananger.Instance.CachedSb.Clear();
        for (int i = m_StartIndex; i <= m_EndIndex; i++)
        {
            ReddotMananger.Instance.CachedSb.Append(m_Source[i]);
        }
        string str = ReddotMananger.Instance.CachedSb.ToString();

        return str;
    }
}
