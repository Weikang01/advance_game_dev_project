import asyncio
import json


async def test_client(host='127.0.0.1', port=12345):
    username = 'testuser36'
    reader, writer = await asyncio.open_connection(host, port)

    # Registration request
    registration_request = {
        "action": "register",
        "username": username,
        "password": "testpassword",
        "email": "testuser34@example.com",
        "phone": "1234567890"
    }
    message = json.dumps(registration_request)
    print(f'Sending registration request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')

    # Login request

    login_request = {
        "action": "login",
        "username": username,
        "password": "testpassword"
    }
    message = json.dumps(login_request)
    print(f'\nSending login request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')

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
    print(f'\nSending create profile request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')

    # Load profile request
    load_profile_request = {
        "action": "load_profile",
        "username": username
    }
    message = json.dumps(load_profile_request)
    print(f'\nSending load profile request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')

    # List rooms request
    list_rooms_request = {"action": "get_room_list"}
    message = json.dumps(list_rooms_request)
    print(f'\nSending list rooms request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')

    # Create room request
    create_room_request = {
        "username": username,
        "action": "create_room",
        "room_id": "test_room",
        "room_data": {"name": "Test Room", "password": "roompassword"}
    }
    message = json.dumps(create_room_request)
    print(f'\nSending create room request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')

    # Enter room request
    enter_room_request = {
        "username": username,
        "action": "enter_room",
        "room_id": "test_room",
        "password": "roompassword"
    }
    message = json.dumps(enter_room_request)
    print(f'\nSending enter room request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')

    # Leave room request
    # Assuming 'leave_room' action is implemented
    leave_room_request = {
        "username": username,
        "action": "leave_room",
        "room_id": "test_room"
    }
    message = json.dumps(leave_room_request)
    print(f'\nSending leave room request: {message}')
    writer.write(message.encode())
    data = await reader.read(1024)
    print(f'Received: {data.decode()}')

    print('Closing the connection')
    writer.close()
    await writer.wait_closed()


if __name__ == '__main__':
    asyncio.run(test_client())
