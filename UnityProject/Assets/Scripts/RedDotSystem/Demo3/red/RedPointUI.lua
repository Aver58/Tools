local RedPointUI = GenerateUIClass()

local RedPointModule = CoreManager:GetManagerByName("RedPointModule")

function RedPointUI:OnInitialize()
    log("LobbyUI:OnInitialize")
    self.Super.OnInitialize(self)
    self:HideAll()
end

-- function RedPointUI:RegistEvents()
--     self:AddCommonEvent(EVENTTYPE_REDPOINT, EventIDDefine_RedPoint.EVENTID_RedPoint_Update, self.OnRedPointUpdateEvent, self)
-- end

function RedPointUI:SetBtnID(btnID)
    self.BtnID = btnID
    RedPointModule:RegisterRedPointUI(btnID, self)
    self:ShowRedPoint()
end

function RedPointUI:OnRedPointUpdateEvent(eventType, eventID, event_param)
    if event_param and event_param == self.BtnID then
        self:ShowRedPoint()
    elseif event_param == nil then
        self:ShowRedPoint()
    end
end

function RedPointUI:ShowRedPoint()
    if not self.Normal then
        return
    end
    local btnRedPontInfo = RedPointModule:CheckBtnRedPointShow(self.BtnID)
    if btnRedPontInfo then
        self.Normal:SetVisibility(UIUtil.BoolToVisible(btnRedPontInfo.RedPointType==_G.REDPOINT_TYPE.NORMAL))
        self.Num:SetVisibility(UIUtil.BoolToVisible(btnRedPontInfo.RedPointType==_G.REDPOINT_TYPE.NUM))
        if btnRedPontInfo.RedPointType==_G.REDPOINT_TYPE.NUM then
            self.NumText:SetText(tostring(btnRedPontInfo.ShowNum))
        end
    else
        self:HideAll()
    end
end

function RedPointUI:HideAll()
    self.Normal:SetVisibility(ESlateVisibility.Collapsed)
    self.Num:SetVisibility(ESlateVisibility.Collapsed)
end

function RedPointUI:ChangeRedPoint(State)
    if not self.Normal then
        return
    end
    self.Normal:SetVisibility(UIUtil.BoolToVisible(State))
end

function RedPointUI:ChangeRedNum(Num)
    if not self.Num then
        return
    end
    if Num > 0 then 
        self.Num:SetVisibility(UIUtil.BoolToVisible(true))
        self.NumText:SetText(tostring(Num))
    else
        self.Num:SetVisibility(UIUtil.BoolToVisible(false))
    end

end

function RedPointUI:Destruct()
    local BtnID = self.BtnID
    self:OnDestroy()
    RedPointModule:UnRegisterRedPointUI(BtnID)
    self.Destroy(self)
end

return RedPointUI