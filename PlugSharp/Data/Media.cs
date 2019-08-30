namespace PlugSharp.Data
{
    public struct Media
    {
        /// <summary>
        /// Media author
        /// </summary>
        public string author;

        /// <summary>
        /// Media format (1 = youtube; 2 = soundcloud)
        /// </summary>
        public int format;

        /// <summary>
        /// Thumbnail (youtube) or Artwork (soundcloud)
        /// </summary>
        public string image;

        /// <summary>
        /// Media ID used on the originating website
        /// </summary>
        public string cid;

        /// <summary>
        /// Duration in seconds
        /// </summary>
        public int duration;

        /// <summary>
        /// Media title
        /// </summary>
        public string title;

        /// <summary>
        /// Internal media ID
        /// </summary>
        public int id;
    }
}