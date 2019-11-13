using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EditorWindowCommon;

namespace AtlasCollect
{
    /// <summary>
    /// 有文件夹的，虽然可能文件夹里就放了一个Prefab文件 e.g: MainCity/Equip/UI_Euqip
    /// </summary>
    public class ACMultiMoudle : ACModule
    {
        private List<ACPrefab> _childPrefabs = new List<ACPrefab>();

        #region Draw相关参数
        private List<bool> _prefabToggleStates = new List<bool>();
        #endregion

        #region 属性
        public List<ACPrefab> ChildPrefabs { get { return _childPrefabs; } }
        #endregion

        public static bool CreateAcMultiModule(string moduleFolder, ACScene parentScene, out ACMultiMoudle acMultiModule)
        {
            acMultiModule = null;

            var allPrefabFiles = Directory.GetFiles(moduleFolder, "*.prefab", SearchOption.AllDirectories);
            if (allPrefabFiles.Length == 0)
            {
                Debug.LogError(moduleFolder + "下未找到prefab类型的文件");
                return false;
            }

            acMultiModule = new ACMultiMoudle(moduleFolder, parentScene);
            for (int i = 0; i < allPrefabFiles.Length; i++) {
                acMultiModule.AddAcPrefab(allPrefabFiles[i]);
            }

            return true;
        }

        private ACMultiMoudle(string modulePath, ACScene parentScene) 
            : base(ModuleType.Multi, parentScene)
        {
            _modulePath = modulePath;
            _name = Path.GetFileName(modulePath);
        }

        private void AddAcPrefab(string file)
        {
            ACPrefab prefab = new ACPrefab(file, this);
            _childPrefabs.Add(prefab);
        }

        #region Draw相关
        protected override void TryInitDrawAcPrefabsParams()
        {
            if (_prefabToggleStates.Count == 0)
            {
                int prefabCount = _childPrefabs.Count;
                if (_childPrefabs.Count > 0)
                {
                    _curSelectPrefab = _childPrefabs[0];
                    _prefabToggleStates = EditorWindowHelper.InitToggleStates(prefabCount);
                }
            }
        }

        protected override void DrawAcPrefabsLabel()
        {
            GUILayout.Label("PREFABS " + _childPrefabs.Count);
        }

        protected override void DrawAcPrefabsContent()
        {
            for (int i = 0; i < _childPrefabs.Count; i++)
            {
                var childPrefab = _childPrefabs[i];

                bool lastToggle = _prefabToggleStates[i];
                _prefabToggleStates[i] = GUILayout.Toggle(_prefabToggleStates[i], childPrefab.Name, ACWindow.ACStyles.PrefabButton);

                if (!lastToggle && _prefabToggleStates[i])
                {
                    EditorWindowHelper.ResetToggleStates(_prefabToggleStates, i);
                    _curSelectPrefab = childPrefab;

                    OnSelectPrefab(_curSelectPrefab);
                }
            }
        }
        #endregion
    }
}