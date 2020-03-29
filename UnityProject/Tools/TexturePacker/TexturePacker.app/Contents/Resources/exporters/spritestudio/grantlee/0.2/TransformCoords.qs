var TrimmedPivotX = function(sprite)
{
    if(sprite.untrimmedSize.width === 0)
    {
        return "" + 0;
    }

    var ppX = -0.5 + (sprite.pivotPoint.x - sprite.sourceRect.x) / sprite.untrimmedSize.width;
    return "" + ppX;
};
TrimmedPivotX.filterName = "TrimmedPivotX";
Library.addFilter("TrimmedPivotX");

var TrimmedMirroredPivotY = function(sprite)
{
    if(sprite.untrimmedSize.height === 0)
    {
        return "" + 0;
    }

    var ppY = 0.5 + (sprite.pivotPoint.y  - sprite.sourceRect.y) / sprite.untrimmedSize.height;
    return "" + (1 - ppY);
};
TrimmedMirroredPivotY.filterName = "TrimmedMirroredPivotY";
Library.addFilter("TrimmedMirroredPivotY");
