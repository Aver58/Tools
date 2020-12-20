---
--- Created by zhiwei.
--- DateTime: 2020/9/25 13:18
--- RedTreeNode 红点节点对象

---@class RedTreeNode : ObjectBase
local _M = class("RedTreeNode")

function _M:ctor(key)
    -- 数据
    self._key = key                                 -- 节点名字
    self._childCount = 0                            -- 子节点数量
    self._redCount = 0                              -- 红点数量【红点实例对应红点数之和】
    self._selfRedCount = 0                          -- 自身节点数量
    self._dirty = true                              -- 红点数据是否脏了【初始化为true】
    self._config = RedDotDefine.ModuleDetail[key]   -- 配置
    -- 实例【与实例解耦，用消息管理】
end

function _M:GetKey()
    return self._key
end

function _M:GetCount()
    return self._redCount
end

-- 主更新逻辑
function _M:UpdateCount()
    if not self._dirty then
        return self._redCount
    end
    -- 自己节点数量
    local count = 0
    self._selfRedCount = self:_GetRedDotCount()
    count = self._selfRedCount
    -- 子节点数量之和
    for i = 1, self._childCount do
        local childCount = self._ChildNodes[i]:UpdateCount()
        count = count + childCount
    end
    self._redCount = count
    self._dirty = false
    GameMsg.SendMessage("RED_DOT_ITEM_UPDATE", {key = self._key,count = count})
    return count
end

function _M:GetChildCount()
    return self._childCount
end

function _M:SetParent(node)
    self._ParentNode = node
end

function _M:AddChild(node)
    if not self._ChildNodes then
        self._ChildNodes = {}
    end
    self._childCount = self._childCount + 1
    table.insert(self._ChildNodes, node)
    node:SetParent(self)
    return self
end

-- 这里如果新项目的时候，应该改成业务推红点到这里保存，红点业务不管红点数量
function _M:_GetRedDotCount()
    local config = self._config
    if not config then
        printATError(gType.Red,"没有找到指定类型的定义, moduleType：",config.moduleType)
        return 0
    end

    local functionType = config.unlockType
    if functionType then
        -- 需要解锁才能有红点的模块
        local isUnlock = FunctionHelper.IsUnlock(functionType,true)
        if not isUnlock then
            return 0
        end
    end

    if config.mid and config.rid then
        -- 有配置服务器数据，直接拿服务器数据
        printA(string.format("moduleType:%s,mid:%s,rid:%s",config.moduleType,config.mid,config.rid))
        return Ctrl.RedCtrl:GetRedNum(config.mid,config.rid)
    else
        local ctrlName = config.ctrlName
        if not ctrlName then
            printAError(string.format("[红点]没有找到指定模块的ctrlName，moduleType：%s",config.moduleType))
            return 0
        end

        local ctrl = Ctrl[ctrlName]
        return ctrl[config.funcName](ctrl,config.param)
    end
end

function _M:OnMsg()
    local oldValue = self._redCount
    local newValue = self:UpdateCount()
    -- 有变化的话通知 父节点
    if oldValue ~= newValue then
        self:SetParentDirty()
    end
end

function _M:SetDirty()
    self._dirty = true
end

function _M:SetParentDirty()
    if self._ParentNode then
        printA("[红点]子节点变化，通知父节点：",self._ParentNode:GetKey())
        self._ParentNode:SetParentDirty()
    end
end

return _M