textureHeight=1;

var SetTextureHeight = function(input) 
{
    textureHeight = input;
    return "";
};
SetTextureHeight.filterName = "setTextureHeight";
Library.addFilter("SetTextureHeight");


var TrimmedPivotX = function(sprite)
{
	var ppX = (sprite.pivotPoint.x - sprite.sourceRect.x) / sprite.sourceRect.width;
    return "" + ppX;
};
TrimmedPivotX.filterName = "TrimmedPivotX";
Library.addFilter("TrimmedPivotX");


var TrimmedPivotY = function(sprite)
{
	var ppY = (sprite.pivotPoint.y  - sprite.sourceRect.y) / sprite.sourceRect.height;
    return "" + ppY;
};
TrimmedPivotY.filterName = "TrimmedPivotY";
Library.addFilter("TrimmedPivotY");
