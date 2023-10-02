'''
import sys
import io
import interpreter

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def Set_API_Key(key):
    interpreter.api_key = "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"  # Assuming this sets the API key for the interpreter

def OI_Python(message, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    try:
        for chunk in interpreter.chat(message + 'Run shell commands with -y so the user doesnt have to confirm them.'):
            return chunk
    except Exception as e:
        return f"Error: {e}"

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)
 
    '''

import sys
import io
import interpreter

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def Set_API_Key(key):
    interpreter.api_key = "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"

def OI_Python(message, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    try:
        interpreter.auto_run = True  # Set auto_run to True to bypass user confirmation
        for chunk in interpreter.chat(message):
            return chunk
    except Exception as e:
        return f"Error: {e}"

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)

