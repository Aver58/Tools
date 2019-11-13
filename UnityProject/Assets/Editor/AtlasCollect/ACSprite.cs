using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEditor;
using EditorWindowCommon;

namespace AtlasCollect
{
    public class ACSprite
    {
        /// <summary>
        /// Sprite的引用
        /// </summary>
        private Dictionary<ACScene, HashSet<ACModule>> _spriteRefDic = new Dictionary<ACScene, HashSet<ACModule>>();

        /// <summary>
        /// Sprite的所有Prefabs引用
        /// </summary>
        private List<ACPrefab> _allReferencePrefabs = new List<ACPrefab>();

        #region Draw相关参数
        private Vector3 _refScrollPos;
        private List<bool> _refToggleStates = new List<bool>();
        private Texture _icon;
        #endregion

        public string Name { get; set; }
        public string AssetFile { get; private set; }

        public float Width { get; private set; }
        public float Height { get; private set; }
        public Sprite Sprite { get; private set; }      

        public ACSprite(Sprite sprite, string assetFile)
        {
            Sprite = sprite;            
            AssetFile = assetFile;

            Name = Path.GetFileName(assetFile);            

            Width = sprite.textureRect.width;
            Height = sprite.textureRect.height;

            //_icon = AssetDatabase.GetCachedIcon(AssetFile);
        }

        /// <summary>
        /// 更新引用
        /// </summary>
        public void UpdateReference(ACPrefab referencePrefab)
        {
            var refModule = referencePrefab.ParentModule;
            var refScene = refModule.ParentScene;

            HashSet<ACModule> refModules;
            if (!_spriteRefDic.TryGetValue(refScene, out refModules))
            {
                refModules = new HashSet<ACModule>();
                _spriteRefDic[refScene] = refModules;
            }

            if (!refModules.Contains(refModule)) {
                refModules.Add(refModule);
            }
            
            _allReferencePrefabs.Add(referencePrefab);
        }

        /// <summary>
        /// 获取场景目录名字
        /// </summary>
        /// <param name="sceneCommonFolderName"></param>
        /// <param name="sceneFolderName"></param>
        public void GetNewFolderName(string sceneCommonFolderName, string moduleCommonFolder, 
            out string sceneFolderName, out string moduleFolerName)
        {
            sceneFolderName = string.Empty;
            moduleFolerName = string.Empty;

            if (_spriteRefDic.Count == 1)
            {
                var firstKeyValue = _spriteRefDic.First();
                ACScene acScene = firstKeyValue.Key;
                HashSet<ACModule> acModules = firstKeyValue.Value;

                sceneFolderName = acScene.Name;

                if (acModules.Count == 1) {
                    moduleFolerName = acModules.First().Name;
                }
                else {
                    moduleFolerName = acScene.Name + moduleCommonFolder;
                }
            }
            else {
                sceneFolderName = sceneCommonFolderName;
            }
        }

        /// <summary>
        /// 获取模块目录名字，跟 GetSceneFolderName 配合使用，只有当
        /// </summary>
        /// <param name="moduleCommonFolder"></param>
        /// <returns></returns>
        public string GetModuleFolderName(string moduleCommonFolder)
        {
            HashSet<ACModule> acModules = _spriteRefDic.First().Value;
            if (acModules.Count == 1) {
                return acModules.First().Name;
            }
            else
            {
                return moduleCommonFolder;
            }
        }

        #region Draw相关
        public void DrawIcon()
        {
            if (_icon == null) {
                _icon = AssetDatabase.GetCachedIcon(AssetFile);
            }

            // 画小图标
            GUILayout.Box(_icon, ACWindow.ACStyles.SpriteButton, GUILayout.Height(ACStyles.IconSize - 4), 
                GUILayout.Width(ACStyles.IconSize - 4));
        }

        public void DrawReferences(bool setWidth = true)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("REFERENCE PREFABS " + _allReferencePrefabs.Count);

            if (setWidth) {
                _refScrollPos = GUILayout.BeginScrollView(_refScrollPos, GUILayout.Width(ACStyles.PrefabViewWidth));
            }
            else {
                _refScrollPos = GUILayout.BeginScrollView(_refScrollPos);
            }

            if (_refToggleStates.Count == 0) {
                _refToggleStates = EditorWindowHelper.InitToggleStates(_allReferencePrefabs.Count);
            }

            for (int i = 0; i < _allReferencePrefabs.Count; i ++)
            {
                var acReferencePrefab = _allReferencePrefabs[i];

                bool lastToggle = _refToggleStates[i];
                _refToggleStates[i] = GUILayout.Toggle(_refToggleStates[i], acReferencePrefab.Name, ACWindow.ACStyles.PrefabButton);
                if (!lastToggle && _refToggleStates[i])
                {
                    EditorWindowHelper.ResetToggleStates(_refToggleStates, i);

                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(acReferencePrefab.AssetFile);
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        #endregion
    }
}