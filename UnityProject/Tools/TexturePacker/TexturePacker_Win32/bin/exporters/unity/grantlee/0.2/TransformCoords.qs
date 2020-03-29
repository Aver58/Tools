
var ExportData = function(result)
{
    var output = "";
    var texture = result.texture;

    // if there is at least one sprite with scale9 enabled, the borders will be imported by unity
    var scale9SpritesFound = false;
    for (var j = 0; j < result.allSprites.length; j++)
    {
        scale9SpritesFound = scale9SpritesFound || result.allSprites[j].scale9Enabled;
    }

    output += ":format=40300\n";
    output += ":texture=" + texture.fullName + "\n";
    output += ":size=" + texture.size.width + "x" + texture.size.height + "\n";
    output += texture.normalMapFileName ? ":normalmap=" + texture.normalMapFileName + "\n" : "";
    output += ":pivotpoints=" + (result.settings.writePivotPoints ? "enabled" : "disabled") + "\n";
    output += ":borders=" + (scale9SpritesFound ? "enabled" : "disabled") + "\n";
    output += "\n";

    for (var j = 0; j < result.allSprites.length; j++)
    {
        var sprite = result.allSprites[j];
        output += EscapeSpecialChars(sprite.trimmedName) + ";";

        // frame rect in sheet
        output += sprite.frameRect.x + ";";
        output += MirroredFrameRectY(sprite, result.texture.size.height) + ";";
        output += sprite.frameRect.width + ";";
        output += sprite.frameRect.height + "; ";

        // pivot point
        output += TrimmedPivotX(sprite) + ";";
        output += TrimmedMirroredPivotY(sprite) + "; ";

        // borders
        if (sprite.scale9Enabled)
        {
            output += sprite.scale9Borders.x + ";";
            output += sprite.frameRect.width - sprite.scale9Borders.x - sprite.scale9Borders.width + ";";
            output += sprite.scale9Borders.y + ";";
            output += sprite.frameRect.height - sprite.scale9Borders.y - sprite.scale9Borders.height + ";";
        }
        else
        {
            output += "0;0;0;0;"
        }

        if (sprite.vertices.length > 0)
        {
            output += PrintVertices(sprite) + ";"
        }

        output = output.slice(0, -1) + "\n"; // replace last ";" by newline
    }
    return output.trim();
}
ExportData.filterName = "ExportData";
Library.addFilter("ExportData");


var EscapeSpecialChars = function(name)
{
    return name.replace(/%/g, "%25")
               .replace(/#/g, "%23")
               .replace(/:/g, "%3A")
               .replace(/;/g, "%3B")
};


var MirroredFrameRectY = function(sprite, textureHeight)
{
    return "" + (textureHeight - sprite.frameRect.y - sprite.frameRect.height);
};


var TrimmedPivotX = function(sprite)
{
    var ppX = sprite.pivotPointNorm.x;

    if(sprite.trimmed && sprite.sourceRect.width > 0) // polygon trim -> normalize pp based on trimmed size
    {
        ppX = (sprite.pivotPoint.x - sprite.sourceRect.x) / sprite.sourceRect.width;
    }
    return "" + ppX;
};


var TrimmedMirroredPivotY = function(sprite)
{
    var ppY = sprite.pivotPointNorm.y;

    if(sprite.trimmed && sprite.sourceRect.height > 0) // polygon trim -> normalize pp based on trimmed size
    {
        ppY = sprite.untrimmedSize.height - sprite.pivotPoint.y; // move origin to top-left
        ppY = ppY - sprite.sourceRect.y;                         // subtract corner offset, as unity does not support trimming
        ppY = ppY / sprite.sourceRect.height;                    // normalized pivot point, based on trimmed size
        ppY = 1 - ppY;                                           // move origin back to bottom-left
    }
    return "" + ppY;
};


var PrintVertices = function(sprite)
{
    var height = sprite.frameRect.height
    var str = ''
    var vertices = sprite.vertices
    var sourceRect = sprite.sourceRect
    str += " " + vertices.length
    for (var i = 0; i < vertices.length; i++)
    {
        str += ";" + (vertices[i].x - sourceRect.x) +
               ";" + (height - vertices[i].y + sourceRect.y)
    }
    var triangleIndices = sprite.triangleIndices
    str += "; " + triangleIndices.length / 3;
    for (i = 0; i < triangleIndices.length; i++)
    {
        str += ";" + triangleIndices[i]
    }
    return str
};
