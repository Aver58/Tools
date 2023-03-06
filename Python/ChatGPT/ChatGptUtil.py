import json
import requests
import asyncio
import aiohttp
 
YOUR_API_KEY = ''
# ai开发文档：https://platform.openai.com/docs/api-reference/chat/create
model_engine = "gpt-3.5-turbo"  # gpt-3.5-turbo and gpt-3.5-turbo-0301
url = 'https://api.openai.com/v1/chat/completions'
headers = {
    'Content-Type': 'application/json',
    'Authorization': f'Bearer {YOUR_API_KEY}'
}

def sync_request(content):
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

async def async_request(content):
    async with aiohttp.ClientSession() as session:
        data = {
            'model': model_engine,
            'messages': [{"role": "user", "content": content.decode("utf-8")}]
        }
        async with session.post(url, headers=headers, json=data) as response:
            responseText = await response.text()
            if response.status == 200:
                response_json = json.loads(responseText)
                result = response_json['choices'][0]['message']['content']
                return result.strip()
            else:
                errorStr = f"Error {response.status}: {responseText}"
                print(errorStr)
                return errorStr

def AsyncRequest(content):
    try:
        result = asyncio.run(asyncio.wait_for(async_request(content), timeout=30))
        return result
    except asyncio.TimeoutError:
        return 'Async request timed out'

# print(AsyncRequest(b'Hello'))