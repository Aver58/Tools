import smtplib
from email.mime.text import MIMEText
from email.mime.image import MIMEImage
from email.mime.multipart import MIMEMultipart
from email.header import Header

port = 25
mail_host = "smtp.qq.com"  #设置服务器
mail_user = "XXXXXXXXXX@qq.com"#用户名
mail_pass = "XXXXXXXXXX"#口令,去邮箱smtp获取


class MailHelper(object):
    @staticmethod
    def BuildMsg(title):
        message = MIMEMultipart('related')
        message['Subject'] = Header(title, 'utf-8')
        message['From'] = Header("机器人", 'utf-8')
        message['To'] = Header("测试", 'utf-8')
        return message

    @staticmethod
    def AttachOneText(msg, mail_body):
        msgText = MIMEText(mail_body)
        msg.attach(msgText)

    @staticmethod
    def AttachOneImage(msg, fileName):
        file = open(fileName, "rb")#"e:/py/dns.jpg"
        img_data = file.read()
        file.close()
        img = MIMEImage(img_data)
        msg.add_header('content-disposition', 'attachment', filename=fileName)
        msg.attach(img)

    @staticmethod
    def AttachOneImageInBody(msg, fileName):
        with open(fileName, 'rb') as f:
            msgImage = MIMEImage(f.read())
            # 定义图片 ID，在 HTML 文本中引用
            msgImage.add_header('Content-ID', '<image1>')
            msg.attach(msgImage)
            #正文显示附件图片
            msg.attach(MIMEText('<html><body>' +
                    '<p><img src="cid:image1"></p>' +
                    '</body></html>', 'html', 'utf-8'))

    # 调试发送多张图片的时候遇到的最蛋疼的问题：
    # 用for循环生成的mail_msg，不能直接attach，需要和content一起attach
    @staticmethod
    def AttachMultipleImageInBody(msg, *args):
        body = '<html><body>'
        for fileName in args:
            with open(fileName, 'rb') as f:
                msgImage = MIMEImage(f.read())
                # 定义图片 ID，在 HTML 文本中引用
                msgImage.add_header('Content-ID', '<%s>' % fileName)
                msg.attach(msgImage)
                body = body + '<p><img src="cid:%s"></p>' % fileName
        #正文显示附件图片
        body = body + '</body></html>'
        msg.attach(MIMEText(body, 'html', 'utf-8'))

    @staticmethod
    def AttachOneImageByData(msg, img_data):
        img = MIMEImage(img_data)
        img.add_header('Content-ID', 'dns_config')
        msg.attach(img)

    def RealSendMail(self, message):
        smtpObj = smtplib.SMTP()
        try:
            smtpObj.connect(mail_host, 25)    # 25 为 SMTP 端口号
            smtpObj.login(mail_user, mail_pass)
            smtpObj.sendmail(mail_user, mail_user, message.as_string())
            print("邮件发送成功")
            smtpObj.quit()
        except smtplib.SMTPException:
            print('Error: 无法发送邮件')
            smtpObj.quit()

    def SendImageMailByData(self, title, imgData):
        message = self.BuildMsg(title)
        self.AttachOneImageByData(message, imgData)
        self.RealSendMail(message)

    def SendImageBodyMail(self, title, fileName):
        message = self.BuildMsg(title)
        self.AttachOneImageInBody(message, fileName)
        self.RealSendMail(message)

    def SendImageMail(self, title, fileName):
        message = self.BuildMsg(title)
        self.AttachOneImage(message, fileName)
        self.RealSendMail(message)

    def SendTextMail(self, title, body):
        message = self.BuildMsg(title)
        self.AttachOneText(message, body)
        self.RealSendMail(message)


# print(MailHelper().SendMail("测试标题2222", "测试内容11111"))