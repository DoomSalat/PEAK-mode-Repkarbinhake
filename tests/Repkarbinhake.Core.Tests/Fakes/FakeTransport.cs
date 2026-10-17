using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;

namespace Repkarbinhake.Core.Tests.Fakes
{
    public sealed class FakeTransport : ILinkTransport
    {
        public FakeTransport(bool echoesOwnUpdates = true)
        {
            EchoesOwnUpdates = echoesOwnUpdates;
        }

        public bool EchoesOwnUpdates { get; }
        public List<LinkUpdate> Published { get; } = new List<LinkUpdate>();
        public List<EndUpdate> PublishedEnds { get; } = new List<EndUpdate>();
        public List<LinkUpdate> Deleted { get; } = new List<LinkUpdate>();
        public LinkUpdate Last => Published[Published.Count - 1];
        public EndUpdate LastEnd => PublishedEnds[PublishedEnds.Count - 1];

        public void Publish(LinkUpdate update)
        {
            Published.Add(update);
        }

        public void PublishEnd(EndUpdate update)
        {
            PublishedEnds.Add(update);
        }

        public void DeleteIfUnchanged(LinkUpdate expected)
        {
            Deleted.Add(expected);
        }
    }
}
