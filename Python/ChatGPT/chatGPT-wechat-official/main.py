import configparser
import json

from urllib.parse import parse_qs

from util import getLogger
from wechat import Bot
from chatGPT import makeAnswer

logger = getLogger()

config = configparser.ConfigParser()
config.read("config.ini")
port = config["COMMON"].getint("Port")


def application(environ, start_response):
    url = environ.get("PATH_INFO")
    method = environ.get("REQUEST_METHOD")
    qs = parse_qs(environ.get("QUERY_STRING"))
    length = environ.get("CONTENT_LENGTH", "0")
    length = 0 if length == "" else int(length)
    data = environ["wsgi.input"].read(length)

    start_response("200 OK", [("Content-type", "application/json; charset=utf-8")])
    try:
        if url == "/wx":
            if method.lower() == "post" and data:
                # response message
                result = Bot.receive(data.decode())
                return [result.encode("utf-8")]
            # check token
            bot = Bot()
            token = bot.check_token(qs)
            return [token.encode("utf-8")]
        elif url == "/chatgpt":
            if method.lower() == "post" and data:
                answer = makeAnswer(data.decode())
                result = json.dumps(
                    {"code": 0, "data": answer},
                )
                return [result.encode("utf-8")]
        return ["Not Found".encode("utf-8")]
    except Exception as e:
        err = f"{e}"
        logger.error(err)
        return [err.encode("utf-8")]


if __name__ == "__main__":
    from wsgiref.simple_server import make_server

    with make_server("", port, application) as httpd:
        logger.info(f"Serving on port {port}...")
        httpd.serve_forever()
