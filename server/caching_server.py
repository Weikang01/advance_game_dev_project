import asyncio
import logging
import json
from redis_manager import RedisManager

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("CachingServer")

redis_manager = RedisManager()


async def handle_request(reader, writer):
    data = await reader.read(1024)
    message = data.decode()

    try:
        request = json.loads(message)

        if request.get("action") == "get_session":
            address = request['address']
            response = await redis_manager.get_session(address)
        elif request.get("action") == "set_session":
            address = request['address']
            session_data = request['session_data']
            response = await redis_manager.set_session(address, session_data)
        elif request.get("action") == "cache_player_profile":
            username = request['username']
            profile_data = request['profile_data']
            response = await redis_manager.cache_player_profile(username, profile_data)
        elif request.get("action") == "get_cached_player_profile":
            username = request['username']
            response = await redis_manager.get_cached_player_profile(username)
        elif request.get("action") == "update_last_activity":
            address = request['address']
            response = await redis_manager.update_last_activity(address)
        elif request.get("action") == "is_session_expired":
            address = request['address']
            response = await redis_manager.is_session_expired(address)
        elif request.get("action") == "create_room":
            room_id = request['room_id']
            room_data = request['room_data']
            username = request['username']
            response = await redis_manager.create_room(room_id, room_data, username)
        elif request.get("action") == "get_room_list":
            response = await redis_manager.get_room_list()
        elif request.get("action") == "get_room_data":
            room_id = request['room_id']
            response = await redis_manager.get_room_data(room_id)
        elif request.get("action") == "enter_room":
            room_id = request['room_id']
            username = request['username']
            password = request['password']
            response = await redis_manager.enter_room(room_id, username, password)
        elif request.get("action") == "get_user_profiles_in_room":
            room_id = request['room_id']
            response = await redis_manager.get_user_profiles_in_room(room_id)
        elif request.get("action") == "leave_room":
            room_id = request['room_id']
            username = request['username']
            response = await redis_manager.leave_room(room_id, username)
        else:
            response = {'error': 'Invalid request'}

    except json.JSONDecodeError:
        response = {'error': 'Invalid JSON'}

    writer.write(json.dumps(response).encode())
    await writer.drain()
    writer.close()


async def start_caching_server(host='0.0.0.0', port=12349):
    server = await asyncio.start_server(handle_request, host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Caching server listening on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(start_caching_server())
