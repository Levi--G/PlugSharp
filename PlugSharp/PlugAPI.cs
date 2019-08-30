using JsonHCSNet.Proxies.ApiDefinition;
using System.Threading.Tasks;
using PlugSharp.Data;

namespace PlugSharp
{
    [Route("https://plug.dj/_/")]
    public abstract class PlugAPI
    {
        [HttpGet("auth/token")]
        public abstract Task<Response<string, object>> GetAuthToken();

        [HttpGet("mobile/init")]
        public abstract Task<Response<cfst, object>> GetMobileInit();

        [HttpPost("auth/login")]
        public abstract Task Login(LoginRequest request);

        [HttpPost("booth/skip")]
        public abstract Task Skip(Skip skip);

        public Task Skip(Advance advance)
        {
            return Skip(new Skip() { userID = advance.c, historyID = advance.h });
        }

        [HttpGet("playlists")]
        public abstract Task<Response<Playlist, object>> GetPlaylists();

        [HttpPost("playlists")]
        public abstract Task<Response<Playlist, object>> CreatePlaylist(CreatePlaylistRequest request);

        [HttpDelete("playlists/{id}")]
        public abstract Task<Response<Playlist, object>> CreatePlaylist(int id);

        [HttpPost("votes")]
        public abstract Task Vote(VoteRequest vote);

        [HttpPost("grabs")]
        public abstract Task Grab(GrabRequest vote);

        [HttpGet("rooms/state")]
        public abstract Task<Response<RoomState, object>> GetRoomState();

        [HttpGet("staff")]
        public abstract Task<Response<User, object>> GetStaff();

        [HttpDelete("chat/{cid}")]
        public abstract Task<Response<object, object>> DeleteMessage(string cid);

        [HttpGet("users/{id}")]
        public abstract Task<Response<User, object>> GetUser(int id);

    }
}