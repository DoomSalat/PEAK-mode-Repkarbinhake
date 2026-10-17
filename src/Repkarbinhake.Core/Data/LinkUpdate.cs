namespace Repkarbinhake.Core.Data
{
    public readonly struct LinkUpdate
    {
        public LinkUpdate(string key, LinkState state, int writerActor, int sequence)
        {
            Key = key;
            State = state;
            WriterActor = writerActor;
            Sequence = sequence;
        }

        public string Key { get; }
        public LinkState State { get; }
        public int WriterActor { get; }
        public int Sequence { get; }
    }
}
