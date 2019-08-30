namespace PlugSharp.Data
{
    public struct Chat
    {
        /// <summary>
        /// The message
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// Sender UserName
        /// </summary>
        public string un { get; set; }
        /// <summary>
        /// Chat ID
        /// </summary>
        public string cid { get; set; }
        /// <summary>
        /// Sender ID
        /// </summary>
        public int uid { get; set; }
        /// <summary>
        /// Is the user is subscribed (0 = no, 1 = yes)
        /// </summary>
        public int sub { get; set; }
    }
}
