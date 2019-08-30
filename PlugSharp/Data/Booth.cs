namespace PlugSharp.Data
{
    public struct Booth
    {
        public int currentDJ;
        public bool isLocked;
        public bool shouldCycle;
        public int[] waitingDJs; //todo: find type
    }
}