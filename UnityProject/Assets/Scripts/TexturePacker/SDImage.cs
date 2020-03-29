
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

public class SDImage : Image
{
    [SerializeField]
    private string m_SpriteName;
    [SerializeField]
    private bool m_UseSpritePacker;

    private AtlasReference m_AtlasRef;
    private static Dictionary<string, Vector4> m_spritePaddings;

    protected SDImage()
    {
        useLegacyMeshGeneration = false;
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


    //X = left, Y = bottom, Z = right, W = top.
    protected Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
    {
        Sprite overrideSprite = this.overrideSprite;
        var padding = GetPadding();
        var size = overrideSprite == null ? Vector2.zero : new Vector2(overrideSprite.rect.width, overrideSprite.rect.height);
        Rect r = GetPixelAdjustedRect();
        int spriteW = Mathf.RoundToInt(size.x);
        int spriteH = Mathf.RoundToInt(size.y);
        float width = spriteW + padding.z + padding.x;
        float height = spriteH + padding.w + padding.y;

        var v = new Vector4(
                padding.x / width,
                padding.y / height,
                (width - padding.z) / width,
                (height - padding.w) / height);

        if(shouldPreserveAspect && size.sqrMagnitude > 0.0f)
        {
            var spriteRatio = size.x / size.y;
            var rectRatio = r.width / r.height;

            if(spriteRatio > rectRatio)
            {
                var oldHeight = r.height;
                r.height = r.width * (1.0f / spriteRatio);
                r.y += (oldHeight - r.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = r.width;
                r.width = r.height * spriteRatio;
                r.x += (oldWidth - r.width) * rectTransform.pivot.x;
            }
        }

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

        GenerateSimpleSprite(toFill, preserveAspect);
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
        Vector4 v = GetDrawingDimensions(lPreserveAspect);

        vh.AddVert(new Vector3(v.x, v.y), color, new Vector2(uv.x, uv.y));
        vh.AddVert(new Vector3(v.x, v.w), color, new Vector2(uv.x, uv.w));
        vh.AddVert(new Vector3(v.z, v.w), color, new Vector2(uv.z, uv.w));
        vh.AddVert(new Vector3(v.z, v.y), color, new Vector2(uv.z, uv.y));

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
