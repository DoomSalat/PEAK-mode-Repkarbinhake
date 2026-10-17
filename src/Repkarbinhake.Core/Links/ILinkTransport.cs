using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Links
{
    public interface ILinkTransport
    {
        bool EchoesOwnUpdates { get; }

        void Publish(LinkUpdate update);
        void PublishEnd(EndUpdate update);
        void DeleteIfUnchanged(LinkUpdate expected);
    }
}
