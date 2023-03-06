import json
import requests
import asyncio
import aiohttp
 
# https://platform.openai.com/docs/api-reference/chat/create
model_engine = "gpt-3.5-turbo-0301"  # gpt-3.5-turbo and gpt-3.5-turbo-0301
YOUR_API_KEY = ''
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

async def async_request(content):
    async with aiohttp.ClientSession() as session:
        data = {
            'model': model_engine,
            'messages': [{"role": "user", "content": content.decode("utf-8")}]
        }
        async with session.post(url, headers=headers, json=data) as response:
            result = await response.text()
            return result

try:
    result = asyncio.run(asyncio.wait_for(async_request(b'Hello'), timeout=30))
    print(result)
except asyncio.TimeoutError:
    print('Async request timed out')