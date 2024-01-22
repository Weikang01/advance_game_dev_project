import asyncio
import logging
import json
import time

from common import send_request_to_caching_server

# Setting up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("GatewayServer")

# Define the address of the login server
LOGIN_SERVER_HOST = '127.0.0.1'
LOGIN_SERVER_PORT = 12346
GAME_SERVER_HOST = '127.0.0.1'
GAME_SERVER_PORT = 12348


async def forward_to_server(data, server_host, server_port):
    """General function to forward data to any server."""
    reader, writer = await asyncio.open_connection(server_host, server_port)
    writer.write(data)
    await writer.drain()

    response = await reader.read(1024)
    writer.close()
    await writer.wait_closed()
    return response


async def handle_client(reader, writer):
    address = writer.get_extra_info('peername')
    address_str = f"{address[0]}:{address[1]}"
    logger.info(f"Connection established with {address}")

    await send_request_to_caching_server("set_session", {"address": address,
                                                         "session_data": {"authenticated": False, "username": None}})

    while True:
        data = await reader.read(1024)
        if not data:
            break

        try:
            data_decoded = data.decode()
            request = json.loads(data_decoded)

            # Update last activity in caching server
            await send_request_to_caching_server("update_last_activity", {"address": address_str})

            # Check if session is expired
            expired = await send_request_to_caching_server("is_session_expired", {"address": address_str})
            if expired:
                logger.info(f"Session expired for {address}")
                await send_request_to_caching_server("set_session", {"address": address_str,
                                                                     "session_data": {"authenticated": False,
                                                                                      "username": None}})
                writer.write(json.dumps({'error': 'Session expired'}).encode())
                await writer.drain()
                continue

            if 'action' in request:
                if request['action'] in ['login', 'register', 'check_username']:
                    response = await forward_to_server(data, LOGIN_SERVER_HOST, LOGIN_SERVER_PORT)
                    writer.write(response)
                else:
                    session_data = await send_request_to_caching_server("get_session", {"address": address_str})
                    if session_data.get("authenticated") == 'true':
                        # Forward game-related requests to the game server
                        if request['action'] in ['load_profile', 'load_character_data', 'load_friends',
                                                 'update_profile', 'create_profile', 'create_room', 'get_room_list',
                                                    'get_room_data', 'enter_room', 'get_user_profiles_in_room',
                                                    'leave_room']:
                            response = await forward_to_server(data, GAME_SERVER_HOST, GAME_SERVER_PORT)
                            writer.write(response)
                        else:
                            writer.write(json.dumps({'error': 'Invalid game action'}).encode())
                    else:
                        writer.write(json.dumps({'error': 'Unauthenticated'}).encode())
        except json.JSONDecodeError:
            logger.error("Received non-JSON data")

        await writer.drain()
    logger.info(f"Connection closed with {address}")
    writer.close()
    await writer.wait_closed()


async def start_gateway_server(host='0.0.0.0', port=12345):
    server = await asyncio.start_server(
        lambda r, w: handle_client(r, w), host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Serving on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(start_gateway_server())
