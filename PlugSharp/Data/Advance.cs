using PlugSharp.Data;

namespace PlugSharp
{
    public struct Advance
    {
        /// <summary>
        /// Current DJ
        /// </summary>
        public int c;
        /// <summary>
        /// Waitlist DJ's
        /// </summary>
        public int[] d;
        /// <summary>
        /// GUID v4 History ID
        /// </summary>
        public string h;
        /// <summary>
        /// Media object
        /// </summary>
        public Media m;
        /// <summary>
        /// Playlist ID
        /// </summary>
        public int p;
        /// <summary>
        /// Starting Timestamp
        /// </summary>
        public string t;
    }
}
