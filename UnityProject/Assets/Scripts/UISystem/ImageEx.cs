using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImageEx : Image
{
    [SerializeField]
    private string m_SpriteName;
    [SerializeField]
    private bool m_UseSpritePacker;

    private AtlasReference m_AtlasRef;
    private static Dictionary<string, Vector4> m_spritePaddings;
    [NonSerialized]
    private Sprite m_OverrideSprite;
    private Sprite activeSprite { get { return m_OverrideSprite != null ? m_OverrideSprite : sprite; } }

    protected ImageEx()
    {
        useLegacyMeshGeneration = false;
    }

    public void SetEmojiName(string name)
    {
        SpriteModule.Instance.LoadEmojiByName(name, onLoadedEmoji);
    }

    private void onLoadedEmoji(object obj, Material mat)
    {
        Sprite sp = obj as Sprite;
        this.sprite = sp;

        if(mat != null)
            this.material = mat;
    }

    public string SpriteName
    {
        get
        {
            return m_SpriteName;
        }
        set
        {
            m_SpriteName = value;
        }
    }

    public bool UseSpritePacker
    {
        get
        {
            return m_UseSpritePacker;
        }
        set
        {
            m_UseSpritePacker = value;
        }
    }

    public AtlasReference GetAtlasReference()
    {
        if(m_AtlasRef == null)
        {
            m_AtlasRef = gameObject.GetComponentInParent<AtlasReference>();
        }
        return m_AtlasRef;
    }

    protected Vector4 GetPadding()
    {
        if(Application.isPlaying)
        {
            if(string.IsNullOrEmpty(SpriteName))
            {
                return Vector4.zero;
            }
            if(m_spritePaddings == null)
            {
                //Debug.LogError("SpritePadding error");
                return Vector4.zero;
            }
            if(m_spritePaddings.ContainsKey(SpriteName))
            {
                return m_spritePaddings[SpriteName];
            }
            return Vector4.zero;
        }
#if UNITY_EDITOR
        if(!overrideSprite)
            return Vector4.zero;
        if(UseSpritePacker)
        {
            return DataUtility.GetPadding(overrideSprite);
        }


        if(m_spritePaddings == null)
        {
            string path = PathDef.UI_ASSETS_PATH + "/atlas_tp_padding.asset";
            PaddingData paddingData = AssetDatabase.LoadAssetAtPath<PaddingData>(path);
            m_spritePaddings = new Dictionary<string, Vector4>();
            foreach(var atlas in paddingData.atlas)
            {
                foreach(var spriteInfo in atlas.sprites)
                {
                    if(m_spritePaddings.ContainsKey(spriteInfo.name)) continue;
                    m_spritePaddings.Add(spriteInfo.name, spriteInfo.padding);
                }
            }
        }

        Vector4 v;
        if(!m_spritePaddings.TryGetValue(overrideSprite.name, out v))
        {
            //Debuger.LogError("图集错误. 白边信息缺失: sprite:" + overrideSprite.name);
            return Vector4.zero;
        }
        return v;
#else
            return Vector4.zero;
#endif
    }

    /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
    /// shouldPreserveAspect为是否按精灵的原比例显示
    protected Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
    {
        Sprite overrideSprite = this.overrideSprite;
        //当前精灵的填充内边框（left,bottom,right,top)，一般情况下都是（0，0，0，0）
        var padding = GetPadding();
        //当前精灵的大小（包含了边框的大小）
        var size = overrideSprite == null ? Vector2.zero : new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
        //目标绘制区域的坐标及大小（x,y为该UI相对于轴心的坐标，width，height为UI宽高）
        Rect r = GetPixelAdjustedRect();
        int spriteW = Mathf.RoundToInt(size.x);
        int spriteH = Mathf.RoundToInt(size.y);
        float width = spriteW + padding.z + padding.x;
        float height = spriteH + padding.w + padding.y;

        //计算出一种比率，为显示出来的图片剔除内边框做准备
        var v = new Vector4(
                padding.x / width,
                padding.y / height,
                (width - padding.z) / width,
                (height - padding.w) / height);

        if(shouldPreserveAspect && size.sqrMagnitude > 0.0f)
        {
            //原图宽高比
            var spriteRatio = size.x / size.y;
            //目标绘制区域宽高比
            var rectRatio = r.width / r.height;
            if(spriteRatio > rectRatio)
            {
                //原图更宽则按宽调整高度大小，以及重新计算坐标位置
                var oldHeight = r.height;
                r.height = r.width * (1.0f / spriteRatio);
                r.y += (oldHeight - r.height) * rectTransform.pivot.y;
            }
            else
            {
                //反之则按高度调整
                var oldWidth = r.width;
                r.width = r.height * spriteRatio;
                r.x += (oldWidth - r.width) * rectTransform.pivot.x;
            }
        }

        //重新计算x,y,z,w的大小
        v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
                );

        return v;
    }

    /// <summary>
    /// 更新UI渲染网格(Polygon模式未开启)
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if(overrideSprite == null)
        {
            base.OnPopulateMesh(toFill);
            return;
        }

        switch(type)
        {
            case Type.Simple:
                GenerateSimpleSprite(toFill, preserveAspect);
                break;
            case Type.Sliced:
                GenerateSlicedSprite(toFill);
                break;
            case Type.Tiled:
                GenerateTiledSprite(toFill);
                break;
            case Type.Filled:
                GenerateFilledSprite(toFill, preserveAspect);
                break;
        }
    }

    /// <summary>
    /// 生成Simple模式Sprite
    /// </summary>
    void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
    {
        Sprite overrideSprite = this.overrideSprite;
        Color32 color = this.color;
        var uv = (overrideSprite != null) ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        vh.Clear();
        //获得图片的位置信息
        Vector4 v = GetDrawingDimensions(lPreserveAspect);

        //添加顶点
        vh.AddVert(new Vector3(v.x, v.y), color, new Vector2(uv.x, uv.y));
        vh.AddVert(new Vector3(v.x, v.w), color, new Vector2(uv.x, uv.w));
        vh.AddVert(new Vector3(v.z, v.w), color, new Vector2(uv.z, uv.w));
        vh.AddVert(new Vector3(v.z, v.y), color, new Vector2(uv.z, uv.y));

        //添加三角形
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
    {
        int startIndex = vertexHelper.currentVertCount;

        vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
        vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
        vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }
    static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
    {
        int startIndex = vertexHelper.currentVertCount;

        for(int i = 0; i < 4; ++i)
            vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);

        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }

    private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
    {
        Rect originalRect = rectTransform.rect;

        for(int axis = 0; axis <= 1; axis++)
        {
            float borderScaleRatio;

            // The adjusted rect (adjusted for pixel correctness)
            // may be slightly larger than the original rect.
            // Adjust the border to match the adjustedRect to avoid
            // small gaps between borders (case 833201).
            if(originalRect.size[axis] != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }

            // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
            // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
            float combinedBorders = border[axis] + border[axis + 2];
            if(adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }
        return border;
    }

    /// <summary>
    /// Generate vertices for a 9-sliced Image.
    /// </summary>

    static readonly Vector2[] s_VertScratch = new Vector2[4];
    static readonly Vector2[] s_UVScratch = new Vector2[4];

    private void GenerateSlicedSprite(VertexHelper toFill)
    {
        //如果没有边框则跟普通精灵顶点三角形是一样的
        if(!hasBorder)
        {
            GenerateSimpleSprite(toFill, false);
            return;
        }

        Vector4 outer, inner, padding, border;

        if(activeSprite != null)
        {
            outer = DataUtility.GetOuterUV(activeSprite);
            inner = DataUtility.GetInnerUV(activeSprite);
            padding = DataUtility.GetPadding(activeSprite);
            border = activeSprite.border;
        }
        else
        {
            outer = Vector4.zero;
            inner = Vector4.zero;
            padding = Vector4.zero;
            border = Vector4.zero;
        }

        Rect rect = GetPixelAdjustedRect();
        //调整后的边框大小
        Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
        padding = padding / pixelsPerUnit;
        //图片的真实坐标和大小
        s_VertScratch[0] = new Vector2(padding.x, padding.y);
        s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

        s_VertScratch[1].x = adjustedBorders.x;
        s_VertScratch[1].y = adjustedBorders.y;

        s_VertScratch[2].x = rect.width - adjustedBorders.z;
        s_VertScratch[2].y = rect.height - adjustedBorders.w;

        for(int i = 0; i < 4; ++i)
        {
            s_VertScratch[i].x += rect.x;
            s_VertScratch[i].y += rect.y;
        }

        s_UVScratch[0] = new Vector2(outer.x, outer.y);
        s_UVScratch[1] = new Vector2(inner.x, inner.y);
        s_UVScratch[2] = new Vector2(inner.z, inner.w);
        s_UVScratch[3] = new Vector2(outer.z, outer.w);

        toFill.Clear();
        //生成9个矩形区域
        for(int x = 0; x < 3; ++x)
        {
            int x2 = x + 1;

            for(int y = 0; y < 3; ++y)
            {
                if(!fillCenter && x == 1 && y == 1)
                    continue;

                int y2 = y + 1;


                AddQuad(toFill,
                    new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                    new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                    color,
                    new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                    new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
            }
        }
    }

    /// <summary>
    /// 生成Tiled模式Sprite
    /// </summary>
    void GenerateTiledSprite(VertexHelper toFill)
    {
        Vector4 outer, inner, border;
        Vector2 spriteSize;

        Sprite overrideSprite = this.overrideSprite;
        Color32 color = this.color;

        if(overrideSprite != null)
        {
            outer = DataUtility.GetOuterUV(overrideSprite);
            inner = DataUtility.GetInnerUV(overrideSprite);
            border = overrideSprite.border;
            spriteSize = overrideSprite.rect.size;
        }
        else
        {
            outer = Vector4.zero;
            inner = Vector4.zero;
            border = Vector4.zero;
            spriteSize = Vector2.one * 100;
        }

        Rect rect = GetPixelAdjustedRect();
        float tileWidth = (spriteSize.x - border.x - border.z) / pixelsPerUnit;
        float tileHeight = (spriteSize.y - border.y - border.w) / pixelsPerUnit;
        border = GetAdjustedBorders(border / pixelsPerUnit, rect);

        var uvMin = new Vector2(inner.x, inner.y);
        var uvMax = new Vector2(inner.z, inner.w);

        var v = UIVertex.simpleVert;
        v.color = color;

        // Min to max max range for tiled region in coordinates relative to lower left corner.
        float xMin = border.x;
        float xMax = rect.width - border.z;
        float yMin = border.y;
        float yMax = rect.height - border.w;

        toFill.Clear();
        var clipped = uvMax;

        // if either with is zero we cant tile so just assume it was the full width.
        if(tileWidth <= 0)
            tileWidth = xMax - xMin;

        if(tileHeight <= 0)
            tileHeight = yMax - yMin;

        if(fillCenter)
        {
            for(float y1 = yMin; y1 < yMax; y1 += tileHeight)
            {
                float y2 = y1 + tileHeight;
                if(y2 > yMax)
                {
                    clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                    y2 = yMax;
                }

                clipped.x = uvMax.x;
                for(float x1 = xMin; x1 < xMax; x1 += tileWidth)
                {
                    float x2 = x1 + tileWidth;
                    if(x2 > xMax)
                    {
                        clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                        x2 = xMax;
                    }
                    AddQuad(toFill, new Vector2(x1, y1) + rect.position, new Vector2(x2, y2) + rect.position, color, uvMin, clipped);
                }
            }
        }

        if(hasBorder)
        {
            // Left and right tiled border
            clipped = uvMax;
            for(float y1 = yMin; y1 < yMax; y1 += tileHeight)
            {
                float y2 = y1 + tileHeight;
                if(y2 > yMax)
                {
                    clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                    y2 = yMax;
                }
                AddQuad(toFill,
                    new Vector2(0, y1) + rect.position,
                    new Vector2(xMin, y2) + rect.position,
                    color,
                    new Vector2(outer.x, uvMin.y),
                    new Vector2(uvMin.x, clipped.y));
                AddQuad(toFill,
                    new Vector2(xMax, y1) + rect.position,
                    new Vector2(rect.width, y2) + rect.position,
                    color,
                    new Vector2(uvMax.x, uvMin.y),
                    new Vector2(outer.z, clipped.y));
            }

            // Bottom and top tiled border
            clipped = uvMax;
            for(float x1 = xMin; x1 < xMax; x1 += tileWidth)
            {
                float x2 = x1 + tileWidth;
                if(x2 > xMax)
                {
                    clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                    x2 = xMax;
                }
                AddQuad(toFill,
                    new Vector2(x1, 0) + rect.position,
                    new Vector2(x2, yMin) + rect.position,
                    color,
                    new Vector2(uvMin.x, outer.y),
                    new Vector2(clipped.x, uvMin.y));
                AddQuad(toFill,
                    new Vector2(x1, yMax) + rect.position,
                    new Vector2(x2, rect.height) + rect.position,
                    color,
                    new Vector2(uvMin.x, uvMax.y),
                    new Vector2(clipped.x, outer.w));
            }

            // Corners
            AddQuad(toFill,
                new Vector2(0, 0) + rect.position,
                new Vector2(xMin, yMin) + rect.position,
                color,
                new Vector2(outer.x, outer.y),
                new Vector2(uvMin.x, uvMin.y));
            AddQuad(toFill,
                new Vector2(xMax, 0) + rect.position,
                new Vector2(rect.width, yMin) + rect.position,
                color,
                new Vector2(uvMax.x, outer.y),
                new Vector2(outer.z, uvMin.y));
            AddQuad(toFill,
                new Vector2(0, yMax) + rect.position,
                new Vector2(xMin, rect.height) + rect.position,
                color,
                new Vector2(outer.x, uvMax.y),
                new Vector2(uvMin.x, outer.w));
            AddQuad(toFill,
                new Vector2(xMax, yMax) + rect.position,
                new Vector2(rect.width, rect.height) + rect.position,
                color,
                new Vector2(uvMax.x, uvMax.y),
                new Vector2(outer.z, outer.w));
        }
    }

    /// <summary>
    /// Generate vertices for a filled Image.
    /// </summary>

    static readonly Vector3[] s_Xy = new Vector3[4];
    static readonly Vector3[] s_Uv = new Vector3[4];
    void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect)
    {
        toFill.Clear();

        if(fillAmount < 0.001f)
            return;

        Vector4 v = GetDrawingDimensions(preserveAspect);
        Vector4 outer = activeSprite != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
        UIVertex uiv = UIVertex.simpleVert;
        uiv.color = color;

        float tx0 = outer.x;
        float ty0 = outer.y;
        float tx1 = outer.z;
        float ty1 = outer.w;

        // Horizontal and vertical filled sprites are simple -- just end the Image prematurely
        if(fillMethod == FillMethod.Horizontal || fillMethod == FillMethod.Vertical)
        {
            if(fillMethod == FillMethod.Horizontal)
            {
                float fill = (tx1 - tx0) * fillAmount;

                if(fillOrigin == 1)
                {
                    v.x = v.z - (v.z - v.x) * fillAmount;
                    tx0 = tx1 - fill;
                }
                else
                {
                    v.z = v.x + (v.z - v.x) * fillAmount;
                    tx1 = tx0 + fill;
                }
            }
            else if(fillMethod == FillMethod.Vertical)
            {
                float fill = (ty1 - ty0) * fillAmount;

                if(fillOrigin == 1)
                {
                    v.y = v.w - (v.w - v.y) * fillAmount;
                    ty0 = ty1 - fill;
                }
                else
                {
                    v.w = v.y + (v.w - v.y) * fillAmount;
                    ty1 = ty0 + fill;
                }
            }
        }

        s_Xy[0] = new Vector2(v.x, v.y);
        s_Xy[1] = new Vector2(v.x, v.w);
        s_Xy[2] = new Vector2(v.z, v.w);
        s_Xy[3] = new Vector2(v.z, v.y);

        s_Uv[0] = new Vector2(tx0, ty0);
        s_Uv[1] = new Vector2(tx0, ty1);
        s_Uv[2] = new Vector2(tx1, ty1);
        s_Uv[3] = new Vector2(tx1, ty0);

        {
            if(fillAmount < 1f && fillMethod != FillMethod.Horizontal && fillMethod != FillMethod.Vertical)
            {
                if(fillMethod == FillMethod.Radial90)
                {
                    if(RadialCut(s_Xy, s_Uv, fillAmount, fillClockwise, fillOrigin))
                        AddQuad(toFill, s_Xy, color, s_Uv);
                }
                else if(fillMethod == FillMethod.Radial180)
                {
                    for(int side = 0; side < 2; ++side)
                    {
                        float fx0, fx1, fy0, fy1;
                        int even = fillOrigin > 1 ? 1 : 0;

                        if(fillOrigin == 0 || fillOrigin == 2)
                        {
                            fy0 = 0f;
                            fy1 = 1f;
                            if(side == even)
                            {
                                fx0 = 0f;
                                fx1 = 0.5f;
                            }
                            else
                            {
                                fx0 = 0.5f;
                                fx1 = 1f;
                            }
                        }
                        else
                        {
                            fx0 = 0f;
                            fx1 = 1f;
                            if(side == even)
                            {
                                fy0 = 0.5f;
                                fy1 = 1f;
                            }
                            else
                            {
                                fy0 = 0f;
                                fy1 = 0.5f;
                            }
                        }

                        s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                        s_Xy[1].x = s_Xy[0].x;
                        s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                        s_Xy[3].x = s_Xy[2].x;

                        s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                        s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                        s_Xy[2].y = s_Xy[1].y;
                        s_Xy[3].y = s_Xy[0].y;

                        s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                        s_Uv[1].x = s_Uv[0].x;
                        s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                        s_Uv[3].x = s_Uv[2].x;

                        s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                        s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                        s_Uv[2].y = s_Uv[1].y;
                        s_Uv[3].y = s_Uv[0].y;

                        float val = fillClockwise ? fillAmount * 2f - side : fillAmount * 2f - (1 - side);

                        if(RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), fillClockwise, ((side + fillOrigin + 3) % 4)))
                        {
                            AddQuad(toFill, s_Xy, color, s_Uv);
                        }
                    }
                }
                else if(fillMethod == FillMethod.Radial360)
                {
                    for(int corner = 0; corner < 4; ++corner)
                    {
                        float fx0, fx1, fy0, fy1;

                        if(corner < 2)
                        {
                            fx0 = 0f;
                            fx1 = 0.5f;
                        }
                        else
                        {
                            fx0 = 0.5f;
                            fx1 = 1f;
                        }

                        if(corner == 0 || corner == 3)
                        {
                            fy0 = 0f;
                            fy1 = 0.5f;
                        }
                        else
                        {
                            fy0 = 0.5f;
                            fy1 = 1f;
                        }

                        s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                        s_Xy[1].x = s_Xy[0].x;
                        s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                        s_Xy[3].x = s_Xy[2].x;

                        s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                        s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                        s_Xy[2].y = s_Xy[1].y;
                        s_Xy[3].y = s_Xy[0].y;

                        s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                        s_Uv[1].x = s_Uv[0].x;
                        s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                        s_Uv[3].x = s_Uv[2].x;

                        s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                        s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                        s_Uv[2].y = s_Uv[1].y;
                        s_Uv[3].y = s_Uv[0].y;

                        float val = fillClockwise ?
                            fillAmount * 4f - ((corner + fillOrigin) % 4) :
                            fillAmount * 4f - (3 - ((corner + fillOrigin) % 4));

                        if(RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), fillClockwise, ((corner + 2) % 4)))
                            AddQuad(toFill, s_Xy, color, s_Uv);
                    }
                }
            }
            else
            {
                AddQuad(toFill, s_Xy, color, s_Uv);
            }
        }
    }

    /// <summary>
    /// Adjust the specified quad, making it be radially filled instead.
    /// </summary>

    static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
    {
        // Nothing to fill
        if(fill < 0.001f) return false;

        // Even corners invert the fill direction
        if((corner & 1) == 1) invert = !invert;

        // Nothing to adjust
        if(!invert && fill > 0.999f) return true;

        // Convert 0-1 value into 0 to 90 degrees angle in radians
        float angle = Mathf.Clamp01(fill);
        if(invert) angle = 1f - angle;
        angle *= 90f * Mathf.Deg2Rad;

        // Calculate the effective X and Y factors
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        RadialCut(xy, cos, sin, invert, corner);
        RadialCut(uv, cos, sin, invert, corner);
        return true;
    }

    /// <summary>
    /// Adjust the specified quad, making it be radially filled instead.
    /// </summary>

    static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
    {
        int i0 = corner;
        int i1 = ((corner + 1) % 4);
        int i2 = ((corner + 2) % 4);
        int i3 = ((corner + 3) % 4);

        if((corner & 1) == 1)
        {
            if(sin > cos)
            {
                cos /= sin;
                sin = 1f;

                if(invert)
                {
                    xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                    xy[i2].x = xy[i1].x;
                }
            }
            else if(cos > sin)
            {
                sin /= cos;
                cos = 1f;

                if(!invert)
                {
                    xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                    xy[i3].y = xy[i2].y;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }

            if(!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
            else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
        }
        else
        {
            if(cos > sin)
            {
                sin /= cos;
                cos = 1f;

                if(!invert)
                {
                    xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                    xy[i2].y = xy[i1].y;
                }
            }
            else if(sin > cos)
            {
                cos /= sin;
                sin = 1f;

                if(invert)
                {
                    xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                    xy[i3].x = xy[i2].x;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }

            if(invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
            else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
        }
    }
}
