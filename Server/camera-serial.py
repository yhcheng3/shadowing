"""camera-serial.py

Camera controller. Sends GET requests to the HTTP server in `control-http-server.py`, and controls camera via serial. Run on a laptop connected via USB to the camera.

To run the script, you can execute it directly, or pass the following as command-line arguments. If no arguments are provided, default values will be used.

Args:
    serial_port (str): Serial port the camera is attached to.
    url (str): URL to the server in control-http-server.py.

Example usage:
    `python camera-serial.py --serial_port COM10 --url http://127.0.0.1:80/orientation`

"""

import argparse
from queue import Queue
import threading
import requests
import time
import serial
import numpy as np

def init():
    '''
    Call when the camera is initialised, to set the camera to a known orientation.
    '''
    pass

def package(is_same, is_down, is_right):
    """Packages booleans into a frame for communication.

    Args:
        is_same (list<bool>): List of three boolean values indicating if the pitch, roll, and yaw are the same as the previous orientation data.
        is_down (bool): Boolean value indicating if the pitch is down.
        is_right (bool): Boolean value indicating if the yaw is right.

    Returns:
        bytearray: Frame for communication.
    """
    # Last entry is pitch (-10000 - 10000); 
    # normal_comm = bytearray([0x72]) + bytearray(struct.pack('h', -10000)) + bytearray(struct.pack('h', 0)) # B, C, D (3-7)
    # now = datetime.now()

    normal_comm = bytearray([0] * 5)
    flight_param = bytearray([0] * 30) # E (8-37)
    
    # Speed control (not position control)
    if all(is_same):
        print('All are the same')
        control_comm = bytearray([0] * 5)
    else:
        # 2: Yaw
        if is_same[2]:
            print('Yaw is the same')
            control_comm = bytearray([0x70, 0, 0])
        elif is_right:
            print('Yaw is right')
            control_comm = bytearray([0x70, 0x64, 0])
        else: # is_left
            print('Yaw is left')
            control_comm = bytearray([0x70, 0x9c, 0xff])
         
        # 0: Pitch   
        if is_same[0]:
            print('Pitch is the same')
            control_comm += bytearray([0, 0])
        elif is_down:
            print('Pitch is down')
            control_comm += bytearray([0x9c, 0xff])
        else:
            print('Pitch is up')
            control_comm += bytearray([0x64, 0])
        
    frame = normal_comm + flight_param + control_comm
    parity = np.bitwise_xor.reduce(frame)  
    frame = bytearray([0xFB, 0x2C]) + frame + bytearray([parity, 0xF0])
    # print(frame.hex())
    
    return frame

def package_dir(command):
    """
    Packages command into a frame for communication.

    Args:
        command (str): The command to be packaged. Valid commands are 'up', 'down', 'left', 'right'.

    Returns:
        frame (bytearray): The packaged frame for communication.
    """
    
    normal_comm = bytearray([0] * 5)
    flight_param = bytearray([0] * 30) # E (8-37)
            
    if command == 0:   # Up
        control_comm = bytearray([0x70, 0, 0, 0x64, 0])
    elif command == 1: # Down
        control_comm = bytearray([0x70, 0, 0, 0x9c, 0xff])
    elif command == 2: # Left
        control_comm = bytearray([0x70, 0x9c, 0xff, 0, 0])
    elif command == 3: # Right
        control_comm = bytearray([0x70, 0x64, 0, 0, 0])
    else:
        control_comm = bytearray([0] * 5)
        
    frame = normal_comm + flight_param + control_comm
    parity = np.bitwise_xor.reduce(frame)  
    frame = bytearray([0xFB, 0x2C]) + frame + bytearray([parity, 0xF0])
    print(frame.hex())
    
    return frame

def send_frame(ser, lens_queue):
    """Receive orientation data from the HoloLens and send frame to the camera.

    Args:
        ser (serial.Serial): Serial object for communication with the camera.
        lens_queue (Queue): Queue for storing orientation data from the HoloLens.
    """
    
    # TODO: Camera covers larger range when Hololens moves slowly (sampling rate issue)
    lens_data = None 
    while True:
        try:
            prev_lens_data = lens_data
            
            # Receives lens_queue from send_get_request()
            lens_data = lens_queue.get() 
            
            if lens_data['is_manual']:
                frame = package_dir(lens_data['direction'])
                ser.write(frame)
            elif prev_lens_data is not None and 'pitch' in prev_lens_data:
                # Compare with prev_lens_data
                diff_data = [(lens_data[key] - prev_lens_data[key] + 180) % 360 - 180 for key in lens_data.keys() if key != 'is_manual']
                
                # Noise threshold = 0.5
                is_same = [abs(data) <= 0.5 for data in diff_data]
                
                # 0: Pitch; 1: Roll; 2: Yaw
                frame = package(is_same, diff_data[0] > 0, diff_data[2] > 0)
                ser.write(frame)
            time.sleep(0.04)
            
        except KeyboardInterrupt:
            break
    
def send_get_request(url, lens_queue):
    """Send a GET request to the specified URL.

    Args:
        url (str): The URL to send the GET request to.
        lens_queue (Queue): Queue for storing orientation data from the HoloLens.
    """
    while True:
        try:
            response = requests.get(url)
            print(response.json())
            
            # Check if the request was successful (status code 200)
            if response.status_code == 200:
                # Parse the JSON response (R, P, Y)
                data = response.json()
                lens_queue.put(data)
            else:
                print(f"Error: {response.status_code}")
        except requests.exceptions.RequestException as e:
            print(f"Error: {e}")
        except KeyboardInterrupt:
            break
        time.sleep(0.10)

def main(serial_port, url):
    baud_rate = 115200
    ser = serial.Serial(serial_port, baud_rate)
    # cam_queue = Queue() # Stores orientation data from camera
    lens_queue = Queue() # Stores orientation data from HoloLens
        
    try:
        # Create and start the threads
        # recv_frame_thread = threading.Thread(target=lambda: recv_frame(ser, cam_queue))
        send_frame_thread = threading.Thread(target=lambda: send_frame(ser, lens_queue))
        get_request_thread = threading.Thread(target=lambda: send_get_request(url, lens_queue))

        # recv_frame_thread.start()
        send_frame_thread.start()
        get_request_thread.start()

        # Keep the main thread running to allow threads to execute
        # recv_frame_thread.join()
        send_frame_thread.join()
        get_request_thread.join()
    except KeyboardInterrupt:
        pass
    finally:
        ser.close()
    
if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--serial_port', required=True)
    parser.add_argument('--url', required=True)
    args = parser.parse_args()

    main(args.serial_port, args.url)