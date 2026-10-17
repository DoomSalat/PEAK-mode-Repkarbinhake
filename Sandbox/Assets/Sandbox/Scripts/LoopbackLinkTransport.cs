using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;

namespace Repkarbinhake.Sandbox
{
    public sealed class LoopbackLinkTransport : ILinkTransport
    {
        public bool EchoesOwnUpdates => false;
        public int PublishedCount { get; private set; }

        public void Publish(LinkUpdate update)
        {
            PublishedCount++;
        }

        public void PublishEnd(EndUpdate update)
        {
            PublishedCount++;
        }

        public void DeleteIfUnchanged(LinkUpdate expected)
        {
        }
    }
}
