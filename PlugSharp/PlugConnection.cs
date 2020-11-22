using JsonHCSNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlugSharp.Data;
using PlugSharp.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace PlugSharp
{
    ///https://github.com/plugcommunity/documentation/tree/master/api/endpoints
    public class PlugConnection : IDisposable
    {
        private JsonHCS WebClient;
        private WebSocketWrapper Websocket;
        public PlugAPI API { get; private set; }

        private string Email { get; set; }
        private string password { get; set; }

        public SynchronizationContext SynchronizationContext { get; set; }

        public event EventHandler<Chat> OnChat;

        public event EventHandler<ChatLog> OnChatLog;

        public event EventHandler<Vote> OnVote;

        public event EventHandler<Grab> OnGrab;

        public event EventHandler<User> OnUserJoin;

        public event EventHandler<UserLeave> OnUserLeave;

        public event EventHandler<Advance> OnAdvance;

        public event EventHandler<PlugConnection> OnConnected;

        public event EventHandler<PlugConnection> OnDisconnected;

        public event EventHandler<ErrorEventArgs> OnError;

        public bool IgnoreErrorDefault { get; set; }

        public bool SocketConnected { get { return Websocket?.State == WebSocketState.Open; } }

        public bool Connecting { get; private set; }

        public bool Reconnect { get; set; }

        public int ReconnectLimit { get; set; } = -1;

        public int Reconnected { get; set; }

        protected string LastSlug { get; set; }

        public PlugConnection(string email, string password, string slug = null)
        {
            this.Email = email;
            this.password = password;
            this.LastSlug = slug;
            _CreateAPI();
            _CreateWebsocket();
        }

        private void _CreateAPI()
        {
            WebClient = new JsonHCS(new JsonHCS_Settings()
            {
                CookieSupport = true,
                AddDefaultAcceptHeaders = true,
                UserAgent = "PlugSharp",
                //Host = "plug.dj",
                //AcceptLanguage = "en-US, en; q=0.5",
                //Referer = "https://plug.dj/nightcore-331",
                //Origin = "https://plug.dj"
            });
            var pg = new JsonHCSNet.Proxies.JsonHCSProxyGenerator(WebClient);
            API = pg.CreateClassProxy<PlugAPI>();
        }

        private void _CreateWebsocket()
        {
            var headers = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            headers.Add("origin", "https://plug.dj");
            Websocket = new WebSocketWrapper(null, headers);
            Websocket.OnMessageReceived += Websocket_OnMessage;
            Websocket.OnConnected += Websocket_OnOpened;
            Websocket.OnError += Websocket_OnError;
            Websocket.OnDisconnected += Websocket_OnClosed;
        }

        #region util

        private void DebugLog(string log)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(log);
#endif
        }

        private string Stringify(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        private int TimeStamp
        {
            get
            {
                return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }

        private void ExecuteSync(Action a)
        {
            if (SynchronizationContext != null && SynchronizationContext.Current != SynchronizationContext)
            {
                SynchronizationContext.Post((s) => { ExecuteSafe(a); }, null);
            }
            else
            {
                ExecuteSafe(a);
            }
        }

        private bool ExecuteSafe(Action a, Action<ErrorEventArgs> ErrorOverride = null)
        {
            try
            {
                a();
                return true;
            }
            catch (Exception e)
            {
                DebugLog(e.ToString());
                ErrorEventArgs args = new ErrorEventArgs() { Error = e, Ignore = IgnoreErrorDefault };

                ErrorOverride?.Invoke(args);
                if (OnError != null)
                {
                    ExecuteSync(() => { OnError(this, args); });
                }

                if (!args.Ignore)
                {
                    throw e;
                }
                return false;
            }
        }

        private async Task<bool> ExecuteSafeAsync(Func<Task> a, Action<ErrorEventArgs> ErrorOverride = null)
        {
            try
            {
                await a();
                return true;
            }
            catch (Exception e)
            {
                ErrorEventArgs args = new ErrorEventArgs() { Error = e, Ignore = IgnoreErrorDefault };

                ErrorOverride?.Invoke(args);
                if (OnError != null)
                {
                    ExecuteSync(() => { OnError(this, args); });
                }

                if (!args.Ignore)
                {
                    throw e;
                }
                return false;
            }
        }

        #endregion util

        #region private

        private async Task<cfst> _GetcfstAsync()
        {
            return (await API.GetMobileInit()).data.FirstOrDefault();
        }

        private Task _LoginAsync(cfst cfstData)
        {
            return API.Login(new LoginRequest() { email = Email, password = password, csrf = cfstData.c });
        }

        private async Task<string> _GetAuthTokenAsync()
        {
            return (await API.GetAuthToken()).data.FirstOrDefault();
        }

        private async void Websocket_OnClosed(object sender, WebSocketWrapper socket)
        {
            await ExecuteSafeAsync(async () =>
            {
                SendChatLog("Websocket: Connection Closed", ChatLogType.Debug);
                await Disconnect(true);
            });
        }

        private void Websocket_OnError(object sender, Exception obj)
        {
            if (OnError != null)
            {
                ExecuteSync(() =>
                {
                    OnError(sender, new ErrorEventArgs() { Error = obj, Ignore = true });
                });
            }
            SendChatLog("Websocket ERROR: " + obj.Message, ChatLogType.Error);
        }

        private void Websocket_OnOpened(object sender, WebSocketWrapper socket)
        {
            SendChatLog("Socket Opened", ChatLogType.Debug);
        }

        private void Websocket_OnMessage(object sender, string obj)
        {
            ExecuteSafe(() =>
            {
                if (obj == "h") { return; }
                if (obj.StartsWith("["))
                {
                    Task.Factory.StartNew(() =>
                    {
                        HandleMessageSync(obj);
                    });
                }
                else
                {
                    DebugLog("Unknown object: " + obj);
                }
            });
        }

        private void HandleMessageSync(string json)
        {
            ExecuteSafe(() =>
            {
                JArray jsonarray = JArray.Parse(json);
                foreach (var jsonevent in jsonarray.Children())
                {
                    string type = jsonevent["a"].Value<string>();
                    string newjson = jsonevent["p"].ToString();
                    HandleMessage(type, newjson);
                }
            });
        }

        private void HandleMessage(string type, string parameters)
        {
            switch (type)
            {
                case "ack":
                    break;

                case "chat":
                    if (OnChat != null)
                    {
                        var c = JsonConvert.DeserializeObject<Chat>(parameters);
                        c.message = System.Net.WebUtility.HtmlDecode(c.message);
                        ExecuteSync(() => { OnChat(this, c); });
                    }
                    break;

                case "vote":
                    if (OnVote != null)
                    {
                        ExecuteSync(() => { OnVote(this, JsonConvert.DeserializeObject<Vote>(parameters)); });
                    }
                    break;

                case "grab":
                    if (OnGrab != null)
                    {
                        ExecuteSync(() => { OnGrab(this, new Grab() { i = int.Parse(parameters) }); });
                    }
                    break;

                case "userJoin":
                    if (OnUserJoin != null)
                    {
                        var join = JsonConvert.DeserializeObject<User>(parameters);
                        ExecuteSync(() => { OnUserJoin(this, join); });
                    }
                    break;

                case "userLeave":
                    if (OnUserLeave != null)
                    {
                        var leaveid = JsonConvert.DeserializeObject<int>(parameters);
                        ExecuteSync(() => { OnUserLeave(this, new UserLeave() { i = leaveid }); });
                    }
                    break;

                case "advance":
                    var ad = JsonConvert.DeserializeObject<Advance>(parameters);
                    if (OnAdvance != null)
                    {
                        ExecuteSync(() => { OnAdvance(this, ad); });
                    }
                    SendChatLog("Song changed to: " + ad.m.title, ChatLogType.SongChange);
                    break;

                default:
                    DebugLog("Unknown type: " + type);
                    break;
            }
        }

        #endregion private

        public async Task ConnectAsync(string slug = null)
        {
            LastSlug = slug ?? LastSlug;
            if (LastSlug == null)
            {
                throw new ArgumentException("Slug was't set, please give the room you want to connect to");
            }
            if (!Connecting)
            {
                if (SocketConnected)
                {
                    await Websocket.Close();
                }
                Connecting = true;
                SendChatLog($"Connecting to {LastSlug}");
                var sfstData = await _GetcfstAsync();
                await _LoginAsync(sfstData);
                var authtoken = await _GetAuthTokenAsync();
                while (!SocketConnected) { await Websocket.Open(sfstData.s); }
                Websocket.StartListen();
                Reconnected = 0;
                await Websocket.Send(Stringify(new SendSocket() { a = "auth", p = authtoken, t = TimeStamp }));
                await WebClient.PostAsync("https://plug.dj/_/rooms/join", new { slug = LastSlug });
                ExecuteSafe(() =>
                {
                    OnConnected?.Invoke(this, this);
                });
                await ExecuteSafeAsync(async () =>
                {
                    if (OnAdvance != null)
                    {
                        try
                        {
                            var advance = await GetFakeAdvanceFromStateAsync();
                            ExecuteSync(() => { OnAdvance(this, advance); });
                        }
                        catch { }
                    }
                });
                Connecting = false;
            }
        }

        public async void TryConnect(string slug = null, Action OnConnected = null, Action<ErrorEventArgs> OnFail = null)
        {
            await ExecuteSafeAsync(async () =>
            {
                await ConnectAsync(slug);
                OnConnected?.Invoke();
            }, OnFail);
        }

        public async Task Disconnect(bool allowreconnect = false)
        {
            if (SocketConnected)
            {
                await Websocket.Close();
            }
            ExecuteSafe(() => OnDisconnected?.Invoke(this, this));
            if (Reconnect && allowreconnect)
            {
                await DoReconnect();
            }
        }

        protected async Task DoReconnect()
        {
            if (ReconnectLimit < 0 || Reconnected + 1 <= ReconnectLimit)
            {
                Reconnected++;
                await ConnectAsync();
            }
        }

        public Task SendChat(string message)
        {
            return Websocket.Send(Stringify(new SendSocket() { a = "chat", p = message, t = TimeStamp }));
        }

        protected void SendChatLog(string message, ChatLogType type = ChatLogType.Info)
        {
            if (OnChatLog != null)
            {
                ExecuteSync(() => { OnChatLog(this, new ChatLog() { Message = message, Type = type }); });
            }
            DebugLog(message);
        }

        private async Task<Advance> GetFakeAdvanceFromStateAsync()
        {
            var state = (await API.GetRoomState()).data.First();
            return new Advance() { c = state.booth.currentDJ, d = state.booth.waitingDJs, h = state.playback.historyID, m = state.playback.media, p = state.playback.playlistID, t = state.playback.startTime };
        }

        public void Dispose()
        {
            Disconnect().Wait();
            Websocket?.Dispose();
            WebClient?.Dispose();
        }

        public class ErrorEventArgs : EventArgs
        {
            public Exception Error { get; set; }
            public bool Ignore { get; set; }
        }

        public struct ChatLog
        {
            public ChatLogType Type;
            public string Message;
        }

        public enum ChatLogType
        {
            SongChange, PlayChange, Info, Error, Debug
        }
    }
}