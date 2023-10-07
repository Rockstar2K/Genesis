#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import sys
import io
import interpreter
import os
import traceback  # Import traceback module for error details


sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def Set_API_Key(key):
    interpreter.api_key = key

# ...

def read_conversation_history():
    # Get the root directory of your project
    root_dir = os.path.dirname(os.path.abspath(__file__))
    
    # Construct the path to the conversation history file
    file_path = os.path.join(root_dir, "memory.txt")
    
    if os.path.exists(file_path):
        with open(file_path, 'r', encoding='utf-8') as file:
            return file.read()
    return ""

# ...

def OI_Python2(message, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    try:


        # Read the conversation history
        conversation_history = read_conversation_history()
        
        # Get the root directory of your project
        root_dir = os.path.dirname(os.path.abspath(__file__))
    
        # Construct the path to the conversation history file
        file_path = os.path.join(root_dir, "memory.txt")
        
        customPrompt = f"Your memory is in {file_path}, add atomic bullet points of the important details of our conversation to the file so you can remember them. Dont tell me anything about it, dont give me a plan, just do it quietly."
        
        interpreter.model = "gpt-3.5-turbo"

        interpreter.auto_run = True  # Set auto_run to True to bypass user confirmation

        if conversation_history:
            for chunk in interpreter.chat(f"Current message/task:{message} Memory of our previous conversations:{conversation_history} Memory instructions:{customPrompt}", stream = True, display = False):
                print(chunk, flush=True)
                #return chunk
        else:
            for chunk in interpreter.chat(f"Current message/task:{message} Memory Instructions:{customPrompt}", stream = True, display = False):
                print(chunk, flush=True)
               # return chunk
           
           
    except Exception as e:
         return f"Error: {e}\n{traceback.format_exc()}"

def OI_Python(message, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    OI_Python2(message, api_key)

# ...

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)