namespace PlugSharp.Data
{
    public struct Playback
    {
        /// <summary>
        /// HistoryID of the playback
        /// </summary>
        public string historyID;

        /// <summary>
        /// ID of the playlist this media is in
        /// </summary>
        public int playlistID;

        /// <summary>
        /// Date and time when the media started playing
        /// </summary>
        public string startTime;

        public Media media;
    }
}