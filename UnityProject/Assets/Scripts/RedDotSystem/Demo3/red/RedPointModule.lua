local BaseManager = require("Client.Base.BaseManager")

---@class RedPointModule
local RedPointModule = Class("RedPointModule", BaseManager)

local BtnRedPointParam = require("Client.UMG.RedPointModule.BtnRedPointParam")
local NetDefine = require("Client.Tools.NetWork.NetDefine")

function RedPointModule:Init()
    self.Base.Init(self)
    self.BtnRedPointDataMap={}
    self.RedEventBtnMap = {}
    self.NetRedDotMap = {}
    self:GenerateRedPointCfg()
    self:ReadSpecialRedPointCfg()
    --self:ActivateRedPoint(_G.REDPOINTEVENT_TYPE.PlayerInfo_Visitor, 1)
end

function RedPointModule:RegisterEvents()
    self:AddCommonEvent(EVENTTYPE_LOGIN , EventIDDefine_Login.EVENTID_LOGIN_SYNC_BASEINFO,self.OnSyncBaseInfo, self)
    --self:AddNetEvent(NetDefine.eGetPlayerRedDotRsp, self.OnGetPlayerRedDotEvent, self)
    self:AddNetEvent(NetDefine.eSetPlayerRedDotRsp, self.OnSetPlayerRedDotRspEvent, self)
    self:AddNetEvent(NetDefine.eNotifyHotDotFix, self.OnNotifyHotDotFixEvent, self)
    self:AddNetEvent(NetDefine.eNotifyRedDot, self.OnNotifyRedDotEvent, self)
    self:AddCommonEvent(EVENTTYPE_REDPOINT, EventIDDefine_RedPoint.EVENTID_RedPoint_Push_Add, self.OnAddRedPointDataEvent, self)
    self:AddCommonEvent(EVENTTYPE_REDPOINT, EventIDDefine_RedPoint.EVENTID_RedPoint_Push_Reduce, self.OnReduceRedPointDataEvent, self)
end

function RedPointModule:OnSyncBaseInfo()
    local result = DataMgr.roleData
    if result == nil then
        return
    end
    self:OnGetPlayerRedDotEvent(result.red_dot)
end

function RedPointModule:OnAddRedPointDataEvent(event_type, event_id, redPointEvent, num, isAdd)
    self:ActivateRedPoint(redPointEvent, num, isAdd)
end

function RedPointModule:OnReduceRedPointDataEvent(event_type, event_id, redPointEvent, num, isReduce)
    self:ReduceRedPoint(redPointEvent, num, isReduce)
end

--[[
    监听服务器记录的红点数据（游戏中只会出现一次的红点数据）
    @red_dot={
        red_dot_id=1,
        red_dot_id=0,
    }
]]
function RedPointModule:OnGetPlayerRedDotEvent(red_dot)
    if red_dot then
        for k, v in pairs(red_dot) do
            local num = 0
            if v == 0 then
                num = 1
            end
            self:ActivateRedPoint(k, num)
        end
    end
end

--[[
    设置红点回包;
]]
function RedPointModule:OnSetPlayerRedDotRspEvent(reason, red_dot_id, red_dot_status)
    if reason == 0 then
        red_dot_status = red_dot_status or 0
        self:ActivateRedPoint(red_dot_id, red_dot_status)
    end
end

--[[
    红点修复通知
    @info = {
        [red_dot_id] = status,
		[1000] = 0,
		...
    }
]]
function RedPointModule:OnNotifyHotDotFixEvent(info)
    if info then
        for k, v in pairs(info) do
            local num = 0
            if v == 0 then
                num = 1
            end
            self:ActivateRedPoint(k, num)
        end
    end
end

--[[
    服务器通知红点状态通知
    @info = {
        [red_dot_id] = status,
		[1000] = 0,
		...
    }
]]
function RedPointModule:OnNotifyRedDotEvent(info)
    log_tree("[RedPointModule] OnNotifyRedDotEvent info = ", info)
    if info then
        for k, v in pairs(info) do
            local num = 0
            if v == 0 then
                num = 1
            end
            self:ActivateRedPoint(k, num)
        end
    end
end

--[[
    请求服务器红点记录
]]
function RedPointModule:GetPlayerRedDotReq()
    NetManager.SendPkg(NetDefine.eGetPlayerRedDotReq)
end

--[[
    请求服务器记录红点状态, 0 显示； 1 隐藏
]]
function RedPointModule:SetPlayerRedDotReq(red_point_type, status)
    NetManager.SendPkg(NetDefine.eSetPlayerRedDotReq, red_point_type, status)
end

function RedPointModule:GenerateRedPointCfg()
    local btnConfig = ScriptHelper.GetTable("RedPointConfigData")
    if btnConfig then
        for k, v in pairs(btnConfig) do
            self:GenerateBtnRedPointParam(v.BtnID_1, v.RedPointType_1, v.DisappearType_1, v)
            self:GenerateBtnRedPointParam(v.BtnID_2, v.RedPointType_2, v.DisappearType_2, v)
            self:GenerateBtnRedPointParam(v.BtnID_3, v.RedPointType_3, v.DisappearType_3, v)
            self:GenerateRedEventBtnMap(v)
        end
    end
end

function RedPointModule:GenerateRedEventBtnMap(cfg)
    if not cfg then return end
    local map = self.RedEventBtnMap[cfg.RedPointEventType] or {}
    map[cfg.BtnID_1] = ""
    map[cfg.BtnID_2] = ""
    map[cfg.BtnID_3] = ""
    self.RedEventBtnMap[cfg.RedPointEventType] = map
end

function RedPointModule:RegisterRedPointUI(btn_id, btn_ui)
    for k, v in pairs(self.RedEventBtnMap) do
        for kk, vv in pairs(v) do
            if kk == btn_id then
                v[btn_id] = btn_ui
            end
        end
    end
end

function RedPointModule:UnRegisterRedPointUI(btn_id)
    for k, v in pairs(self.RedEventBtnMap) do
        for kk, vv in pairs(v) do
            if kk == btn_id then
                v[btn_id] = "btn_ui"
            end
        end
    end
end

function RedPointModule:GenerateBtnRedPointParam(btn_id, red_point_type, disappear_type, cfg)
    if self:CheckBtnID(btn_id) then
        local btnParam = BtnRedPointParam:New()
        btnParam.ButtonID = btn_id
        btnParam.RedPointType = red_point_type
        btnParam.DisappearType = disappear_type
        btnParam.RedPointEvent = cfg.RedPointEventType
        btnParam.MiniLevel = cfg.MiniLevel
        btnParam.MaxLevel = cfg.MaxLevel
        btnParam:SetShowTime(cfg.BeginTime, cfg.OverTime)
        local redMap = self.BtnRedPointDataMap[btn_id] or {}
        table.insert(redMap, btnParam)
        self.BtnRedPointDataMap[btn_id] = redMap
        return btnParam
    end
    return nil
end

function RedPointModule:ReadSpecialRedPointCfg()
    local cfg = require("Client.UMG.RedPointModule.RedPointSpecialConfig")
    if cfg then
        for k, v in pairs(cfg) do
            self:ActivateRedPoint(k, v)
        end
    end
end

function RedPointModule:CheckBtnID(btn_id)
    if btn_id and type(btn_id) == "number" and btn_id > 0 then
        return true
    else
        return false
    end
end
--[[
    激活红点
    @redPointEvent：红点事件类型对应_G.REDPOINTEVENT_TYPE枚举值
    @num：红点数量
    @isAdd：是否叠加 默认为true，为true时 会在对应红点事件类型的数量基础上加上num值，false直接替换
]]
function RedPointModule:ActivateRedPoint(redPointEvent, num, isAdd)
    if num == nil then
        num = 1
    end

    if isAdd == nil then
        isAdd = true
    end
    self:SetBtnRedPointInfo(redPointEvent, num, isAdd)
end

--[[
    减去红点个数
    @redPointEvent：红点事件类型对应_G.REDPOINTEVENT_TYPE枚举值
    @num：红点数量
    @isReduce：是否减少，默认为true，为true时 会在对应红点事件类型的数量基础上减少num值，false直接替换
]]
function RedPointModule:ReduceRedPoint(redPointEvent, num, isReduce)
    if num == nil then
        num = 1
    end

    if isReduce == nil then
        isReduce = true
    end
    local reduceNum = num
    if isReduce then 
        reduceNum = -num
    end
    self:SetBtnRedPointInfo(redPointEvent, reduceNum, isReduce)
end

function RedPointModule:SetBtnRedPointInfo(redPointEvent,num, isAdd)
    if isAdd == nil then
        isAdd = true
    end
    
    for k, v in pairs(self.BtnRedPointDataMap) do
        for kk, vv in pairs(v) do
            if vv.RedPointEvent == redPointEvent then
                local lastNum = vv.ShowNum
                if isAdd then
                    vv.ShowNum = vv.ShowNum + num
                else
                    vv.ShowNum = num
                end

                if vv.ShowNum <= 0 then
                    vv.ShowNum = 0
                    vv.IsShow = false
                elseif lastNum < vv.ShowNum then
                    vv.IsShow = true
                end
                self:UpdateRedPointStatus(redPointEvent)
            end
        end
    end
    --发送红点刷新事件;
    --EventSystem:PostEvent(EVENTTYPE_REDPOINT, EventIDDefine_RedPoint.EVENTID_RedPoint_Update, nil)
end

function RedPointModule:UpdateRedPointStatus(red_event_type)
    local map = self.RedEventBtnMap[red_event_type]
    if map then
        for k, v in pairs(map) do
            if v.ShowRedPoint then
                v:ShowRedPoint()
            end
        end
    end
end

--过滤红点显示，获取最高显示权的红点数据;
function RedPointModule:CheckBtnRedPointShow(btnID)
    local btnParam = nil
    local params = self.BtnRedPointDataMap[btnID]
    if params then
        for k, v in pairs(params) do
            if v then
                if btnParam == nil and v.IsShow then
                    btnParam = v
                end
                if v.IsShow then
                    if btnParam and btnParam.IsShow then
                        if btnParam.RedPointType < v.RedPointType then
                            btnParam = v
                        end
                    else
                        btnParam = v
                    end
                end
            end
        end
    end
    return btnParam
end

--按钮点击红点数据处理;
function RedPointModule:OnClickBtnEvent(btnID)
    local params = self.BtnRedPointDataMap[btnID]
    if params == nil then
        return
    end
    for k, v in pairs(params) do
        if v.DisappearType == _G.REDPOINTDISAPPEAR_TYPE.CLICK then
            v.IsShow = false
        end
    end
    for k, v in pairs(self.RedEventBtnMap) do
        for kk, vv in pairs(v) do
            if kk == btnID and vv.ShowRedPoint then
                vv:ShowRedPoint()
            end
        end
    end
    --EventSystem:PostEvent(EVENTTYPE_REDPOINT, EventIDDefine_RedPoint.EVENTID_RedPoint_Update, btnID)
end

function RedPointModule:BtnHasRedPointCfg(btnID)
    local params = self.BtnRedPointDataMap[btnID]
    return params ~= nil
end

--- 保存红点数据到本地
--- 参数说明：modelKey 所属模块；event_Type: REDPOINTEVENT_TYPE；state：0 显示； 1 隐藏
---
function RedPointModule:SetRedPointToLocal(modelKey, event_Type, state)
    local needSave = false
    if self.RedPointLocalData then
        if self.RedPointLocalData[modelKey] then
            if self.RedPointLocalData[modelKey][event_Type] then
                if self.RedPointLocalData[modelKey][event_Type] ~= state then
                    self.RedPointLocalData[modelKey][event_Type] = state
                    needSave = true
                end
            else
                if state == 0 then
                    self.RedPointLocalData[modelKey][event_Type] = state
                    needSave = true
                end
            end
        else
            if state == 0 then
                self.RedPointLocalData[modelKey] = {}
                self.RedPointLocalData[modelKey][event_Type] = 0
                needSave = true
            end
        end
    else
        if state == 0 then
            self.RedPointLocalData = {}
            self.RedPointLocalData[modelKey] = {}
            self.RedPointLocalData[modelKey][event_Type] = 0
            needSave = true
        end
    end
    if needSave then
        self:SaveRedPointDataToLocal()
    end
end

function RedPointModule:SaveRedPointDataToLocal()
    if self.RedPointLocalData then
        local data  ={}
        for m,v in pairs(self.RedPointLocalData) do
            if not data[m] then
                data[m] = {}
            end

            for t, s in pairs(v) do
                --log ("RedPoint Local Data: ", m, t, s)
                if s == 0 then
                    data[m][t] = 0
                end
            end
        end

        --log_tree("Save RedPointData to local 1", self.RedPointLocalData)
        --log_tree("Save RedPointData to local 2", data)
        self.RedPointLocalData = data

        self.configJson = json.encode(self.RedPointLocalData)
        ScriptHelper.SaveStringToFile(self.configJson, DataMgr.roleData.openID .. '_RedPointData.json')
    end
end


function RedPointModule:LoadRedPointDataFromLocal()
    self.RedPointLocalData = nil
    self.configStr = ScriptHelper.LoadFileToString(DataMgr.roleData.openID .. '_RedPointData.json')
    if self.configStr ~= "" then
        local data = json.decode(self.configStr)

        if data then
            self.RedPointLocalData = {}
            for m,v in pairs(data) do
                self.RedPointLocalData[m] = {}
                for t, s in pairs(v) do
                    if s == 0 then
                        local k = tonumber(t)
                        self.RedPointLocalData[m][k] = 0
                        self:OnAddRedPointDataEvent(_,_, k, 1, true)
                    end
                end
            end
        end
    end
end



return RedPointModule