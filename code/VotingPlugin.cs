using System;
using Exiled.API.Features;

namespace VotingPlugin
{
    public class VotingPlugin : Plugin<Config>
    {
        public override string Name => "Poll";
        public override string Author => "katana";
        public override Version Version => new Version(1, 0);
        public override Version RequiredExiledVersion => new Version(9, 0, 0);

        public static VotingPlugin Instance { get; private set; }
        public PollManager PollManager { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;
            PollManager = new PollManager();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            PollManager?.Cleanup();
            PollManager = null;
            Instance = null;
            base.OnDisabled();
        }
    }
}
