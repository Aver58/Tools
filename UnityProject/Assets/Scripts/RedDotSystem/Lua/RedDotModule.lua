---
--- Created by zhiwei.
--- DateTime: 2020/4/28 19:17
---

-- 1. 倒计时队列，把一些需要倒计时的红点注册进来，然后挑出最小的时间进行倒计时
-- 红点todo：
-- 2. 红点缓存红点数量，数据变化的时候刷新红点数据，减少红点计算次数
-- 3. OnOpen生成，OnClose回收
-- 4. 树可以只记parent节点，不再用字符串裁剪方式分析父节点，而是直接配置父节点定义
--# 2020.09.30 重构
--            + RedTreeNode 对象
--            + 脏标记 ： 处理同一帧多个子节点同时改变状态，将导致父节点进行额外的无用刷新，所以引入脏标记模式
--            + 只有叶子节点需要取红点数量函数，父节点红点数都由子红点数累加 【todo 这个没做】
--            + 子节点数据变化，比对是否变化，变化才通知父节点刷新，减少无用刷新
--            + Debug Tree 生成item在Hierarchy
-- todo 重构代价太大：红点系统本身只关注红点的产生和管理以及表现，不关心数据的来源。==》发消息的时候直接传数量，红点模块记住数量进行比对

---@generic T
---@class redTreeNode
---@field nodeName string
---@field parent redTreeNode
---@field childList redTreeNode[]
---@field item RedDotItem
---@field target T

---@class RedDotModule:ModuleBase
---@field _redDotMap table<string,redTreeNode>
local _M = class("RedDotModule", me.ModuleBase)

local _dirtyNodeCount = 0
local ModuleDetail = RedDotDefine.ModuleDetail
local RedTreeNode = import(".logic.modules.red.RedTreeNode")

function _M:ctor()
    _M.super.ctor(self)
    self._timeID = nil          -- 正在倒计时的TimerID
    self._timestampList = {}    -- 注册的倒计时时间戳列表
    self._functionUnlockMap = {}-- 功能解锁Map
    _dirtyNodeCount = 0       -- 脏标记节点数量
    self._dirtyNodeMap = {}     -- 脏标记节点列表
    self._redDotMap = nil       -- 所有红点节点的Map
end

function _M:Init()
    _M.super.Init(self)
    GameMsg.AddMessage("RED_DOT_UPDATE", self, self.OnRedDotCountChange)
    GameMsg.AddMessage("RED_DOT_UNLOCK", self, self.OnRedDotFunctionUnlock)
    GameMsg.AddMessage("ON_SERVER_UPDATE_RED_DOT", self, self.OnServerRedDotCountChange)
    GameMsg.AddMessage("GAME_TODAY_OVER", self, self.OnTodayOver) --隔天刷新红点
    GameMsg.AddMessage("GAME_RESTART_TIMER", self, self.OnRestartTimer)

    self._redDotMap = {}
    self:InitTree()
end

--region 响应

function _M:OnTodayOver()
    for i, moduleType in pairs(RedDotDefine.ModuleType) do
        self:OnRedDotCountChange(moduleType)
    end
end

function _M:OnRestartTimer()
    TimerMgr.StopTimer(self, "_timerID")
    self:ReStartTimer()
end

-- 红点对应的功能解锁
function _M:OnRedDotFunctionUnlock(unlockType)
    local moduleType = self._functionUnlockMap[unlockType]
    if moduleType then
        printA("[红点]功能解锁刷新红点：",unlockType)
        self:OnRedDotCountChange(moduleType)
    end
end

--endregion

--region server RedDot 服务器相关红点

local RedModuleType = redDef.RedModuleType
local ModuleType = RedDotDefine.ModuleType

local ModuleTypeMap ={
    [RedModuleType.Arena] = ModuleType.HeroArena_Record,
    [RedModuleType.Alliance] = ModuleType.Alliance_Member_Btn,
    [RedModuleType.Journey] = ModuleType.JourneyClue,
}

-- 服务器红点模块刷新
function _M:OnServerRedDotCountChange(param)
    local mid = param.mid
    local rid = param.rid
    local moduleType = ModuleTypeMap[mid]
    self:OnRedDotCountChange(moduleType)
end

function _M:GetRedNum(moduleType)
    local redDotDetail = ModuleDetail[moduleType]
    if not redDotDetail then
        printAError("[红点]没有找到指定模块的红点信息！moduleType：", moduleType)
        return
    end
    local mid = redDotDetail.mid
    local rid = redDotDetail.rid
    return Ctrl.RedCtrl:GetRedNum(mid,rid)
end

--4018 2 请求某一个点已读
function _M:reqRedReadRid(moduleType)
    local redDotDetail = ModuleDetail[moduleType]
    if not redDotDetail then
        printAError("[红点]没有找到指定模块的红点信息！moduleType：", moduleType)
        return
    end
    local mid = redDotDetail.mid
    local rid = redDotDetail.rid
    Ctrl.RedCtrl:reqRedReadRid(mid,rid)

    self:OnRedDotCountChange(moduleType)
end

--endregion

--region 主逻辑

-- 初始化节点 【结构层】
function _M:InitTree()
    -- 构造所有节点
    for key, moduleType in pairs(RedDotDefine.ModuleType) do
        local redTreeNode = RedTreeNode.new(moduleType)
        redTreeNode:Init(moduleType)
        self._redDotMap[moduleType] = redTreeNode
    end

    -- 构造节点的关系
    for key, node in pairs(self._redDotMap) do
        local redDotDetail = ModuleDetail[key]
        local parentKey = redDotDetail.parent
        if parentKey then
            local parent = self._redDotMap[parentKey]
            if parent then
                parent:AddChild(node)
                node:SetParent(parent)
            end
        end
    end
end

-- 找到指定节点
---@return redTreeNode
function _M:_GetTargetNode(moduleType)
    return self._redDotMap[moduleType]
end

-- 注册红点 【驱动层】
---@param target ViewBase @传入view对象
---@param moduleType number @模块枚举
---@return RedDotItem
function _M:RegisterRedDot(target, moduleType, redDotItem,params)
    local redDotDetail = ModuleDetail[moduleType]
    if not redDotDetail then
        printAError("[红点]没有找到指定模块的红点信息！moduleType：", moduleType)
        return
    end

    ---@field redTreeNode redTreeNode
    local node = self:_GetTargetNode(moduleType)

    local parentName = redDotDetail.parentName
    if not parentName then
        printAError("[红点]没有找到parentName！moduleType:", moduleType)
        return
    end

    local prefabName = redDotDetail.prefabName
    prefabName = prefabName or "RedDotItem"
    if not target[parentName] then
        printAError("[红点]没有找到指定父节点",parentName,"moduleType:",moduleType)
        return
    end

    if redDotDetail.unlockType then
        self._functionUnlockMap[redDotDetail.unlockType] = moduleType
    end

    if redDotItem == nil then
        redDotItem = target:GenerateOne(me.RedDotItem, prefabName, target[parentName])
    end
    redDotItem:Init(moduleType,params)
    node:AddItem(redDotItem,target,params)

    printAF("[红点]注册成功：%s ,name: %s, childCount:%s", moduleType,node:GetChildCount(),node:GetChildCount())
    return redDotItem
end

-- 取消注册红点
---@param moduleType ModuleType @模块枚举
---@param targetItem RedDotItem
function _M:UnRegisterRedDot(moduleType, targetItem)
    if not moduleType or not targetItem then
        return
    end

    local node = self:_GetTargetNode(moduleType)
    local result = false
    if node then
        result = node:RemoveItem(targetItem)
    end

    if result == false then
        printA("[红点]UnRegisterRedDot没有找到指定模块的红点实例！moduleType：", moduleType)
    end
end

function _M:ClearRedDot(moduleType, targetItem)
    if not moduleType or not targetItem then
        return
    end

    local node = self:_GetTargetNode(moduleType)

    if node then
        node:ClearItem()
    else
        printA("[红点]UnRegisterRedDot没有找到指定模块的红点实例！moduleType：", moduleType)
    end
end

-- 红点数量变化
function _M:OnRedDotCountChange(moduleType)
    if not moduleType then
        return
    end
    local node = self:_GetTargetNode(moduleType)
    if node then
        self:MarkDirtyNode(node)
    end
end

--endregion

--region 脏标记

-- 如果同一帧多个子节点同时改变状态，将导致父节点进行额外的无用刷新，所以引入脏标记
function _M:Update(dt)
    if _dirtyNodeCount > 0 then
        for moduleType, node in pairs(self._dirtyNodeMap) do
            node:ChangeValue()
            self._dirtyNodeMap[moduleType] = nil
            --printA("[红点]清理脏节点:",node:GetName())
        end
        _dirtyNodeCount = 0
    end
end

-- 标记为脏节点
function _M:MarkDirtyNode(node)
    if node == nil then
        return
    end
    self._dirtyNodeMap[node:GetName()] = node
    _dirtyNodeCount = _dirtyNodeCount + 1
    --printA("[红点]标记脏节点",node:GetName(),_dirtyNodeCount)
end

--endregion

--region RedDot timer 倒计时队列，把一些需要倒计时的红点注册进来，然后挑出最小的时间进行倒计时

local function SortByTime(l,r)
    return l.timestamp < r.timestamp
end

function _M:RegisterTimer(moduleType,timestamp)
    if not moduleType or not timestamp then
        return
    end
    if timestamp < TimeUtil.ServerTime() then
        -- 时间戳已经过了
        return
    end

    table.insert(self._timestampList,{moduleType = moduleType,timestamp = timestamp})
    printA("[红点]注册timer：moduleType",moduleType,"timestamp",timestamp)
    table.sort(self._timestampList,SortByTime)
    self:ReStartTimer()
end

function _M:ReStartTimer()
    local timestampList = self._timestampList
    if timestampList and #timestampList > 0 then
        local timerItem = timestampList[1]
        local moduleType = timerItem.moduleType
        local timestamp = timerItem.timestamp
        self._curTimerModuleType = moduleType

        TimerMgr.StopTimer(self, "_timerID")
        local remainTime = timestamp - TimeUtil.ServerTime()
        if remainTime > 0 then
            printA("[红点]：开启定时器:",remainTime,"moduleType",moduleType)
            self._timeID = TimerMgr.Start(self,self.OnTimer,remainTime)
        else
            table.remove(self._timestampList,1)
        end
    end
end

function _M:OnTimer()
    if self._curTimerModuleType then
        self:OnRedDotCountChange(self._curTimerModuleType)
    end
    table.remove(self._timestampList,1)
    printA("[红点]倒计时结束，刷新红点：moduleType：",self._curTimerModuleType,"剩余timer数量:",#self._timestampList)
    self:ReStartTimer()
end

--endregion

--region Debug Tree

-- 测试代码：自己在随便按钮加    me.modules.redDot:DebugTree()
function _M:DebugTree()
    local root = GameObject("root")
    for moduleType, node in pairs(self._redDotMap) do
        local parent = node:GetParent()
        if not parent then
            self:RecursiveOneTree(node,root)
        end
    end
end

-- 递归一棵树
function _M:RecursiveOneTree(node,parentGO)
    local obj = GameObject(node:GetName())
    obj:SetParent(parentGO.transform)
    if node:GetChildCount() > 0 then
        for i, childNode in ipairs(node:GetChildList()) do
            self:RecursiveOneTree(childNode,obj)
        end
    end
    return obj
end

--endregion

return _M