import asyncio
import sqlite3
import json
import logging

# Database setup (Assuming a table `users` with fields `username`, `password`, `email`, `phone`)
DATABASE = 'user_data.db'

# Setting up logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger("LoginServer")


def db_connect():
    return sqlite3.connect(DATABASE)


def authenticate_user(username, password):
    conn = db_connect()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM users WHERE username = ? AND password = ?", (username, password))
    user = cursor.fetchone()
    conn.close()
    return user is not None


def register_user(username, password, email, phone=None):
    conn = db_connect()
    cursor = conn.cursor()

    # Check if the username already exists
    cursor.execute("SELECT * FROM users WHERE username = ?", (username,))
    if cursor.fetchone():
        conn.close()
        return {"status": "error", "message": "Username already exists"}

    # Insert new user
    cursor.execute("INSERT INTO users (username, password, email, phone) VALUES (?, ?, ?, ?)",
                   (username, password, email, phone))
    conn.commit()
    conn.close()
    return {"status": "success"}


async def handle_login_request(reader, writer):
    data = await reader.read(1024)
    message = data.decode()

    try:
        request = json.loads(message)
        response = {}

        if 'register' in request:
            # Handle registration
            register_response = register_user(request['username'], request['password'], request['email'],
                                              request.get('phone'))
            if register_response["status"] == "error":
                response['status'] = 'error'
                response['message'] = register_response["message"]
            else:
                response['status'] = 'registered'
        elif 'login' in request:
            # Handle login
            authenticated = authenticate_user(request['username'], request['password'])
            response['authenticated'] = authenticated
            # Add additional logic for IP check and multi-factor authentication
        else:
            response['error'] = 'Invalid request'

        writer.write(json.dumps(response).encode())
    except json.JSONDecodeError:
        writer.write(json.dumps({'error': 'Invalid JSON'}).encode())

    await writer.drain()
    writer.close()


async def main(host='0.0.0.0', port=12346):
    server = await asyncio.start_server(handle_login_request, host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Login server listening on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(main())
