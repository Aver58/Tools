---
--- Created by zhiwei.
--- DateTime: 2020/9/25 13:18
--- RedTreeNode 红点节点对象

---@class RedTreeNode : ObjectBase
local _M = class("RedTreeNode",me.ObjectBase)

function _M:ctor(key)
    _M.super.ctor(self,key)
    -- 数据
    self._key = key                                 -- 节点名字
    self._childCount = 0                            -- 子节点数量
    self._redCount = 0                              -- 红点数量【红点实例对应红点数之和】
    self._selfRedCount = 0                          -- 自身节点数量
    self._config = RedDotDefine.ModuleDetail[key]   -- 配置
    -- 实例【与实例解耦，用消息管理】
end

function _M:GetKey()
    return self._key
end

function _M:GetCount()
    return self._redCount
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

-- 红点的数量 = 自身节点红点数 + 子节点红点数之和
-- count 传入count，刷新自身节点数量，不传就用初始化的数量
function _M:UpdateCount(count)
    -- 自己节点数量
    if not count then
        count = self._selfRedCount
    else
        self._selfRedCount = count
    end

    -- 子节点数量之和
    for i = 1, self._childCount do
        local childCount = self._ChildNodes[i]:GetCount()
        count = count + childCount
    end
    self._redCount = count
    GameMsg.SendMessage(GetRedUpdateMsg(self._key), self._key,count)
    return count
end

function _M:OnRedDotUpdate(count)
    local oldValue = self._redCount
    local newValue = self:UpdateCount(count)
    -- 数量有变化才通知 父节点，减少不必要的刷新
    if oldValue ~= newValue then
        if self._ParentNode then
            printA("[红点]子节点变化，通知父节点：",self._ParentNode:GetKey())
            self._ParentNode:OnRedDotUpdate()
        end
    end
end

return _M