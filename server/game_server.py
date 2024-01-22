import asyncio
import logging
import json
from common import send_request_to_storage_server, send_request_to_caching_server

# Additional imports for database handling, etc.

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("GameServer")


# Functions to handle specific data requests
async def load_player_profile(username):
    # First try to get the profile from cache
    cached_profile = await send_request_to_caching_server("get_cached_player_profile", {"username": username})
    if cached_profile:
        return cached_profile
    # If not in cache, load from storage server
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
    # Save profile to storage server and cache
    response = await send_request_to_storage_server("create_profile", profile_data)
    await send_request_to_caching_server("cache_player_profile", {"username": username, "profile_data": profile_data})
    return response


async def update_player_profile(action, username, display_name=None, avatar_url=None, level=None, experience=None):
    update_data = {
        "username": username,
        "display_name": display_name,
        "avatar_url": avatar_url,
        "level": level,
        "experience": experience
    }
    # Update profile in storage server and cache
    response = await send_request_to_storage_server("update_profile", update_data)
    await send_request_to_caching_server("cache_player_profile", {"username": username, "profile_data": update_data})
    return response


async def handle_game_request(reader, writer):
    data = await reader.read(1024)
    message = data.decode()

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
        elif request.get("action") == "get_room_list":
            response = await send_request_to_caching_server("get_room_list", {})
        elif request.get("action") == "get_user_profiles_in_room":
            room_id = request['room_id']
            response = await send_request_to_caching_server("get_user_profiles_in_room", {"room_id": room_id})
        elif request.get("action") == "create_room":
            room_id = request['room_id']
            room_data = request['room_data']
            username = request['username']  # Assume username is provided in the request
            response = await send_request_to_caching_server("create_room", {"room_id": room_id, "room_data": room_data,
                                                                            "username": username})
        elif request.get("action") == "enter_room":
            room_id = request['room_id']
            password = request.get('password')
            username = request['username']

            response = await send_request_to_caching_server("enter_room", {"room_id": room_id, "username": username,
                                                                           "password": password})
        elif request.get("action") == "leave_room":
            room_id = request['room_id']
            username = request['username']
            response = await send_request_to_caching_server("leave_room", {"room_id": room_id, "username": username})
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
