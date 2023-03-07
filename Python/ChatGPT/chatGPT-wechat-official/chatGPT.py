import configparser
from revChatGPT.V1 import Chatbot

from util import getLogger


config = configparser.ConfigParser()
config.read("config.ini")
chatGPT = config["chatGPT"]

logger = getLogger("chatGPT")


class ChatGPT:
    _instance = None
    chatbot = None

    def __new__(cls, *args, **kw):
        if cls._instance is None:
            cls._instance = object.__new__(cls, *args, **kw)
        return cls._instance

    def __init__(self) -> None:
        if self.chatbot == None:
            try:
                self.chatbot = Chatbot(
                    config={
                        "access_token": chatGPT["AccessToken"],
                    }
                )
            except Exception as e:
                logger.error(f"chatGPT err: {e}")
                raise ValueError(e)

    def sendMessage(self, input):
        try:
            for data in self.chatbot.ask(
                input,
                conversation_id=chatGPT["ConversationId"],
                parent_id=chatGPT["ParentId"],
            ):
                result = data["message"]
        except Exception as e:
            result = "Error happend!"
            logger.error(e)

        # log message
        logger.info(f"Q::{input}")
        logger.info(f"A::{result}")
        print()
        return result


def makeAnswer(input):
    chat_gpt = ChatGPT()
    result = chat_gpt.sendMessage(input)
    return result
