using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using EditorWindowCommon;

namespace AtlasCollect
{
    public enum AtlasCollectRule
    {
        WithScene,
        WithSceneAndModel,
    }

    public class ACWindow : EditorWindow
    {
        public static ACStyles ACStyles;

        /// <summary>
        /// 目标UIPrefab的目录
        /// </summary>
        private string _prefabTargetFolder = string.Empty;

        /// <summary>
        /// 输出Sprite的目录
        /// </summary>
        private string _spriteOutFolder = string.Empty;

        /// <summary>
        /// 关联Sprite的目录
        /// </summary>
        private string _spriteReferencetFolder = "Assets/Res/Arts/UI";

        /// <summary>
        /// 图集生成的规则
        /// </summary>
        private AtlasCollectRule _atlasCollectRule = AtlasCollectRule.WithScene;

        /// <summary>
        /// 超过这个宽度，高度移动到背景目录
        /// </summary>
        private float _width = 500;
        private float _height = 400;

        private bool _startAtlasCollect = false;
        private ACProject _acProject;

        [MenuItem("Window/AtlasCollect")]
        public static void Init()
        {
            EditorWindow.GetWindow<ACWindow>("Atlas Collect");
        }

        private void OnEnable()
        {
            var script = MonoScript.FromScriptableObject(this);
            string currentScriptFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(script));

            var uiTidySkin = (GUISkin)AssetDatabase.LoadAssetAtPath(currentScriptFolder + "/AC Assets/ACGUISkin.guiskin", typeof(GUISkin));
            ACStyles = new ACStyles(uiTidySkin);

            _prefabTargetFolder = PlayerPrefs.GetString("SpriteCollectTargetPath");
            _spriteOutFolder = PlayerPrefs.GetString("SpriteCollectOutputPath");
            _spriteReferencetFolder = PlayerPrefs.GetString("SpriteCollectReferencePath");
        }

        #region GUI
        private void OnGUI()
        {
            DrawSetting();
            DrawAcProject();
        }

        /// <summary>
        /// 绘制设置
        /// </summary>
        private void DrawSetting()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                _prefabTargetFolder = EditorGUILayout.TextField("目标UIPrefab的目录", _prefabTargetFolder);
                if (GUILayout.Button("Choose", GUILayout.Width(70f))) {
                    _prefabTargetFolder = EditorUtility.OpenFolderPanel("选择要整理UIPrefab的目录", Application.dataPath, "");
                    PlayerPrefs.SetString("SpriteCollectTargetPath", _prefabTargetFolder);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                _spriteOutFolder = EditorGUILayout.TextField("输出Sprite的目录", _spriteOutFolder);
                if (GUILayout.Button("Choose", GUILayout.Width(70f)))
                {
                    _spriteOutFolder = EditorUtility.OpenFolderPanel("选择要输出Sprite的目录", Application.dataPath, "");
                    _spriteOutFolder = EditorWindowHelper.DataPathToAssetPath(_spriteOutFolder);
                    PlayerPrefs.SetString("SpriteCollectOutputPath", _spriteOutFolder);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                _spriteReferencetFolder = EditorGUILayout.TextField("关联Sprite的目录", _spriteReferencetFolder);
                if (GUILayout.Button("Choose", GUILayout.Width(70f)))
                {
                    _spriteReferencetFolder = EditorUtility.OpenFolderPanel("选择要关联Sprite的目录", Application.dataPath, "");
                    _spriteReferencetFolder = EditorWindowHelper.DataPathToAssetPath(_spriteReferencetFolder);
                    PlayerPrefs.SetString("SpriteCollectReferencePath", _spriteReferencetFolder);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                _atlasCollectRule = (AtlasCollectRule)EditorGUILayout.EnumPopup("图集整理规则", _atlasCollectRule);
                GUILayout.EndHorizontal();

                ACProject.SceneBackgounrdFolderName = EditorGUILayout.TextField("场景背景目录", ACProject.SceneBackgounrdFolderName);
                ACProject.SceneCommonFolderName = EditorGUILayout.TextField("场景图集通用目录", ACProject.SceneCommonFolderName);
                ACProject.ModuleCommonFolderName = EditorGUILayout.TextField("模块图集通用目录", ACProject.ModuleCommonFolderName);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("超出范围的Sprite会被移动到background目录");
                _width = EditorGUILayout.FloatField("背景图最小宽度", _width);
                _height = EditorGUILayout.FloatField("背景图最小高度", _height);
                GUILayout.EndHorizontal();

                if (_acProject != null)
                {
                    EditorGUILayout.LabelField("待整理的UIPrefab个数", _acProject.PrefabCount.ToString());
                    EditorGUILayout.LabelField("待整理的Sprite个数", _acProject.AllSpriteCount.ToString());
                    EditorGUILayout.LabelField("待整理中被引用的Sprite个数", _acProject.RefSpriteCount.ToString());
                    EditorGUILayout.LabelField("待整理中未引用的Sprite个数", _acProject.UnRefSpriteCount.ToString());
                }

                if (GUILayout.Button("LOAD"))
                {
                    if (string.IsNullOrEmpty(_prefabTargetFolder)) {
                        ShowNotification(new GUIContent("目标UIPrefab的目录未设置！"));
                    }
                    else if (string.IsNullOrEmpty(_spriteReferencetFolder)) {
                        ShowNotification(new GUIContent("关联Sprite的目录未设置！"));
                    }
                    else
                    {
                        _acProject = new ACProject(_prefabTargetFolder, _spriteReferencetFolder, _atlasCollectRule);
                        _startAtlasCollect = true;
                    }
                }

                if (GUILayout.Button("START"))
                {
                    if (!_startAtlasCollect || _acProject == null)
                    {
                        ShowNotification(new GUIContent("需要先点击LOAD按钮！"));
                        return;
                    }

                    if (string.IsNullOrEmpty(_spriteOutFolder))
                    {
                        ShowNotification(new GUIContent("输出Sprite的目录未设置！"));
                        return;
                    }

                    if (string.IsNullOrEmpty(ACProject.SceneBackgounrdFolderName))
                    {
                        ShowNotification(new GUIContent("场景背景目录不能为空！"));
                        return;
                    }

                    if (string.IsNullOrEmpty(ACProject.SceneCommonFolderName))
                    {
                        ShowNotification(new GUIContent("场景图集通用目录不能为空！"));
                        return;
                    }

                    if (string.IsNullOrEmpty(ACProject.ModuleCommonFolderName))
                    {
                        ShowNotification(new GUIContent("模块图集通用目录不能为空！"));
                        return;
                    }

                    _acProject.StartAtlasCollect(_width, _height, _spriteOutFolder);
                    _startAtlasCollect = false;

                    ShowNotification(new GUIContent("DONE!"));
                }
            }
            GUILayout.EndVertical();
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 绘制配置文件
        /// </summary>
        private void DrawAcProject()
        {
            if (_acProject == null) {
                return;
            }

            _acProject.DrawAcProject();
        }

        public static void DrawSeparator()
        {
            GUILayout.Box("", ACStyles.SeparatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }
        #endregion
    }
}