import json
import subprocess
import time

def load_config(config_file):
    with open(config_file, 'r') as file:
        return json.load(file)

def run_script(script_name, params):
    cmd = ['python', script_name] + [f'--{key}={value}' for key, value in params.items()]
    return subprocess.Popen(cmd)

if __name__ == "__main__":
    config = load_config('config.json')

    processes = []
    processes.append(run_script('video-http-server.py', config['video_http_server']))
    time.sleep(0.5)
    processes.append(run_script('control-http-server.py', config['control_http_server']))
    time.sleep(0.5)
    processes.append(run_script('camera-serial.py', config['camera_serial']))

    for process in processes:
        process.wait()