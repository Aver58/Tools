var MakeSelectorFilter = function(input)
{
  var input = input.rawString();
  input = input.replace(/\s+/g,"_");
  return input.replace(/-hover/,":hover");
};
MakeSelectorFilter.filterName = "makecssselector";
MakeSelectorFilter.isSafe = false;
Library.addFilter("MakeSelectorFilter");
