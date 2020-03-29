// create a css selector by replacing -hover with :hover
var MakeSelectorFilter = function(input)
{
  input = input.rawString();
  return input.replace("-hover",":hover");
};
MakeSelectorFilter.filterName = "makecssselector";
MakeSelectorFilter.isSafe = false;
Library.addFilter("MakeSelectorFilter");


// some global variables to store width and height in
width=1;
height=1;

// set the global width variable {{value|setWidth}}
var SetWidth = function(input)
{
    width=input;
    return "";
};
SetWidth.filterName = "setWidth";
SetWidth.isSafe = false;
Library.addFilter("SetWidth");

// set the global height variable {{value|setHeight}}
var SetHeight = function(input)
{
    height=input;
    return "";
};
SetHeight.filterName = "setHeight";
SetHeight.isSafe = false;
Library.addFilter("SetHeight");

// calculate relative x value {{value|makeRelX}}
var MakeRelX = function(input)
{
    return String(input/width);
};
MakeRelX.filterName = "makeRelX";
MakeRelX.isSafe = false;
Library.addFilter("MakeRelX");

// calculate relative y value {{value|makeRelY}}
var MakeRelY = function(input)
{
    return String(input/height);
};
MakeRelY.filterName = "makeRelY";
MakeRelY.isSafe = false;
Library.addFilter("MakeRelY");

var introspect = function(value, name, indent)
{
    indent = indent || "";
    name = name || "";

    if (value === null)
    {
        return indent+name+" = null";
    }

    var objType = typeof value;
    var info = indent+name+" = ";

    if (objType === "undefined")
    {
        return info+"undefined\n";
    }
    else if (objType === "object")
    {
        var propInfo = "";
        var prop;
        for (prop in value)
        {
            if(prop !== "objectName") // ignore objectName - it's currently empty
            {
                var p = introspect(value[prop], prop, indent+"    ");
                if(p !== "")
                {
                    propInfo += p +"\n";
                }
            }
        }
        if(propInfo==="")
        {
            info += "{"+value+"}";
        }
        else
        {
            info += "{\n" + propInfo +indent+"}";
        }
    }
    else if (objType === "function")
    {
        return "";
    }
    else {
        info+=value;
    }

    return info;
};

// print some detail about object {{value|makeRelY}}
var ObjectInfo = function(input)
{
    return introspect(input);
};
ObjectInfo.filterName = "objectInfo";
ObjectInfo.isSafe = true;
Library.addFilter("ObjectInfo");
