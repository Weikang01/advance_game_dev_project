from bidict import bidict


class Clients:
    def __init__(self):
        self.clients = bidict()
        self.id_list = []

    def add_client(self, client_id, client_socket):
        self.clients.put(client_id, client_socket)
        self.id_list.append(client_id)

    def remove_client(self, client_id):
        self.clients.pop(client_id)
        self.id_list.remove(client_id)

    def remove_client_socket(self, client_socket):
        self.clients.inverse.pop(client_socket)

    def get_client_id(self, client_socket):
        return self.clients.inverse.get(client_socket)

    def get_client(self, client_id):
        return self.clients.get(client_id)

    def get_client_ids(self):
        return self.clients.keys()

    def get_client_sockets(self):
        return self.clients.values()

    def get_client_count(self):
        return len(self.clients)

    def __len__(self):
        return len(self.clients)

    def __getitem__(self, item):
        try:
            return self.id_list[item]
        except Exception as e:
            print("[Clients] Error getting item:", str(e))
            return None
