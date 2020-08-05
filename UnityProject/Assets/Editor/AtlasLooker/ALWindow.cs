using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;

namespace AtlasLooker
{
    public class ALWindow : EditorWindow
    {
        public static ALStyles ALStyles;

        private static EditorWindow ALWindowInstance;

        private int _selectedAtlasIndex = 0;
        private int _atlasNameScrollRectCount;
        private Vector2 _atlasNameScrollPos;
        private Rect _atlasNameScrollRect;

        private List<bool> _atlasToggleStates = new List<bool>();
        private List<ALAtlas> _alAtlases = new List<ALAtlas>();

        #region 反射参数
        private Type _packerWindowType;
        private EditorWindow _packerWindow;
        #endregion

        [MenuItem("Window/AtlasLooker")]
        public static void Init()
        {
            ALWindowInstance = EditorWindow.GetWindow<ALWindow>("Atlas Looker");
        }

        private void OnEnable()
        {
            // 打开 SpritePacker 并保存窗口实例（如果没打开）
            Assembly assembly = Assembly.Load("UnityEditor");
            _packerWindowType = assembly.GetType("UnityEditor.Sprites.PackerWindow");
            _packerWindow = EditorWindow.GetWindow(_packerWindowType);

            // 加载样式
            var script = MonoScript.FromScriptableObject(this);
            string currentScriptFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(script));
            var uiTidySkin = (GUISkin)AssetDatabase.LoadAssetAtPath(currentScriptFolder + "/AL Assets/ALGUISkin.guiskin", typeof(GUISkin));
            ALStyles = new ALStyles(uiTidySkin);

            LoadAtalsNames();            
        }

        private void OnDisable()
        {
            _packerWindow.Close();
        }

        #region GUI
        private void OnGUI()
        {
            ProcessKeyboardInput();

            GUILayout.BeginVertical();
            {
                DrawButton();
                DrawSeparator();

                _atlasNameScrollPos = GUILayout.BeginScrollView(_atlasNameScrollPos);
                for (int i = 0; i < _alAtlases.Count; i++)
                {
                    var atlas = _alAtlases[i];
                    string atalsName = string.Format("{0}[{1}]", atlas.AtlasName, atlas.AtlasCount);

                    bool lastToggle = _atlasToggleStates[i];                    
                    bool newToggle = GUILayout.Toggle(_atlasToggleStates[i], atalsName, ALStyles.AtlasNameButton, GUILayout.Height(ALStyles.AtlasNameButtonHeight));

                    int lastSelectedIndex = _selectedAtlasIndex;
                    if (!lastToggle && lastToggle != newToggle)
                    {
                        _selectedAtlasIndex = i;
                        SetSelectAtlasName(_selectedAtlasIndex);

                        KeyCode keyCode = _selectedAtlasIndex > lastSelectedIndex ? KeyCode.DownArrow : KeyCode.UpArrow;
                        SetScrollViewPos(_selectedAtlasIndex, keyCode);
                    }
                }
                GUILayout.EndScrollView();

                if (Event.current.type == EventType.Repaint)
                {
                    _atlasNameScrollRect = GUILayoutUtility.GetLastRect();

                    // 计算 ScrollRect 所能容纳的 Item 个数
                    var tempNum = _atlasNameScrollRect.height / ALStyles.AtlasNameButtonHeight;
                    _atlasNameScrollRectCount = Mathf.FloorToInt(tempNum);
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawButton()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load&Reload")) {
                LoadAtalsNames();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSeparator()
        {
            GUILayout.Box("", ALStyles.SeparatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        private void ProcessKeyboardInput()
        {
            Event ev = Event.current;      
            if (!ev.isKey) {
                return;
            }

            if (ev.type == EventType.KeyDown && (ev.keyCode == KeyCode.UpArrow || ev.keyCode == KeyCode.DownArrow))
            {
                if (ev.keyCode == KeyCode.UpArrow) {
                    _selectedAtlasIndex--;
                }
                else {
                    _selectedAtlasIndex ++;
                }

                _selectedAtlasIndex = Mathf.Clamp(_selectedAtlasIndex, 0, _alAtlases.Count - 1);
                SetSelectAtlasName(_selectedAtlasIndex);
                SetScrollViewPos(_selectedAtlasIndex, ev.keyCode);

                Repaint();
            }
        }

        private void SetSelectAtlasName(int selectIndex)
        {
            if (_packerWindow == null) {
                _packerWindow = EditorWindow.GetWindow(_packerWindowType);
            }

            // 设置被选中的状态
            _atlasToggleStates[selectIndex] = true;

            // 重置其他的状态
            for (int n = 0; n < _atlasToggleStates.Count; n++)
            {
                if (n != selectIndex) {
                    _atlasToggleStates[n] = false;
                }                
            }

            SetPackerWindowFieldValue("m_SelectedAtlas", selectIndex, BindingFlags.Instance | BindingFlags.NonPublic);
            CallPackerWindowMethod("RefreshAtlasPageList", null, BindingFlags.Instance | BindingFlags.NonPublic);
            CallPackerWindowMethod("Repaint", null, BindingFlags.Instance | BindingFlags.Public);
        }

        // WTF........................................
        private void SetScrollViewPos(int selectIndex, KeyCode keyCode)
        {            
            float scrollPosY = _atlasNameScrollPos.y;
            int startIndex = Mathf.CeilToInt(scrollPosY / ALStyles.AtlasNameButtonHeight);
            if (keyCode == KeyCode.UpArrow)
            {
                if (selectIndex < startIndex + 2)
                {
                    int dis = (Math.Abs(selectIndex - startIndex)) == 0 ? 2 : 1;
                    _atlasNameScrollPos.y = startIndex * ALStyles.AtlasNameButtonHeight - dis * ALStyles.AtlasNameButtonHeight;
                }
            }
            else
            {
                selectIndex = selectIndex + 1;
                if (selectIndex > startIndex + _atlasNameScrollRectCount - 2) {
                    _atlasNameScrollPos.y = (selectIndex - _atlasNameScrollRectCount) * ALStyles.AtlasNameButtonHeight + ALStyles.AtlasNameButtonHeight * 2;
                }
            }
        }
        #endregion

        #region 反射
        /// <summary>
        /// 设置 Unity Sprite Packer(PackerWindow)内部的值，通过反射
        /// </summary>
        private void SetPackerWindowFieldValue(string fieldName, object value, BindingFlags bindFlags)
        {
            FieldInfo fieldInfo = _packerWindowType.GetField(fieldName, bindFlags);
            fieldInfo.SetValue(_packerWindow, value);
        }

        /// <summary>
        /// 调用 Unity Sprite Packer(PackerWindow)的内部函数，通过反射
        /// </summary>
        private void CallPackerWindowMethod(string methodName, object[] parameters, BindingFlags bindFlags)
        {
            MethodInfo methodInfo = _packerWindowType.GetMethod(methodName, bindFlags);
            methodInfo.Invoke(_packerWindow, parameters);
        }
        #endregion

        private void LoadAtalsNames()
        {
            _atlasToggleStates.Clear();
            _alAtlases.Clear();

            var atlasNames = Packer.atlasNames;
            for (int i = 0; i < atlasNames.Length; i++)
            {
                string atlasName = atlasNames[i];
                Texture2D[] texture = Packer.GetTexturesForAtlas(atlasName);
                ALAtlas alAtlas = new ALAtlas(atlasName, texture.Length);
                _alAtlases.Add(alAtlas);
                _atlasToggleStates.Add(false);
            }
        }
    }
}