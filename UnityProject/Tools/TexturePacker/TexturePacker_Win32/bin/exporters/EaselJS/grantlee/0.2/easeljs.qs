
var printImageFiles = function(result, subPath)
{
    var images = [];
    for (var i = 0; i < result.textures.length; i++)
    {
        images.push(subPath + result.textures[i].fullName);
    }
    return '"images": [\n    "' + images.join('",\n    "') + '"\n],\n\n';
}

var spriteList = [];

var printFrames = function(result, writePivotPoints)
{
    var frames = [];
    for (var i = 0; i < result.textures.length; i++)
    {
        var texture = result.textures[i];
        for (var j = 0; j < texture.allSprites.length; j++)
        {
            var sprite = texture.allSprites[j];
            var pivotPointX = writePivotPoints ? sprite.pivotPoint.x : 0;
            var pivotPointY = writePivotPoints ? sprite.pivotPoint.y : 0;
            var frameValues = [sprite.frameRect.x,
                               sprite.frameRect.y,
                               sprite.frameRect.width,
                               sprite.frameRect.height,
                               i,
                               pivotPointX-sprite.sourceRect.x,
                               pivotPointY-sprite.sourceRect.y];
            frames.push(frameValues.join(", "));
            spriteList.push(sprite);
        }
    }
    return '"frames": [\n    [' + frames.join('],\n    [') + ']\n],\n\n';
}


var printAnimations = function(detectAnimations) {
    var animations = [];
    for (var i = 0; i < spriteList.length; i++)
    {
        var spriteName = spriteList[i].trimmedName;
        var nameAndFrame = spriteName.match(/(.+)[-_.\//](\d+)$/);
        if (nameAndFrame && detectAnimations)
        {
            var name = nameAndFrame[1];
            if (!animations[name])
                animations[name] = [];
            animations[name][parseInt(nameAndFrame[2], 10)] = i;
        }
        else
        {
            animations[spriteName] = [i];
        }
    }
    var str = '';
    for (var name in animations)
    {
        str += '"' + name + '": { "frames": [' + (animations[name]).filter(function(n){ return n !== undefined }).join(', ')+ '] },\n    ';
    }
    return '"animations": {\n    ' + str.trim().slice(0,-1) + '\n},';
}


var ExportData = function(root)
{
    var variant = root.allResults[root.variantIndex];

    return printImageFiles(variant, root.settings.textureSubPath) +
           printFrames(variant, root.settings.writePivotPoints) +
           printAnimations(root.settings.autodetectAnimations);
}
ExportData.filterName = "exportData";
Library.addFilter("ExportData");

