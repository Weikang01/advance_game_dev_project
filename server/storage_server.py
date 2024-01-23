import asyncio
import sqlite3
import json
import logging

# Database setup
DATABASE = 'user_data.db'

# Setting up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("StorageServer")


def db_connect():
    return sqlite3.connect(DATABASE)


def get_user_id(username):
    conn = db_connect()
    cursor = conn.cursor()
    cursor.execute("SELECT id FROM users WHERE username = ?", (username,))
    user = cursor.fetchone()
    conn.close()
    return user[0] if user else None


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


def check_username_exists(username):
    conn = db_connect()
    cursor = conn.cursor()

    cursor.execute("SELECT 1 FROM users WHERE username = ?", (username,))
    exists = cursor.fetchone() is not None
    conn.close()
    return exists


def authenticate_user(username, password):
    conn = db_connect()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM users WHERE username = ? AND password = ?", (username, password))
    user = cursor.fetchone()
    conn.close()
    return user is not None


def create_player_profile(username, display_name, avatar_url, level, experience):
    user_id = get_user_id(username)
    if user_id is None:
        return {"status": "error", "message": "User not found"}

    conn = db_connect()
    cursor = conn.cursor()
    try:
        cursor.execute(
            "INSERT INTO player_profiles (user_id, display_name, avatar_url, level, experience) VALUES (?, ?, ?, ?, ?)",
            (user_id, display_name, avatar_url, level, experience))
        conn.commit()
    finally:
        conn.close()
    return {"status": "success"}


def update_player_profile(username, display_name, avatar_url, level, experience):
    user_id = get_user_id(username)
    if user_id is None:
        return {"status": "error", "message": "User not found"}

    conn = db_connect()
    cursor = conn.cursor()
    try:
        cursor.execute(
            "UPDATE player_profiles SET display_name = ?, avatar_url = ?, level = ?, experience = ? WHERE user_id = ?",
            (display_name, avatar_url, level, experience, user_id))
        conn.commit()
    finally:
        conn.close()
    return {"status": "success"}


def load_player_profile(username):
    user_id = get_user_id(username)
    if user_id is None:
        return {"status": "error", "message": "User not found"}

    conn = db_connect()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM player_profiles WHERE user_id = ?", (user_id,))
    profile = cursor.fetchone()
    conn.close()
    return profile or {"status": "error", "message": "Profile not found"}


def load_character_data(username):
    user_id = get_user_id(username)
    if user_id is None:
        return {"status": "error", "message": "User not found"}

    conn = db_connect()
    cursor = conn.cursor()
    cursor.execute("SELECT * FROM character_data WHERE user_id = ?", (user_id,))
    character_data = cursor.fetchone()
    conn.close()
    return character_data or {"status": "error", "message": "Character data not found"}


def load_friend_list(username):
    user_id = get_user_id(username)
    if user_id is None:
        return {"status": "error", "message": "User not found"}

    conn = db_connect()
    cursor = conn.cursor()
    cursor.execute("SELECT friend_id FROM friend_list WHERE user_id = ?", (user_id,))
    friends = cursor.fetchall()
    conn.close()
    return {"friends": [friend[0] for friend in friends]}


async def handle_request(reader, writer):
    data = await reader.read(1024)
    message = data.decode()

    try:
        request = json.loads(message)

        if request['action'] == 'register':
            response = register_user(request['username'], request['password'], request['email'], request.get('phone'))
        elif request['action'] == 'login':
            user_exists = authenticate_user(request['username'], request['password'])
            response = {'user_exists': user_exists}
        elif request['action'] == 'check_username':
            username_exists = check_username_exists(request['username'])
            response = {'username_exists': username_exists}
        elif request['action'] == 'create_profile':
            response = create_player_profile(request['username'], request['display_name'], request['avatar_url'],
                                             request['level'], request['experience'])
        elif request['action'] == 'update_profile':
            response = update_player_profile(request['username'], request['display_name'], request['avatar_url'],
                                             request['level'], request['experience'])
        elif request['action'] == 'load_profile':
            response = load_player_profile(request['username'])
        elif request['action'] == 'load_character_data':
            response = load_character_data(request['username'])
        elif request['action'] == 'load_friends':
            response = load_friend_list(request['username'])
        else:
            response = {'error': 'Invalid action'}

    except json.JSONDecodeError:
        response = {'error': 'Invalid JSON'}

    writer.write(json.dumps(response).encode())
    await writer.drain()
    writer.close()


async def start_storage_server(host='0.0.0.0', port=12347):
    server = await asyncio.start_server(handle_request, host, port)
    addr = server.sockets[0].getsockname()
    logger.info(f"Storage server listening on {addr}")

    async with server:
        await server.serve_forever()


if __name__ == '__main__':
    asyncio.run(start_storage_server())
