## WeChat official access chatGPT

### dev

1.  create venv `python -m venv .venv`
2.  rename `example.ini` to `config.ini`
3.  run `pip install -U -r requirements.txt`
4.  run `python main.py`

### deploy

1.  run `docker compose build`
2.  run `docker compose up -d`

### note

-   [Customer Service API - Send Message](https://developers.weixin.qq.com/doc/offiaccount/en/Message_Management/Service_Center_messages.html) permission is required 需要微信认证才有接口权限，朋友们，微信认证需要营业执照和对公账户，寄
-   The URL that responds to the wechat server is `http[s]://{YOUR DOMAIN}/wx`
-   We can request an API [Test Account](https://mp.weixin.qq.com/debug/cgi-bin/sandbox?t=sandbox/login)
-   For wechat offical configuration please refer to [here](https://developers.weixin.qq.com/doc/offiaccount/Basic_Information/Access_Overview.html)
-   For chatGPT configuration please refer to [here](https://github.com/acheong08/ChatGPT/blob/main/README.md)

### credit

-   [ChatGPT for python](https://github.com/acheong08/ChatGPT)
