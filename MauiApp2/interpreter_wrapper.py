#!/usr/bin/env python3
# -- coding: utf-8 --
import sys
import io
import interpreter
import os
import traceback  # Import traceback module for error details


sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def Set_API_Key(key):
    interpreter.api_key = key

# ...

def OI_Python2(message, api_key=None):
    if api_key:
        Set_API_Key(api_key)
    try:

        interpreter.model = "gpt-3.5-turbo"
        interpreter.auto_run = True  # Set auto_run to True to bypass user confirmation

        for chunk in interpreter.chat(f"Current message/task:{message}", stream=True, display=False):
            print(chunk, flush=True)
                  
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