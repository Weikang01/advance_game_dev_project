import asyncio
import logging
import json
from common import send_request_to_storage_server

# Additional imports for database handling, etc.

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("GameServer")


# Functions to handle specific data requests
async def load_player_profile(username):
    return await send_request_to_storage_server("load_profile", {"username": username})


async def load_character_data(username):
    return await send_request_to_storage_server("load_character_data", {"username": username})


async def load_friend_list(username):
    return await send_request_to_storage_server("load_friends", {"username": username})


async def create_player_profile(action, username, display_name, avatar_url, level, experience):
    profile_data = {
        "username": username,
        "display_name": display_name,
        "avatar_url": avatar_url,
        "level": level,
        "experience": experience
    }
    return await send_request_to_storage_server("create_profile", profile_data)


async def update_player_profile(action, username, display_name=None, avatar_url=None, level=None, experience=None):
    update_data = {
        "username": username,
        "display_name": display_name,
        "avatar_url": avatar_url,
        "level": level,
        "experience": experience
    }
    return await send_request_to_storage_server("update_profile", update_data)


async def handle_game_request(reader, writer):
    data = await reader.read(1024)
    message = data.decode()
    response = {}

    try:
        request = json.loads(message)

        if request.get("action") == "load_profile":
            username = request['username']
            response = await load_player_profile(username)
        elif request.get("action") == "load_character_data":
            username = request['username']
            response = await load_character_data(username)
        elif request.get("action") == "load_friends":
            username = request['username']
            response = await load_friend_list(username)
        elif request.get("action") == "create_profile":
            # Extract data from request
            response = await create_player_profile(**request)
        elif request.get("action") == "update_profile":
            # Extract data from request
            response = await update_player_profile(**request)
        else:
            response = {'error': 'Invalid request'}

    except json.JSONDecodeError:
        response = {'error': 'Invalid JSON'}

    writer.write(json.dumps(response).encode())
    await writer.drain()
    writer.close()


async def start_game_server(host='0.0.0.0', port=12348):
    server = await asyncio.start_server(handle_game_request, host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Game server listening on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(start_game_server())
