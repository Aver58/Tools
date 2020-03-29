var StripPathSeparators = function(input)
{
  var input = input.rawString();
  input = input.replace(/\s+/g,"_");
  input = input.replace(/\\+/g,"_");
  input = input.replace(/\/+/g,"_");
  return input.charAt(0).toUpperCase() + input.slice(1);
};
StripPathSeparators.filterName = "stripPathSeparators";
StripPathSeparators.isSafe = false;
Library.addFilter("StripPathSeparators");
