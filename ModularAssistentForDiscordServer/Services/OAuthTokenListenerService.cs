using System.Net;
using System.Text;
using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MADS.Services;

public class TokenListener : IDisposable, IHostedService
{
    // lang=html
    private const string PageData =
        """
            <!DOCTYPE>
            <html lang="en">
            <head>
                <title>HttpListener Example</title>
            </head>
            <body onload="myFunction()"> </body>
            <script type="application/javascript"> function myFunction(){ window.close() }</script>
            </html >
            """;

    private static HttpListener _listener;
    private static string _url;
    private Thread _listenTask;
    private DiscordClient _client;

    public TokenListener(string port, DiscordClient client, string path = "/")
    {
        _url = $"http://localhost:{port}{path}";
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);
        _client = client;
    }

    public void Dispose()
    {
        _listener.Abort();
    }

    private static async Task HandleIncomingConnections(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // Will wait here until we hear from a connection
            var ctx = await _listener.GetContextAsync();

            // Peel out the requests and response objects
            var req = ctx.Request;
            var resp = ctx.Response;

            //TODO add token saving
            //var userToken = req.QueryString.Get("code");

            // Write the response info
            var data = Encoding.UTF8.GetBytes(PageData);
            resp.ContentType = "text/html";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;
            //resp.StatusCode = 200;

            // Write out to the response stream (asynchronously), then close it
            await resp.OutputStream.WriteAsync(data, 0, data.Length, token);

            resp.Close();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        _client.Logger.LogInformation("Listening for connections on {url}", _url);
        
        _listenTask = new Thread(() => HandleIncomingConnections(cancellationToken)) 
        {
            
            IsBackground = true
        };
        _listenTask.Start();

        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _listener.Abort();
        _listenTask.Interrupt();
        _client.Logger.LogInformation("Tokenlistener stopped");
        return Task.CompletedTask;
    }
}