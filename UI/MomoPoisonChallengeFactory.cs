using LiveSplit.Model;
using System;

namespace LiveSplit.UI.Components
{
    class MomoPoisonChallengeFactory : IComponentFactory
    {
        public string ComponentName => "Momodora RUtM Poison Challenge";

        public string Description => "An interesting challenge where you are permanently poisoned for Momodora: Reverie Under the Moonlight";

        public ComponentCategory Category => ComponentCategory.Other;

        public string UpdateName => ComponentName;

        public string XMLURL => UpdateURL + "update.MomoPoisonChallenge.xml";

        public string UpdateURL => "https://raw.githubusercontent.com/Ladnok/MomodoraPoisonChallenge/main/Updates/";

        public Version Version => Version.Parse("1.0");

        public IComponent Create(LiveSplitState state)
        {
            return new MomoPoisonChallenge(state);
        }
    }
}
