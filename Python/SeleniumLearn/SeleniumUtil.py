import time
from selenium import webdriver
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.common.touch_actions import TouchActions
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.common.by import By
from Helper.BaseSetting import download_directory
import datetime


class SeleniumUtil(object):
    def __init__(self):
        options = webdriver.ChromeOptions()
        prefs = {'profile.default_content_settings.popups': 0, 'download.default_directory': download_directory}
        options.add_experimental_option('prefs', prefs)
        options.add_argument('lang=zh_CN.UTF-8')
        # options.add_argument('--headless') # 后台运行，不开界面 ，这个在jenkins上为啥就失败了？？？
        driver = webdriver.Chrome(chrome_options=options)
        # 添加隐式等待300秒 当查找元素或元素并没有立即出现的时候，隐式等待将等待一段时间再查找 DOM，默认的时间是0
        driver.implicitly_wait(30)
        driver.maximize_window()
        self._driver = driver
        wait = WebDriverWait(driver, 30) # 20分钟等待，已经很过分了
        self._wait = wait
        self.WebDriverWait = WebDriverWait
        # Action = TouchActions(driver)
        self._ActionChains = ActionChains

    def GetUrl(self, url):
        driver = self._driver
        driver.get(url)
        print("打开网页：", driver.title)

    def LoginOnemt(self, url, username, password):
        self.GetUrl(url)
        driver = self._driver
        element_username = driver.find_element_by_name("username")
        print("element_username 搜索结果：", element_username)
        element_username.send_keys(username)
        element_password = driver.find_element_by_name("password")
        print("element_password 搜索结果：", element_password)
        element_password.send_keys(password)

        BtnCommit = driver.find_element_by_id("btnSubmit")
        print("登录按钮 搜索结果：", BtnCommit)
        BtnCommit.click()

    def find_element_by_css_selector(self, css_selector, printName=None):
        driver = self._driver
        # css_selector 可以用谷歌的自带的 检查 - copy selector
        element = driver.find_element_by_css_selector(css_selector)
        print("%s 搜索css_selector结果：%s" % (printName, element))
        return element

    def find_element_by_class_name(self, class_name, printName=None):
        driver = self._driver
        element = driver.find_element_by_class_name(class_name)
        print("%s 搜索class_name结果：%s" % (printName, element))
        return element

    def sleep(self, times):
        time.sleep(times)

    def quit(self):
        self._driver.quit()

    # 所有下载完成
    @staticmethod
    def every_downloads_chrome(driver):
        if not driver.current_url.startswith("chrome://downloads"):
            driver.get("chrome://downloads/")
        return driver.execute_script("""
            var items = downloads.Manager.get().items_;
            if (items.every(e => e.state === "COMPLETE"))
                return items.map(e => e.file_url);
            """)

    def waitByXPath(self, xPath, printName=None):
        element = self._wait.until(EC.element_to_be_clickable((By.XPATH, xPath)))
        print("%s 搜索waitByXPath结果：%s" % (printName, element))
        return element

    def waitByCSS(self, css, printName=None):
        element = self._wait.until(EC.element_to_be_clickable((By.CSS_SELECTOR, css)))
        print("%s 搜索waitByCSS结果：%s" % (printName, element))
        return element

    def waitByCSSWithTime(self, css, printName=None, time=3):
        element = WebDriverWait(self._driver, time, 0.5).until(EC.element_to_be_clickable((By.CSS_SELECTOR, css)))
        print("%s 搜索waitByCSS结果：%s" % (printName, element))
        return element

    # 等待所有下载结束
    def waitUntilAllDownloaded(self):
        tryTimes = 0
        paths = None
        while paths == None and tryTimes < 5:
            try:
                tryTimes = tryTimes + 1
                print("开始第%s次尝试等待下载结束" % tryTimes)
                paths = self._wait.until(self.every_downloads_chrome)
            except:
                continue

        print("成功等待所有下载结束！！")
        return paths

    def waitByClassName(self, strClassName):
        tryTimes = 0
        element = None
        while element == None and tryTimes < 5:
            try:
                tryTimes = tryTimes + 1
                print("开始第%s次尝试等待搜索结果，类名：%s" % (tryTimes, strClassName))
                element = self._wait.until(EC.element_to_be_clickable((By.CLASS_NAME, strClassName)))
            except:
                continue
        print("成功等待到%s 元素" % strClassName)
        return element

    @staticmethod
    def GetYesterday():
        yesterday = datetime.date.today() + datetime.timedelta(-1)
        return yesterday

    @staticmethod
    def GetToday():
        return datetime.date.today()