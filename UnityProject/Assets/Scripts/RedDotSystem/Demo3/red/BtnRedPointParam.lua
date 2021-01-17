--[[
    按钮红点数据结构表
    包含了一些简单的判断函数
    {
        InShowTime;     --判断是否在显示时间内
        IsAchieveLevel; --判断是否达到显示等级
        IsShow;         --判断红点是否需要显示
        MinLevel;          --红点可显示的最低等级;
        ButtonID;
        RedPointEvent;
        DisappearType;
        RedPointType;
        ShowNum;
    }
--]]
local BtnRedPointParam = {}

function BtnRedPointParam:New()
    local obj = {
        BeginTime = 0;    --红点显示开始时间
        EndTime = 0; --红点显示结束时间
        IsShow = false;         --判断红点是否需要显示
        MiniLevel = 1;          --红点可显示的最低等级;
        MaxLevel = 99;  --红点显示最高等级;
        ButtonID = 10000;
        RedPointEvent = _G.REDPOINTEVENT_TYPE.NONE;
        DisappearType = _G.REDPOINTDISAPPEAR_TYPE.CLICK;
        RedPointType = _G.REDPOINT_TYPE.NORMAL;
        ShowNum = 0;
    }
    setmetatable(obj, self)
    self.__index = self
    return obj
end

function BtnRedPointParam:CanShowRedPoint()
    return self:CheckShowLevel() and self.IsShow and self:CheckShowTime()
end

function BtnRedPointParam:CheckShowLevel()
    return DataMgr.roleData.level >= self.MiniLevel and DataMgr.roleData.level<= self.MaxLevel
end

function BtnRedPointParam:CheckShowTime()
    local inShowTime = true
    if self.BeginTime == 0 and self.EndTime == 0 then
        inShowTime = true
    elseif self.BeginTime > 0 and self.EndTime == 0 then
        local nowTime = TimeUtil.GetServerTimeInSec()
        inShowTime = nowTime >= self.BeginTime
    elseif self.BeginTime == 0 and self.EndTime > 0 then
        local nowTime = TimeUtil.GetServerTimeInSec()
        inShowTime = nowTime <= self.EndTime
    else
        local nowTime = TimeUtil.GetServerTimeInSec()
        inShowTime = nowTime >= self.BeginTime and nowTime <= self.EndTime
    end
    return inShowTime
end

function BtnRedPointParam:SetShowTime(showTime, endTime)
    if endTime == nil or string.len(endTime) == 0 then
        self.mInShowTime = true;
    else
        self.BeginTime = TimeUtil.TimeStringToUnixstamp(showTime)
        self.EndTime = TimeUtil.TimeStringToUnixstamp(endTime)
    end
end

return BtnRedPointParam