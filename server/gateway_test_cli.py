import asyncio
import json
import threading
from asyncio import sleep

GATEWAY_HOST = '127.0.0.1'
GATEWAY_PORT = 12345
TEST_USERNAME1 = 'testuser1'
TEST_USERNAME2 = 'testuser2'


async def test_registration(reader, writer, username):
    # Registration request
    registration_request = {
        "action": "register",
        "username": username,
        "password": "testpassword",
        "email": "testuser34@example.com",
        "phone": "1234567890"
    }
    message = json.dumps(registration_request)
    print(f'{username}: Sending registration request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_login(reader, writer, username):
    # Login request
    login_request = {
        "action": "login",
        "username": username,
        "password": "testpassword"
    }
    message = json.dumps(login_request)
    print(f'\n{username}: Sending login request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_check_username(reader, writer, username):
    # Check username request
    check_username_request = {
        "action": "check_username",
        "username": username
    }
    message = json.dumps(check_username_request)
    print(f'\n{username}: Sending check username request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_create_profile(reader, writer, username):
    # Create profile request
    create_profile_request = {
        "action": "create_profile",
        "username": username,
        "display_name": "Test User",
        "avatar_url": "http://example.com/avatar.jpg",
        "level": 1,
        "experience": 0
    }
    message = json.dumps(create_profile_request)
    print(f'\n{username}: Sending create profile request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_update_profile(reader, writer, username):
    # Update profile request
    update_profile_request = {
        "action": "update_profile",
        "username": username,
        "display_name": "Test User",
        "avatar_url": "http://example.com/avatar.jpg",
        "level": 1,
        "experience": 0
    }
    message = json.dumps(update_profile_request)
    print(f'\n{username}: Sending update profile request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_load_profile(reader, writer, username):
    # Load profile request
    load_profile_request = {
        "action": "load_profile",
        "username": username
    }
    message = json.dumps(load_profile_request)
    print(f'\n{username}: Sending load profile request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_load_character_data(reader, writer, username):
    # Load character data request
    load_character_data_request = {
        "action": "load_character_data",
        "username": username
    }
    message = json.dumps(load_character_data_request)
    print(f'\n{username}: Sending load character data request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_load_friends(reader, writer, username):
    # Load friends request
    load_friends_request = {
        "action": "load_friends",
        "username": username
    }
    message = json.dumps(load_friends_request)
    print(f'\n{username}: Sending load friends request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_get_room_list(reader, writer):
    # List rooms request
    list_rooms_request = {"action": "get_room_list"}
    message = json.dumps(list_rooms_request)
    print(f'\nSending list rooms request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')


async def test_create_room(reader, writer, username):
    # Create room request
    create_room_request = {
        "username": username,
        "action": "create_room",
        "room_id": "test_room",
        "room_data": {"name": "Test Room", "password": "roompassword"}
    }
    message = json.dumps(create_room_request)
    print(f'\n{username}: Sending create room request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_enter_room(reader, writer, username):
    # Enter room request
    enter_room_request = {
        "username": username,
        "action": "enter_room",
        "room_id": "test_room",
        "password": "roompassword"
    }
    message = json.dumps(enter_room_request)
    print(f'\n{username}: Sending enter room request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def test_leave_room(reader, writer, username):
    # Leave room request
    # Assuming 'leave_room' action is implemented
    leave_room_request = {
        "username": username,
        "action": "leave_room",
        "room_id": "test_room"
    }
    message = json.dumps(leave_room_request)
    print(f'\n{username}: Sending leave room request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'{username}: Received: {data.decode()}')


async def listen_for_broadcasts(reader, username):
    """Listen for broadcast messages and print them."""
    try:
        while True:
            data = await reader.read(1024)
            if not data:
                break
            print(f'{username} received broadcast: {data.decode()}')
    except asyncio.CancelledError:
        pass  # Handle cancellation of the listening task
    except Exception as e:
        print(f'Error in listen_for_broadcasts for {username}: {e}')


async def test_client(username):
    reader, writer = await asyncio.open_connection(GATEWAY_HOST, GATEWAY_PORT)

    await test_registration(reader, writer, username)
    await test_login(reader, writer, username)
    await test_create_profile(reader, writer, username)
    await test_load_profile(reader, writer, username)

    if username == TEST_USERNAME1:
        await test_create_room(reader, writer, username)
    else:
        await asyncio.sleep(1)
        await test_enter_room(reader, writer, username)

    # Start listening for broadcast messages
    asyncio.create_task(listen_for_broadcasts(reader, username))

    await asyncio.sleep(5)  # Keep the connection open to listen for broadcasts

    if username == TEST_USERNAME1:
        await test_leave_room(reader, writer, username)

    writer.close()
    await writer.wait_closed()


async def run_test_sequences():
    await asyncio.gather(
        test_client(TEST_USERNAME1),
        test_client(TEST_USERNAME2)
    )


if __name__ == '__main__':
    asyncio.run(run_test_sequences())
