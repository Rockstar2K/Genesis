import win32pipe, win32file
import interpreter_wrapper

def send_message(message):
    win32file.WriteFile(pipe_handle, message.encode('utf-8'))


def main():
    pipe_handle = win32pipe.CreateNamedPipe(
        r'\\.\pipe\OpenAIPipe',
        win32pipe.PIPE_ACCESS_DUPLEX,
        win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
        1, 65536, 65536,
        0,
        None)
    win32pipe.ConnectNamedPipe(pipe_handle, None)
    
    while True:
        message, api_key = win32file.ReadFile(pipe_handle, 4096)
        result = interpreter_wrapper.OI_Python(message.decode('utf-8'), api_key)
        send_message(result)

if __name__ == "__main__":
    main()
