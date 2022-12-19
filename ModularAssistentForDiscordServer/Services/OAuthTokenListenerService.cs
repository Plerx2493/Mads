using System.Net;
using System.Text;

namespace MADS.Services;

public class TokenListener
{
    // lang=html
    private const string pageData =
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
    private static string       _url;
    private        Task         _listenTask;

    public TokenListener(string port, string path = "/")
    {
        _url = $"http://localhost:{port}{path}";
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);
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
            var data = Encoding.UTF8.GetBytes(pageData);
            resp.ContentType = "text/html";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;
            //resp.StatusCode = 200;

            // Write out to the response stream (asynchronously), then close it
            await resp.OutputStream.WriteAsync(data, 0, data.Length);

            resp.Close();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Console.WriteLine("Listening for connections on {0}", _url);

        _listenTask = HandleIncomingConnections(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _listener.Abort();
        _listenTask.Dispose();
        return Task.CompletedTask;
    }
}