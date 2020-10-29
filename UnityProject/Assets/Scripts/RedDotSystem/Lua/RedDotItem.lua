---
--- Created by zhiwei.
--- DateTime: 2020/4/28 19:33
--- 【表现层】

-- 需要应对的红点类型：
--1. 同种类型红点生成多个，使用同一个获取红点方法，需要传参
--2. 同一个节点生成多个同一类型红点【x】todo
--3. 同一类型红点，在不同节点生成

---@class RedDotItem:PanelBase
local _M = class("RedDotItem", me.PanelBase)

function _M:ctor(go,parent)
    _M.super.ctor(self,go,parent)
    self._isInit = false
end

function _M:Dispose()
    _M.super.Dispose(self)
    -- 框架管理，取消注册
    if self._moduleType then
        me.modules.redDot:UnRegisterRedDot(self._moduleType,self)
    end
end

function _M:Init(moduleType,param)
    self._param = param
    self._moduleType = moduleType
    local redDotDetail = RedDotDefine.ModuleDetail[moduleType]
    if not redDotDetail then
        printAError("[红点]没有找到指定模块的红点信息！moduleType：",moduleType)
        return
    end
    self._redDotDetail = redDotDetail
    self._isSceneItem = redDotDetail.prefabName == "RedDotItemScene"

    -- 初始化位置
    local offset = redDotDetail.offset
    if offset and #offset >= 2 then
        self:SetOffset(offset[1],offset[2])
    else
        self.transform:SetLocalPositionZero()
    end

    -- 是否展示数量
    local showCount = redDotDetail.showCount
    self._showCount = showCount

    -- 红点个数
    local funcName = redDotDetail.funcName
    funcName = funcName or "GetRedDotCount"
    self._funcName = funcName
    local mid = redDotDetail.mid
    local rid = redDotDetail.rid
    self._mid = mid
    self._rid = rid

    local redDotCount = self:_GetRedDotCount(redDotDetail,param,moduleType)
    self:SetRedDotCount(redDotCount)

    self._isInit = true
end

-- 初始化位置
function _M:SetOffset(offsetX,offsetY)
    self.transform:SetLocalPositionEx(offsetX,offsetY,0)
end

function _M:OnRedDotCountChange(isFirst)
    if not self._isInit then
        printA("[红点]还未初始化!")
        return false
    end

    local redDotCount = self:_GetRedDotCount(self._redDotDetail,self._param,self._moduleType,isFirst)
    local result = self:SetRedDotCount(redDotCount)
    return result
end

function _M:_GetRedDotCount(redDotDetail,param,moduleType,isFirst)
    local functionType = redDotDetail.unlockType
    if functionType then
        -- 需要解锁才能有红点的模块
        local isUnlock = FunctionHelper.IsUnlock(functionType,true)
        if not isUnlock then
            return 0
        end
    end

    if self._mid and self._rid then
        -- 有配置服务器数据，直接拿服务器数据
        printA(string.format("moduleType:%s,mid:%s,rid:%s",moduleType,self._mid,self._rid))
        return Ctrl.RedCtrl:GetRedNum(self._mid,self._rid)
    else
        local ctrlName = redDotDetail.ctrlName
        if not ctrlName then
            printAError(string.format("[红点]没有找到指定模块的ctrlName，moduleType：%s",moduleType))
            return 0
        end

        local ctrl = Ctrl[ctrlName]
        self._ctrl = ctrl
        self._ctrlName = ctrlName
        return ctrl[self._funcName](ctrl,param,isFirst)
    end
end

-- 提供接口，供业务手动生成调用
---@param count number @数量
---@param bShowNum boolean @展示数量
function _M:SetRedDotCount(count,bShowNum)
    bShowNum = bShowNum or self._bShowNum
    local newState = count and count > 0
    if self._ctrlName and self._funcName then
        printA(string.format("[红点]  %s %s:%s，count:%s,lastState:%s,newState:%s",self._moduleType,self._ctrlName,self._funcName, count,tostring(self._state) or "nil",tostring(newState)))
    end
    if self._state and self._state == newState then
        return false
    end

    self._state = newState
    if self._isSceneItem then
        self:SetDataSceneItem(count,bShowNum)
    else
        self:SetDataCommonItem(count,bShowNum)
    end
    -- 强行解决：【【妃子系统】精力值大于0，红点变为灰点（非必现）】
    --https://www.tapd.cn/57858150/bugtrace/bugs/view/1157858150001010480
    --LuaHelper.SetUIGray(self.ImgBG, false) -- //如果没找到UIGray组件，并且设置正常颜色，则不处理
    self.ImgBG.color = ColorDefine.White.color
    return true
end

-- 通用红点预制
function _M:SetDataCommonItem(count,bShowNum)
    self.TxtNum:SetActive(bShowNum)
    if count and count > 0 then
        if bShowNum then
            -- 有数字的红点
            self.ImgBG:SetActive(false)
            self.ImgBigBG:SetActive(true)
            self.TxtNum.text = count
        else
            -- 没数字的红点
            self.ImgBG:SetActive(true)
            self.ImgBigBG:SetActive(false)
        end
    else
        -- 隐藏
        self.ImgBG:SetActive(false)
        self.ImgBigBG:SetActive(false)
    end
end

-- 主界面红点预制
function _M:SetDataSceneItem(count,bShowNum)
    if count and count > 0 then
        -- 没数字的红点
        self.ImgBG:SetActive(true)
        self.fxui_common_RedDotItem_01:SetActive(true)
    else
        -- 隐藏
        self.ImgBG:SetActive(false)
        self.fxui_common_RedDotItem_01:SetActive(false)
    end
end

return _M