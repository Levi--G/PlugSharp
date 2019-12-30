using System.Collections.Generic;

namespace PlugSharp.Data
{
    public struct RoomState
    {
        public Booth booth;
        public object fx;
        public Dictionary<string, int> grabs;
        public RoomStateMeta meta;
        public IDictionary<string, int> mutes;
        public Playback playback;
        public StaffRole role;
        public User[] users;
        public IDictionary<string, int> votes;
    }
}