from PIL import Image
from io import BytesIO

import socket
import struct
import ctypes
import threading
from enum import Enum

clients = []  # list of clients connected to the server


class MessageType(Enum):
    MESSAGE = 0
    SCREENSHOT = 1


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


class IngameMessage(ctypes.Structure):
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


def send_data_to_client(client_socket, data):
    # Calculate the length of the data and pack it into 2 bytes
    try:
        # Send the actual data
        client_socket.send(data)
    except Exception as e:
        print("[send_data_to_client] Error sending data to client:", str(e))
        # Remove the client from the list of clients
        clients.remove(client_socket)


def broadcast_message(sender, message):
    for client in clients:
        if client != sender:
            try:
                # Pack the location data
                send_data_to_client(client, message.get_bytes())
            except Exception as e:
                print("[broadcast_location] Error sending data to client:", str(e))
                # Remove the client from the list of clients
                clients.remove(client)


def handle_world_message(client_socket):
    while True:
        try:
            recvmsg = client_socket.recv(65536)
            if len(recvmsg) == 0:
                break  # client disconnected

            # print("MessageHeader.get_size(): ", MessageHeader.get_size())
            header = MessageHeader.from_bytes(recvmsg[:MessageHeader.get_size()])

            # print("header.messageType: ", header.messageType)
            # print("header.messageLength: ", header.messageLength)

            if header.messageType == MessageType.SCREENSHOT.value:
                message = ScreenShotMessage.from_bytes(recvmsg[:ScreenShotMessage.get_size()])
                message.data = recvmsg[ScreenShotMessage.get_size():]
                image_data = BytesIO(message.data)
                image = Image.open(image_data)
                # image.show()
                # print("Screenshot received from client", message.data)
            elif header.messageType == MessageType.MESSAGE.value:
                t = recvmsg[:IngameMessage.get_size()]
                # print("size: ", IngameMessage.get_size())
                message = IngameMessage.from_bytes(recvmsg[:IngameMessage.get_size()])

                # Send the data to the client with the length field
                broadcast_message(client_socket, message)
        except Exception as e:
            print("Error:", str(e))

            # Remove the client from the list of clients
            clients.remove(client_socket)
            break
    client_socket.close()


# create a socket object
socket_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
HOST = "127.0.0.1"
PORT = 5009
# bind to the port
socket_server.bind((HOST, PORT))
# set the maximum number of connections to accept
socket_server.listen(5)
# socket_server.accept() returns a tuple (conn, address),
# where conn is a new socket object usable to send and receive data on the connection,

while True:
    print("Waiting for a client...")
    conn, address = socket_server.accept()
    print("Connected to", address)

    clients.append(conn)
    # start a new thread to handle the client
    threading.Thread(target=handle_world_message, args=(conn,)).start()
