namespace PlugSharp.Data
{
    public struct Vote
    {
        public enum VoteType
        {
            woot = 1, meh = -1
        }
        public int i;
        public VoteType v;
    }
}
