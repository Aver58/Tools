var ExportData = function(root)
{
    var sprites = "";
    var header = "";
    var sprites2x = "";
    var header2x = "";

    var subPath = root.settings.textureSubPath;
    var spritePrefix = root.exporterProperties.sprite_prefix;

    var comment = "";

    for(var v = 0; v<root.settings.autoSDSettings.length; v++)
    {
        var variant = root.allResults[v];
        var extension = root.settings.autoSDSettings[v].extension;
        if(extension == "-2x")
        {
            indent = "    ";
            sprites2x += spriteLines2x(variant, subPath, indent, spritePrefix);        
        }
        else
        {
            sprites += spriteLines(variant, subPath, "", spritePrefix);    
        }
    }

    if(sprites2x)
    {
        var query = root.exporterProperties.media_query_2x;

        mediaStart = "@media "+query+ " {\n";
        mediaEnd = "\n}\n";
        return  comment + header + "\n\n" + sprites + "\n\n" + mediaStart + header2x + "\n\n" + sprites2x + mediaEnd;
    }
    else
    {
        return  comment + header + "\n\n" + sprites;
    }
}

ExportData.filterName = "exportData";
Library.addFilter("ExportData");

var makeImagePath = function(variant, subPath)
{
    var imageName = variant.textures[0].fullName;
    if(subPath)
    {
        imageName = subPath+"/"+imageName;
    }
    return imageName;
}

var spriteLines = function(variant, subPath, indent, spritePrefix)
{
    var lines = [];
    var texture = variant.textures[0];

    var imageName = makeImagePath(variant, subPath, spritePrefix);

    for (var j = 0; j < texture.allSprites.length; j++)
    {
        var sprite = texture.allSprites[j];

        var cssClassName = makeSelector(sprite.trimmedName, spritePrefix);

        var line1 = cssClassName + " {"
         + "max-width:"+sprite.frameRect.width+"px; "
         + "max-height:"+sprite.frameRect.height+"px; "
         + "}";

        var divX = (texture.size.width-sprite.frameRect.width);
        var x = (divX == 0) ? 0 : 100*(sprite.frameRect.x)/divX;

        var divY = (texture.size.height-sprite.frameRect.height);
        var y = (divY == 0) ? 0 : 100*(sprite.frameRect.y)/divY;

        var ratio = 100*sprite.frameRect.height/sprite.frameRect.width;
        var width = 100*texture.size.width/sprite.frameRect.width;
        var height = 100*texture.size.height/sprite.frameRect.height;

       var line2 = cssClassName + "::after {"
            + "content: ' ';"
            + "display: inline-block; "
            + "width:"+sprite.frameRect.width+"px; "
            + "height:"+sprite.frameRect.height+"px; "
            + "background-position: "+x+"% "+y+"%;"
            + "background-size: "+width+"% "+height+"%;"
            + "background-image: url("+imageName+");"
            + "padding: 0; "
            + "}";

        var line3 = "div" + cssClassName + "::after {"
            + "max-width:"+sprite.frameRect.width+"px; "
            + "width:100%;"
            + "height:0;"
            + "padding: 0 0 "+ratio+"% 0;"
            + "}"

        lines.push(line1+"\n"+line2+"\n"+line3);
    }

    lines.sort();

    return indent+lines.join('\n'+indent);
}

var spriteLines2x = function(variant, subPath, indent, spritePrefix)
{
    var lines = [];
    var texture = variant.textures[0];

    var imageName = makeImagePath(variant, subPath);

    for (var j = 0; j < texture.allSprites.length; j++)
    {
        var sprite = texture.allSprites[j];

        var x = 100*(sprite.frameRect.x)/(texture.size.width-sprite.frameRect.width);
        var y = 100*(sprite.frameRect.y)/(texture.size.height-sprite.frameRect.height);

        var line = makeSelector(sprite.trimmedName, spritePrefix) + "::after {"
         + "background-position: "+x+"% "+y+"%; "
         + "background-image: url("+imageName+"); "
         + "}";

        lines.push(line);
    }

    lines.sort();

    return indent+lines.join('\n'+indent);
}

var makeSelector = function(input, spritePrefix)
{
    input = input.replace(/\//g,"-");
    input = input.replace(/\\/g,"-");
    input = input.replace(/\s+/g,"-");
    input = input.replace(/-link/,":link");
    input = input.replace(/-visited/,":visited");
    input = input.replace(/-focus/,":focus");
    input = input.replace(/-active/,":active");
    input = input.replace(/-hover/,":hover");

    return "."+spritePrefix+input;
}
