using System.Text;
using System.Net;
using Microsoft.Extensions.Hosting;

namespace MADS.Services;

public class TokenListener
{
    Task                        listenTask;
    private static HttpListener listener;
    private static string       url;
    
    // lang=html
    private const string pageData = 
            """
            <!DOCTYPE>
            <html lang="en">
                <head>
                    <title>HttpListener Example</title>
                </head>
                <body onload = "myFunction()"> </body>
                <script type= "application/javascript"> function myFunction(){ window.close() }</script>
            </html >
            """;

    private static async Task HandleIncomingConnections(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // Will wait here until we hear from a connection
            HttpListenerContext ctx = await listener.GetContextAsync();

            // Peel out the requests and response objects
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;
            
            //TODO add token saving
            //var userToken = req.QueryString.Get("code");

            // Write the response info
            byte[] data = Encoding.UTF8.GetBytes(pageData);
            resp.ContentType = "text/html";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;
            //resp.StatusCode = 200;

            // Write out to the response stream (asynchronously), then close it
            await resp.OutputStream.WriteAsync(data, 0, data.Length);
            
            resp.Close();
        }
    }

    public TokenListener(string port, string path = "/")
    {
        url = $"http://localhost:{port}{path}";
        listener = new HttpListener();
        listener.Prefixes.Add(url);
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        listener.Start();
        Console.WriteLine("Listening for connections on {0}", url);
        
        listenTask = HandleIncomingConnections(cancellationToken);
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        listener.Abort();
        listenTask.Dispose();
        return Task.CompletedTask;
    }
}