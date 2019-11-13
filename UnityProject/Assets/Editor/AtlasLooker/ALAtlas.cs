using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AtlasLooker
{
    public class ALAtlas
    {
        public string AtlasName { get; private set; }        
        public int AtlasCount { get; private set; }

        private List<Sprite> _atlasSprites;

        public ALAtlas(string atlasName, int atlasCount)
        {
            AtlasName = atlasName;
            AtlasCount = atlasCount;
        }
    }
}
