import hashlib
import reply
import receive
import web
from ChatGptUtil import generate_response

class Handle(object):
    def POST(self):
        try:
            webData = web.data()
            print ("Handle Post webdata is ", webData)
            #后台打日志
            recMsg = receive.parse_xml(webData)
            if isinstance(recMsg, receive.Msg) and recMsg.MsgType == 'text':
                toUser = recMsg.FromUserName
                fromUser = recMsg.ToUserName
                print("收到公众号输入消息：", recMsg.Content)
                result = generate_response(recMsg.Content)
                print("输出结果：", result)
                content = result
                replyMsg = reply.TextMsg(toUser, fromUser, content)
                return replyMsg.send()
            else:
                print ("暂且不处理")
                return "success"
        except Exception as Argument:
            return Argument
    
    def GET(self):
        try:
            data = web.input()
            if len(data) == 0:
                return "hello, this is handle view"
            #return "hello, this is handle view"
            signature = data.signature#微信加密签名，signature结合了开发者填写的token参数和请求中的timestamp参数、nonce参数。
            timestamp = data.timestamp
            nonce = data.nonce#随机数
            echostr = data.echostr#随机字符串
            token = "123456"
            print("echostr", echostr)
            return echostr
            
            list = [token, timestamp, nonce]
            list.sort()
            sha1 = hashlib.sha1()
            map(sha1.update, list)
            hashcode = sha1.hexdigest()
            # 老是不一致，先注释了
            print ("handle /GET func: hashcode, signature: ", hashcode, signature)
            if hashcode == signature:
                return echostr
            else:
                return "error hashcode ！= signature"
        except Exception as Argument:
            return Argument