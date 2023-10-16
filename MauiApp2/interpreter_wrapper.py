#!/usr/bin/env python3
# -- coding: utf-8 --
import sys
import io
import interpreter
import os
import json
import traceback  # Import traceback module for error details

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
root_dir = os.path.dirname(os.path.abspath(__file__))


def Set_API_Key(key):
    interpreter.api_key = key

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
    
    # Step 1: Load existing messages
    existing_messages = load_chat_history(filename)
    
    # Step 2: Append new messages to the existing ones
    existing_messages.extend(messages)
    
    # Step 3: Save all messages back to the file
    with open(file_path, 'w', encoding='utf-8') as file:
        json.dump(existing_messages, file, ensure_ascii=False, indent=4)
        
# ...

def load_chat_history(filename='chat_history.txt'):
    file_path = os.path.join(root_dir, "pMEMORY", filename)
    messages = []
    
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            messages = json.load(file)
    except FileNotFoundError:
        pass  # It's okay if the file does not exist
    except json.JSONDecodeError:
        print(f"Error: Could not decode JSON from {filename}")
    
    return messages  

# ...

def OI_Python2(prompt, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    try:
        #print("\n The conversation data saved is: ", load_chat_history())

        interpreter.messages += load_chat_history()

        interpreter.system_message = read_prompt()
        interpreter.model = "gpt-3.5-turbo"
        interpreter.auto_run = True  # Set auto_run to True to bypass user confirmation

        # Load the conversation history as a list
        #interpreter.messages = list(load_chat_history())
        output = interpreter.chat(f"{prompt}", stream=True, display=False)
        for chunk in output:
            print(chunk, flush=True)

        interpreter.messages += output
        save_chat_history(interpreter.messages)
        #print("\n The conversation data saved is: ", load_chat_history())
    except Exception as e:
         return f"Error: {e}\n{traceback.format_exc()}"

def OI_Python(message, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    OI_Python2(message, api_key)

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)