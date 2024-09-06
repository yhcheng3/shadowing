'''video-http-server.py

Basic port forwarder. Run on a laptop connected via USB to the camera. Forwards RTSP stream from the camera and allows retrieval by the client (i.e., Hololens).

To run the script, you can execute it directly or pass the following as command-line arguments. If no arguments are provided, default values will be used.

Args:
    local_port (int): Local port to run the server on. 
    remote_host (str): Camera IP.
    remote_port (int): Camera port.
    
    
Example usage:
    `python camera-serial.py --local_port 8008 --remote_host 192.168.8.101 --remote_port 554`
    The client will then be able to access the stream at `rtsp://localhost:8008`.
'''

import argparse
import socket
import sys
import threading

def handle_client(client_socket, remote_host, remote_port):
    # Connect to the remote host
    remote_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    remote_socket.connect((remote_host, remote_port))

    # Function to forward data between sockets
    def forward(source, destination):
        while True:
            data = source.recv(4096)
            if len(data) == 0:
                break
            destination.send(data)

    # Start forwarding data in both directions
    threading.Thread(target=forward, args=(client_socket, remote_socket)).start()
    threading.Thread(target=forward, args=(remote_socket, client_socket)).start()

def start_forwarding(local_port, remote_host, remote_port):
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind(('0.0.0.0', local_port))
    server.listen(5)
    print(f'Forwarding from 0.0.0.0:{local_port} to {remote_host}:{remote_port}')

    while True:
        client_socket, addr = server.accept()
        print(f'Accepted connection from {addr}')
        handle_client(client_socket, remote_host, remote_port)


def main(local_port, remote_host, remote_port):
    start_forwarding(int(local_port), remote_host, int(remote_port))
    
if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--local_port', required=True)
    parser.add_argument('--remote_host', required=True)
    parser.add_argument('--remote_port', required=True)
    args = parser.parse_args()

    main(args.local_port, args.remote_host, args.remote_port)
    