// Copyright 2023 Plerx2493
//
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Net;
using System.Text;
using Microsoft.Extensions.Hosting;
using Serilog;

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

    private HttpListener _listener;
    private string _url;
    private Thread? _listenTask;

    private static readonly ILogger Logger = Log.ForContext<TokenListener>();

    public TokenListener(string port, string path = "/")
    {
        _url = $"http://localhost:{port}{path}";
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);
    }

    public void Dispose()
    {
        _listener.Abort();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Logger.Information("Listening for connections on {Url}", _url);

        _listenTask = new Thread(() => _ = HandleIncomingConnections(cancellationToken))
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
        Logger.Information("Tokenlistener stopped");
        return Task.CompletedTask;
    }

    private async Task HandleIncomingConnections(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // Will wait here until we hear from a connection
            var ctx = await _listener.GetContextAsync();

            // Peel out the requests and response objects
            var req = ctx.Request;
            var resp = ctx.Response;

            //TODO add token saving
            var userToken = req.QueryString.Get("code");

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
}