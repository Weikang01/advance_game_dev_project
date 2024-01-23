import asyncio
import logging
import json
import time

import redis_manager
from common import send_request_to_caching_server

# Global dictionary to track connected clients by username
connected_clients = {}

# Setting up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("GatewayServer")

# Define the address of the login server
LOGIN_SERVER_HOST = '127.0.0.1'
LOGIN_SERVER_PORT = 12346
GAME_SERVER_HOST = '127.0.0.1'
GAME_SERVER_PORT = 12348

redis_manager = redis_manager.RedisManager()


async def forward_to_server(data, server_host, server_port):
    """General function to forward data to any server."""
    reader, writer = await asyncio.open_connection(server_host, server_port)
    writer.write(data)
    await writer.drain()

    response = await reader.read(1024)
    writer.close()
    await writer.wait_closed()
    return response


async def broadcast_to_room(room_id, message):
    """Broadcast a message to all clients in a room."""
    participants = await send_request_to_caching_server("get_user_profiles_in_room", {"room_id": room_id})

    for username in participants.keys():
        print(f"Broadcasting to {username}: {message} current connected clients: {connected_clients}")
        client = connected_clients.get(username)
        if client:
            try:
                client.write(message.encode())
                print(f"Broadcasting to {username}: {message}")
                await client.drain()
            except Exception as e:
                logger.error(f"Error broadcasting to {username}: {e}")
                client.close()


async def handle_client(reader, writer):
    global connected_clients
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
                        if request['action'] in ['create_room', 'enter_room']:
                            room_id = request['room_id']
                            username = session_data.get("username")
                            connected_clients[username] = writer
                            await broadcast_to_room(room_id, username)

                        elif request['action'] == 'leave_room':
                            room_id = request['room_id']
                            username = session_data.get("username")
                            await broadcast_to_room(room_id, username)
                            connected_clients.pop(username, None)

                        # Forward game-related requests to the game server
                        elif request['action'] in ['load_profile', 'load_character_data', 'load_friends',
                                                 'update_profile', 'create_profile', 'get_room_list',
                                                 'get_room_data', 'get_user_profiles_in_room']:

                            response = await forward_to_server(data, GAME_SERVER_HOST, GAME_SERVER_PORT)
                            writer.write(response)
                        else:
                            writer.write(json.dumps({'error': 'Invalid game action'}).encode())
                    else:
                        writer.write(json.dumps({'error': 'Unauthenticated'}).encode())
        except json.JSONDecodeError:
            logger.error("Received non-JSON data")
        except asyncio.CancelledError:
            logger.info(f"Client connection {address} cancelled.")
        except Exception as e:
            logger.error(f"An error occurred: {e}")

        await writer.drain()

    logger.info(f"Closing connection with {address}")

    writer.close()
    await writer.wait_closed()


async def start_gateway_server(host='0.0.0.0', port=12345):
    server = await asyncio.start_server(
        lambda r, w: handle_client(r, w), host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Serving on {addr}")

    async with server:
        await server.serve_forever()

    # If your server has a shutdown sequence, gracefully close client connections
    tasks = [t for t in asyncio.all_tasks() if t is not asyncio.current_task()]
    for task in tasks:
        task.cancel()
        try:
            await task
        except asyncio.CancelledError:
            pass


if __name__ == '__main__':
    asyncio.run(start_gateway_server())
