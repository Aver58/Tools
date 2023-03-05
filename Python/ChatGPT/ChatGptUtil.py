import json
import requests
import asyncio
 
# https://platform.openai.com/docs/api-reference/chat/create
YOUR_API_KEY = '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'
model_engine = "gpt-3.5-turbo-0301"  # gpt-3.5-turbo and gpt-3.5-turbo-0301
url = 'https://api.openai.com/v1/chat/completions'
headers = {
    'Content-Type': 'application/json',
    'Authorization': f'Bearer {YOUR_API_KEY}'
}

def generate_response(content):
    data = {
        'model': model_engine,
        'messages': [{"role": "user", "content": content.decode("utf-8")}]
    }
    response = requests.post(url, headers=headers, json=data)
    print("response.content：", response.content)
    if response.status_code == 200:
        response_json = json.loads(response.content)
        result = response_json['choices'][0]['message']['content']
        return result.strip()
    else:
        errorStr = f"Error {response.status_code}: {response.text}"
        print(errorStr)
        return errorStr

async def coroutine1():
    print('coroutine1 started')
    await generate_response(b'are you ok?')
    print('coroutine1 ended')

async def main():
    task1 = asyncio.create_task(coroutine1())
    await task1

asyncio.run(main())