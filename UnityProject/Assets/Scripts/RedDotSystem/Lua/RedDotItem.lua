---
--- Created by zhiwei.
--- DateTime: 2020/4/28 19:33
--- 【表现层】

-- 需要应对的红点类型：
--1. 同种类型红点生成多个，使用同一个获取红点方法，需要传参
--2. 同一个节点生成多个同一类型红点【x】todo
--3. 同一类型红点，在不同节点生成

---@class RedDotItem:PanelBase
local _M = class("RedDotItem", me.UIItemBase)

function _M:Dispose()
    _M.super.Dispose(self)
    -- 框架管理，取消注册
    if self._moduleType then
        me.modules.redDot:UnRegisterRedDot(self._moduleType,self)
    end
end

function _M:OnItemOpen()
    _M.super.OnItemOpen(self)
    GameMsg.AddMessage("RED_DOT_ITEM_UPDATE", self, self.OnRedDotUpdate)
end

function _M:OnItemClose()
    _M.super.OnItemClose(self)
    GameMsg.RemoveMessage("RED_DOT_ITEM_UPDATE", self, self.OnRedDotUpdate)
end

function _M:OnRedDotUpdate(data)
    dumpAError(data,"data")
    if data.key == self._moduleType then
        self:SetRedDotCount(data.count)
    end
end

function _M:Init(moduleType)
    self._moduleType = moduleType
    local redDotDetail = RedDotDefine.ModuleDetail[moduleType]
    if not redDotDetail then
        printAError("[红点]没有找到指定模块的红点信息！moduleType：",moduleType)
        return
    end
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
    local redDotCount = me.modules.redDot:GetRedDotCount(moduleType)
    self:SetRedDotCount(redDotCount)
end

function _M:SetShowCount(value)
    self._showCount = value
end

-- 初始化位置
function _M:SetOffset(offsetX,offsetY)
    self.transform:SetLocalPositionEx(offsetX,offsetY,0)
end

-- 提供接口，供业务手动生成调用
---@param count number @数量
---@param bShowNum boolean @展示数量
function _M:SetRedDotCount(count,bShowNum)
    if not count then
        return
    end

    bShowNum = bShowNum or self._showCount
    printAT(gType.Red,string.format("%s，count:%s",self._moduleType, count))
    if self._isSceneItem then
        self:SetDataSceneItem(count,bShowNum)
    else
        self:SetDataCommonItem(count,bShowNum)
    end

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
    else
        -- 隐藏
        self.ImgBG:SetActive(false)
    end
end

return _M