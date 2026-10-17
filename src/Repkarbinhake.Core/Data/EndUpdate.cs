namespace Repkarbinhake.Core.Data
{
    public readonly struct EndUpdate
    {
        public EndUpdate(string key, EndState state)
        {
            Key = key;
            State = state;
        }

        public string Key { get; }
        public EndState State { get; }
    }
}
