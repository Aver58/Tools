
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debuger = LuaInterface.Debugger;

/// <summary>
/// 纯自动适配无限循环滚动  
/// 目前仅支持 自上而下/自左向右 方向的循环滚动
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class LoopScrollRect : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ICanvasElement
{
    public bool horizontal;                                       //水平移动
    public bool vertical;                                         //垂直移动
    public RectTransform viewport;                                //显示
    public RectTransform content;                                 //内容

    protected Vector2 m_PointerStartLocalCursor = Vector2.zero;
    protected Vector2 contentStartPosition = Vector2.zero;
    protected Bounds contentBounds;
    protected Bounds m_ViewBounds;
    protected bool m_Dragging;
    protected bool m_IsEnable;

    #region Content与ViewPort便捷属性

    /// <summary>
    /// Content的坐标
    /// </summary>
    protected Vector2 ContentPosition
    {
        get { return content.anchoredPosition; }
        set { content.anchoredPosition = value; }
    }

    /// <summary>
    /// Content高度
    /// </summary>
    protected float ContentHeight
    {
        get { return content.rect.height; }
    }

    /// <summary>
    /// Content宽度
    /// </summary>
    protected float ContentWidth
    {
        get { return content.rect.width; }
    }


    /// <summary>
    /// ViewPort宽度
    /// </summary>
    protected float ViewWidth
    {
        get { return viewport.rect.width; }
    }

    /// <summary>
    /// ViewPort高度
    /// </summary>
    protected float ViewHeight
    {
        get { return viewport.rect.height; }
    }

    protected RectTransform viewRect
    {
        get { return viewport; }
    }
    #endregion

    #region 无限循环滚动属性

    protected bool _useLoop;              //使用Loop功能
    protected int _loopIndex;             //当前标记位 content第一个元素的行
    protected int _loopMaxCount;          //循环最大数
    protected int _loopCacheCount;        //循环缓存数
    protected int _columCount = 1;        //列数
    protected int _rowCount = 1;          //行数
    protected int _loopCacheRow = 1;      //缓存行数
    protected int _loopCacheColumn = 1;      //缓存列数
    protected float _cellHeight = 0;
    protected float _cellWidth = 0;

    protected RectOffset padding;
    // protected float _spacing =0;
    protected Vector2 _spacing = new Vector2();

    protected List<RectTransform> _loopItems;
    protected Dictionary<int, bool> _visibleFlagMap; //记录数据的显隐状态

    protected System.Action<int, int> onFlushItem; //<gameobjectIdx, realIdx> -->item实例索引，数据索引
    protected System.Action<int, bool> onVisibleItem;

    #endregion

    #region 惯性

    protected Vector2 _lastDistance;                    //最终距离
    protected Vector2 InertiaDestPoint                  //惯性目标点
    {
        get
        {
            Vector2 targetPos;
            if(vertical)
            {
                targetPos = _lastDistance * InertiaStep;
                if (targetPos.y > ViewHeight / 2)
                {
                    targetPos.y = ViewHeight / 2;
                }
                if (targetPos.y < -ViewHeight / 2)
                {
                    targetPos.y = -ViewHeight / 2;
                }
            }
            else
            {
                targetPos = _lastDistance * InertiaStep;
                if (targetPos.x > ViewWidth / 2)
                {
                    targetPos.x = ViewWidth / 2;
                }
                if (targetPos.x < -ViewWidth / 2)
                {
                    targetPos.x = -ViewWidth / 2;
                }
            }

            return targetPos;
        }
    }

    protected const float InertiaStep = 5f;                               //惯性距离倍数
    protected const float InertiaSpeed = 10f;                             //惯性速度
    protected const float OutInertiaSpeed = 30f;                          //出界后
    protected float _nowInertiaSpeed;                                     //当前惯性
    protected Vector2 _lastPoint = Vector2.zero;                          //最后的点
    protected Vector2 _firstPoint = Vector2.zero;                         //最先的点

    protected const float ElasticTime = 0.2f;                             //弹性
    protected const float MoveTime = 0.3f;                                //移动时间

    #endregion

    #region 刷新
    protected const float RELEASEBORDER = 50;
    protected const string SWIPEUPKEY = "des_660";  //上拉获取
    protected const string RELEASEKEY = "des_661";  //放开刷新
    protected string m_swipeUpStr, m_releseStr;

    protected bool m_isRelease;                     //是否刷新功能
    protected bool m_isDragRelease;                 //是否拖拽刷新（m_releaseRectTr!=null，true）
    protected bool m_isNorReleaseText = false;      //当前是否一般状态文本
    protected System.Action onRelease;              //拖拽刷新时拖拽后回调，非拖拽刷新在刷新出当前最后一个时回调

    public RectTransform m_releaseRectTr;           //刷新文本的Item
    protected TextEx m_releaseText;                   //刷新文本
    #endregion

    protected override void Awake()
    {
        base.Awake();
        if (m_releaseRectTr)
        {
            m_releaseText = m_releaseRectTr.GetComponentInChildren<TextEx>();
        }
        m_swipeUpStr = LuaEventMgr.Instance.InvokeFunc<string>(LuaEventDefine.GETLANGUAGETEXT, SWIPEUPKEY);
        m_releseStr = LuaEventMgr.Instance.InvokeFunc<string>(LuaEventDefine.GETLANGUAGETEXT, RELEASEKEY);
    }

    protected override void Start()
    {
        base.Start();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Clean();
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Clean();
        m_PointerStartLocalCursor = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
        contentStartPosition = content.anchoredPosition;
        m_Dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        UpdateBounds();
        Vector2 min = contentBounds.min;
        Vector2 max = contentBounds.max;
        TweenBack();
        if (vertical)
        {
            if (max.y < m_ViewBounds.max.y || min.y > m_ViewBounds.min.y)
            {
                _nowInertiaSpeed = OutInertiaSpeed;
            }
            else
            {
                _nowInertiaSpeed = InertiaSpeed;
            }

            if (m_isRelease && m_isDragRelease && CheckDragRelease())
            {
                SetReleaseText(true);
                if (onRelease != null)
                    onRelease();
            }
        }
        else
        {
            if (max.x < m_ViewBounds.max.x || min.x > m_ViewBounds.min.x)
            {
                _nowInertiaSpeed = OutInertiaSpeed;
            }
            else
            {
                _nowInertiaSpeed = InertiaSpeed;
            }

            if (m_isRelease && m_isDragRelease && CheckDragRelease())
            {
                SetReleaseText(true);
                if (onRelease != null)
                    onRelease();
            }
        }
        _lastPoint = Vector2.zero;
        _firstPoint = Vector2.zero;
        m_Inertia = true;
        m_Dragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        var pointerDelta = localCursor - m_PointerStartLocalCursor;
        Vector2 position = contentStartPosition + pointerDelta;
        _lastDistance = position - ContentPosition;
        MoveDistance(_lastDistance);
    }

    protected virtual void SetContentAnchoredPosition(Vector2 position)
    {
        if (!horizontal)
            position.x = content.anchoredPosition.x;
        if (!vertical)
            position.y = content.anchoredPosition.y;

        if (position != content.anchoredPosition)
        {
            content.anchoredPosition = position;
            UpdateBounds();
        }
    }

    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="delta"></param>
    protected bool MoveDistance(Vector2 delta)
    {
        UpdateBounds();
        bool border = false;
        Vector2 min = contentBounds.min;
        Vector2 max = contentBounds.max;
        if (!horizontal)
        {
            delta.x = 0;
        }
        if (!vertical)
        {
            delta.y = 0;
        }
        if (vertical)
        {
            if (max.y + delta.y < m_ViewBounds.max.y)
            {
                border = true;
            }
            else if (ViewHeight > ContentHeight)
            {
                if (max.y + delta.y > m_ViewBounds.max.y)
                {
                    border = true;
                }
            }
            else if (min.y + delta.y > m_ViewBounds.min.y)
            {
                border = true;
            }

            if (m_isRelease && m_isDragRelease)
            {
                SetReleaseText(!CheckDragRelease());
            }
        }
        else
        {
            if (max.x + delta.x < m_ViewBounds.max.x)
            {
                border = true;
            }
            else if (ViewWidth > ContentWidth)
            {
                if (max.x + delta.x > m_ViewBounds.max.x)
                {
                    border = true;
                }
            }
            else if (min.x + delta.x > m_ViewBounds.min.x)
            {
                border = true;
            }

            if (m_isRelease && m_isDragRelease)
            {
                SetReleaseText(!CheckDragRelease());
            }
        }

        Vector2 position = ContentPosition + delta;
        delta = CalculateOffset(delta);
        position += delta;
        if (delta.y != 0)
            position.y = position.y - RubberDelta(delta.y, m_ViewBounds.size.y);
        if (delta.x != 0)
            position.x = position.x - RubberDelta(delta.x, m_ViewBounds.size.x);

        ContentPosition = position;
        ResetLoopPosition();
        return border;
    }
    
    protected virtual void SetItemVisibleFlag(int addIndex = -1)
    {
        if(onVisibleItem == null)
            return;

        if(_visibleFlagMap == null)
            _visibleFlagMap = new Dictionary<int, bool>();

        bool isAdd = addIndex >= 0;
        bool isTrigger = false;

        if (vertical)
        {
            for (int i = 0; i < _loopCacheRow; i++)
            {
                int dataRow = _loopIndex + i;
                int dataIndex = dataRow * _columCount;

                Vector3 cellPos = new Vector3(contentBounds.center.x, contentBounds.max.y, contentBounds.center.z) + i * Vector3.down * _cellHeight;
                bool isShow = m_ViewBounds.Contains(cellPos);
                isShow = isShow || m_ViewBounds.Contains(cellPos + Vector3.down * _cellHeight);
                bool preFlag = false;
                _visibleFlagMap.TryGetValue(dataIndex, out preFlag);
                if (preFlag != isShow)
                {
                    _visibleFlagMap[dataIndex] = isShow;
                    if (isShow)
                    { //目前只抛进入事件
                        for (int j = 0; j < _columCount; j++)
                        {
                            int index = dataIndex + j;
                            if (index >= _loopMaxCount)
                                break;
                            onVisibleItem(index, isShow);

                            if (isAdd && index == addIndex)
                                isTrigger = true;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < _loopCacheColumn; i++)
            {
                int dataColumn = _loopIndex + i;
                int dataIndex = dataColumn * _rowCount;                
                Vector3 cellPos = new Vector3(contentBounds.min.x, contentBounds.center.y, contentBounds.center.z) + i * Vector3.right * _cellWidth;
                bool isShow = m_ViewBounds.Contains(cellPos);
                isShow = isShow || m_ViewBounds.Contains(cellPos + Vector3.right * _cellWidth);
                bool preFlag = false;
                _visibleFlagMap.TryGetValue(dataIndex, out preFlag);
                if (preFlag != isShow)
                {
                    _visibleFlagMap[dataIndex] = isShow;
                    if (isShow)
                    { //目前只抛进入事件
                        for (int j = 0; j < _rowCount; j++)
                        {
                            int index = dataIndex + j;
                            if (index >= _loopMaxCount)
                                break;
                            onVisibleItem(index, isShow);

                            if (isAdd && index == addIndex)
                                isTrigger = true;
                        }
                    }
                }
            }
        }

        if (isAdd && !isTrigger)
        {
            bool isAddShow;
            if (_visibleFlagMap.TryGetValue(addIndex, out isAddShow))
            {
                if (isAddShow)
                    onVisibleItem(addIndex, isAddShow);
            }
        }
    }

    protected Vector2 CalculateOffset(Vector2 delta)
    {
        Vector2 offset = Vector2.zero;

        Vector2 min = contentBounds.min;
        Vector2 max = contentBounds.max;
        if (vertical)
        {
            min.y += delta.y;
            max.y += delta.y;
            if (max.y < m_ViewBounds.max.y)
                offset.y = m_ViewBounds.max.y - max.y;
            else if (min.y > m_ViewBounds.min.y)
                offset.y = m_ViewBounds.min.y - min.y;
        }
        else
        {
            min.x += delta.x;
            max.x += delta.x;
            if (max.x < m_ViewBounds.max.x)
                offset.x = m_ViewBounds.max.x - max.x;
            else if (min.x > m_ViewBounds.min.x)
                offset.x = m_ViewBounds.min.x - min.x;
        }
        return offset;
    }

    protected static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    public void SetOnVisible(System.Action<int, bool> onVisibleItem)
    {
        this.onVisibleItem = onVisibleItem;
    }

    /// <summary>
    /// 初始化循环滚动
    /// </summary>
    /// <param name="loopItems"></param>
    /// <param name="curIndex">当前行</param>
    public virtual void InitLoop(RectTransform[] loopItems, int curIndex, int loopMax, System.Action<int, int> listener, bool isLocation)
    {
        if(_visibleFlagMap != null)
            _visibleFlagMap.Clear();
        _spacing.Set(0,0);
        _loopIndex = Mathf.Max(curIndex, 0);
        _loopItems = new List<RectTransform>(loopItems);
        _loopCacheCount = _loopItems.Count;
        onFlushItem = listener;
        var itemCount = 0;

        if (isLocation)
        {
            ContentPosition = Vector2.zero;
        }
        else
        {
            m_initLastFrame = 3;
        }

        if(vertical)
        {
            // 计算列数 行高
            CalColumCount();
            itemCount = _columCount;

            // 缓存行数
            _loopCacheRow = Mathf.CeilToInt(_loopCacheCount / (float)_columCount);

            // 最大item数 并计算最大行数
            SetLoopMax(loopMax);

            if (_rowCount - _loopCacheRow < _loopIndex)
            {
                _loopIndex = _rowCount - _loopCacheRow;
                float totalHeight = _cellHeight * _loopCacheRow + _spacing.y * Mathf.Max(0, _loopCacheRow - 1) + padding.top + padding.bottom;
                if (isLocation && totalHeight > ViewHeight)
                {
                    int deltaIndex = (curIndex - _loopIndex);
                    float offset = Mathf.Min(totalHeight - ViewHeight, deltaIndex * _cellHeight + _spacing.y * Mathf.Max(0, deltaIndex - 1));
                    ContentPosition = Vector2.up * offset;
                }
            }

            // Debuger.Log("---------列数:{0}, 列高：{1}, 缓存行数：{2},最大行数:{3}", _columCount, _cellHeight, _loopCacheRow, _rowCount);
        }
        else
        {
            // 计算列数 行高
            CalRowCount();
            itemCount = _rowCount;

            // 缓存列数
            _loopCacheColumn = Mathf.CeilToInt(_loopCacheCount / (float)_rowCount);

            // 最大item数 并计算最大行数
            SetLoopMax(loopMax);

            if (_columCount - _loopCacheColumn < _loopIndex)
            {
                _loopIndex = _columCount - _loopCacheColumn;
                float totalWidth = _cellWidth * _loopCacheColumn + _spacing.x * Mathf.Max(0, _loopCacheColumn - 1) + padding.left + padding.right;
                if (isLocation && totalWidth > ViewWidth)
                {
                    int deltaIndex = (curIndex - _loopIndex);
                    float offset = Mathf.Min(totalWidth - ViewWidth, deltaIndex * _cellWidth + _spacing.x * Mathf.Max(0, deltaIndex - 1));
                    ContentPosition = Vector2.left * offset;
                }
            }
        }

        InitRelease();

        int currItemStart = _loopIndex * itemCount;
        for (int i = 0; i < _loopCacheCount; i++)
        {
            RectTransform rTrans = _loopItems[i];
            rTrans.gameObject.name = "item" + i;
            rTrans.SetAsLastSibling();
            int dataIndex = currItemStart + i;
            if (dataIndex >= _loopMaxCount)
            {
                rTrans.gameObject.SetActive(false);
            }
            else
            {
                rTrans.gameObject.SetActive(true);
                onFlushItem(i, dataIndex);
            }
        }
        ResetReleasePos();

        UpdateBounds();
        SetItemVisibleFlag();
    }

    /// <summary>
    /// 设置循环滚动最大数值
    /// </summary>
    /// <param name="loopMax"></param>
    public void SetLoopMax(int loopMax)
    {
        _loopMaxCount = loopMax;
        if(vertical)
        {
            _rowCount = Mathf.CeilToInt(_loopMaxCount / (float)_columCount);
        }
        else
        {
            _columCount = Mathf.CeilToInt(_loopMaxCount / (float)_rowCount);
        }
        _useLoop = _loopItems.Count < loopMax;
        //Debug.LogFormat("SetLoopMax--->{0}, {1}", _loopMaxCount, _rowCount);
    }

    // 计算
    protected virtual void CalColumCount()
    {
        padding = new RectOffset(0, 0, 0, 0);
        _spacing.y = 0;

        if (_loopItems.Count == 0)
        {
            return;
        }
        // 行高
        RectTransform item1 = _loopItems[0];
        _cellHeight = item1.rect.height;

        _columCount = 1;
        LayoutGroup layout = content.GetComponent<LayoutGroup>();
        if (layout != null)
        {
            padding = layout.padding;
            VerticalLayoutGroup verLayout = layout as VerticalLayoutGroup;
            if (verLayout != null)
            {
                _spacing.y = verLayout.spacing;
            }
            else
            {
                GridLayoutGroup gridLayout = layout as GridLayoutGroup;
                if (gridLayout != null)
                {
                    float contentWidth = content.rect.width;
                    contentWidth -= layout.padding.left;
                    contentWidth -= layout.padding.right;
                    contentWidth += gridLayout.spacing.x;

                    float cellWidth = gridLayout.cellSize.x;
                    cellWidth += gridLayout.spacing.x;

                    _cellHeight = gridLayout.cellSize.y;

                    _columCount = Mathf.FloorToInt(contentWidth / cellWidth);
                    if(_columCount <= 0)
                    {
                        _columCount = 1;
                    }

                    _spacing.y = gridLayout.spacing.y;
                }
            }
        }
    }

    // 计算
    protected virtual void CalRowCount()
    {
        padding = new RectOffset(0, 0, 0, 0);
        _spacing.x = 0;

        if (_loopItems.Count == 0)
        {
            return;
        }
        // 行高
        RectTransform item1 = _loopItems[0];
        _cellWidth = item1.rect.width;

        _rowCount = 1;
        LayoutGroup layout = content.GetComponent<LayoutGroup>();
        if (layout != null)
        {
            padding = layout.padding;
            HorizontalLayoutGroup horLayout = layout as HorizontalLayoutGroup;
            if (horLayout != null)
            {
                _spacing.x = horLayout.spacing;
            }
            else
            {
                GridLayoutGroup gridLayout = layout as GridLayoutGroup;
                if (gridLayout != null)
                {
                    float contentHeight = content.rect.height;
                    contentHeight -= layout.padding.top;
                    contentHeight -= layout.padding.bottom;
                    contentHeight += gridLayout.spacing.y;

                    float cellHeight = gridLayout.cellSize.y;
                    cellHeight += gridLayout.spacing.y;

                    _cellWidth = gridLayout.cellSize.x;

                    _rowCount = Mathf.FloorToInt(contentHeight / cellHeight);
                    if(_rowCount <= 0)
                    {
                        _rowCount = 1;
                    }

                    _spacing.x = gridLayout.spacing.x;
                }
            }
        }
    }


    /// <summary>
    /// 重置循环滚动
    /// </summary>
    /// <param name="loopItems"></param>
    public void ResetLoop(RectTransform[] loopItems)
    {
        var itemCount = 0;
        if(vertical)
        {
            itemCount = _columCount;
        }
        else
        {
            itemCount = _rowCount;
        }
        _loopItems = new List<RectTransform>(loopItems);
        _loopCacheCount = _loopItems.Count;
        _loopCacheRow = Mathf.CeilToInt(_loopCacheCount / (float)itemCount);
        for (int i = 0; i < _loopCacheCount; i++)
        {
            _loopItems[i].gameObject.name = "item" + i;
        }
    }


    /// <summary>
    /// 改变当前标记位
    /// </summary>
    /// <param name="addCount">行数</param>
    public void AddCurrentIndex(int addCount)
    {
        _loopIndex = _loopIndex + addCount;
        if(vertical)
        {
            SetLoopMax(_loopMaxCount + addCount * _columCount);
        }
        else
        {
            SetLoopMax(_loopMaxCount + addCount * _rowCount);
        }
    }

    /// <summary>
    /// 获取循环滚动最大数值
    /// </summary>
    /// <param name="loopMax"></param>
    public int GetLoopMax(GameObject go)
    {
        int maxCount = 0;
        if (go)
        {
            RectTransform tran = go.GetComponent<RectTransform>();
            if(vertical)
            {
                if (tran)
                    maxCount = Mathf.CeilToInt(ViewHeight / tran.rect.height);
            }
            else
            {
                if (tran)
                    maxCount = Mathf.CeilToInt(ViewWidth / tran.rect.width);
            }
        }
        return maxCount;
    }

    /// <summary>
    /// 检测当前状态是否达到需要重置Item项
    /// </summary>
    protected virtual void ResetLoopPosition()
    {
        if (!_useLoop)
        {
            if (onVisibleItem != null)
            {
                UpdateBounds();
                SetItemVisibleFlag();
            }            
            return;
        }
        UpdateBounds();
        Vector2 min = contentBounds.min;
        Vector2 max = contentBounds.max;
        if (vertical)
        {
            float bottomHeight = padding.bottom + _cellHeight;
            float topHeight = padding.top + _cellHeight;
            if (_loopIndex + _loopCacheRow < _rowCount && m_ViewBounds.min.y - min.y < bottomHeight && max.y - m_ViewBounds.max.y > topHeight)
            {
                VerticalUp();
                _loopIndex++;
                return;
                //Debug.LogFormat("向前移动,行：{0}", _loopIndex + _loopCacheRow);
            }
            else if (_loopIndex > 0 && max.y - m_ViewBounds.max.y < topHeight)
            {
                VerticalDown();
                _loopIndex--;
                return;
                //Debug.LogFormat("向后移动：{0}", _loopIndex);
            }
        }
        else
        {
            float leftWidth = padding.left + _cellWidth;
            float rightWidth = padding.right + _cellWidth;
            if (_loopIndex > 0 && m_ViewBounds.min.x - min.x < leftWidth && max.x - m_ViewBounds.max.x > rightWidth)
            {
                HorizontalRight();
                _loopIndex--;
                return;
                // Debug.LogFormat("向后移动：{0}", _loopIndex);
            }
            else if (_loopIndex + _loopCacheColumn < _columCount && max.x - m_ViewBounds.max.x < rightWidth)
            {
                HorizontalLeft();
                _loopIndex++;
                return;
                // Debug.LogFormat("向前移动,行：{0}", _loopIndex + _loopCacheRow);
            }
        }
        SetItemVisibleFlag();
    }

    protected int GetIdxFromName(string strName)
    {
        int idx = 0;
        // "item" 字符串长度是4
        string strIdx = strName.Substring(4);
        int.TryParse(strIdx, out idx);
        return idx;
    }

    /// <summary>
    /// 向上拖拽(Item下移)
    /// </summary>
    protected virtual void VerticalUp()
    {
        int currRowStart = _loopIndex * _columCount + _loopCacheCount; // 当前行最新
        int moveCount = _columCount;

        // 每次都往末尾补一行的元素
        for (int i = 0; moveCount > 0; moveCount--, i++)
        {
            RectTransform rt = _loopItems[0];
            rt.SetAsLastSibling();
            _loopItems.RemoveAt(0);
            _loopItems.Add(rt);

            int dataIdx = currRowStart + i;
            if (dataIdx + 1 > _loopMaxCount)
            {
                rt.gameObject.SetActive(false);
            }
            else
            {
                if (onFlushItem != null && m_IsEnable)
                {
                    int objIdx = GetIdxFromName(rt.name);
                    onFlushItem(objIdx, dataIdx);
                    if (m_isRelease && !m_isDragRelease && dataIdx + 1 == _loopMaxCount && onRelease != null)
                    {
                        onRelease();
                    }
                }
            }
        }
        ResetReleasePos();

        Vector2 offset = new Vector2(0, _cellHeight + _spacing.y);
        ContentPosition = ContentPosition - offset;
        contentStartPosition = contentStartPosition - offset;
    }

    /// <summary>
    /// 向左拖拽(Item右移)
    /// </summary>
    protected virtual void HorizontalLeft()
    {
        int currColumnStart = _loopIndex * _rowCount + _loopCacheCount; // 当前列最新
        int moveCount = _rowCount;

        // 每次都往末尾补一列的元素
        for (int i = 0; moveCount > 0; moveCount--, i++)
        {
            RectTransform rt = _loopItems[0];
            rt.SetAsLastSibling();
            _loopItems.RemoveAt(0);
            _loopItems.Add(rt);

            int dataIdx = currColumnStart + i;
            if (dataIdx + 1 > _loopMaxCount)
            {
                rt.gameObject.SetActive(false);
            }
            else
            {
                if (onFlushItem != null && m_IsEnable)
                {
                    int objIdx = GetIdxFromName(rt.name);
                    onFlushItem(objIdx, dataIdx);
                    if (m_isRelease && !m_isDragRelease && dataIdx + 1 == _loopMaxCount && onRelease != null)
                    {
                        onRelease();
                    }
                }
            }
        }
        ResetReleasePos();

        Vector2 offset = new Vector2(_cellWidth + _spacing.x, 0);
        ContentPosition = ContentPosition + offset;
        contentStartPosition = contentStartPosition + offset;
    }


    /// <summary>
    /// 向下拖拽(Item上移)
    /// </summary>
    protected virtual void VerticalDown()
    {
        int moveCount = _columCount;
        int lastIndex = _loopItems.Count - 1;
        int currRowStart = _loopIndex * _columCount;

        // 往开头补一行的元素
        for (int i = 1; moveCount > 0; moveCount--, i++)
        {
            RectTransform rt = _loopItems[lastIndex];
            rt.SetAsFirstSibling();
            _loopItems.RemoveAt(lastIndex);
            _loopItems.Insert(0, rt);
            if (!rt.gameObject.activeSelf)
                rt.gameObject.SetActive(true);

            if (onFlushItem != null && m_IsEnable)
            {
                int objIdx = GetIdxFromName(rt.name);
                onFlushItem(objIdx, currRowStart - i);
            }
        }

        Vector2 offset = new Vector2(0, _cellHeight + _spacing.y);
        ContentPosition = ContentPosition + offset;
        contentStartPosition = contentStartPosition + offset;
    }

    /// <summary>
    /// 向右拖拽(Item左移)
    /// </summary>
    protected virtual void HorizontalRight()
    {
        int moveCount = _rowCount;
        int lastIndex = _loopItems.Count - 1;
        int currColumnStart = _loopIndex * _rowCount;

        // 往开头补一行的元素
        for (int i = 1; moveCount > 0; moveCount--, i++)
        {
            RectTransform rt = _loopItems[lastIndex];
            rt.SetAsFirstSibling();
            _loopItems.RemoveAt(lastIndex);
            _loopItems.Insert(0, rt);
            if (!rt.gameObject.activeSelf)
                rt.gameObject.SetActive(true);

            if (onFlushItem != null && m_IsEnable)
            {
                int objIdx = GetIdxFromName(rt.name);
                onFlushItem(objIdx, currColumnStart - i);
            }
        }

        Vector2 offset = new Vector2(_cellWidth + _spacing.x, 0);
        ContentPosition = ContentPosition - offset;
        contentStartPosition = contentStartPosition - offset;
    }

    protected void UpdateBounds()
    {
        m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
        contentBounds = GetBounds();

        if (content == null)
            return;

        Vector3 contentSize = contentBounds.size;
        Vector3 contentPos = contentBounds.center;
        Vector3 excess = m_ViewBounds.size - contentSize;
        if (excess.x > 0)
        {
            contentPos.x -= excess.x * (content.pivot.x - 0.5f);
            contentSize.x = m_ViewBounds.size.x;
        }
        if (excess.y > 0)
        {
            contentPos.y -= excess.y * (content.pivot.y - 0.5f);
            contentSize.y = m_ViewBounds.size.y;
        }

        contentBounds.size = contentSize;
        contentBounds.center = contentPos;
    }

    protected readonly Vector3[] m_Corners = new Vector3[4];
    protected Bounds GetBounds()
    {
        if (content == null)
            return new Bounds();

        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        var toLocal = viewRect.worldToLocalMatrix;
        content.GetWorldCorners(m_Corners);
        for (int j = 0; j < 4; j++)
        {
            Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
    }

    protected LTDescr _ltDescrVertical;                   //垂直归位动画.
    protected LTDescr _ltDescrHorizontal;                   //水平归位动画.

    /// <summary>
    /// 归位动画
    /// </summary>
    protected void TweenBack()
    {
        //		Debug.Log ("---------TweenBack-------");
        Clean();
        UpdateBounds();
        Vector2 min = contentBounds.min;
        Vector2 max = contentBounds.max;

        if (vertical)
        {
            if (max.y < m_ViewBounds.max.y)
            {
                float distance = m_ViewBounds.max.y - max.y + ContentPosition.y;
                _ltDescrVertical = LeanTween.moveY(content, distance, ElasticTime).setOnComplete(OnCompleteTweenBack);
            }
            else if (ViewHeight > ContentHeight)
            {
                if (max.y > m_ViewBounds.max.y)
                {
                    float distance = m_ViewBounds.max.y - max.y + ContentPosition.y;
                    _ltDescrVertical = LeanTween.moveY(content, distance, ElasticTime).setOnComplete(OnCompleteTweenBack);
                }
                else
                {
                    return;
                }
            }
            else if (min.y > m_ViewBounds.min.y)
            {
                float distance = m_ViewBounds.min.y - min.y + ContentPosition.y;
                _ltDescrVertical = LeanTween.moveY(content, distance, ElasticTime).setOnComplete(OnCompleteTweenBack);
            }
            else
            {
                return;
            }
        }
        else
        {
            if (max.x < m_ViewBounds.max.x)
            {
                float distance = m_ViewBounds.max.x - max.x + ContentPosition.x;
                _ltDescrHorizontal = LeanTween.moveX(content, distance, ElasticTime).setOnComplete(OnCompleteTweenBack);
            }
            else if (ViewWidth > ContentWidth)
            {
                if (max.x > m_ViewBounds.max.x)
                {
                    float distance = m_ViewBounds.max.x - max.x + ContentPosition.x;
                    _ltDescrHorizontal = LeanTween.moveX(content, distance, ElasticTime).setOnComplete(OnCompleteTweenBack);
                }
                else
                {
                    return;
                }
            }
            else if (min.x > m_ViewBounds.min.x)
            {
                float distance = m_ViewBounds.min.x - min.x + ContentPosition.x;
                _ltDescrHorizontal = LeanTween.moveX(content, distance, ElasticTime).setOnComplete(OnCompleteTweenBack);
            }
            else
            {
                return;
            }
        }
    }

    protected void OnCompleteTweenBack()
    {
        MoveDistance(Vector2.zero);
    }

    protected bool m_Inertia = false;
    protected int m_initLastFrame = 0;

    void LateUpdate()
    {
        if (!m_IsEnable || m_Dragging) return;
        if (m_initLastFrame > 0)
        {
            m_initLastFrame--;
        }
        if (m_Inertia || m_initLastFrame == 1)
        {
            m_initLastFrame = 0;
            if (_nowInertiaSpeed == OutInertiaSpeed)
            {
                TweenBack();
                m_Inertia = false;
                return;
            }
            _firstPoint = Vector2.Lerp(_firstPoint, InertiaDestPoint, _nowInertiaSpeed * Time.deltaTime);
            Vector2 moveOffset = _firstPoint - _lastPoint;
            bool border = MoveDistance(moveOffset);
            _lastPoint = _firstPoint;
            float dis = (_firstPoint - InertiaDestPoint).magnitude;
            if (dis < 1 || border)
            {
                TweenBack();
                m_Inertia = false;
                return;
            }
        }
        else
        {
//			if (!_inBack)
//			{
//				TweenBack();
//			}
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        m_IsEnable = true;
        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

    protected override void OnDisable()
    {
        CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
        m_IsEnable = false;
        m_Inertia = false;
        LayoutRebuilder.MarkLayoutForRebuild(transform.transform as RectTransform);
        base.OnDisable();
    }

    /// <summary>
    /// 清理(动画与惯性)
    /// </summary>
    protected void Clean()
    {
        m_Inertia = false;
        if (_ltDescrVertical != null)
        {
        	// LeanTween.cancel (_ltDescrVertical.id);
            LeanTween.cancel(gameObject);
        	_ltDescrVertical = null;
        }
        if (_ltDescrHorizontal != null)
        {
        	// LeanTween.cancel (_ltDescrVertical.id);
            LeanTween.cancel(gameObject);
        	_ltDescrHorizontal = null;
        }
    }
    public void Rebuild(CanvasUpdate executing)
    { }

    public void LayoutComplete()
    { }

    public void GraphicUpdateComplete()
    { }

    #region 添加/移除
    public virtual void AddItem(int index, RectTransform rectTr = null)     //新增的rectTr必须在列表的最后一个
    {
        var groupCount = 0;
        if(vertical)
        {
            groupCount = _columCount;
        }
        else
        {
            groupCount = _rowCount;
        }


        SetLoopMax(_loopMaxCount + 1);
        int startIndex = _loopIndex * groupCount;

        if (rectTr != null)
        {
            _loopCacheCount++;
            _loopCacheRow = Mathf.CeilToInt(_loopCacheCount / (float)_columCount);

            int objIdx = _loopItems.Count;
            rectTr.name = "item" + objIdx.ToString();

            int insertIndex = index - startIndex;
            rectTr.SetSiblingIndex(insertIndex);
            _loopItems.Insert(insertIndex, rectTr);

            if (onFlushItem != null)
            {
                onFlushItem(objIdx, index);
            }
        }
        else if (startIndex <= index)
        {
            if (index <= startIndex + _loopCacheCount - 1)
            {
                int lastIndex = _loopCacheCount - 1;
                int insertIndex = index - startIndex;
                RectTransform rt = _loopItems[lastIndex];
                rt.SetSiblingIndex(insertIndex);
                if (!rt.gameObject.activeSelf)
                    rt.gameObject.SetActive(true);

                _loopItems.RemoveAt(lastIndex);
                _loopItems.Insert(insertIndex, rt);

                if (onFlushItem != null)
                {
                    int objIdx = GetIdxFromName(rt.name);
                    onFlushItem(objIdx, index);
                }
            }
        }
        else
        { //startIndex > index
            if (_columCount == 1)
            {
                _loopIndex++;
            }
            else
            {
                int lastIndex = _loopCacheCount - 1;
                RectTransform rt = _loopItems[lastIndex];
                rt.SetSiblingIndex(0);
                if (!rt.gameObject.activeSelf)
                    rt.gameObject.SetActive(true);

                _loopItems.RemoveAt(lastIndex);
                _loopItems.Insert(0, rt);

                if (onFlushItem != null)
                {
                    int objIdx = GetIdxFromName(rt.name);
                    onFlushItem(objIdx, startIndex);
                }
            }
        }
        SetItemVisibleFlag(index);
    }

    public virtual int RemoveItem(int index, bool removeTr)
    {
        SetLoopMax(_loopMaxCount - 1);
        var groupCount = 0;
        if(vertical)
        {
            groupCount = _columCount;
        }
        else
        {
            groupCount = _rowCount;
        }
        int startIndex = _loopIndex * groupCount;

        if (removeTr)
        {
            _loopCacheCount--;
            _loopCacheRow = Mathf.CeilToInt(_loopCacheCount / (float)groupCount);

            int removeIndex = index - startIndex;
            RectTransform rt = _loopItems[removeIndex];
            int objIdx = GetIdxFromName(rt.name);

            _loopItems.RemoveAt(removeIndex);
            for (int i = 0; i < _loopItems.Count; i++)
            {
                int objIndex = GetIdxFromName(_loopItems[i].name);
                if (objIndex > objIdx)
                {
                    _loopItems[i].name = "item" + (--objIndex).ToString();
                }
            }
            return objIdx;
        }
        else if (groupCount == 1)
        {
            if (startIndex <= index)
            {
                int endIndex = startIndex + _loopCacheCount - 1;
                if (index <= endIndex)
                {
                    int removeIndex = index - startIndex;
                    RectTransform rt = _loopItems[removeIndex];
                    _loopItems.RemoveAt(removeIndex);

                    int flushIndex = startIndex - 1;
                    if (endIndex >= _loopMaxCount)
                    { //刷新前面一个item
                        _loopIndex--;
                        rt.SetAsFirstSibling();
                        _loopItems.Insert(0, rt);
                        if (!rt.gameObject.activeSelf)
                            rt.gameObject.SetActive(true);
                    }
                    else
                    {
                        rt.SetAsLastSibling();
                        _loopItems.Add(rt);
                        if (!rt.gameObject.activeSelf)
                            rt.gameObject.SetActive(true);
                        flushIndex = endIndex;

                        ResetReleasePos();
                    }

                    if (onFlushItem != null)
                    {
                        int objIdx = GetIdxFromName(rt.name);
                        onFlushItem(objIdx, flushIndex);
                    }
                }
            }
            else
            {
                _loopIndex--;
            }
        }
        else
        {
            int endIndex = startIndex + _loopCacheCount - 1;
            if (index <= endIndex)
            {
                int removeIndex = Mathf.Max(index - startIndex, 0);
                RectTransform rt = _loopItems[removeIndex];
                _loopItems.RemoveAt(removeIndex);

                rt.SetAsLastSibling();
                _loopItems.Add(rt);

                if (endIndex >= _loopMaxCount)
                {
                    rt.gameObject.SetActive(false);
                }
                else if (onFlushItem != null)
                {
                    int objIdx = GetIdxFromName(rt.name);
                    onFlushItem(objIdx, endIndex);
                }
            }
        }
        return -1;
    }
    #endregion

    #region 刷新
    protected void InitRelease()
    {
        m_isDragRelease = (m_releaseRectTr != null);
        m_isRelease = true;
        SetReleaseActive(false);
    }

    protected virtual void ResetReleasePos()
    {
        if (m_isDragRelease)
            m_releaseRectTr.transform.SetAsLastSibling();
    }

    protected virtual bool CheckDragRelease()
    {
        return (contentBounds.min.y > m_ViewBounds.min.y + RELEASEBORDER);
    }

    protected virtual void SetReleaseText(bool isNor)
    {
        if (m_isNorReleaseText == isNor)
            return;
        m_isNorReleaseText = isNor;
        if(m_releaseText != null)
            m_releaseText.text = isNor ? m_swipeUpStr : m_releseStr;
    }

    public virtual void SetReleaseActive(bool active)
    {
        if (m_isRelease == active)
            return;

        m_isRelease = active;
        if (m_isDragRelease)
        {
            if (active)
                SetReleaseText(true);

            m_releaseRectTr.gameObject.SetActive(active);
            if(padding != null)
                padding.bottom = active ? (int)m_releaseRectTr.rect.height : 0;
        }
    }

    public void SetOnRelease(System.Action listener)
    {
        onRelease = listener;
    }
    #endregion

    #region 获取最前/最后的ObjIndex
    public int GetFirstObjIndex()
    {
        if (_loopItems.Count <= 0)
            return -1;
        RectTransform tr = _loopItems[0];
        int objIndex = GetIdxFromName(tr.name);
        return objIndex;
    }

    public int GetLastObjIndex()
    {
        if (_loopItems.Count <= 0)
            return -1;
        RectTransform tr = _loopItems[_loopItems.Count - 1];
        int objIndex = GetIdxFromName(tr.name);
        return objIndex;
    }
    #endregion

    public int GetFirstRow()
    {
        return _loopIndex;
    }

    public int GetMaxItemNumber(float itemHeight)
    {
        return Mathf.CeilToInt((ViewHeight + _spacing.y) / (itemHeight + _spacing.y)) + 4;
    }
}
