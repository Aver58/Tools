import logging
import json
from urllib import request


def head(list):
    return list[0] if list else None


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


def getLogger(name="main", level=logging.INFO):
    logger = logging.getLogger(name)
    fmt = logging.Formatter("%(asctime)s:%(levelname)s:%(name)s:%(message)s")
    logger.setLevel(level)
    # stop multiple log
    logger.propagate = False
    stream_handler = logging.StreamHandler()
    stream_handler.setFormatter(fmt)
    stream_handler.setLevel(level)
    logger.addHandler(stream_handler)
    return logger
