import socket
import struct
import ctypes
import threading

clients = []  # list of clients connected to the server


class MessageHeader(ctypes.Structure):
    _fields_ = [
        ("clientID", ctypes.c_short),
        ("messageLength", ctypes.c_short)
    ]

    def __init__(self, clientID=0, messageLength=0):
        super().__init__()
        self.clientID = clientID
        self.messageLength = messageLength

    def get_bytes(self):
        return struct.pack("hh", self.clientID, self.messageLength)

    @classmethod
    def from_bytes(cls, data):
        unpacked_data = struct.unpack("hh", data)
        return cls(*unpacked_data)

    @classmethod
    def get_size(cls):
        return ctypes.sizeof(cls)


class IngameMessage(ctypes.Structure):
    _fields_ = [
        ("header", MessageHeader),
        ("x", ctypes.c_float),
        ("y", ctypes.c_float),
        ("action_type", ctypes.c_short),
        ("padding", ctypes.c_short),
    ]

    def __init__(self, header=MessageHeader(), x=0, y=0, action_type=0, *args):
        super().__init__()
        self.header = header
        self.x = x
        self.y = y
        self.action_type = action_type

    def get_bytes(self):
        return self.header.get_bytes() + struct.pack("ffhh", self.x, self.y, self.action_type, 0)

    @classmethod
    def from_bytes(cls, data):
        header = MessageHeader.from_bytes(data[:MessageHeader.get_size()])
        unpacked_data = struct.unpack("ffhh", data[MessageHeader.get_size():])
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
        if True:
            # if client != sender:
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
            recvmsg = client_socket.recv(1024)
            if len(recvmsg) == 0:
                break  # client disconnected

            t = recvmsg[:IngameMessage.get_size()]
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
