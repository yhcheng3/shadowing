"""control-http-server.py

Basic HTTP server. Run on a laptop connected via USB to the camera. The client (i.e., Hololens) POSTs orientation data to it, and `camera-serial.py` GETs this data.

To run the script, you can execute it directly, or pass the following as command-line arguments. If no arguments are provided, default values will be used.

Args:
    ip (str): IP address to run the server on.
    port (int): Port to run the server on.

Example usage:
    `python control-http-server.py --ip 0.0.0.0 --port 80`
    The client will then be able to POST and GET orientation data to/from `http://0.0.0.0:80/orientation`.
"""

import argparse
from flask import Flask, request, jsonify

app = Flask(__name__)

# Global variable to store orientation data
orientation_data = {}

@app.route('/orientation', methods=['POST', 'GET'])
def orientation():
    """Receives and returns orientation data, which is stored in the global variable `orientation_data`.
    
    """
    global orientation_data

    if request.method == 'POST':
        data = request.json
        if not data:
            return jsonify({'error': 'No data provided'}), 400

        is_manual = data.get('isManual')
        
        if is_manual:
            print(is_manual) 
            direction = data.get('direction')
            
            orientation_data = {
            'is_manual': is_manual,
            'direction': direction
        }
        else:
            roll = data.get('roll')
            pitch = data.get('pitch')
            yaw = data.get('yaw')
            
            orientation_data = {
            'is_manual': is_manual,
            'roll': roll,
            'pitch': pitch,
            'yaw': yaw
        }

        return jsonify({'message': 'Data received'}), 200

    elif request.method == 'GET':
        if not orientation_data:
            return jsonify({'error': 'No data available'}), 404

        return jsonify(orientation_data), 200

def main(ip, port):
    app.run(host=ip, port=int(port))
    
if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('--ip', required=True)
    parser.add_argument('--port', required=True)
    args = parser.parse_args()

    main(args.ip, args.port)