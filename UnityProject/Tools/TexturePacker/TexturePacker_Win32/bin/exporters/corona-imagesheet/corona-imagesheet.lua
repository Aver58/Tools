--
-- created with TexturePacker (http://www.codeandweb.com/texturepacker)
--
-- {{smartUpdateKey}}
--
-- local sheetInfo = require("mysheet")
-- local myImageSheet = graphics.newImageSheet( "mysheet.png", sheetInfo:getSheet() )
-- local sprite = display.newSprite( myImageSheet , {frames={sheetInfo:getFrameIndex("sprite")}} )
--

local SheetInfo = {}

SheetInfo.sheet =
{
    frames = {
    {% for sprite in allSprites %}
        {
            -- {{sprite.trimmedName}}
            x={{sprite.frameRect.x}},
            y={{sprite.frameRect.y}},
            width={{sprite.frameRect.width}},
            height={{sprite.frameRect.height}},
{% if sprite.trimmed %}
            sourceX = {{sprite.sourceRect.x}},
            sourceY = {{sprite.sourceRect.y}},
            sourceWidth = {{sprite.untrimmedSize.width}},
            sourceHeight = {{sprite.untrimmedSize.height}}{% endif %}
        },{% endfor %}
    },
    
    sheetContentWidth = {{texture.size.width}},
    sheetContentHeight = {{texture.size.height}}
}

SheetInfo.frameIndex =
{
{% for sprite in allSprites %}
    ["{{sprite.trimmedName}}"] = {{ forloop.counter }},{% endfor %}
}

function SheetInfo:getSheet()
    return self.sheet;
end

function SheetInfo:getFrameIndex(name)
    return self.frameIndex[name];
end

return SheetInfo
