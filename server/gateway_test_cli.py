import asyncio
import json


async def test_client(host='127.0.0.1', port=12345):
    reader, writer = await asyncio.open_connection(host, port)

    # Example registration request
    registration_request = {
        "register": True,
        "username": "testuser",
        "password": "testpassword",
        "email": "testuser@example.com",
        "phone": "1234567890"
    }
    message = json.dumps(registration_request)
    print(f'Sending registration request: {message}')
    writer.write(message.encode())

    data = await reader.read(1024)
    registration_response = json.loads(data.decode())
    print(f'Received: {registration_response}')

    if registration_response.get('status') == 'registered':
        # Proceed with login if registration was successful
        login_request = {
            "login": True,
            "username": "testuser",
            "password": "testpassword"
        }
        message = json.dumps(login_request)
        print(f'\nSending login request: {message}')
        writer.write(message.encode())

        data = await reader.read(1024)
        login_response = json.loads(data.decode())
        print(f'Received: {login_response}')
    else:
        print("Registration failed or user already exists.")

    print('Closing the connection')
    writer.close()
    await writer.wait_closed()


if __name__ == '__main__':
    asyncio.run(test_client())
