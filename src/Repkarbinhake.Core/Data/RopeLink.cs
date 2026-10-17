namespace Repkarbinhake.Core.Data
{
    public sealed class RopeLink
    {
        public RopeLink(int firstActor, int secondActor, long tieId)
        {
            FirstActor = firstActor < secondActor ? firstActor : secondActor;
            SecondActor = firstActor < secondActor ? secondActor : firstActor;
            First = new RopeEnd(FirstActor);
            Second = new RopeEnd(SecondActor);
            TieId = tieId;
        }

        public int FirstActor { get; }
        public int SecondActor { get; }
        public RopeEnd First { get; }
        public RopeEnd Second { get; }
        public long TieId { get; private set; }
        public string Key => LinkKeys.Build(FirstActor, SecondActor);
        public bool IsHeld => First.IsHeld || Second.IsHeld;
        public float TotalReel => First.HeldReel + Second.HeldReel;

        public bool Contains(int actor)
        {
            return actor == FirstActor || actor == SecondActor;
        }

        public int Partner(int actor)
        {
            return actor == FirstActor ? SecondActor : FirstActor;
        }

        public RopeEnd EndOf(int actor)
        {
            return actor == FirstActor ? First : Second;
        }

        public bool IsHeldBy(int actor)
        {
            return Contains(actor) && EndOf(actor).IsHeld;
        }

        public string EndKey(int actor)
        {
            return LinkKeys.BuildEnd(FirstActor, SecondActor, actor);
        }

        public void Retie(long tieId)
        {
            TieId = tieId;
            First.Reset();
            Second.Reset();
        }
    }
}
