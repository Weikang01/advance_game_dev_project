from PIL import Image
from io import BytesIO
from torchvision import transforms

from messages import MessageHeader, MessageType, ScreenShotMessage, ServerMessage
from clients import Clients
import socket

import threading

dqn = None

clients = Clients()


def remove_client(client: int or socket):
    if isinstance(client, int):
        id_to_remove = client
    else:
        id_to_remove = clients.get_client_id(client)
        if id_to_remove is None:
            return

    for client_id in clients.get_client_ids():
        if client_id != id_to_remove:
            try:
                # Pack the location data
                send_data_to_client(client_id, ServerMessage.from_json(client_id,
                                                                       {"action": "disconnect",
                                                                        "id": id_to_remove}).get_bytes())
            except Exception as e:
                print("[remove_client] Error sending data to client:", str(e))
                # Remove the client from the list of clients
                remove_client(client_id)

    try:
        clients.remove_client_socket(client)
    except Exception as e:
        print("[remove_client] Error removing client:", str(e))


def send_data_to_client(client_id, data):
    # Calculate the length of the data and pack it into 2 bytes
    try:
        # Send the actual data
        clients.get_client(client_id).send(data)
    except Exception as e:
        print("[send_data_to_client] Error sending data to client:", str(e))
        # Remove the client from the list of clients
        remove_client(client_id)


def broadcast_message(message):
    for client_id in clients.get_client_ids():
        try:
            # Pack the location data
            send_data_to_client(client_id, message.get_bytes())
        except Exception as e:
            print("[broadcast_location] Error sending data to client:", str(e))
            # Remove the client from the list of clients
            remove_client(client_id)


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
                last_client_id = clients[-1] if len(clients) > 0 else 0
                clients.add_client(header.clientID, client_socket)
                last_client_id = clients[-1]

                print("Client", header.clientID, "connected， last_client_id:", last_client_id)
                if clients.get_client_count() % 1 == 0:
                    broadcast_message(ServerMessage.from_json(header.clientID,
                                                              {"action": "create_string",
                                                               "from": last_client_id,
                                                               "to": header.clientID}))

            if header.messageType == MessageType.SCREENSHOT.value:
                message = ScreenShotMessage.from_bytes(recvmsg[:ScreenShotMessage.get_size()])
                message.data = recvmsg[ScreenShotMessage.get_size():]
                image_data = BytesIO(message.data)
                image = Image.open(image_data)
                transform = transforms.ToTensor()
                tensor_image = transform(image)
                # print(tensor_image.shape)

        except Exception as e:
            print("[handle_message] Error:", str(e))

            # Remove the client from the list of clients
            remove_client(client_socket)
            break
    client_socket.close()


# create a socket object
socket_server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
HOST = "127.0.0.1"
PORT = 5010
# bind to the port
socket_server.bind((HOST, PORT))
# set the maximum number of connections to accept
socket_server.listen(5)

while True:
    print("Waiting for a client...")
    conn, address = socket_server.accept()

    # start a new thread to handle the client
    threading.Thread(target=handle_message, args=(conn,)).start()
