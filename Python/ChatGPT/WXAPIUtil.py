import requests
import time
from urllib import request
import json

# 微信开发者文档：https://developers.weixin.qq.com/doc/offiaccount/Message_Management/Service_Center_messages.html#%E5%AE%A2%E6%9C%8D%E6%8E%A5%E5%8F%A3-%E5%8F%91%E6%B6%88%E6%81%AF

# 客服接口-发消息
# http请求方式: POST 

grant_type = "client_credential"
appid = "xxx"
appsecret = "xxx"

baseSupportUrl = "https://api.weixin.qq.com/cgi-bin/token?grant_type={grant_type}&appid={appid}&secret={appsecret}"

def PostToUser(openId, content):
    url_postToUser = "https://api.weixin.qq.com/cgi-bin/message/custom/send?access_token={ACCESS_TOKEN}"
    postData = {
        "touser": openId,
        "msgtype":"text",
        "text":{
             "content": content
        }
    }

    requests.post(url_postToUser, json=postData)

response = requests.post(baseSupportUrl)

def make_request(url, **kwargs):
    req = request.Request(url=url, **kwargs)
    response = request.urlopen(req)
    data = response.read().decode("utf-8")
    result = json.loads(data)
    code = result.get("errcode")
    message = result.get("errmsg")
    if code and code != 0:
        raise ValueError(message)
    return result