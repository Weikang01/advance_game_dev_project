import logging
import json
import aioredis
import time
import os
import subprocess

REDIS_HOST = 'localhost'  # Redis server address
REDIS_PORT = 6379  # Default Redis port
# Session management
SESSION_TIMEOUT = 1800  # 30 minutes

# Configure logging
logger = logging.getLogger("RedisManager")


class RedisManager:
    @staticmethod
    def get_address_string(address):
        return f"{address[0]}:{address[1]}"

    def __init__(self):
        self.redis = None
        self.redis_uri = f'redis://{REDIS_HOST}:{REDIS_PORT}'

    @staticmethod
    def start_redis_server():
        def find_redis_executable(file_name="redis-server.exe"):
            for path in os.environ["PATH"].split(os.pathsep):
                full_path = os.path.join(path, file_name)
                if os.path.isfile(full_path) and os.access(full_path, os.X_OK):
                    return full_path
            return None

        try:
            # Start the Redis server
            redis_server_path = find_redis_executable()
            if redis_server_path is None:
                raise Exception("Redis server executable not found")
            else:
                subprocess.Popen([redis_server_path])
            print("Redis server started successfully.")
        except Exception as e:
            print(f"Error starting Redis server: {e}")

    async def connect(self):
        if self.redis is None:
            try:
                self.redis = await aioredis.from_url(self.redis_uri)
                await self.redis.ping()
            except aioredis.exceptions.RedisError as e:
                try:
                    self.start_redis_server()
                    self.redis = await aioredis.from_url(self.redis_uri)
                    await self.redis.ping()
                except Exception as e:
                    logger.error(f"Error connecting to Redis server: {e}")
                    self.redis = None

    async def get_session(self, address):
        session_data_bytes = await self.redis.hgetall(self.get_address_string(address))
        # Convert byte data to string
        session_data = {k.decode('utf-8'): v.decode('utf-8') for k, v in session_data_bytes.items()}
        return session_data

    async def set_session(self, address, session_data):
        if not self.redis:
            await self.connect()
            if not self.redis:
                return
        address_str = self.get_address_string(address)
        session_data_serialized = {k: json.dumps(v) if isinstance(v, (bool, dict, list, tuple)) or v is None else v
                                   for k, v in session_data.items()}
        await self.redis.hset(address_str, mapping=session_data_serialized)
        await self.redis.expire(address_str, SESSION_TIMEOUT)

    async def cache_player_profile(self, username, profile_data):
        if not self.redis:
            await self.connect()
        profile_key = f"profile:{username}"
        await self.redis.hmset(profile_key, profile_data)
        await self.redis.expire(profile_key, SESSION_TIMEOUT)

    async def get_cached_player_profile(self, username):
        if not self.redis:
            await self.connect()
        profile_key = f"profile:{username}"
        profile_data = await self.redis.hgetall(profile_key)
        if profile_data:
            return {k.decode(): v.decode() for k, v in profile_data.items()}
        return None

    async def update_last_activity(self, address):
        await self.redis.hset(self.get_address_string(address), "last_activity", time.time())

    async def is_session_expired(self, address):
        last_activity = await self.redis.hget(self.get_address_string(address), "last_activity")
        return last_activity is None or (time.time() - float(last_activity)) > SESSION_TIMEOUT

    async def create_room(self, room_id, room_data, creator_username):
        if not self.redis:
            await self.connect()
        room_key = f"room:{room_id}"
        participants_key = f"{room_key}:participants"
        room_data['host'] = creator_username
        await self.redis.hmset(room_key, room_data)
        await self.redis.sadd("rooms", room_key)
        await self.redis.sadd(participants_key, creator_username)
        return json.dumps({"status": "success", "message": "Room created successfully"})

    async def get_room_list(self):
        if not self.redis:
            await self.connect()
        rooms = await self.redis.smembers("rooms")
        return json.dumps(list(rooms))

    async def get_room_data(self, room_id):
        room_key = f"room:{room_id}"
        room_data = await self.redis.hgetall(room_key)
        return json.dumps({k.decode(): v.decode() for k, v in room_data.items()})

    async def enter_room(self, room_id, username, password=None):
        room_key = f"room:{room_id}"
        participants_key = f"{room_key}:participants"
        current_room_key = f"player_current_room:{username}"
        room_data = await self.redis.hgetall(room_key)

        if room_data:
            if room_data.get(b"password") and room_data[b"password"].decode() != password:
                return json.dumps({"status": "error", "message": "Invalid password"})
            await self.redis.sadd(participants_key, username)
            await self.redis.set(current_room_key, room_id)  # Set the player's current room
            return json.dumps({"status": "success", "message": "Entered room successfully"})
        return json.dumps({"status": "error", "message": "Room not found"})

    async def leave_room(self, room_id, username):
        room_key = f"room:{room_id}"
        participants_key = f"{room_key}:participants"
        current_room_key = f"player_current_room:{username}"

        if await self.redis.sismember(participants_key, username):
            await self.redis.srem(participants_key, username)
            await self.redis.delete(current_room_key)  # Clear the player's current room
            participants = await self.redis.smembers(participants_key)
            if not participants:
                await self.redis.delete(room_key)
                await self.redis.srem("rooms", room_key)
                return json.dumps({"status": "success", "message": "Room deleted, as it is now empty"})
            elif await self.redis.hget(room_key, "host") == username:
                new_host = next(iter(participants), None)
                if new_host:
                    await self.redis.hset(room_key, "host", new_host)
                    return json.dumps({"status": "success", "message": f"New host assigned: {new_host.decode()}"})
                else:
                    await self.redis.delete(room_key)
                    await self.redis.srem("rooms", room_key)
                    return json.dumps({"status": "success", "message": "Room deleted, as it is now empty"})
            return json.dumps({"status": "success", "message": "Left room successfully"})
        return json.dumps({"status": "error", "message": "User not in room"})

    async def get_user_profiles_in_room(self, room_id):
        if not self.redis:
            await self.connect()

        room_key = f"room:{room_id}"
        participants_key = f"{room_key}:participants"
        participants = await self.redis.smembers(participants_key)

        profiles = []
        for username in participants:
            profile_key = f"profile:{username.decode()}"  # usernames are stored as bytes in Redis set
            profile_data = await self.redis.hgetall(profile_key)
            if profile_data:
                profile = {k.decode(): v.decode() for k, v in profile_data.items()}
                profiles.append(profile)

        return json.dumps(profiles)

    async def close(self):
        if self.redis:
            self.redis.close()
            await self.redis.wait_closed()
