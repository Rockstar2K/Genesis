#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import sys
import io
import interpreter
import os
import traceback  # Import traceback module for error details


sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def Set_API_Key(key):
    interpreter.api_key = "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"

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

def OI_Python(message, api_key=None):
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
        
        interpreter.auto_run = True  # Set auto_run to True to bypass user confirmation
        
        if conversation_history:
            for chunk in interpreter.chat(f"Current message:{message} Memory of our conversation:{conversation_history} Memory:{customPrompt}"):
                return chunk
        else:
            for chunk in interpreter.chat(f"Current message:{message} Memory:{customPrompt}"):
                return chunk
           
           
    except Exception as e:
         return f"Error: {e}\n{traceback.format_exc()}"

# ...

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)