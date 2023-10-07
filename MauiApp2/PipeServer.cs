using System.Diagnostics;
using System.IO.Pipes;

public class PipeServer
{
    private NamedPipeServerStream pipeServer;

    public void Start()
    {
        pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut);
        Debug.WriteLine("Named Pipe server waiting for client connection...");
        pipeServer.WaitForConnection();
        Debug.WriteLine("Client connected.");
    }

    public async Task<string> WaitForMessageAsync()
    {
        using (var reader = new StreamReader(pipeServer))
        {
            return await reader.ReadToEndAsync();
        }
    }
}
