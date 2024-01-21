import asyncio
import json
import logging
from common import send_request_to_storage_server

# Setting up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("LoginServer")


async def handle_login_request(reader, writer):
    data = await reader.read(1024)
    message = data.decode()

    try:
        request = json.loads(message)

        if request.get('action') == 'check_username':
            response = await send_request_to_storage_server('check_username', {"username": request['username']})
        elif request.get('action') in ['login', 'register']:
            response = await send_request_to_storage_server(request.get('login_action'), request)
        else:
            response = {'error': 'Invalid request'}

        writer.write(json.dumps(response).encode())
    except json.JSONDecodeError:
        writer.write(json.dumps({'error': 'Invalid JSON'}).encode())

    await writer.drain()
    writer.close()


async def start_login_server(host='0.0.0.0', port=12346):
    server = await asyncio.start_server(handle_login_request, host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Login server listening on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(start_login_server())
