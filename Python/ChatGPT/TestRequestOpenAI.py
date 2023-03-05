import openai

# 将YOUR_API_KEY替换成自己的API Key
openai.api_key = YOUR_API_KEY

# 指定请求的模型ID和文本
model_engine = "davinci"  # 模型ID，可选davinci、curie、babbage等
prompt_text = "Hello, ChatGPT!"

# 发送API请求，获取响应
response = openai.Completion.create(
    engine=model_engine,
    prompt=prompt_text,
    max_tokens=5
)

# 解析响应结果
if response.choices[0].text:
    answer = response.choices[0].text.strip()
    print(answer)
else:
    print("No response received")

# print(response.json())