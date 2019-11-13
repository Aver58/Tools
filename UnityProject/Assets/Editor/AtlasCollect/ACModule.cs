using UnityEditor;
using UnityEngine;

namespace AtlasCollect
{
    public enum ModuleType
    {
        Multi,
        Single
    }

    public abstract class ACModule
    {
        protected string _modulePath;
        public string ModulePath { get { return _modulePath; } }

        protected string _name;
        public string Name { get { return _name; } }

        protected ModuleType _moduleType;
        public ModuleType ModuleType { get { return _moduleType; } }

        /// <summary>
        /// 属于哪个场景
        /// </summary>
        protected ACScene _parecntScene;
        public ACScene ParentScene { get { return _parecntScene; } }

        #region Draw相关参数
        protected Vector2 _prefabScrollPos;
        protected ACPrefab _curSelectPrefab;
        #endregion 

        public ACModule(ModuleType moduleType, ACScene parentScene)
        {
            _moduleType = moduleType;
            _parecntScene = parentScene;
        }

        #region Draw相关
        public void DrawAcPrefabs()
        {
            GUILayout.BeginVertical(GUILayout.Width(ACStyles.PrefabViewWidth));
            DrawAcPrefabsLabel();
            _prefabScrollPos = GUILayout.BeginScrollView(_prefabScrollPos);

            TryInitDrawAcPrefabsParams();
            DrawAcPrefabsContent();

            GUILayout.EndScrollView();
            GUILayout.EndVertical();            

            if (_curSelectPrefab != null) {
                _curSelectPrefab.DrawAcSprites();
            }
        }

        protected abstract void DrawAcPrefabsLabel();

        protected abstract void TryInitDrawAcPrefabsParams();

        protected abstract void DrawAcPrefabsContent();

        protected void OnSelectPrefab(ACPrefab selectPrefab)
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(selectPrefab.AssetFile);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        #endregion
    }
}