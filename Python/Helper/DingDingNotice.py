import json
import requests

ROBOT_DEFINE = {
    '机器人总动员': "https://oapi.dingtalk.com/robot/send?access_token=e221daeb201d111f7255da8b3ba0f68aa72edb16b74ed9bd2502bee20a5c5219",
    'testRobot': "https://oapi.dingtalk.com/robot/send?access_token=3fab04359eec2d32419a49fc91135841999e28ac1a260c06694d8d1fd53976ee",
}

headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36',
           'Content-Type': 'application/json'}


class DingDingNotice(object):
    def __init__(self):
        pass

    # https://open-doc.dingtalk.com/microapp/serverapi2/qf2nxq#-3
    #发送钉钉消息
    def SendTextMessage(self, content, robot='机器人总动员'):
        data = {
            "msgtype": "text",
            "text": {"content": content},
            "isAtAll": False
            }
        ret = self.SendPostRequest(data, robot)
        print("Response:", ret.text)

    # 发送文件
    # def SendFileMessage(self):
    #     data = {
    #         "msgtype": "file",
    #         "file": {
    #             "media_id": "MEDIA_ID"
    #         }
    #     }
    #     ret = self.SendPostRequest(data)
    #     print("Response:", ret.text)

    def SendLinkMessage(self, title, content, robot, messageUrl=None, picUrl=None):
        print("title:%s, content:%s" % (title, content))
        data = {"msgtype": "link",
                "link": {
                    "text": content,
                    "title": title,
                    "messageUrl": messageUrl,
                    "picUrl": picUrl,
                    "isAtAll": "false"}}

        ret = self.SendPostRequest(data, robot)
        print("Response:", ret.text)

    def SendMarkDownMessage(self, title, content, messageUrl=None):
        data = {"msgtype": "markdown",
                "markdown": {
                    "title": title,
                    "text": content,
                    "messageUrl": messageUrl,
                    "isAtAll": "false"}}
        ret = self.SendPostRequest(data)
        print("Response:", ret.text)

    @staticmethod
    def SendPostRequest(data, robot='机器人总动员'):
        print("Send data:", data)
        posturl = ROBOT_DEFINE[robot]
        response = requests.post(url=posturl, headers=headers,  data=json.dumps(data))
        return response