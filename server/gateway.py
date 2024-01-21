import asyncio
import logging
import json

# Setting up logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger("GatewayServer")

# Define the address of the login server
LOGIN_SERVER_HOST = '127.0.0.1'
LOGIN_SERVER_PORT = 12346


async def forward_to_login_server(data):
    """Forward data to the login server and return the response."""
    reader, writer = await asyncio.open_connection(LOGIN_SERVER_HOST, LOGIN_SERVER_PORT)
    writer.write(data)
    await writer.drain()

    response = await reader.read(1024)
    writer.close()
    await writer.wait_closed()
    return response


async def handle_client(reader, writer):
    address = writer.get_extra_info('peername')
    logger.info(f"Connection established with {address}")

    while True:
        data = await reader.read(1024)
        if not data:
            break

        # Decode the received data to check if it's a login request
        try:
            data_decoded = data.decode()
            request = json.loads(data_decoded)
            if 'login' in request or 'register' in request:
                # Forward login request to the login server
                response = await forward_to_login_server(data)
                writer.write(response)
            else:
                # Handle other types of requests
                # For now, echoing back the data
                writer.write(data)
        except json.JSONDecodeError:
            logger.error("Received non-JSON data")

        await writer.drain()

    logger.info(f"Connection closed with {address}")
    writer.close()
    await writer.wait_closed()


async def main(host='0.0.0.0', port=12345):
    server = await asyncio.start_server(handle_client, host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Serving on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(main())
