namespace PlugSharp.Data
{
    public struct RoomStateMeta
    {
        /// <summary>
        /// Description of the room
        /// </summary>
        public string description;

        /// <summary>
        /// Is this room favorited by you?
        /// </summary>
        public bool favorite;

        /// <summary>
        /// Amount of guests connected
        /// </summary>
        public int guests;

        /// <summary>
        /// ID of the host
        /// </summary>
        public int hostID;

        /// <summary>
        /// Name of the host
        /// </summary>
        public string hostName;

        /// <summary>
        /// ID of the room
        /// </summary>
        public int id;

        /// <summary>
        /// Minimum level needed to chat
        /// </summary>
        public int minChatLevel;

        /// <summary>
        /// Name of the room
        /// </summary>
        public string name;

        /// <summary>
        /// Amount of real users in the room (guests excluded)
        /// </summary>
        public int population;

        /// <summary>
        /// URL conform representation of the room's name
        /// </summary>
        public string slug;

        /// <summary>
        /// Welcome message of the room
        /// </summary>
        public string welcome;
    }
}