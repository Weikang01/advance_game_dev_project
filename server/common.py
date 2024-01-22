import asyncio
import json

# Storage server address and port
STORAGE_SERVER_HOST = '127.0.0.1'
STORAGE_SERVER_PORT = 12347


async def send_request_to_storage_server(action, data):
    """Send a request to the storage server and return the response."""
    reader, writer = await asyncio.open_connection(STORAGE_SERVER_HOST, STORAGE_SERVER_PORT)

    request = {"action": action, **data}
    writer.write(json.dumps(request).encode())
    await writer.drain()

    response_data = await reader.read(1024)
    writer.close()
    await writer.wait_closed()

    return json.loads(response_data.decode())


CACHING_SERVER_HOST = '127.0.0.1'
CACHING_SERVER_PORT = 12349


async def send_request_to_caching_server(action, data):
    """Send a request to the caching server and return the response."""
    reader, writer = await asyncio.open_connection(CACHING_SERVER_HOST, CACHING_SERVER_PORT)

    request = {"action": action, **data}
    writer.write(json.dumps(request).encode())
    await writer.drain()

    response_data = await reader.read(1024)
    writer.close()
    await writer.wait_closed()

    return json.loads(response_data.decode())
