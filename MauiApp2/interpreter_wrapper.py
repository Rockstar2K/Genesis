#!/usr/bin/env python3
# -- coding: utf-8 --
import sys
import io
import interpreter
import os
import json
import traceback  # Import traceback module for error details
import subprocess


sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
root_dir = os.path.dirname(os.path.abspath(__file__))
 
#...

def Set_API_Key(key):
    interpreter.api_key = key

# ...


def log_error(*args):
    # Add proper logging here
    print("Log Error:", *args)

# ...


def read_prompt(filename='lyris_Prompt.txt'):
    
    # Construct the full file path
    file_path = os.path.join(root_dir, "Resources", "pPROMPTS", filename)

    if os.path.exists(file_path):
        with open(file_path, 'r', encoding='utf-8') as file:
            return file.read()
    return ""

# ...

def save_chat_history(messages, filename='chat_history.txt'):
    file_path = os.path.join(root_dir, "pMEMORY", filename)
    with open(file_path, 'w', encoding='utf-8') as file:
        json.dump(messages, file, ensure_ascii=False, indent=4)

# ...

def load_chat_history(filename='chat_history.txt'):
    file_path = os.path.join(root_dir, "pMEMORY", filename)
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            return json.load(file)
    except (FileNotFoundError, json.JSONDecodeError) as e:
        # Log errors rather than print them for production-ready code
        log_error("Loading chat history failed:", e)
        return []

# ...

def run_interpreter(userPrompt, api_key=None, interpreter_model=None):
    if api_key:
        Set_API_Key(api_key)
    try:
        # It would be better to manage the chat history state outside this function
        messages = load_chat_history()
        interpreter.messages.extend(messages)

        interpreter.system_message = read_prompt()
        interpreter.model = interpreter_model
        interpreter.auto_run = True
        #interpreter.vision = True

        output = interpreter.chat(userPrompt, stream=True, display=False)
        for chunk in output:
            print(chunk, flush=True)

        interpreter.messages.extend(output)
        save_chat_history(interpreter.messages)
        
    except Exception as e:
        log_error(f"Error: {e}\n{traceback.format_exc()}")

    
#...

if __name__ == "__main__":
    userPrompt = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    interpreter_model = sys.argv[3] if len(sys.argv) > 3 else None
    result = run_interpreter(userPrompt, api_key, interpreter_model)