using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AtlasCollect
{
    /// <summary>
    /// 没有文件夹，只是一个单一Prefab放在特定的场景文件夹下 e.g: Login/UI_Login
    /// </summary>
    public class ACSingleModule : ACModule
    {
        #region Draw相关参数
        private bool _toggleState = true;
        #endregion

        public ACPrefab ChildPrefab { get; private set; }

        public static ACSingleModule CreateAcSingleModule(string prefabFile, ACScene parentScene)
        {
            var singleModuleConfig = new ACSingleModule(prefabFile, parentScene);
            singleModuleConfig.TryInitDrawAcPrefabsParams();

            return singleModuleConfig;
        }

        private ACSingleModule(string prefabFile, ACScene parentScene)
            : base(ModuleType.Single, parentScene)
        {
            _modulePath = prefabFile;

            ChildPrefab = new ACPrefab(prefabFile, this);
            _name = ChildPrefab.Name;
        }

        #region Draw相关
        protected override void TryInitDrawAcPrefabsParams()
        {
            _curSelectPrefab = ChildPrefab;
        }

        protected override void DrawAcPrefabsLabel()
        {
            GUILayout.Label("PREFABS 1");
        }

        protected override void DrawAcPrefabsContent()
        {
            bool lastToggleState = _toggleState;
            _toggleState = GUILayout.Toggle(_toggleState, _name, ACWindow.ACStyles.PrefabButton);

            if (!lastToggleState && _toggleState) {
                OnSelectPrefab(ChildPrefab);
            }
        }
        #endregion
    }
}