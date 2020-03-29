
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

class AtlasEditorHelper
{
    public static void BuildMakeAtlas(string name, string pathSrc, string pathDst, bool trim = false)
    {
        MakeAtlas(name, pathSrc, pathDst, trim);
    }

    // 拆分通道
    public static void BuildSplitChannel(string name, string pathSrc, string pathDst, bool isCompress = true, bool isSplitChannel = true)
    {
        FileHelper.CreateDirectory(pathDst);
        SplitChannel(name, pathSrc, pathDst, isCompress, isSplitChannel);
    }

    // 生成材质球
    public static void BuildCreateMaterial(string name, string pathDst, bool isSplitChannel = true, bool isUI = true)
    {
        pathDst = string.Format("{0}/{1}", pathDst, name);
        FileHelper.CreateDirectory(pathDst);
        CreateMaterial(name, pathDst, isUI, isSplitChannel);
    }

    // 导入小图
    public static void BuildImportSprite(string name, string pathSrc, string pathTP, string pathDst, bool isCompress = true)
    {
        pathDst = string.Format("{0}/{1}", pathDst, name);
        FileHelper.CreateDirectory(pathDst);
        ImportSprite(name, pathSrc, pathTP, pathDst, isCompress);
    }

    // 生成预制
    public static void BuildCreateSpritePrefab(string name, string pathTP, string pathDst)
    {
        pathDst = string.Format("{0}/{1}", pathDst, name);
        FileHelper.CreateDirectory(pathDst);
        CreateSpritePrefab(name, pathDst, pathTP);
    }

    private static void SplitChannel(string name, string pathSrc, string pathDst, bool isCompress, bool isSplitChannel)
    {
        string path = string.Format("{0}/{1}.png", pathSrc, name);
        string pathColor = string.Format("{0}/{1}_c.png", pathDst, name);
        string pathAlpha = string.Format("{0}/{1}_a.png", pathDst, name);
        string pathTpSheet = string.Format("{0}/{1}_c.tpsheet", pathDst, name);
        if(File.Exists(pathTpSheet))
        {
            File.Delete(pathTpSheet);
        }

        if(!Directory.Exists(pathDst))
        {
            Directory.CreateDirectory(pathDst);
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        TextureImporter importTex = (TextureImporter)AssetImporter.GetAtPath(path);
        if(importTex == null)
        {
            Debug.LogError(path + "is null");
            return;
        }
        bool isChangeImporter = false;
        if(!importTex.isReadable)
        {
            importTex.isReadable = true;
            isChangeImporter = true;
        }
        if(importTex.alphaSource != TextureImporterAlphaSource.FromInput)
        {
            importTex.alphaSource = TextureImporterAlphaSource.FromInput;
            isChangeImporter = true;
        }
        if(isChangeImporter)
        {
            importTex.SaveAndReimport();
        }
        if(isSplitChannel)
        {
            Texture2D textureColor = new Texture2D(texture.width, texture.height);
            Texture2D textureAlpha = new Texture2D(texture.width, texture.height);

            for(int i = 0; i < texture.width; ++i)
            {
                for(int j = 0; j < texture.height; ++j)
                {
                    Color color = texture.GetPixel(i, j);
                    textureAlpha.SetPixel(i, j, new Color(color.a, color.a, color.a));
                    textureColor.SetPixel(i, j, new Color(color.r, color.g, color.b));
                }
            }

            File.WriteAllBytes(pathColor, textureColor.EncodeToPNG());
            File.WriteAllBytes(pathAlpha, textureAlpha.EncodeToPNG());
        }
        else
        {
            Texture2D textureColor = new Texture2D(texture.width, texture.height);

            for(int i = 0; i < texture.width; ++i)
            {
                for(int j = 0; j < texture.height; ++j)
                {
                    Color color = texture.GetPixel(i, j);
                    textureColor.SetPixel(i, j, new Color(color.r, color.g, color.b, color.a));
                }
            }
            File.WriteAllBytes(pathColor, textureColor.EncodeToPNG());
            if(File.Exists(pathAlpha))
            {
                File.Delete(pathAlpha);
            }
        }

        AssetDatabase.Refresh();

        if(isSplitChannel)
        {
            TextureImporter importAlpha = (TextureImporter)AssetImporter.GetAtPath(pathAlpha);
            importAlpha.textureType = TextureImporterType.Default;
            importAlpha.mipmapEnabled = false;
            importAlpha.isReadable = false;
            importAlpha.alphaSource = TextureImporterAlphaSource.None;
            importAlpha.textureCompression = isCompress ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
            int scale = 1;//AtlasConfig.GetAtlasAphlaScale(name);
            importAlpha.maxTextureSize = texture.width / scale;
            importAlpha.filterMode = FilterMode.Bilinear;
            importAlpha.SaveAndReimport();
        }
        else
        {
            TextureImporter importColor = (TextureImporter)AssetImporter.GetAtPath(pathColor);
            importColor.textureType = TextureImporterType.Default;
            importColor.mipmapEnabled = false;
            importColor.isReadable = false;
            importColor.alphaSource = TextureImporterAlphaSource.FromInput;
            importColor.textureCompression = isCompress ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
            importColor.maxTextureSize = texture.width;
            importColor.filterMode = FilterMode.Bilinear;
            importColor.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    private static void MakeAtlas(string name, string pathSrc, string pathDst, bool trim)
    {
        string fullPathDst = string.Format("{0}/{1}", pathDst, name);

        if(!Directory.Exists(pathSrc))
        {
            Debug.LogError(string.Format("No Found Atlas Path: {0}", pathSrc));
            return;
        }

        var TexturePackerBatPath = GetTexturePackerBatPath("maker");
        if(string.IsNullOrEmpty(TexturePackerBatPath))
        {
            return;
        }

        int padding = 0;
        string args = string.Format("{0} {1} {2} {3}", pathSrc, fullPathDst, trim ? 1 : 0, padding);
        ShellHelper.ProcessCommand(TexturePackerBatPath, args);
        // if (!string.IsNullOrEmpty(error))
        // {
        //     Debug.LogError(string.Format("Make Atlas Error. name:{0}, msg:{1}", name, error));
        // }
        AssetDatabase.Refresh();

        //string pathTxt = string.Format("{0}.txt", fullPathDst);
        //if(!File.Exists(pathTxt))
        //{
        //    Debug.LogError(string.Format("TexturePacker Config No Exist: {0}", name));
        //    return;
        //}
        string path = string.Format("{0}.png", fullPathDst);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.isReadable = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.Refresh();
    }

    static void CreateMaterial(string name, string pathDst, bool isUI, bool isSplitChannel)
    {
        string pathColor = string.Format("{0}/{1}_c.png", pathDst, name);
        string pathAlpha = string.Format("{0}/{1}_a.png", pathDst, name);
        string pathMaterial = string.Format("{0}/{1}_mat.mat", pathDst, name);

        Texture2D textureColor = AssetDatabase.LoadAssetAtPath<Texture2D>(pathColor);
        Texture2D textureAlpha = AssetDatabase.LoadAssetAtPath<Texture2D>(pathAlpha);

        string shaderName;
        if(isUI)
        {
            if(isSplitChannel)
            {
                shaderName = "UI/Default(RGB+A)";
            }
            else
            {
                shaderName = "UI/Default(ARGB)";
            }
        }
        else
        {
            shaderName = "Custom/Transparent/Diffuse(RGB+A)_Lambert";
        }
        Shader shader = Shader.Find(shaderName);
        Material material = null;
        if(File.Exists(pathMaterial))
        {
            material = AssetDatabase.LoadAssetAtPath<Material>(pathMaterial);
            material.shader = shader;
        }
        else
        {
            material = new Material(shader);
        }
        //todo: 真彩的时候这里是错的要改...
        material.SetTexture("_MainTex", textureColor);
        material.SetTexture("_AlphaTex", textureAlpha);

        if(File.Exists(pathMaterial))
        {
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.CreateAsset(material, pathMaterial);
        }
    }

    private static void ImportSprite(string name, string pathSrc, string pathTP, string pathDst, bool isCompress)
    {
        string pathTxt = string.Format("{0}/{1}.txt", pathTP, name);

        if(!File.Exists(pathTxt))
        {
            Debug.LogError(string.Format("TexturePacker Config No Exist: {0}", pathTxt));
            return;
        }
        string text = FileHelper.ReadTextFromFile(pathTxt);

        string pathTexture = string.Format("{0}/{1}_c.png", pathDst, name);
        TextureImporter importer = AssetImporter.GetAtPath(pathTexture) as TextureImporter;
        if(importer == null)
        {
            Debug.LogError(pathTexture + ":is null");
            return;
        }
        List<SpriteMetaData> sprites = TexturePacker.ProcessToSprites(text);
        RewriteInfo(sprites, pathSrc);
        importer.spritesheet = sprites.ToArray();
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.sRGBTexture = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.spritePackingTag = null;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.maxTextureSize = 2048;
        if(isCompress)
        {
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.textureCompression = TextureImporterCompression.Compressed;
        }
        else
        {
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
        importer.SaveAndReimport();
        AssetDatabase.ImportAsset(pathTexture, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.Refresh();
    }

    private static void RewriteInfo(List<SpriteMetaData> sprites, string pathSrc)
    {
        string[] strDirectorys = null;
        for(int i = 0; i < sprites.Count; ++i)
        {
            SpriteMetaData data = sprites[i];
            string pathTexture = string.Format("{0}/{1}.png", pathSrc, data.name);
            TextureImporter importer = AssetImporter.GetAtPath(pathTexture) as TextureImporter;
            if(importer == null)
            {
                // 取不到，可能是有子目录，从子目录再找下看看
                if(strDirectorys == null)
                    strDirectorys = GetTopDirectory(pathSrc);

                if(strDirectorys == null)
                    continue;

                for(int j = 0; j < strDirectorys.Length; ++j)
                {
                    pathTexture = string.Format("{0}/{1}/{2}.png", pathSrc, strDirectorys[j], data.name);
                    importer = AssetImporter.GetAtPath(pathTexture) as TextureImporter;
                    if(importer)
                    {
                        break;
                    }
                }

                if(importer == null)
                    continue;
            }

            data.border = importer.spriteBorder;
            // if (data.alignment != 9)
            // {
            //     data.pivot = importer.spritePivot;
            // }
            data.pivot = importer.spritePivot;
            data.alignment = PivotToAlignment(importer.spritePivot);
            sprites[i] = data;
        }
    }

    static void CreateSpritePrefab(string name, string path, string pathTP)
    {
        string pathPrefab = string.Format("{0}/{1}.prefab", path, name);
        string pathColor = string.Format("{0}/{1}_c.png", path, name);
        string pathMaterial = string.Format("{0}/{1}_mat.mat", path, name);

        if(!File.Exists(pathColor))
        {
            Debug.LogError(string.Format("File No Found: {0}", pathColor));
            return;
        }

        if(!File.Exists(pathMaterial))
        {
            Debug.LogError(string.Format("File No Found: {0}", pathMaterial));
            return;
        }

        string pathTxt = string.Format("{0}/{1}.txt", pathTP, name);
        string text = FileHelper.ReadTextFromFile(pathTxt);
        List<TexturePacker.PackedFrame> frames = TexturePacker.ProcessToFrames(text);

        SpriteMetaData meta;
        Dictionary<string, SpriteMetaData> metas = new Dictionary<string, SpriteMetaData>();

        TextureImporter importer = AssetImporter.GetAtPath(pathColor) as TextureImporter;

        for(int i = 0; i < importer.spritesheet.Length; ++i)
        {
            meta = importer.spritesheet[i];
            if(metas.ContainsKey(meta.name))
                Debug.LogError("key is already exist the dictionary:  " + meta.name);
            metas.Add(meta.name, meta);
        }
        //AtlasData data = new AtlasData();
        //data.material = AssetDatabase.LoadAssetAtPath<Material>(pathMaterial);
        //data.sprites = new List<AtlasData.SpriteData>();
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(pathMaterial);

        List<Sprite> sprites = new List<Sprite>();
        List<Vector4> rts = new List<Vector4>();
        List<bool> trims = new List<bool>();

        Sprite sprite = null;
        UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(pathColor);
        for(int i = 0; i < objects.Length; ++i)
        {
            sprite = objects[i] as Sprite;
            if(sprite == null) continue;

            bool trim = false;
            if(metas.TryGetValue(sprite.name, out meta))
            {
                trim = meta.alignment == 9;
            }

            Vector4 rt = Vector4.zero;
            foreach(TexturePacker.PackedFrame frame in frames)
            {
                string spritename = frame.name.Replace(".png", "");
                if(string.Equals(spritename, sprite.name))
                {
                    float left = frame.spriteSourceSize.x;
                    float top = frame.spriteSourceSize.y;
                    float right = frame.sourceSize.x - frame.spriteSourceSize.x - frame.spriteSourceSize.width;
                    float bottom = frame.sourceSize.y - frame.spriteSourceSize.y - frame.spriteSourceSize.height;

                    rt.x = left;
                    rt.y = bottom;
                    rt.z = right;
                    rt.w = top;
                    break;
                }
            }
            sprites.Add(sprite);
            rts.Add(rt);
            trims.Add(trim);
            //data.sprites.Add(new AtlasData.SpriteData(sprite, rt, trim));
        }

        GameObject prefab = null;
        GameObject go = null;
        if(File.Exists(pathPrefab))
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathPrefab);
            go = GameObject.Instantiate(prefab);
        }
        else
        {
            go = new GameObject();
        }

        go.name = name;

        AtlasHierarchy hierarchy = go.GetComponent<AtlasHierarchy>() ?? go.AddComponent<AtlasHierarchy>();

        hierarchy.SetAtlasData(sprites, mat);
        //hierarchy.atlas = data;

        if(prefab != null)
        {
            PrefabUtility.ReplacePrefab(go, prefab);
        }
        else
        {
            PrefabUtility.CreatePrefab(pathPrefab, go);
        }
        GameObject.DestroyImmediate(go);

        if(path.Contains(PathDef.UI_PATH + "/atlas_tp"))
        {
            BuildOnePaddingData(name, rts, trims, false, hierarchy);
        }
    }

    public static void BuildOnePaddingData(string name, List<Vector4> paddings, List<bool> trims, bool isPoly, AtlasHierarchy hierarchy)
    {
        bool needCreate = false;
        string paddingPath = PathDef.UI_ASSETS_PATH + "/atlas_tp_padding.asset";

        PaddingData data = AssetDatabase.LoadAssetAtPath<PaddingData>(paddingPath);
        if(data == null)
        {
            data = ScriptableObject.CreateInstance<PaddingData>();
            needCreate = true;
        }

        data.RemoveAtlasItem(name);

        List<string> spriteNames = hierarchy.GetSpriteDataName();

        data.AddAtlasEx(name, isPoly, spriteNames, paddings, trims);
        CheckAtlas(data.atlas);
        if(needCreate)
        {
            AssetDatabase.CreateAsset(data, paddingPath);
        }
        else
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }
        AssetDatabase.Refresh();
    }

    static void CheckAtlas(List<PaddingData.AtlasInfo> atlas)
    {
        for(int i = atlas.Count - 1; i >= 0; i--)
        {
            string name = atlas[i].name;
            var pathAtlasInfo = string.Format("{0}/{1}/{2}.prefab", PathDef.AtlasTPPath, name, name);
            var asset = AssetDatabase.LoadAssetAtPath<AtlasHierarchy>(pathAtlasInfo);
            if(asset == null)
            {
                atlas.Remove(atlas[i]);
            }
        }
    }
    ////////////////////////////////////helper//////////////////////////////////////

    public static int PivotToAlignment(Vector2 pos)
    {
        SpriteAlignment align = SpriteAlignment.Custom;
        if(pos.x == 0)
        {
            if(pos.y == 0)
            {
                align = SpriteAlignment.BottomLeft;
            }
            if(pos.y == 0.5f)
            {
                align = SpriteAlignment.LeftCenter;
            }
            if(pos.y == 1)
            {
                align = SpriteAlignment.TopLeft;
            }
        }
        else if(pos.x == 0.5f)
        {
            if(pos.y == 0)
            {
                align = SpriteAlignment.BottomCenter;
            }
            if(pos.y == 0.5f)
            {
                align = SpriteAlignment.Center;
            }
            if(pos.y == 1)
            {
                align = SpriteAlignment.TopCenter;
            }
        }
        else if(pos.x == 1)
        {
            if(pos.y == 0)
            {
                align = SpriteAlignment.BottomRight;
            }
            if(pos.y == 0.5f)
            {
                align = SpriteAlignment.RightCenter;
            }
            if(pos.y == 1)
            {
                align = SpriteAlignment.TopRight;
            }
        }
        return (int)align;   //Custom
    }

    // 取子目录
    private static string[] GetTopDirectory(string pathSrc)
    {
        DirectoryInfo direction = new DirectoryInfo(pathSrc);
        FileInfo[] files = direction.GetFiles("*", SearchOption.TopDirectoryOnly);
        if(files.Length <= 0)
            return null;

        string[] strDirectorys = new String[files.Length];
        for(int i = 0; i < files.Length; ++i)
        {
            strDirectorys.SetValue(files[i].Name.Replace(".meta", ""), i);
        }

        return strDirectorys;
    }

    public static string GetFullPath(string relativelyPath)
    {
        string[] strArray = Application.dataPath.Split('/');
        StringBuilder stringBuilder = new StringBuilder();
        for(int index = 0; index < strArray.Length; ++index)
        {
            if(index < strArray.Length - 1)
                stringBuilder.Append(strArray[index] + "/");
        }
        stringBuilder.Append(relativelyPath);
        return stringBuilder.ToString();
    }

    static string GetTexturePackerBatPath(string name)
    {
        string stuffix;
        switch(Application.platform)
        {
            case RuntimePlatform.OSXEditor:
                stuffix = ".sh";
                break;
            case RuntimePlatform.WindowsEditor:
                stuffix = ".bat";
                break;
            default:
                Debug.LogError("错误平台");
                return "";
        }

        string fullpath = GetFullPath("Tools/TexturePacker/" + name + stuffix);

        switch(Application.platform)
        {
            case RuntimePlatform.OSXEditor:
                ShellHelper.ProcessCommand("chmod", string.Format("+x {0}", fullpath));
                break;
        }

        return fullpath;
    }
}
