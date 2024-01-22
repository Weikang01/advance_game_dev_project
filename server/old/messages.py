import json
import struct
import ctypes
from enum import Enum


class MessageType(Enum):
    CLIENT_MESSAGE = 0
    SCREENSHOT = 1
    SYSTEM_MESSAGE = 2


class ActionType(Enum):
    MOVE = 9,
    JUMP = 1,
    ATTACK = 2,
    SKILL = 3,
    DIE = 4,
    RESPAWN = 5,
    GAMEOVER = 6,
    QUIT = 7,
    ENTER = 8,


class MessageHeader(ctypes.Structure):
    _fields_ = [
        ("clientID", ctypes.c_short),
        ("messageLength", ctypes.c_short),
        ("messageType", ctypes.c_short),
    ]

    def __init__(self, clientID=0, messageLength=0, messageType=0):
        super().__init__()
        self.clientID = clientID
        self.messageLength = messageLength
        self.messageType = messageType

    def get_bytes(self):
        return struct.pack("hhh", self.clientID, self.messageLength, self.messageType)

    @classmethod
    def from_bytes(cls, data):
        unpacked_data = struct.unpack("hhh", data)
        return cls(*unpacked_data)

    @classmethod
    def get_size(cls):
        return ctypes.sizeof(cls)


class ServerMessage(ctypes.Structure):
    _fields_ = [
        ("header", MessageHeader),
        ("message", ctypes.c_char_p),
    ]

    def __init__(self, client_id, message: str or bytes, message_type: MessageType or int):
        super().__init__()
        if isinstance(message_type, MessageType):
            self.header = MessageHeader(client_id, len(message), message_type.value())
        else:
            self.header = MessageHeader(client_id, len(message), message_type)

        if isinstance(message, str):
            self.message = str.encode(message)
        else:
            self.message = message

    def get_bytes(self):
        return self.header.get_bytes() + self.message

    @classmethod
    def from_bytes(cls, data):
        header = MessageHeader.from_bytes(data[:MessageHeader.get_size()])
        unpacked_data = data[MessageHeader.get_size():MessageHeader.get_size() + header.messageLength]
        return cls(header.clientID, unpacked_data, header.messageType)

    @classmethod
    def get_size(cls):
        return ctypes.sizeof(cls)

    @classmethod
    def from_json(cls, client_id, obj):
        return cls(client_id, json.dumps(obj), MessageType.SYSTEM_MESSAGE.value)


class ClientMessage(ctypes.Structure):
    _fields_ = [
        ("header", MessageHeader),
        ("action_type", ctypes.c_short),
        ("padding", ctypes.c_short),
        ("x", ctypes.c_float),
        ("y", ctypes.c_float),
        ("face_direction", ctypes.c_float),
    ]

    def __init__(self, header=MessageHeader(), action_type=0, padding=0, x=0, y=0, face_direction=0, *args):
        super().__init__()
        self.header = header
        self.action_type = action_type
        self.padding = 0
        self.x = x
        self.y = y
        self.face_direction = face_direction

    def get_bytes(self):
        return self.header.get_bytes() + struct.pack("hhfff", self.action_type, 0, self.x, self.y, self.face_direction)

    @classmethod
    def from_bytes(cls, data):
        header = MessageHeader.from_bytes(data[:MessageHeader.get_size()])
        unpacked_data = struct.unpack("hhfff", data[MessageHeader.get_size():])
        return cls(header, *unpacked_data)

    @classmethod
    def get_size(cls):
        return ctypes.sizeof(cls)


class ScreenShotMessage(ctypes.Structure):
    _fields_ = [
        ("header", MessageHeader),
        ("width", ctypes.c_short),
        ("height", ctypes.c_short),
    ]

    def __init__(self, header=MessageHeader(), width=0, height=0, data=b'', *args):
        super().__init__()
        self.header = header
        self.width = width
        self.height = height
        self.data = data

    def get_bytes(self):
        return self.header.get_bytes() + struct.pack("hh", self.width, self.height) + self.data

    @classmethod
    def from_bytes(cls, data):
        header = MessageHeader.from_bytes(data[:MessageHeader.get_size()])
        unpacked_data = struct.unpack("hh", data[MessageHeader.get_size():MessageHeader.get_size() +
                                                                          ScreenShotMessage.get_size()])
        return cls(header, *unpacked_data)

    @classmethod
    def get_size(cls):
        return ctypes.sizeof(cls)
