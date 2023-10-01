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
        for chunk in interpreter.chat(message):
            return chunk
    except Exception as e:
        return f"Error: {e}"

if __name__ == "__main__":
    message = sys.argv[1]
    api_key = sys.argv[2] if len(sys.argv) > 2 else None
    result = OI_Python(message, api_key)
    print(result)

