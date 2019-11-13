using UnityEngine;

namespace AtlasLooker
{
    public class ALStyles
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

        public float AtlasNameButtonHeight = 18f;

        public GUIStyle AtlasNameButton { get; private set; }

        public ALStyles(GUISkin guiSkin)
        {
            AtlasNameButton = guiSkin.GetStyle("AtlasNameButton");            
        }
    }
}