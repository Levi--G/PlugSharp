namespace PlugSharp.Data
{
    public struct RoomHistory
    {
        public string id;           // Internal GUID v4 history ID
        public Media media;
        public Room room;
        public Score score;
        public string timestamp;    // Indicates when this media was played
        public User user;
    }
}