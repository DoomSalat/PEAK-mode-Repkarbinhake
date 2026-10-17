namespace Repkarbinhake.Core.Data
{
    public readonly struct LinkState
    {
        public const long NoTie = 0;

        private const int WriterShift = 32;

        public LinkState(bool isTied, long tieId)
        {
            IsTied = isTied;
            TieId = tieId;
        }

        public bool IsTied { get; }
        public long TieId { get; }

        public static LinkState Untied => new LinkState(false, NoTie); // Static: constant value of the struct

        public static long ComposeTieId(int writerActor, int sequence) // Static: pure function shared by registry and tests
        {
            return ((long)writerActor << WriterShift) | (uint)sequence;
        }
    }
}
