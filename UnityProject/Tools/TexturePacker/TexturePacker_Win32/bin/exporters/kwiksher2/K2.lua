-- created with TexturePacker (http://www.texturepacker.com)
           frames = {
             {% for sprite in allSprites %}
               { x={{sprite.frameRect.x}}, y={{sprite.frameRect.y}}, width={{sprite.frameRect.width}}, height={{sprite.frameRect.height}} }, -- {{sprite.trimmedName}}{% endfor %}
           },
    
           sheetContentWidth = {{texture.size.width}},
           sheetContentHeight = {{texture.size.height}}
