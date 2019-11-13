using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using EditorWindowCommon;

namespace AtlasCollect
{
    public class ACPrefab
    {
        private List<ACSprite> _acSprites = new List<ACSprite>();

        #region Draw相关
        private ACSprite _curSelectSprite;
        private List<bool> _spriteToggleStates = new List<bool>();
        private Vector2 _spriteScrollPos;
        #endregion

        public string File { get; private set; }

        public string AssetFile { get; private set; }

        public string Name { get; private set; }

        public ACModule ParentModule { get; private set; }

        #region 属性
        public List<ACSprite> ACSprites { get { return _acSprites; } }
        #endregion

        public ACPrefab(string file, ACModule parentModule)
        {
            File = file;
            AssetFile = EditorWindowHelper.DataPathToAssetPath(file);

            ParentModule = parentModule;

            string fileName = Path.GetFileName(file);
            {
                fileName = Regex.Replace(fileName, ".*?_", "", RegexOptions.IgnoreCase);
                Name = Regex.Replace(fileName, ".prefab", "", RegexOptions.IgnoreCase);
            }
        }

        public void AddACSprite(ACSprite acSprite)
        {
            _acSprites.Add(acSprite);
        }

        #region Draw相关
        private void TryInitDrawAcSpritesParams()
        {
            if (_spriteToggleStates.Count == 0)
            {
                int spriteCount = _acSprites.Count;
                if (spriteCount > 0)
                {
                    _curSelectSprite = _acSprites[0];
                    _spriteToggleStates = EditorWindowHelper.InitToggleStates(spriteCount);
                }
            }
        }

        public void DrawAcSprites()
        {
            GUILayout.BeginVertical(GUILayout.Width(ACStyles.SpriteViewWidth));
            GUILayout.Label("SPRITES " + _acSprites.Count);
            _spriteScrollPos = GUILayout.BeginScrollView(_spriteScrollPos);

            TryInitDrawAcSpritesParams();

            for (int i = 0; i < _acSprites.Count; i ++)
            {
                var sprite = _acSprites[i];

                GUILayout.BeginHorizontal();
                {
                    sprite.DrawIcon();

                    bool lastToggle = _spriteToggleStates[i];
                    _spriteToggleStates[i] = GUILayout.Toggle(_spriteToggleStates[i], sprite.AssetFile, ACWindow.ACStyles.SpriteButton, 
                        GUILayout.Height(ACStyles.IconSize), GUILayout.Height(ACStyles.IconSize));

                    if (!lastToggle && _spriteToggleStates[i])
                    {
                        _curSelectSprite = sprite;
                        EditorWindowHelper.ResetToggleStates(_spriteToggleStates, i);

                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(sprite.AssetFile);
                        EditorGUIUtility.PingObject(Selection.activeObject);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (_curSelectSprite != null) {
                _curSelectSprite.DrawReferences();
            }
        }
        #endregion
    }
}