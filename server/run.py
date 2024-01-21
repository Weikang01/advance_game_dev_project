import asyncio
import logging

# Import the main functions from your server modules
from gateway import start_gateway_server
from login_server import start_login_server
from storage_server import start_storage_server
from game_server import start_game_server

# Configure logging
logging.basicConfig(level=logging.INFO)


async def run_servers():
    # Create a task for each server
    gateway_server_task = asyncio.create_task(start_gateway_server())
    login_server_task = asyncio.create_task(start_login_server())
    storage_server_task = asyncio.create_task(start_storage_server())
    game_server_task = asyncio.create_task(start_game_server())

    # Wait for all server tasks to complete
    await asyncio.gather(gateway_server_task, login_server_task, storage_server_task, game_server_task)


if __name__ == '__main__':
    asyncio.run(run_servers())
