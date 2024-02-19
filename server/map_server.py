import asyncio
import json
import logging

# Setting up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("MapServer")


async def handle_player_request(reader, writer):
    data = await reader.read(1024)
    message = data.decode()

    try:
        request = json.loads(message)

        if request.get('action') == 'move_player':
            # Update player position
            new_position = request.get('new_position')
            # response = await update_player_position(request['username'], new_position)
            response = {'error': 'Not implemented'}
            # Notify nearby players (this requires additional logic)
            await notify_nearby_players(request['username'], new_position)

        elif request.get('action') == 'interact':
            # Handle player interaction with the environment or other players
            interaction_details = request.get('details')
            # response = await send_request_to_game_logic_server('handle_interaction', interaction_details)
            response = {'error': 'Not implemented'}
        else:
            response = {'error': 'Invalid request'}

        writer.write(json.dumps(response).encode())

    except json.JSONDecodeError:
        writer.write(json.dumps({'error': 'Invalid JSON'}).encode())

    await writer.drain()
    writer.close()


async def notify_nearby_players(username, position):
    # Additional logic to notify nearby players of this player's new position
    # This might involve querying a database or in-memory data structure
    # that keeps track of player positions and then sending updates to
    # relevant clients.
    pass


async def start_map_server(host='0.0.0.0', port=12350):
    server = await asyncio.start_server(handle_player_request, host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Map server listening on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(start_map_server())
