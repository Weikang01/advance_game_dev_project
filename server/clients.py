from bidict import bidict


class Clients:
    def __init__(self):
        self.clients = bidict()

    def add_client(self, client_id, client_socket):
        self.clients.put(client_id, client_socket)

    def remove_client(self, client_id):
        self.clients.pop(client_id)

    def remove_client_socket(self, client_socket):
        self.clients.inverse.pop(client_socket)

    def get_client(self, client_id):
        return self.clients.get(client_id)

    def get_client_ids(self):
        return self.clients.keys()

    def get_client_sockets(self):
        return self.clients.values()

    def get_client_count(self):
        return len(self.clients)
