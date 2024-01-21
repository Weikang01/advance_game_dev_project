import asyncio
import logging
import json
import aioredis
import time

# Setting up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("GatewayServer")

# Define the address of the login server
LOGIN_SERVER_HOST = '127.0.0.1'
LOGIN_SERVER_PORT = 12346

# Define the address of other servers (e.g., game server, storage server)
GAME_SERVER_HOST = '127.0.0.1'
GAME_SERVER_PORT = 12348

REDIS_HOST = 'localhost'  # Redis server address
REDIS_PORT = 6379  # Default Redis port

# Session management
SESSION_TIMEOUT = 1800  # 30 minutes

SESSION_CLEANUP_INTERVAL = 60  # Interval in seconds to clean up expired sessions


def get_address_string(address):
    return f"{address[0]}:{address[1]}"


async def get_redis_connection():
    return await aioredis.from_url(f'redis://{REDIS_HOST}:{REDIS_PORT}')


async def set_session(redis, address, session_data):
    # Ensure all values are strings
    session_data_serialized = {k: json.dumps(v) if isinstance(v, (bool, dict, list, tuple)) or v is None else v
                               for k, v in session_data.items()}

    address = get_address_string(address)

    await redis.hset(address, mapping=session_data_serialized)
    await redis.expire(address, SESSION_TIMEOUT)


async def get_session(redis, address):
    session_data_bytes = await redis.hgetall(get_address_string(address))
    # Convert byte data to string
    session_data = {k.decode('utf-8'): v.decode('utf-8') for k, v in session_data_bytes.items()}
    return session_data


async def update_last_activity(redis, address):
    await redis.hset(get_address_string(address), "last_activity", time.time())


async def is_session_expired(redis, address):
    last_activity = await redis.hget(get_address_string(address), "last_activity")
    return last_activity is None or (time.time() - float(last_activity)) > SESSION_TIMEOUT


async def is_session_expired(redis, address):
    address_str = get_address_string(address)
    last_activity = await redis.hget(address_str, "last_activity")
    return last_activity is None or (time.time() - float(last_activity)) > SESSION_TIMEOUT


async def forward_to_server(data, server_host, server_port):
    """General function to forward data to any server."""
    reader, writer = await asyncio.open_connection(server_host, server_port)
    writer.write(data)
    await writer.drain()

    response = await reader.read(1024)
    writer.close()
    await writer.wait_closed()
    return response


async def handle_client(reader, writer, redis):
    address = writer.get_extra_info('peername')
    logger.info(f"Connection established with {address}")

    await set_session(redis, address, {"authenticated": False, "username": None, "last_activity": time.time()})

    while True:
        data = await reader.read(1024)
        if not data:
            break

        try:
            data_decoded = data.decode()
            request = json.loads(data_decoded)

            await update_last_activity(redis, address)

            if await is_session_expired(redis, address):
                logger.info(f"Session expired for {address}")
                await set_session(redis, address, {"authenticated": False, "username": None})
                writer.write(json.dumps({'error': 'Session expired'}).encode())
                await writer.drain()
                continue

            if 'action' in request:
                if request['action'] in ['login', 'register', 'check_username']:
                    response = await forward_to_server(data, LOGIN_SERVER_HOST, LOGIN_SERVER_PORT)
                    response_data = json.loads(response.decode())

                    if response_data.get('user_exists', False):
                        await set_session(redis, address, {
                            "authenticated": True,
                            "username": request.get('username'),
                            "last_activity": time.time()
                        })

                    writer.write(response)
                else:
                    session = await get_session(redis, address)
                    if session.get("authenticated") == 'true':
                        # Forward game-related requests to the game server
                        if request['action'] in ['load_profile', 'load_character_data', 'load_friends',
                                                 'update_profile', 'create_profile']:
                            response = await forward_to_server(data, GAME_SERVER_HOST, GAME_SERVER_PORT)
                            writer.write(response)
                        else:
                            writer.write(json.dumps({'error': 'Invalid game action'}).encode())
                    else:
                        writer.write(json.dumps({'error': 'Unauthenticated'}).encode())
            else:
                writer.write(json.dumps({'error': 'Invalid request'}).encode())
        except json.JSONDecodeError:
            logger.error("Received non-JSON data")

        await writer.drain()

    logger.info(f"Connection closed with {address}")
    writer.close()
    await writer.wait_closed()


async def start_gateway_server(host='0.0.0.0', port=12345):
    redis = await get_redis_connection()

    server = await asyncio.start_server(
        lambda r, w: handle_client(r, w, redis), host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Serving on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(start_gateway_server())
