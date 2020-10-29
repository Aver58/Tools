---
--- Created by zhiwei.
--- DateTime: 2020/9/25 13:18
--- RedTreeNode 红点节点对象

---@class RedTreeNode : ObjectBase
local _M = class("RedTreeNode",me.ObjectBase)

function _M:ctor()
    _M.super.ctor(self)
    -- 数据
    self._name = ""             -- 节点名字
    self._fullPath = ""         -- 全路径
    self._parent = nil          -- 父节点
    --self._value = 0             -- 红点数量【红点实例对应红点数之和】
    self._isDirty = false       -- 红点数据是否脏了
    self._config = nil          -- 配置
    self._childrenList = nil    -- 子节点
    self._childrenCount = 0     -- 子节点数量
    -- 实例
    self._itemInfoList = nil    -- 子节点实例对象
    self._itemMap = nil         -- 子节点实例对象Map
end

function _M:Init(name)
    self._name = name
    self._config = RedDotDefine.ModuleDetail[name]
    self._childrenCount = 0
    self._isDirty = false
    self._childrenList = {}
    self._itemInfoList = {}
    self._itemMap = {}
end

function _M:SetParent(node)
    self._parent = node
end

function _M:AddChild(node)
    table.insert(self._childrenList,node)
    self._childrenCount = self._childrenCount + 1
end

local function GetItemKey(redDotItem,target)
    return tostring(redDotItem)..tostring(target)
end

-- 添加实例item -- 解决同一种类型红点，生成在多个节点下
function _M:AddItem(redDotItem,target,params)
    local itemKey = GetItemKey(redDotItem,target)
    if self._itemMap[itemKey] then
        --重复注册Item
        --printA("[红点]重复注册Item")
        return
    end
    self._itemMap[itemKey] = 1
    table.insert(self._itemInfoList,{ item = redDotItem, target = target , params = params})
end

function _M:RemoveItem(redDotItem)
    local infoList = self:GetInfoList()
    if #infoList > 0 then
        for i, info in ipairs(infoList) do
            -- 2020.09.05 item会不一样，是外部解决，还是换一种方式去取消注册 ==》ClearRedDot
            if info.item == redDotItem then
                -- 2020.05.08 红点复用bug，OnItemClose去RecycleOne，然后把PanelBase的_childItem表缩短了，导致OnCloseSubItem遍历的时候出错
                info.target:RecycleOne(info.item)
                table.remove(infoList, i)
                printA("[红点]回收实例成功:"," childCount：", #infoList, " nodeName：", self:GetName())
                return true
            end
        end
    end
    return false
end

function _M:ClearItem()
    local infoList = self:GetInfoList()
    if #infoList > 0 then
        for i, info in ipairs(infoList) do
            info.target:RecycleOne(info.item)
            printA("[红点]回收实例成功"," nodeName：", self:GetName())
        end
        for i, info in ipairs(infoList) do
            table.remove(infoList, i)
            printA("[红点]回收数据成功:", " childList：", #infoList, " nodeName：", self:GetName())
        end
    end
end

-- 子节点数量
function _M:GetChildCount()
    return self._childrenCount
end

function _M:GetChildList()
    return self._childrenList
end

-- 生成的实例数量
function _M:GetItemCount()
    return #self._itemInfoList
end

-- 生成的实例
function _M:GetInfoList()
    return self._itemInfoList
end

-- 名字
function _M:GetName()
    return self._name
end

-- 父节点
function _M:GetParent()
    return self._parent
end

-- 生成全路径
function _M:GetFullPath()
    if self._fullPath == "" then
        local path = self._name
        local node = self._parent
        while node ~= nil do
            path = path .. "_" ..node:GetName()
            node = node:GetParent()
        end
        self._fullPath = path
    end

    return self._fullPath
end

-- 数量变化
function _M:ChangeValue()
    local infoList = self:GetInfoList()
    local isChange = false
    if #infoList > 0 then
        for i, info in ipairs(infoList) do
            local isChanged = info.item:OnRedDotCountChange()
            if isChanged then
                isChange = true
            end
        end
    else
        -- 没有view层依赖的时候，直接刷新父节点
        isChange = true
    end

    -- 有变化的话通知 父节点
    if isChange then
        local parent = self:GetParent()
        if parent then
            printA("[红点]子节点变化，通知父节点：",parent:GetName())
            parent:ChangeValue()
        end
    end
end

return _M