using System.Collections.Generic;
using System.IO;
using UnityEngine;
using EditorWindowCommon;

namespace AtlasCollect
{
    public class ACScene
    {
        private List<ACModule> _childModules = new List<ACModule>();

        #region Draw相关参数
        private ACModule _curSelectModule;
        private List<bool> _moduleToggleStates = new List<bool>();
        private Vector2 _moduleScrollPos;
        #endregion

        #region 属性
        public List<ACModule> ChildModules { get { return _childModules; } }
        #endregion

        public string FolderPath { get; private set; }
        public string Name { get; private set; }

        public static bool CreateAcScene(string folderPath, out ACScene acScene)
        {
            acScene = new ACScene(folderPath);

            // 初始化子文件夹下的配置（子文件夹的Prefab）
            var moduleFolders = Directory.GetDirectories(folderPath);
            for (int i = 0; i < moduleFolders.Length; i++)
            {
                ACMultiMoudle multiModule;
                if (ACMultiMoudle.CreateAcMultiModule(moduleFolders[i], acScene, out multiModule)) {
                    acScene.AddAcModule(multiModule);
                }
            }

            // 初始化当前文件夹下的配置（非子文件夹的Prefab)
            var singlePrefabFiles = Directory.GetFiles(folderPath, "*.prefab", SearchOption.TopDirectoryOnly);
            if (singlePrefabFiles.Length >= 0)
            {
                for (int i = 0; i < singlePrefabFiles.Length; i++)
                {
                    ACSingleModule singleModule = ACSingleModule.CreateAcSingleModule(singlePrefabFiles[i], acScene);
                    acScene.AddAcModule(singleModule);
                }
            }

            if (acScene._childModules.Count > 0) {
                return true;
            }
            else {
                return false;
            }
        }

        private ACScene(string folderPath)
        {
            FolderPath = folderPath;
            Name = Path.GetFileName(folderPath);
        }

        private void AddAcModule(ACModule acModule)
        {
            _childModules.Add(acModule);
        }

        #region Draw相关
        private void TryInitDrawAcModulesParams()
        {
            if (_moduleToggleStates.Count == 0)
            {
                int moduleConut = _childModules.Count;
                if (moduleConut > 0)
                {
                    _curSelectModule = _childModules[0];
                    _moduleToggleStates = EditorWindowHelper.InitToggleStates(moduleConut);
                }
            }
        }

        public void DrawAcModules()
        {
            GUILayout.BeginVertical(GUILayout.Width(ACStyles.ModuleViewWidth));
            GUILayout.Label("MODULES " + _childModules.Count);
            _moduleScrollPos = GUILayout.BeginScrollView(_moduleScrollPos);            

            TryInitDrawAcModulesParams();

            for (int i = 0; i < _childModules.Count; i++)
            {
                var childModule = _childModules[i];

                bool lastToggle = _moduleToggleStates[i];
                _moduleToggleStates[i] = GUILayout.Toggle(_moduleToggleStates[i], childModule.Name, ACWindow.ACStyles.ModuleButton);

                if (!lastToggle && _moduleToggleStates[i])
                {
                    EditorWindowHelper.ResetToggleStates(_moduleToggleStates, i);
                    _curSelectModule = childModule;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (_curSelectModule != null) {
                _curSelectModule.DrawAcPrefabs();
            }
        }
        #endregion
    }
}