using UnityEditor;
using UnityEngine;

namespace AtlasCollect
{
    public class ACStyles
    {
        private GUIStyle _separatorStyle;

        public GUIStyle SeparatorStyle
        {
            get
            {
                if (_separatorStyle == null)
                {
                    _separatorStyle = new GUIStyle(GUI.skin.box);
                    _separatorStyle.border.top = 0;
                    _separatorStyle.border.bottom = 1;
                }
                return _separatorStyle;
            }
        }

        /// <summary>
        /// 图标宽高
        /// </summary>
        public const float IconSize = 20f;

        public const float SceneViewWidth = 200f;
        public const float ModuleViewWidth = 200f;
        public const float PrefabViewWidth = 250f;
        public const float SpriteViewWidth = 600f;

        public GUIStyle SceneButton { get; private set; }
        public GUIStyle ModuleButton { get; private set; }
        public GUIStyle PrefabButton { get; private set; }
        public GUIStyle SpriteButton { get; private set; }

        public GUIStyle ToolbarSearchField { get { return GetStyle("ToolbarSeachTextField"); } }
        public GUIStyle ToolbarSeachTextFieldPopup { get { return GetStyle("ToolbarSeachTextFieldPopup"); } }
        public GUIStyle ToolbarSeachCancelButton { get { return GetStyle("ToolbarSeachCancelButton"); } }
        public GUIStyle ToolbarSeachCancelButtonEmpty { get { return GetStyle("ToolbarSeachCancelButtonEmpty"); } }

        public ACStyles(GUISkin guiSkin)
        {
            SceneButton = guiSkin.GetStyle("SceneButton");
            ModuleButton = guiSkin.GetStyle("ModuleButton");
            PrefabButton = guiSkin.GetStyle("PrefabButton");
            SpriteButton = guiSkin.GetStyle("SpriteButton");
        }

        private GUIStyle GetStyle(string styleName)
        {
            GUIStyle guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (guiStyle == null)
            {
                Debug.LogError((object)("Missing built-in guistyle " + styleName));
                guiStyle = GUI.skin.button;
            }
            return guiStyle;
        }
    }
}