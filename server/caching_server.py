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
        action = request.pop('action')
        request = json.loads(request['session_data'])
        address = f"{request.get('address')[0]}:{request.get('address')[1]}" if type(request.get('address')) == list else request.get('address')
        print(f"Received: {request} action: {action} request['session_data'] {request.get('session_data')} address: {address}")

        if action == "get_session":
            response = await redis_manager.get_session(address)
        elif action == "set_session":
            session_data = request['session_data']
            response = await redis_manager.set_session(address, session_data)
        elif action == "cache_player_profile":
            username = request['username']
            profile_data = request['profile_data']
            response = await redis_manager.cache_player_profile(username, profile_data)
        elif action == "get_cached_player_profile":
            username = request['username']
            response = await redis_manager.get_cached_player_profile(username)
        elif action == "update_last_activity":
            response = await redis_manager.update_last_activity(address)
        elif action == "is_session_expired":
            response = await redis_manager.is_session_expired(address)
        elif action == "create_room":
            room_id = request['room_id']
            room_data = request['room_data']
            username = request['username']
            response = await redis_manager.create_room(room_id, room_data, username)
        elif action == "get_room_list":
            response = await redis_manager.get_room_list()
        elif action == "get_room_data":
            room_id = request['room_id']
            response = await redis_manager.get_room_data(room_id)
        elif action == "enter_room":
            room_id = request['room_id']
            username = request['username']
            password = request['password']
            response = await redis_manager.enter_room(room_id, username, password)
        elif action == "get_user_profiles_in_room":
            room_id = request['room_id']
            response = await redis_manager.get_user_profiles_in_room(room_id)
        elif action == "leave_room":
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
