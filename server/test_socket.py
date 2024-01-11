import socket
import struct
import threading

clients = []  # list of clients connected to the server


def send_data_to_client(client_socket, data):
    # Calculate the length of the data and pack it into 2 bytes
    data_length = len(data)
    length_bytes = struct.pack('h', data_length)

    data = length_bytes + data
    try:
        # Send the actual data
        client_socket.send(data)
    except Exception as e:
        print("[send_data_to_client] Error sending data to client:", str(e))
        # Remove the client from the list of clients
        clients.remove(client_socket)


def broadcast_location(sender, x, y):
    for client in clients:
        if True:
        # if client != sender:
            try:
                # Pack the location data
                print("broadcasting location", x, y)
                data = struct.pack('f', x) + struct.pack('f', y)
                # Send the data to the client with the length field
                send_data_to_client(client, data)
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
            length = struct.unpack('h', recvmsg[:2])[0]

            # extract the data
            # x-axis coordinate of the client (float)
            x = struct.unpack('f', recvmsg[2:6])[0]
            # y-axis coordinate of the client (float)
            y = struct.unpack('f', recvmsg[6:10])[0]

            # Send the data to the client with the length field
            broadcast_location(client_socket, x, y)
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
