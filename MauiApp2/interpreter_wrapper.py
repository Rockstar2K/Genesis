'''
# coding=utf-8
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
        for chunk in interpreter.chat(message): #, stream=True, display=False
            return chunk
    except Exception as e:
        return f"Error: {e}"

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)

    
# coding=utf-8
import sys
import io
import interpreter
import os

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def Set_API_Key(key):
    interpreter.api_key = "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"

def read_conversation_history():
    # Specify the path to the conversation history file
    file_path = "C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\user_prompts_and_responses.txt"
    if os.path.exists(file_path):
        with open(file_path, 'r', encoding='utf-8') as file:
            return file.read()
    return ""

def OI_Python(message, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    try:
        # Read the conversation history
        conversation_history = read_conversation_history()
        interpreter.auto_run = True  # Set auto_run to True to bypass user confirmation
        for chunk in interpreter.chat(message + "context of our previous conversation: " + conversation_history): #, stream=True, display=False
            print(chunk)
            return chunk
    except Exception as e:
        return f"Error: {e}"

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)
'''
# coding=utf-8
import sys
import io
import interpreter
import os

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def Set_API_Key(key):
    interpreter.api_key = "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"

def read_conversation_history():
    # Specify the path to the conversation history file
    file_path = "C:\\Users\\thega\\source\\repos\\MauiApp2\\MauiApp2\\user_prompts_and_responses.txt"
    if os.path.exists(file_path):
        with open(file_path, 'r', encoding='utf-8') as file:
            return file.read()
    return ""

def OI_Python(message, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    try:
        # Read the conversation history
        conversation_history = read_conversation_history()
        interpreter.auto_run = True  # Set auto_run to True to bypass user confirmation
        
        if conversation_history:
            for chunk in interpreter.chat(f"{message}context of our previous conversation: {conversation_history}"):
                print(chunk)
                return chunk
        else:
            for chunk in interpreter.chat(message): #, stream=True, display=False for the chunks but doesnt works
                print(chunk)
                return chunk    
    except Exception as e:
        return f"Error: {e}"

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)