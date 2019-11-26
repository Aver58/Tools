import sys
projectPath = 'F:\\PythonLearn'
if projectPath not in sys.path:
    sys.path.append(projectPath)
from SeleniumLearn.SeleniumUtil import SeleniumUtil
from Helper.MailHelper import MailHelper
from Helper.IOHelper import IOHelper
from Helper.DingDingNotice import DingDingNotice


class SendWeatherMail(SeleniumUtil):
    def MainLogic(self):
        self.GetUrl('http://www.nmc.cn/publish/forecast/AFJ/fuzhou3.html')
        driver = self._driver
        ele_baseWeather = driver.find_element_by_css_selector("body > div:nth-child(3) > div.container > div.real")
        ele_Day0 = driver.find_element_by_css_selector("#day0")
        ele_realFeelst = driver.find_element_by_css_selector("#realFeelst").text
        ele_realHumidity = driver.find_element_by_css_selector("#realHumidity").text
        ele_realRain = driver.find_element_by_css_selector("#realRain").text
        ele_realIcomfort = driver.find_element_by_css_selector("#realIcomfort > span").text
        ele_aqi = driver.find_element_by_css_selector("#aqi > span").text
        imgName = "imageToSave1.png"
        imgName2 = "imageToSave2.png"
        IOHelper().Write(imgName, ele_baseWeather.screenshot_as_png, 'wb')
        IOHelper().Write(imgName2, ele_Day0.screenshot_as_png, 'wb')

        title = '%s %s 降水:%s 空气%s，湿度%s' % \
                (ele_realFeelst, ele_realIcomfort, ele_realRain, ele_aqi, ele_realHumidity)
        # 构建邮件正文
        MailHelperInst = MailHelper()
        msg = MailHelperInst.BuildMsg(title)
        MailHelperInst.AttachMultipleImageInBody(msg, imgName, imgName2)
        MailHelperInst.RealSendMail(msg)

        # 发送钉钉
        DingDingNoticeInst = DingDingNotice()
        DingDingNoticeInst.SendTextMessage(title)
        driver.close()


if __name__ == '__main__':
    SendWeatherMail = SendWeatherMail()
    SendWeatherMail.MainLogic()
