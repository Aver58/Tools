---
--- Created by zhiwei.
--- DateTime: 2020/4/28 19:26
---

---@alias ModuleDetail table<string, RedDotDefineNode>
---@alias ModuleType table<string, RedDotDefineNode>
---@class RedDotDefine
---@field ModuleDetail ModuleDetail
---@field ModuleType ModuleType
local RedDotDefine = {}
local RedModuleType = redDef.RedModuleType
-- 定义模块类型 -- 直接用下划线做切割，就不用ModuleDetail配置表了，这边不再做实现
RedDotDefine.ModuleType = {
    -- 妃子
    PrincessMain                       = "PrincessMain",
    Princess                           = "PrincessMain_Princess",
    Princess_Energy                    = "PrincessMain_Princess_Energy",
    Children                           = "PrincessMain_Children",
    DanceParty                         = "PrincessMain_DanceParty",
    Marriage                           = "PrincessMain_Marriage",

}

---@class RedDotDefineNode
---@field parentName string @红点生成位置，如果不想新建节点，可以配合offset使用
---@field ctrlName string @模块名
---@field funcName string @获取红点方法名，不填默认为GetRedDotCount，为了兼容同一个ctrl，多个红点，可空
---@field showCount boolean @是否展示数字，可空
---@field offset number[] @红点预制相对于parent的偏移，可空
---@field prefabName string @预制名，可空
local ModuleType = RedDotDefine.ModuleType
RedDotDefine.ModuleDetail = {
    -- 妃子二级界面
    [ModuleType.PrincessMain]                       = { parentName = "ImgRed" },
    [ModuleType.Princess]                           = { parent = ModuleType.PrincessMain, parentName = "ImgRed", ctrlName = "princessCtrl", funcName = "GetPrincessRedDotNum", prefabName = "RedDotItemScene" },
    [ModuleType.Princess_Energy]                    = { parent = ModuleType.Princess, parentName = "BtnRandomLove", ctrlName = "princessCtrl", funcName = "GetEnergyNum", offset = { 100, 30 } },
    [ModuleType.DanceParty]                         = { parent = ModuleType.PrincessMain, parentName = "ImgRed", ctrlName = "DancingPartyCtrl", funcName = "GetCity2DRedDot", prefabName = "RedDotItemScene" },
    [ModuleType.Marriage]                           = { parent = ModuleType.PrincessMain, parentName = "ImgRed", ctrlName = "MarriageCtrl", funcName = "GetMarriageRedDot", prefabName = "RedDotItemScene"},
    [ModuleType.Children]                           = { parent = ModuleType.PrincessMain },
}

return RedDotDefine