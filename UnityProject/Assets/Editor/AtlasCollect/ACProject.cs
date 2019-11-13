using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Linq;
using EditorWindowCommon;
using Object = UnityEngine.Object;

namespace AtlasCollect
{
    public class ACProject
    {
        public static string SceneCommonFolderName = "common";
        public static string SceneBackgounrdFolderName = "gamebackgound";
        public static string ModuleCommonFolderName = "common";
        public static string SpriteNeedDeleteFolderName = "SpriteNeedDeleteFolder";

        public enum ReferenceSpriteType
        {
            ReferenceFolderRefSprite,
            UnReferenceFolderRefSprite
        }

        /// <summary>
        /// 所有的UI场景列表（根据目录划分获得）
        /// </summary>
        private List<ACScene> _allAcScenes = new List<ACScene>();

        /// <summary>
        /// 所有引用目标目录里不重复的被Prefab引用的Sprite
        /// </summary>
        private Dictionary<Sprite, ACSprite> _allReferenceFolderSpritesDic = new Dictionary<Sprite, ACSprite>();

        /// <summary>
        /// 所有非引用目标目录里不重复的被Prefab引用的Sprite
        /// </summary>
        private Dictionary<Sprite, ACSprite> _allUnReferenceFolderSpritesDic = new Dictionary<Sprite, ACSprite>();

        /// <summary>
        /// 所有的Prefab
        /// </summary>
        private List<ACPrefab> _allAcPrefabs = new List<ACPrefab>();

        /// <summary>
        /// 引用目标目录里所有未被使用的Spirte
        /// </summary>
        private List<string> _allReferenceFolderUnUseSprites = new List<string>();

        #region Draw相关参数
        private ACScene _curSelectScene;

        private ACSprite _curSelectSprite;
        private List<ACSprite> _referenceFolderRefSprites = new List<ACSprite>();
        private List<ACSprite> _unReferenceFolderRefSprites = new List<ACSprite>();

        //private Rect _selectSpritesScrollArea;
        private string _searchString = string.Empty;

        private List<bool> _toggleStates = new List<bool>();
        private Vector2 _scrollPos;

        private string[] _gridSelects = { "关联的场景", "关联目录的引用图片", "非关联目录的引用图片" };
        private int _gridSelectIndex = 0;
        #endregion

        private string _uiSpriteTargetFolder;
        private AtlasCollectRule _atlasCollectRule;

        #region 属性
        public int PrefabCount { get { return _allAcPrefabs.Count; } }
        public int AllSpriteCount { get; private set; }
        public int RefSpriteCount { get { return _allReferenceFolderSpritesDic.Count; } }
        public int UnRefSpriteCount { get { return _allReferenceFolderUnUseSprites.Count; } }
        #endregion

        public ACProject(string prefabTargetFolder, string uiSpriteTargetFolder, AtlasCollectRule atlasCollectRule)
        {
            _uiSpriteTargetFolder = uiSpriteTargetFolder.ToLower();
            _atlasCollectRule = atlasCollectRule;

            InitAllAcScenes(prefabTargetFolder);
            CollectAllSprites();

            _referenceFolderRefSprites = new List<ACSprite>(_allReferenceFolderSpritesDic.Values);
            _unReferenceFolderRefSprites = new List<ACSprite>(_allUnReferenceFolderSpritesDic.Values);
        }

        #region 初始化
        /// <summary>
        /// 根据目标路径，初始化配置数据
        /// </summary>
        /// <param name="prefabTargetFolder">
        /// UI Prefab所在的目录，结构必须为2层结构
        /// e.g: Login/UI_Login, Battle/Battle/UI_Battle 场景/.prefab 或者 场景/模块/.prefab
        /// </param>
        private void InitAllAcScenes(string prefabTargetFolder)
        {
            var sceneFolders = Directory.GetDirectories(prefabTargetFolder);
            for (int i = 0; i < sceneFolders.Length; i ++)
            {
                ACScene scene;
                if (ACScene.CreateAcScene(sceneFolders[i], out scene)) {
                    _allAcScenes.Add(scene);
                }
            }
        }
        #endregion

        #region 收集所有的Sprites
        private void CollectAllSprites()
        {
            // 收集所有预制
            for (int i = 0; i < _allAcScenes.Count; i ++) {
                CollectAllPrefabs(_allAcScenes[i]);
            }

            int prefabCount = _allAcPrefabs.Count;
            for (int i = 0; i < prefabCount; i ++)
            {
                var tidyPrefab = _allAcPrefabs[i];
                // 收集所有预制引用的图片
                CollectPrefabAllSpritesByDependencies(tidyPrefab);

                int curIndex = i + 1;
                float progress = (float)curIndex / prefabCount;
                EditorUtility.DisplayProgressBar("进度", string.Format("正在读取UI_Prefab_{0}/{1} {2}", curIndex, prefabCount, tidyPrefab.Name), progress);
            }

            // 收集所有未使用的图片
            CollectAllUnUseSprites();

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 收集关联目录下所有没被使用的Sprites
        /// </summary>
        private void CollectAllUnUseSprites()
        {
            // 获取所有指定目标目录下的图片文件
            var allSprites = Directory.GetFiles(_uiSpriteTargetFolder, "*.*", SearchOption.AllDirectories)
                .Where(s => !s.EndsWith(".meta")).ToList();
            allSprites = allSprites.Select(s => s.ToLower().Replace("\\", "/")).ToList();

            AllSpriteCount = allSprites.Count;

            // 获取指定目录下所有被引用的图片的完整路径
            var refFolderAllRefSprites = _allReferenceFolderSpritesDic.Values.Select(ac => ac.AssetFile.ToLower()).ToList();

            // 计算未被使用的图片集合
            _allReferenceFolderUnUseSprites = allSprites.Except(refFolderAllRefSprites).ToList();
        }

        private void CollectAllPrefabs(ACScene acScene)
        {
            for (int i = 0; i < acScene.ChildModules.Count; i ++) {
                CollectAllPrefabs(acScene.ChildModules[i]);
            }
        }

        private void CollectAllPrefabs(ACModule acModel)
        {
            if (acModel.ModuleType == ModuleType.Single)
            {
                ACSingleModule acSingleModule = acModel as ACSingleModule;
                if (acSingleModule != null) {
                    _allAcPrefabs.Add(acSingleModule.ChildPrefab);
                }
            }
            else
            {
                ACMultiMoudle acMultiMoudle = acModel as ACMultiMoudle;
                if (acMultiMoudle != null) {
                    _allAcPrefabs.AddRange(acMultiMoudle.ChildPrefabs);
                }
            }
        }

        /// <summary>
        /// 收集该预制所引用的所有图片
        /// </summary>
        /// <param name="acPrefab"></param>
        private void CollectPrefabAllSpritesByDependencies(ACPrefab acPrefab)
        {
            Object uiPrefab = AssetDatabase.LoadAssetAtPath<Object>(acPrefab.AssetFile);
            Object[] allResObjects = EditorUtility.CollectDependencies(new Object[] { uiPrefab });

            for (int i = 0; i < allResObjects.Length; i++)
            {
                Sprite sprite = allResObjects[i] as Sprite;
                if (sprite == null) {
                    continue;
                }

                string spriteFilePath = AssetDatabase.GetAssetPath(sprite.GetInstanceID());
                if (!IsSpriteCanCollect(spriteFilePath))
                {
                    // 更新非关联目录被引用的Sprite个数
                    ACSprite tempSprite;
                    if (!_allUnReferenceFolderSpritesDic.TryGetValue(sprite, out tempSprite))
                    {
                        tempSprite = new ACSprite(sprite, spriteFilePath);
                        _allUnReferenceFolderSpritesDic[sprite] = tempSprite;
                    }

                    tempSprite.UpdateReference(acPrefab);
                    continue;
                }

                // 更新关联目录里被引用到的Sprite个数
                ACSprite acSprite;
                if (!_allReferenceFolderSpritesDic.TryGetValue(sprite, out acSprite))
                {
                    acSprite = new ACSprite(sprite, spriteFilePath);
                    _allReferenceFolderSpritesDic[sprite] = acSprite;
                }

                acPrefab.AddACSprite(acSprite);
                acSprite.UpdateReference(acPrefab);
            }
        }

        /// <summary>
        /// 指定的Sprite是否是AssetBundle包内的
        /// </summary>
        /// <returns></returns>
        public bool IsSpriteCanCollect(string spritePath)
        {
            spritePath = spritePath.ToLower();
            if (spritePath.Contains(_uiSpriteTargetFolder)) {
                return true;
            }

            return false;
        }
        #endregion

        #region AtlasCollect
        /// <summary>
        /// 整理收集的所有Sprites
        /// </summary>
        public void StartAtlasCollect(float minBackgroundWidth, float minBackgroundHeight, string spriteOutFolder)
        {
            MoveAllUseSpritesToFolder(minBackgroundWidth, minBackgroundHeight, spriteOutFolder);
            MoveAllUnUseSpritesToFolder(spriteOutFolder);
            EditorWindowHelper.DeleteEmptyFolders(spriteOutFolder);
        }

        private void MoveAllUseSpritesToFolder(float minBackgroundWidth, float minBackgroundHeight, string spriteOutFolder)
        {
            var allAcSprites = new List<ACSprite>(_allReferenceFolderSpritesDic.Values);

            int allAcSpritesCount = allAcSprites.Count;
            for (int i = 0; i < allAcSpritesCount; i ++)
            {
                ACSprite acSprite = allAcSprites[i];

                var newFolderPath = TryCreateTargetFolder(minBackgroundWidth, minBackgroundHeight, spriteOutFolder, acSprite);

                string newSpritePath = newFolderPath + "/" + acSprite.Name;
                if (string.Compare(acSprite.AssetFile, newSpritePath, StringComparison.OrdinalIgnoreCase) == 0) {
                    continue;
                }

                // 计算进度
                int curIndex = i + 1;
                string log = string.Format("REF_{0}/{1}_正在移动_{2}_到_{3}", curIndex, allAcSpritesCount, acSprite.Name, newFolderPath);
                Debug.Log(string.Format("AtlasCollect_{0}", log));
                {
                    float progress = (float)curIndex / allAcSpritesCount;
                    EditorUtility.DisplayProgressBar("进度", log, progress);
                }

                string result = AssetDatabase.MoveAsset(acSprite.AssetFile, newSpritePath);
                if (result != string.Empty) {
                    Debug.LogError(string.Format("REF_{0}/{1}_{2}", curIndex, allAcSpritesCount, result));
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private void MoveAllUnUseSpritesToFolder(string spriteOutFolder)
        {
            if (_allReferenceFolderUnUseSprites.Count == 0) {
                return;
            }

            string targetFolder = spriteOutFolder + "/" + SpriteNeedDeleteFolderName;

            // 目录不存在就新建
            if (!AssetDatabase.IsValidFolder(targetFolder)) {
                AssetDatabase.CreateFolder(spriteOutFolder, SpriteNeedDeleteFolderName);
            }

            int allUnUseSpritesCount = _allReferenceFolderUnUseSprites.Count;
            for (int i = 0; i < _allReferenceFolderUnUseSprites.Count; i++)
            {
                var assetFile = _allReferenceFolderUnUseSprites[i];
                if(assetFile.EndsWith(".ds_store"))
                {
                    continue;
                }

                string fileName = Path.GetFileName(assetFile);
                string newSpritePath = targetFolder + "/" + fileName;
                if (string.Compare(assetFile, newSpritePath, StringComparison.OrdinalIgnoreCase) == 0) {
                    continue;
                }

                // 计算进度
                int curIndex = i + 1;
                string log = string.Format("UNREF_{0}/{1}_正在移动_{2}_到_{3}", curIndex, allUnUseSpritesCount, fileName, newSpritePath);
                Debug.Log(string.Format("AtlasCollect_{0}", log));
                {
                    float progress = (float)curIndex / allUnUseSpritesCount;
                    EditorUtility.DisplayProgressBar("进度", log, progress);
                }

                string result = AssetDatabase.MoveAsset(assetFile, newSpritePath);
                if (result != string.Empty) {
                    Debug.LogError(string.Format("UNREF_{0}/{1}_{2}", curIndex, allUnUseSpritesCount, result));
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private string TryCreateTargetFolder(float minBackgroundWidth, float minBackgroundHeight, string spriteOutFolder,
            ACSprite acSprite)
        {
            // 计算目标新目录
            string targetFolder = spriteOutFolder + "/";
            if (acSprite.Width >= minBackgroundWidth && acSprite.Height >= minBackgroundHeight)
            {
                targetFolder += SceneBackgounrdFolderName;

                // 目录不存在就新建
                if (!AssetDatabase.IsValidFolder(targetFolder)) {
                    AssetDatabase.CreateFolder(spriteOutFolder, SceneBackgounrdFolderName);
                }
            }
            else
            {
                string newSceneFolderName;
                string newModuleFolderName;
                acSprite.GetNewFolderName(SceneCommonFolderName, ModuleCommonFolderName, out newSceneFolderName, out newModuleFolderName);

                // 先创建场景目录，如果不存在
                targetFolder += newSceneFolderName;
                if (!AssetDatabase.IsValidFolder(targetFolder)) {
                    AssetDatabase.CreateFolder(spriteOutFolder, newSceneFolderName);
                }

                if (_atlasCollectRule == AtlasCollectRule.WithSceneAndModel)
                {
                    if (!string.IsNullOrEmpty(newModuleFolderName))
                    {
                        string sceneFolderPath = targetFolder;
                        targetFolder += "/" + newModuleFolderName;

                        // 目录不存在就新建
                        if (!AssetDatabase.IsValidFolder(targetFolder)) {
                            AssetDatabase.CreateFolder(sceneFolderPath, newModuleFolderName);
                        }
                    }
                }
            }

            return targetFolder;
        }
        #endregion

        #region Draw相关

        public void DrawAcProject()
        {
            GUILayout.BeginVertical();
            ACWindow.DrawSeparator();

            int lastSelectIndex = _gridSelectIndex;
            _gridSelectIndex = GUILayout.SelectionGrid(_gridSelectIndex, _gridSelects, _gridSelects.Length);
            if (lastSelectIndex != _gridSelectIndex)
            {
                ClearScrollViewToogleStates();

                switch (_gridSelectIndex)
                {
                    case 0:
                        TryInitDrawAcScenesParams();
                        break;

                    case 1:
                        if (_searchString != string.Empty) {
                            DoSerachSprites(ReferenceSpriteType.ReferenceFolderRefSprite, _searchString);
                        }
                        else {
                            _referenceFolderRefSprites = new List<ACSprite>(_allReferenceFolderSpritesDic.Values);
                        }
                        TryInitDrawReferenceFolderRefSpritesParams();
                        break;

                    case 2:
                        if (_searchString != string.Empty) {
                            DoSerachSprites(ReferenceSpriteType.UnReferenceFolderRefSprite, _searchString);
                        }
                        else {
                            _unReferenceFolderRefSprites = new List<ACSprite>(_allUnReferenceFolderSpritesDic.Values);
                        }
                        TryInitDrawUnReferenceFolderRefSpritesParams();
                        break;
                }
            }

            ACWindow.DrawSeparator();

            GUILayout.BeginHorizontal();
            switch (_gridSelectIndex)
            {
                case 0:
                    DrawAcScenes();
                    break;

                case 1:
                    DrawReferenceFolderRefSprites();
                    break;

                case 2:
                    DrawUnReferenceFolderRefSprites();
                    break;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void ClearScrollViewToogleStates()
        {
            _toggleStates.Clear();
            _scrollPos = Vector3.zero;
        }

        private void TryInitDrawReferenceFolderRefSpritesParams()
        {
            if (_toggleStates.Count == 0)
            {
                int spriteCount = _referenceFolderRefSprites.Count;
                if (spriteCount > 0)
                {
                    _curSelectSprite = _referenceFolderRefSprites[0];
                    _toggleStates = EditorWindowHelper.InitToggleStates(_referenceFolderRefSprites.Count);
                }
            }
        }

        private void TryInitDrawUnReferenceFolderRefSpritesParams()
        {
            if (_toggleStates.Count == 0)
            {
                int spriteCount = _unReferenceFolderRefSprites.Count;
                if (spriteCount > 0)
                {
                    _curSelectSprite = _unReferenceFolderRefSprites[0];
                    _toggleStates = EditorWindowHelper.InitToggleStates(spriteCount);
                }
            }
        }

        private void DrawUnReferenceFolderRefSprites()
        {
            GUILayout.BeginVertical(GUILayout.Width(ACStyles.SpriteViewWidth));
            GUILayout.Label("Un Reference Folder Reference Sprites " + _unReferenceFolderRefSprites.Count);
            DrawSearchToolBar(ReferenceSpriteType.UnReferenceFolderRefSprite);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            TryInitDrawUnReferenceFolderRefSpritesParams();
            DrawSprites(_unReferenceFolderRefSprites);

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            //if (Event.current.type == EventType.Repaint) {
            //    _selectSpritesScrollArea = GUILayoutUtility.GetLastRect();
            //}

            if (_curSelectSprite != null) {
                _curSelectSprite.DrawReferences(false);
            }
        }

        private void DrawReferenceFolderRefSprites()
        {
            GUILayout.BeginVertical(GUILayout.Width(ACStyles.SpriteViewWidth));
            GUILayout.Label("Reference Folder Reference Sprites " + _referenceFolderRefSprites.Count);
            DrawSearchToolBar(ReferenceSpriteType.ReferenceFolderRefSprite);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            TryInitDrawReferenceFolderRefSpritesParams();
            DrawSprites(_referenceFolderRefSprites);

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            //if (Event.current.type == EventType.Repaint) {
            //    _selectSpritesScrollArea = GUILayoutUtility.GetLastRect();
            //}

            if (_curSelectSprite != null) {
                _curSelectSprite.DrawReferences(false);
            }
        }

        private void DrawSprites(List<ACSprite> sprites)
        {
            for (int i = 0; i < sprites.Count; i ++)
            {
                var sprite = sprites[i];

                GUILayout.BeginHorizontal();
                {
                    sprite.DrawIcon();

                    bool lastToggle = _toggleStates[i];
                    _toggleStates[i] = GUILayout.Toggle(_toggleStates[i], sprite.AssetFile, ACWindow.ACStyles.SpriteButton,
                        GUILayout.Height(ACStyles.IconSize), GUILayout.Height(ACStyles.IconSize));

                    if (!lastToggle && _toggleStates[i])
                    {
                        _curSelectSprite = sprite;
                        EditorWindowHelper.ResetToggleStates(_toggleStates, i);

                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(sprite.AssetFile);
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawSearchToolBar(ReferenceSpriteType refSpriteType)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(ACStyles.SpriteViewWidth));
            {
                Rect position = GUILayoutUtility.GetRect(50, 250, 10, 50, EditorStyles.toolbarTextField);
                position.width -= 16;
                position.x += 16;

                string lastSearchString = _searchString;
                _searchString = GUI.TextField(position, _searchString, EditorStyles.toolbarTextField);
                if (_searchString != lastSearchString)
                {
                    if (_searchString == string.Empty) {
                        ClearSerachSprites(refSpriteType);
                    }
                    else {
                        DoSerachSprites(refSpriteType, _searchString);
                    }
                }

                position.x = position.x - 18;
                position.width = 20;
                if (GUI.Button(position, "", ACWindow.ACStyles.ToolbarSeachTextFieldPopup))
                {
                }

                position = GUILayoutUtility.GetRect(10, 10, ACWindow.ACStyles.ToolbarSeachCancelButton);
                position.x -= 5;
                if (GUI.Button(position, "", ACWindow.ACStyles.ToolbarSeachCancelButton))
                {
                    _searchString = string.Empty;
                    ClearSerachSprites(refSpriteType);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DoSerachSprites(ReferenceSpriteType refSpriteType, string serachString)
        {
            ClearScrollViewToogleStates();

            List<ACSprite> sprites = null;
            Dictionary<Sprite, ACSprite> refSpritesDic = null;
            switch (refSpriteType)
            {
                case ReferenceSpriteType.ReferenceFolderRefSprite:
                    sprites = _referenceFolderRefSprites;
                    refSpritesDic = _allReferenceFolderSpritesDic;
                    break;

                case ReferenceSpriteType.UnReferenceFolderRefSprite:
                    sprites = _unReferenceFolderRefSprites;
                    refSpritesDic = _allUnReferenceFolderSpritesDic;
                    break;
            }

            if (sprites == null || refSpritesDic == null) {
                return;
            }

            sprites.Clear();
            foreach (var ac in refSpritesDic)
            {
                Match match = Regex.Match(ac.Value.AssetFile, serachString, RegexOptions.IgnoreCase);
                if (match.Success) {
                    sprites.Add(ac.Value);
                }
            }
        }

        private void ClearSerachSprites(ReferenceSpriteType refSpriteType)
        {
            ClearScrollViewToogleStates();

            switch (refSpriteType)
            {
                case ReferenceSpriteType.ReferenceFolderRefSprite:
                    _referenceFolderRefSprites = new List<ACSprite>(_allReferenceFolderSpritesDic.Values);
                    break;

                case ReferenceSpriteType.UnReferenceFolderRefSprite:
                    _unReferenceFolderRefSprites = new List<ACSprite>(_allUnReferenceFolderSpritesDic.Values);
                    break;
            }            
        }

        private void TryInitDrawAcScenesParams()
        {
            if (_toggleStates.Count == 0)
            {
                int sceneCount = _allAcScenes.Count;
                if (sceneCount > 0)
                {
                    _curSelectScene = _allAcScenes[0];
                    _toggleStates = EditorWindowHelper.InitToggleStates(sceneCount);
                }
            }
        }

        private void DrawAcScenes()
        {
            GUILayout.BeginVertical(GUILayout.Width(ACStyles.SceneViewWidth));
            GUILayout.Label("SCENES " + _allAcScenes.Count);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);            

            TryInitDrawAcScenesParams();
                 
            for (int i = 0; i < _allAcScenes.Count; i++)
            {
                var acScene = _allAcScenes[i];

                bool lastToggle = _toggleStates[i];
                _toggleStates[i] = GUILayout.Toggle(_toggleStates[i], acScene.Name, ACWindow.ACStyles.SceneButton);

                if (!lastToggle && _toggleStates[i])
                {
                    EditorWindowHelper.ResetToggleStates(_toggleStates, i);
                    _curSelectScene = acScene;
                }
            }            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (_curSelectScene != null) {
                _curSelectScene.DrawAcModules();
            }
        }
        #endregion
    }
}