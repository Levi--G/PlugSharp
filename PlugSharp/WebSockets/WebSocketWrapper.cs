using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlugSharp.WebSockets
{
    internal class WebSocketWrapper : IDisposable
    {
        public event EventHandler<WebSocketWrapper> OnConnected;
        public event EventHandler<string> OnMessageReceived;
        public event EventHandler<WebSocketWrapper> OnDisconnected;
        public event EventHandler<Exception> OnError;

        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private ClientWebSocket socket;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;
        private AsyncLock _asyncSendLock = new AsyncLock();
        private AsyncLock _asyncConnectLock = new AsyncLock();

        public WebSocketState State => socket.State;

        public WebSocketWrapper(string protocol = null, IDictionary<string, string> headers = null)
        {
            socket = new ClientWebSocket();
            socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _cancellationToken = _cancellationTokenSource.Token;
            if (protocol != null)
            {
                socket.Options.AddSubProtocol(protocol);
            }
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    socket.Options.SetRequestHeader(header.Key, header.Value);
                }
            }
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public async Task Open(string uri)
        {
            using (await _asyncConnectLock.LockAsync())
            {
                if (socket.State != WebSocketState.Open)
                {
                    await socket.ConnectAsync(new Uri(uri), _cancellationToken);
                    OnConnected?.Invoke(this, this);
                }
            }
        }

        public async Task Close()
        {
            try
            {
                using (await _asyncConnectLock.LockAsync())
                {
                    _cancellationTokenSource.Cancel();
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public async Task Send(string message)
        {
            if (socket.State != WebSocketState.Open)
            {
                OnError?.Invoke(this, new Exception("WebSocket:Send : Connection is not open."));
                return;
            }
            try
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                using (await _asyncSendLock.LockAsync())
                {
                    await socket.SendAsync(new ArraySegment<byte>(messageBuffer, 0, messageBuffer.Length), WebSocketMessageType.Text, true, _cancellationToken);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
            }
        }

        public async void StartListen()
        {
            await Listen().ConfigureAwait(false);
        }

        public async Task Listen()
        {
            var buffer = new byte[ReceiveChunkSize];

            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    var stringResult = new StringBuilder();

                    WebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken).ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                            //OnDisconnected?.Invoke(this, this);
                            break;
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!(result?.EndOfMessage ?? true));
                    if (result != null && result.MessageType == WebSocketMessageType.Text)
                    {
                        OnMessageReceived?.Invoke(this, stringResult.ToString());
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, ex);
                }
            }
            OnDisconnected?.Invoke(this, this);
        }

        public void Dispose()
        {
            if (socket != null)
            {
                _cancellationTokenSource.Cancel();
                socket.Dispose();
                socket = null;
            }
        }
    }
}
