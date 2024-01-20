import socket
import threading
import logging

from messages import MessageHeader, MessageType, ServerMessage, ClientMessage, ActionType
from clients import Clients

dqn = None

clients = Clients()

# initialize logging
logging.getLogger().addHandler(logging.StreamHandler())
logging.getLogger().setLevel(logging.INFO)


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
        clients.remove_client(id_to_remove)
    except Exception as e:
        logging.error("[remove_client] Error removing client: %s", str(e), exc_info=True)


def send_data_to_client(client_id, data):
    # Calculate the length of the data and pack it into 2 bytes
    try:
        # Send the actual data
        clients.get_client(client_id).send(data)
    except Exception as e:
        remove_client(client_id)


def broadcast_message(message):
    ids_to_remove = []
    client_ids = list(clients.get_client_ids())

    for client_id in client_ids:
        try:
            # Pack the location data
            send_data_to_client(client_id, message.get_bytes())
        except Exception as e:
            logging.error("[broadcast_location] Error sending data to client: %s", str(e), exc_info=True)
            ids_to_remove.append(client_id)

    for client_id in ids_to_remove:
        remove_client(client_id)


def handle_message(client_socket):
    global dqn
    while True:
        try:
            # handle client socket closing
            if client_socket.fileno() == -1:
                remove_client(client_socket)
                break

            recvmsg = client_socket.recv(1024)
            if len(recvmsg) == 0:
                break  # client disconnected

            # print("MessageHeader.get_size(): ", MessageHeader.get_size())
            header = MessageHeader.from_bytes(recvmsg[:MessageHeader.get_size()])

            if header.clientID not in clients.get_client_ids():
                last_client_id = clients[-1] if len(clients) > 0 else 0
                clients.add_client(header.clientID, client_socket)

                logging.log(msg="Client connected with ID: " + str(header.clientID), level=logging.INFO)
                if clients.get_client_count() % 2 == 0:
                    broadcast_message(ServerMessage.from_json(header.clientID,
                                                              {"action": "create_string",
                                                               "from": last_client_id,
                                                               "to": header.clientID}))

            if header.messageType == MessageType.CLIENT_MESSAGE.value:
                message = ClientMessage.from_bytes(recvmsg[:ClientMessage.get_size()])

                if message.action_type == ActionType.QUIT.value:
                    remove_client(message.id)
                    break
                else:
                    # Send the data to the client with the length field
                    broadcast_message(message)
        except Exception as e:
            if isinstance(e, ConnectionResetError):
                logging.log(msg="Client disconnected", level=logging.INFO)
            else:
                logging.error("[handle_message] Error: %s", str(e), exc_info=True)

            # Remove the client from the list of clients
            remove_client(client_socket)
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
    logging.log(msg="Waiting for a client...", level=logging.INFO)
    conn, address = socket_server.accept()

    # start a new thread to handle the client
    threading.Thread(target=handle_message, args=(conn,)).start()
