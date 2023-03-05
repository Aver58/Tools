import werobot
import openai
openai.api_key = "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"
robot = werobot.WeRoBot()
class RobotConfig(object):
    HOST="127.0.0.1"
    PORT= 80
    TOKEN = "123456"
robot.config.from_object(RobotConfig)
def generate_response(prompt):
    response = openai.Completion.create(
        model="text-davinci-003",
        prompt=prompt,
        temperature=0.7,
        max_tokens=3000,
        top_p=1,
        frequency_penalty=0,
        presence_penalty=0
    )
    message = response.choices[0].text
    print(message.strip())
    return message.strip()

@robot.handler
def hello (messages):
    print(messages.content)
    return generate_response(messages.content)

if __name__ == "__main__":
    #robot.run()
    print(generate_response("test"))