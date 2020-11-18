//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;
//using Spine.Unity;
//using UnityEditor;
//using UnityEngine;

//public class ImporterSpine
//{
//    public class SGenConfig
//    {
//        public string prefabPath;
//        public bool needFade;
//        public string finishHanderName;
//        public float scale = 1;
//        public Vector2 pos = Vector2.zero;
//        public SGenConfig(string prefabPath,  bool needFade=false, string finishHanderName=null, float scale = 1, float posX=0, float posY=0)
//        {
//            this.prefabPath = prefabPath;
//            this.needFade = needFade;
//            this.finishHanderName = finishHanderName;
//            this.scale = scale;
//            this.pos = new Vector2(posX, posY);
//        }
//    }
//    // 新增模型路径映射：资源路径，预制路径
//    private static Dictionary<string, SGenConfig> m_dicGenConfig = new Dictionary<string, SGenConfig>()
//    {
//        { "Assets/Art/spine/hero", new SGenConfig("Assets/Data/spine/npc", true) },
//        { "Assets/Art/spine/princess", new SGenConfig("Assets/Data/spine/npc", true, "PrincessFinishHandler",1,0, -492) },
//        { "Assets/Art/spine/prince", new SGenConfig("Assets/Data/spine/npc", true, "PrincessFinishHandler",1,0, -492) },
//        { "Assets/Art/spine/other", new SGenConfig("Assets/Data/spine/other") },
//        { "Assets/Art/spine/children", new SGenConfig("Assets/Data/spine/children",true,"ChildrenFinishHandler",1) },
//        { "Assets/Art/spine/search", new SGenConfig("Assets/Data/spine/npc",false,null,1,-535,-475 )},
//        { "Assets/Art/spine/chariot", new SGenConfig("Assets/Data/spine/chariot",false,null,1,-535,-475 )},
//        { "Assets/Art/spine/npc", new SGenConfig("Assets/Data/spine/npc") },
//        { "Assets/Art/spine/marriage", new SGenConfig("Assets/Data/spine/marriage", false, "MarriageFinishHandler") },
//        { "Assets/Art/spine/lordhead", new SGenConfig("Assets/Data/spine/lord", false, null, 1, 0, -330) },
//        { "Assets/Art/spine/lordfashion", new SGenConfig("Assets/Data/spine/lord", true, null, 1, 0, -330) },
//        { "Assets/Art/spine/allianceDungeon", new SGenConfig("Assets/Data/spine/allianceDungeon", false, null, 1, 0, -2000) },
//    };

//    static void MatchRectTransformWithBounds(SkeletonGraphic skeletonGraphic)
//    {
//        //var skeletonGraphic = (SkeletonGraphic)command.context;
//        Mesh mesh = skeletonGraphic.GetLastMesh();
//        if(mesh == null)
//        {
//            Debug.Log("Mesh was not previously generated.");
//            return;
//        }

//        if(mesh.vertexCount == 0)
//        {
//            skeletonGraphic.rectTransform.sizeDelta = new Vector2(50f, 50f);
//            skeletonGraphic.rectTransform.pivot = new Vector2(0.5f, 0.5f);
//            return;
//        }

//        mesh.RecalculateBounds();
//        var bounds = mesh.bounds;//todo 批量处理的时候拿到的mesh贼小？？？
//        var size = bounds.size;
//        var center = bounds.center;
//        var p = new Vector2(
//                    0.5f - (center.x / size.x),
//                    0.5f - (center.y / size.y)
//                );

//        skeletonGraphic.rectTransform.sizeDelta = size;
//        skeletonGraphic.rectTransform.localPosition = -center;
//        skeletonGraphic.rectTransform.pivot = p;
//    }

//    private void MarriageFinishHandler(string sourcePath, string destinationPath, string spinePrefabName)
//    {
//        Debug.Log(spinePrefabName);
//        var root = GameObject.Find("UI").transform;
//        string[] prefabs = AssetDatabase.FindAssets(spinePrefabName, new string[] { destinationPath });
//        if (prefabs.Length == 0)
//        {
//            Debug.LogError("【结束回调】没有找到" + spinePrefabName);
//            return;
//        }

//        for (int i = 0; i < prefabs.Length; i++)
//        {
//            string prefabAssetPath = AssetDatabase.GUIDToAssetPath(prefabs[i]);
//            GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
//            GameObject newGO = GameObject.Instantiate(oldPrefab, root);
//            Debug.Log(oldPrefab.name);

//            SkeletonGraphic skeletonGraphic = newGO.transform.Find("spine").GetComponent<SkeletonGraphic>();
//            // skeletonGraphic.rectTransform.SetAnchoredPosition(-160, -225);
//            // skeletonGraphic.startingAnimation = spinePrefab.name;

//            MatchRectTransformWithBounds(skeletonGraphic);
//            // ExportPanelHierarchy.ExportNested(newGO);
//            PrefabUtility.ReplacePrefab(newGO, oldPrefab);
//            MonoBehaviour.DestroyImmediate(newGO);
//        }
//    }

//    private void ChildrenFinishHandler(string sourcePath, string destinationPath, string spinePrefabName)
//    {
//        Debug.Log(spinePrefabName);

//        string[] prefabs = AssetDatabase.FindAssets(spinePrefabName, new string[] { destinationPath });
//        if (prefabs.Length == 0)
//        {
//            Debug.LogError("【子嗣结束回调】没有找到" + spinePrefabName);
//            return;
//        }
//        var root = GameObject.Find("UI").transform;

//        for (int i = 0; i < prefabs.Length; i++)
//        {
//            string prefabAssetPath = AssetDatabase.GUIDToAssetPath(prefabs[i]);
//            GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
//            GameObject newGO = GameObject.Instantiate(oldPrefab, root);
//            Debug.Log(oldPrefab.name);
       
    
//            SkeletonGraphic skeletonGraphic = newGO.transform.Find("spine").GetComponent<SkeletonGraphic>();
//            skeletonGraphic.rectTransform.SetAnchoredPosition(-160,-225);
//            skeletonGraphic.startingAnimation = spinePrefabName;

//            Transform spineNode = newGO.transform.Find("spine");
//            MatchRectTransformWithBounds(spineNode.GetComponent<SkeletonGraphic>());
//            // ExportPanelHierarchy.ExportNested(newGO);
//            PrefabUtility.ReplacePrefab(newGO, oldPrefab);
//            MonoBehaviour.DestroyImmediate(newGO);
//        }
//    }

//    private void PrincessFinishHandler(string sourcePath, string destinationPath, string spinePrefabName)
//    {
//        Debug.Log(spinePrefabName);

//        var root = GameObject.Find("UI").transform;

//        string[] prefabs = AssetDatabase.FindAssets(spinePrefabName, new string[] { destinationPath });
//        if (prefabs.Length == 0)
//        {
//            Debug.LogError("【妃子结束回调】没有找到" + spinePrefabName);
//            return;
//        }

//        for (int i = 0; i < prefabs.Length; i++)
//        {
//            string prefabAssetPath = AssetDatabase.GUIDToAssetPath(prefabs[i]);
//            GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
//            GameObject newGO = GameObject.Instantiate(oldPrefab, root);
//            Debug.Log(oldPrefab.name);
            
//            if (oldPrefab.transform.childCount > 1)
//            {
//                // 已经处理过的预制
//                //Transform interactiveAreaNode = newGO.transform.Find("InteractiveArea");
//                //interactiveAreaNode.GetComponent<ButtonEx>().interactable = true;
//                ////trans.SetActive(false);
//                //Transform spineNode = newGO.transform.Find("spine");
//                //spineNode.GetComponent<SkeletonGraphic>().raycastTarget = false;
//            }
//            else
//            {
//                // 第一次创建
//                Transform spineNode = newGO.transform.Find("spine");
//                SkeletonGraphic skeletonGraphic = spineNode.GetComponent<SkeletonGraphic>();
//                skeletonGraphic.raycastTarget = false;
//                GameObject topPosNode = new GameObject("TopPos");
//                GameObject interactiveNode = new GameObject("InteractiveArea");
//                topPosNode.transform.SetParent(newGO.transform);
//                interactiveNode.transform.SetParent(newGO.transform);

//                //topPosNode
//                RectTransform topPosTrans = topPosNode.AddComponent<RectTransform>();
//                topPosTrans.localPosition = Vector3.zero;
//                topPosTrans.anchoredPosition = new Vector2(-200, 520);
//                topPosTrans.localScale = Vector3.one;
//                topPosNode.AddComponent<UIExportItem>();

//                //interactiveAreaNode
//                RectTransform interactiveTrans = interactiveNode.AddComponent<RectTransform>();
//                interactiveNode.AddComponent<EmptyRaycast>();
//                ExButton btn = interactiveNode.AddComponent<ExButton>();
//                btn.interactable = true;
//                interactiveNode.AddComponent<UIExportItem>();
//                interactiveNode.SetActive(false);//默认不可点击，只有妃子详情界面才有点击需求，自己去启用
//                interactiveTrans.localPosition = Vector3.zero;
//                interactiveTrans.localScale = Vector3.one;
//                interactiveTrans.sizeDelta = new Vector2(500,1000);

//                ExportPanelHierarchy.ExportNested(newGO);

//                MatchRectTransformWithBounds(spineNode.GetComponent<SkeletonGraphic>());
//                spineNode.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -492);

//                PrefabUtility.ReplacePrefab(newGO, oldPrefab);
//            }
//            MonoBehaviour.DestroyImmediate(newGO);
//        }
//    }

//    public static void InvokeEndHandler(string sourcePath, SGenConfig config, string spinePrefabName)
//    {
//        string destinationPath = config.prefabPath;
//        // 结束回调
//        var finishHandlerName = config.finishHanderName;
//        if (!string.IsNullOrEmpty(finishHandlerName))
//        {
//            System.Type type = System.Type.GetType("ImporterSpine");            // 通过类名获取同名类
//            System.Object obj = System.Activator.CreateInstance(type);          // 创建实例
//            MethodInfo method = type.GetMethod(finishHandlerName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);      // 获取方法信息
//            if (method != null)
//            {
//                method.Invoke(obj, new object[] { sourcePath, destinationPath, spinePrefabName });
//            }
//            else
//            {
//                Debug.LogError("没有找到结束回调！" + finishHandlerName);
//            }
//        }
//    }

//    public static void ImportSelectedFolder(Object obj)
//    {
//        string strPath = AssetDatabase.GetAssetPath(obj);
//        System.Type typeObj = obj.GetType ();
//        if (typeObj != typeof(DefaultAsset))
//        {
//            Debug.Log("选中的不是一个目录");
//        }

//        int iPos = strPath.LastIndexOf("/");
//        string strKeyPath = iPos>0?strPath.Substring(0, iPos):"";

//        SGenConfig config;
//        if (m_dicGenConfig.TryGetValue(strKeyPath, out config))
//        {
//            // 选中整个父文件夹
//            Debug.LogFormat("处理spine, artPath:{0}, dataPath:{1}, needFade:{2}", strPath, config.prefabPath, config.needFade);
//            ImportOneBusinessFolder(strPath, config);
//        }
//    }

//    // 导入一个业务的文件夹
//    public static void ImportOneBusinessFolder(string sourcePath,  SGenConfig config)
//    {
//        string spinePrefabName = ImportOneSpine(sourcePath, config);
//        InvokeEndHandler(sourcePath, config, spinePrefabName);

//        EditorUtility.ClearProgressBar();
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//    }

//    // 导入一个spine
//    public static string ImportOneSpine(string path, SGenConfig config)
//    {
//        string name = path.Substring(path.LastIndexOf("/") + 1);

//        string pathMaterial = string.Format("{0}/{1}_Material.mat", path, name);
//        string pathSkeletonDataAsset = string.Format("{0}/{1}_SkeletonData.asset", path, name);

//        Material mat = AssetDatabase.LoadAssetAtPath<Material>(pathMaterial);
//        if(mat==null)
//        {
//            Debug.LogErrorFormat("name:{0},材质：{1} 不存在", name, pathMaterial);
//            return null;
//        }
//        SkeletonDataAsset skelData = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(pathSkeletonDataAsset);
//        if(skelData==null)
//        {
//            Debug.LogErrorFormat("name:{0},SkeletonDataAsset：{1} 不存在", name, pathSkeletonDataAsset);
//            return null;
//        }

//        string prefabPath = string.Format("{0}/{1}.prefab", config.prefabPath, name);
//        GameObject actorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
//        bool isNew = false;
//        if(actorPrefab==null)
//        {
//            actorPrefab = new GameObject(name, typeof(RectTransform));
//            RectTransform transPrefab = actorPrefab.transform as RectTransform;
//            transPrefab.anchoredPosition = Vector2.zero;
//            transPrefab.sizeDelta = Vector2.zero;
//            GameObject goSpine = new GameObject("spine", typeof(RectTransform), typeof(CanvasRenderer), typeof(SkeletonGraphic));
//            RectTransform transSkel = goSpine.transform as RectTransform;
//            transSkel.SetParent(transPrefab);
//            transSkel.localScale = new Vector3(config.scale, config.scale, config.scale);
//            transSkel.anchoredPosition = config.pos;
//            goSpine.AddComponent<UIExportItem>();
//            isNew = true;
//        }
//        ExportPanelHierarchy.ExportNested(actorPrefab);

//        SkeletonGraphic sg = actorPrefab.GetComponentInChildren<SkeletonGraphic>();
//        (sg.transform as RectTransform).anchoredPosition = config.pos;
        
//        // 材质球设置，渐隐开关设置
//        sg.MeshGenerator.settings.bottomShade = config.needFade;
//        sg.MeshGenerator.settings.useClipping = false;
//        sg.raycastTarget = false;
//        // mat.SetFloat("_Range", 0.06f);
//        // mat.SetFloat("_Mix", 1.3f);
//        // EditorUtility.SetDirty (mat);

//        sg.material = mat;
//        sg.skeletonDataAsset = skelData;
//        sg.startingLoop = true;
//        sg.startingAnimation = skelData.GetSkeletonData(false).Animations.Items[0].Name;

//        string actorPrefabName = actorPrefab.name;

//        if(isNew)
//        {
//            GameObject go = actorPrefab;
//            actorPrefab = PrefabUtility.CreatePrefab (prefabPath, go);
//            PrefabUtility.ConnectGameObjectToPrefab (go, actorPrefab);
//            MonoBehaviour.DestroyImmediate(go);
//        }
//        else
//        {
//            EditorUtility.SetDirty (actorPrefab);
//        }

//        return actorPrefabName;
//    }
//}
