import json

from PIL import Image
from io import BytesIO
from torchvision import transforms

from messages import MessageHeader, MessageType, ScreenShotMessage, ServerMessage, ClientMessage
from clients import Clients
import socket

import threading

dqn = None

clients = Clients()


def send_data_to_client(client_id, data):
    # Calculate the length of the data and pack it into 2 bytes
    try:
        # Send the actual data
        clients.get_client(client_id).send(data)
    except Exception as e:
        print("[send_data_to_client] Error sending data to client:", str(e))
        # Remove the client from the list of clients
        clients.remove_client(client_id)


def broadcast_message(client_id, message):
    for cur_id in clients.get_client_ids():
        if cur_id == client_id:
            try:
                # Pack the location data
                send_data_to_client(cur_id, message.get_bytes())
            except Exception as e:
                print("[broadcast_location] Error sending data to client:", str(e))
                # Remove the client from the list of clients
                clients.remove_client(cur_id)


def send_system_message(client_id, message, message_type):
    try:
        # Pack the location data
        r = ServerMessage(client_id, message, message_type)
        r = r.get_bytes()
        send_data_to_client(client_id, r)
    except Exception as e:
        print("[send_system_message] Error sending data to client:", str(e))
        # Remove the client from the list of clients
        clients.remove_client(client_id)


def handle_message(client_socket):
    global dqn
    while True:
        try:
            recvmsg = client_socket.recv(65536)
            if len(recvmsg) == 0:
                break  # client disconnected

            # print("MessageHeader.get_size(): ", MessageHeader.get_size())
            header = MessageHeader.from_bytes(recvmsg[:MessageHeader.get_size()])

            if header.clientID not in clients.get_client_ids():
                clients.add_client(header.clientID, client_socket)
                print("Client", header.clientID, "connected")
                if clients.get_client_count() % 1 == 0:
                    send_system_message(header.clientID, json.dumps({"type": "start"}), MessageType.SYSTEM_MESSAGE.value)

            if header.messageType == MessageType.SCREENSHOT.value:
                message = ScreenShotMessage.from_bytes(recvmsg[:ScreenShotMessage.get_size()])
                message.data = recvmsg[ScreenShotMessage.get_size():]
                image_data = BytesIO(message.data)
                image = Image.open(image_data)
                transform = transforms.ToTensor()
                tensor_image = transform(image)
                # print(tensor_image.shape)
            elif header.messageType == MessageType.CLIENT_MESSAGE.value:
                message = ClientMessage.from_bytes(recvmsg[:ClientMessage.get_size()])

                # Send the data to the client with the length field
                broadcast_message(header.clientID, message)
        except Exception as e:
            print("[handle_message] Error:", str(e))

            # Remove the client from the list of clients
            clients.remove_client_socket(client_socket)
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


while True:
    print("Waiting for a client...")
    conn, address = socket_server.accept()

    # start a new thread to handle the client
    threading.Thread(target=handle_message, args=(conn,)).start()
